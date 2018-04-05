using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Stock_Trade_Configuration
{
    public class StockOrderTicker
    {
        private static StockOrderTicker _instance = new StockOrderTicker();
        private bool _isCancelled = true;
        public static StockOrderTicker Instance
        {
            get { return _instance; }
        }


        private StockOrderTicker()
        {

        }

        KiteConnect.Kite _kite;
        
        IEnumerable<RangeBreakOutConfiguration> _configurations;
        public async void Start(TimeSpan interval, KiteConnect.Kite kite, IEnumerable<RangeBreakOutConfiguration> stockSymbols)
        {
            _kite = kite;
            _configurations = stockSymbols;
            if (_isCancelled)
            {
                _isCancelled = false;
                await CheckAndOrderStockWhenRangeBreaks();
            }

        }

        private async Task CheckAndOrderStockWhenRangeBreaks()
        {

            await Task.Run(() =>
            {
                while (true)
                {
                    OrderStockWhenItBreaksRange();
                    Task.Delay(1000);
                    if (_isCancelled)
                        break;
                }
            });

        }
        public void Stop()
        {
            _isCancelled = true;
        }

        private bool IsEligible(RangeBreakOutConfiguration stockConfiguration, KiteConnect.LTP ltp)
        {
            switch(stockConfiguration.OrderCondition)
            {
                case Condition.GreaterThanEqualTo:
                    return ltp.LastPrice >= stockConfiguration.BreakOutPrice;
                case Condition.LessThanEqualTo:
                    return ltp.LastPrice <= stockConfiguration.BreakOutPrice;
            }
            return false;
        }

        private bool PlaceOrder(RangeBreakOutConfiguration stockConfiguration, KiteConnect.LTP ltp)
        {
            var orderTypeStr = GetOrderString(stockConfiguration.OrderType);
            if(!string.IsNullOrEmpty(orderTypeStr))
            {
                try
                {
                    var target = String.Format("{0:0.00}", Math.Abs(stockConfiguration.BreakOutPrice - stockConfiguration.BuySellPrice));
                    var symbol = stockConfiguration.StockSymbol.Split(new char[] { ':' });
                    var request = _kite.PlaceOrder(symbol[0], symbol[1], stockConfiguration.OrderMode.ToString(), stockConfiguration.Quantity,  OrderType:"MARKET", Price: stockConfiguration.BreakOutPrice, Product: "MIS", StoplossValue: stockConfiguration.StopLoss,TrailingStoploss:0, SquareOffValue: stockConfiguration.StopLoss, Variety:"co",Validity:"DAY");
                    return request.Any(s=>s.Key.ToLower()=="status" && s.Value.ToLower()=="success");
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        private string GetOrderString(OrderType orderType)
        {
            switch(orderType)
            {
                case OrderType.BracketOrder:
                    return "BO";
                    
                case OrderType.CoverOrder:
                    return "CO";
            }
            return "";
        }

        private void OrderStockWhenItBreaksRange()
        {
            try
            {
                var orderWhichIsNotPlaced = _configurations.Where(s => s.OrderStatus == OrderStatus.YetToPlaceOrder);
                if (!orderWhichIsNotPlaced.Any())
                    Stop();

                var stockSymbols = orderWhichIsNotPlaced.Select(s => s.StockSymbol).Distinct().ToArray();
                var stocksLTP = _kite.GetLTP(stockSymbols);
                foreach (var stockLTP in stocksLTP)
                {
                    var buyStock = orderWhichIsNotPlaced.FirstOrDefault(s => s.StockSymbol == stockLTP.Key && s.OrderMode == OrderMode.BUY);
                    var sellStock = orderWhichIsNotPlaced.FirstOrDefault(s => s.StockSymbol == stockLTP.Key && s.OrderMode == OrderMode.SELL);
                    bool isPlaced = false;
                    if(buyStock!= null)
                    {
                        if (IsEligible(buyStock, stockLTP.Value))
                            if (PlaceOrder(buyStock, stockLTP.Value))
                                buyStock.OrderStatus = OrderStatus.OrderPlaced;
                    }
                    if(sellStock!=null && !isPlaced)
                    {
                        if (IsEligible(sellStock, stockLTP.Value))
                            if (PlaceOrder(sellStock, stockLTP.Value))
                                sellStock.OrderStatus = OrderStatus.OrderPlaced;
                    }
                    
                }
            }
            catch (Exception ex)
            {

            }
        }

        private List<string> _currentPlaceOrders = new List<string>();
        private void CheckOrderStatus()
        {

        }
    }
}
