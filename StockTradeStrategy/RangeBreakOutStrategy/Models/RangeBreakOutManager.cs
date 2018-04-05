using KiteConnect;
using StockTradeConfiguration.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.RangeBreakOutStrategy.Models
{
    public enum RangeBreakoutProfitType
    {
        Absolute,
        Percentage,
        Ratio
    }

    public class RangeBreakOutManager
    {
        #region private variables
        private ConcurrentDictionary<string, RangeBreakOutStockConfig> _configStockDictionary;
        #endregion

        #region Singleton
        private RangeBreakoutProfitType profitType;
        private KiteConnect.Kite _kite;
        private static RangeBreakOutManager _instance = new RangeBreakOutManager();
        public static RangeBreakOutManager Instance
        {
            get { return _instance; }
        }

        private RangeBreakOutManager()
        {
            Events.OrderHistoryFetchedEvent += Events_OrderHistoryFetchedEvent;
            Events.GiveKiteInstanceEvent += Events_GiveKiteInstanceEvent;
            Events.RaiseAskForKiteInstanceEvent();
        }

        private void Events_GiveKiteInstanceEvent(Kite kite)
        {
            _kite = kite;
            Events.GiveKiteInstanceEvent -= Events_GiveKiteInstanceEvent;
        }

        #endregion

        #region Public Methods
        public void WatchStocksForRangeBreakout(IEnumerable<RangeBreakOutStockConfig> configs)
        {
            _configStockDictionary = new ConcurrentDictionary<string, RangeBreakOutStockConfig>(configs.Select(s => s.CloneObject<RangeBreakOutStockConfig>()).Select(s => new KeyValuePair<string, RangeBreakOutStockConfig>(s.TradingSymbol, s)));
            foreach (var stockSymbol in _configStockDictionary)
            {
                var stock = stockSymbol.Value;
                stock.OrderStatus = OrderStatus.CheckOrderStarted;
                Events.RaiseAskForStockSubscriptionEvent(stockSymbol.Key, StockSubscribeMode.LTP, true);
                Events.RaiseStatusChangedEvent(null, stockSymbol.Key, OrderStatus.CheckOrderStarted.ToString());
            }

            Events.StockLTPChangedEvent += Events_StockLTPChangedEvent;
        }

        #endregion

        #region private methods
        private void Events_OrderHistoryFetchedEvent(string tradingSymbol, IEnumerable<Order> orderHistory)
        {
            CheckOrderAsync(tradingSymbol, orderHistory);
        }

        private async void CheckOrderAsync(string tradingSymbol, IEnumerable<Order> orderHistory)
        {
            await Task.Factory.StartNew(() =>
            {
                if (_configStockDictionary.ContainsKey(tradingSymbol))
                {
                    var stock = _configStockDictionary[tradingSymbol];

                    var bs = stock.AllOrderStatus.FirstOrDefault(s => s.Key == OrderType.BracketOrder);
                    if (bs.Key !=null && bs.Value.Value ==  OrderStatus.Ordered)
                    {
                        if (orderHistory.Any(s => s.OrderId == bs.Value.Key && s.Status.Equals("complete", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            var order = orderHistory.FirstOrDefault(s => s.OrderId == bs.Value.Key);
                            stock.AllOrderStatus[OrderType.BracketOrder] = new KeyValuePair<string, OrderStatus>(order.OrderId, order.TransactionType.Equals("buy", StringComparison.InvariantCultureIgnoreCase) ? OrderStatus.BuyOrderInProgress: OrderStatus.SellOrderInProgress);
                            Events.RaiseAskForOrderSubscriptionEvent(order.OrderId, false);
                        }
                    }

                    ManipulateCoverOrderInfo(orderHistory, stock);

                    RaiseStatusChangeEvent(stock);
                }
            });
        }

        private static bool ManipulateCoverOrderInfo(IEnumerable<Order> orderHistory, RangeBreakOutStockConfig stock)
        {
            bool hasBothEntered;
            if (orderHistory.Any(s => s.ParentOrderId == stock.ParentOrderId))
            {
                var order = orderHistory.FirstOrDefault(s => s.ParentOrderId == stock.ParentOrderId);
                stock.TargetOrderId = order.OrderId;
                hasBothEntered = true;

            }

            if (orderHistory.Any(s => s.OrderId == stock.ParentOrderId))
            {
                try
                {
                    var order = orderHistory.FirstOrDefault(s => s.AveragePrice != 0 && s.OrderId == stock.ParentOrderId);
                    {
                        stock.TargetPrice = order.TransactionType.Equals("buy", StringComparison.InvariantCultureIgnoreCase) ?
                                           order.AveragePrice + (order.AveragePrice * (stock.ProfitMargin / 100.0m)) :
                                           order.AveragePrice - (order.AveragePrice * (stock.ProfitMargin / 100.0m));
                    }
                    if (stock.TargetPrice != null)
                        hasBothEntered = true;
                    else
                        hasBothEntered = false;
                }
                catch (Exception)
                {

                    hasBothEntered = false;
                }
            }
            else
                hasBothEntered = false;

            if (hasBothEntered)
                Events.RaiseAskForOrderSubscriptionEvent(stock.ParentOrderId, false);
            return hasBothEntered;
        }

        private void Events_StockLTPChangedEvent(string tradingSymbol, decimal lastPrice)
        {
            CheckBreakoutAndPlaceOrderAsync(tradingSymbol, lastPrice);
        }

        private async void CheckBreakoutAndPlaceOrderAsync(string tradingSymbol, decimal lastPrice)
        {
            await Task.Factory.StartNew(() =>
            {
                if (_configStockDictionary.ContainsKey(tradingSymbol))
                {
                    var stock = _configStockDictionary[tradingSymbol];

                    //Avoid calling multiple check when there is already Check process is going on for stock
                    if (stock.IsCheckInProgress)
                        return;

                    stock.IsCheckInProgress = true;

                    if (stock.OrderStatus != OrderStatus.TargetHit)
                    {
                        var orderToExecute = IsEligible(stock, lastPrice);
                        bool hasOrderPlacedSuccessfully = false;
                        if (orderToExecute != null)
                        {
                            hasOrderPlacedSuccessfully = PlaceOrder(stock, orderToExecute.Value);
                            if (hasOrderPlacedSuccessfully)
                            {
                                stock.OrderStatus = orderToExecute.Value == OrderMode.BUY ? OrderStatus.BuyOrderInProgress : OrderStatus.SellOrderInProgress;
                                RaiseStatusChangeEvent(stock);
                            }
                        }

                        if(stock.AllOrderStatus.Any(s => s.Key == OrderType.BracketOrder && s.Value.Value == OrderStatus.Ordered))
                        {
                            //if(IsEligibleToCancelBracketOrder(stock, lastPrice))
                            //{
                            //    var order = stock.AllOrderStatus.FirstOrDefault(s => s.Key == OrderType.BracketOrder);
                            //    _kite.CancelOrder(order.Value.Key);
                            //}
                        }
                    }
                    
                    stock.IsCheckInProgress = false;
                }
            });
        }

        private bool IsEligibleToCancelBracketOrder(RangeBreakOutStockConfig stock, decimal lastPrice)
        {
            var targetInfo = stock.AllOrderTargetPrice.FirstOrDefault(s => s.Key == OrderType.BracketOrder);
            if(targetInfo.Key != null)
            {
                if (targetInfo.Value.Key == OrderMode.BUY && lastPrice >= targetInfo.Value.Value)
                {
                    return true;
                }
                else if (targetInfo.Value.Key == OrderMode.SELL && lastPrice <= targetInfo.Value.Value)
                    return true;
            }
            return false;

        }

        //Exiting order for profit booking
        private void ExitOrder(RangeBreakOutStockConfig stock)
        {
            try
            {
                if (!string.IsNullOrEmpty(stock.TargetOrderId) && !string.IsNullOrEmpty(stock.ParentOrderId))
                {
                    var orderResponse = _kite.CancelOrder(stock.TargetOrderId, Variety: "co", ParentOrderId: stock.ParentOrderId);
                    RaiseStatusChangeEvent(stock);
                    if (orderResponse.Any(s => s.Key.ToLower() == "status" && s.Value.ToLower() == "success"))
                    {
                        RangeBreakOutStockConfig stockOut;
                        _configStockDictionary.TryRemove(stock.TradingSymbol, out stock);
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        
        private bool PlaceOrder(RangeBreakOutStockConfig stock, OrderMode mode)
        {
            var quantity = stock.GetQuantity();
            try
            {
                    if (stock.OrderType == OrderType.CoverOrder)
                        return PlaceCoverOrder(stock, mode, quantity);
                    else if(stock.OrderType == OrderType.BracketOrder)
                        return PlaceCoverOrder(stock, mode, quantity);
                    else if(stock.OrderType == OrderType.CoverAndBracketOrder)
                    {
                       Task task1 = Task.Factory.StartNew(()=> PlaceCoverOrder(stock, mode, quantity));
                    Task task2 = Task.Factory.StartNew(() => PlaceBracketOrder(stock, mode, quantity));
                    Task.WaitAll(task1, task2);
                    return true;
                    }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool PlaceCoverOrder(RangeBreakOutStockConfig stock, OrderMode mode, int quantity)
        {
            stock.StopLoss = mode == OrderMode.BUY ? stock.SellBreakOutPrice : stock.BuyBreakOutPrice;
            Dictionary<string, dynamic> orderResponse = new Dictionary<string, dynamic>();

            orderResponse = _kite.PlaceOrder(stock.Exchange, stock.Symbol, mode.ToString(), quantity, Product: Constants.PRODUCT_MIS, OrderType: Constants.ORDER_TYPE_MARKET, TriggerPrice: stock.StopLoss, Variety: stock.Variety, Validity: Constants.VALIDITY_DAY);
            if (orderResponse.Any(s => s.Key.ToLower() == "status" && s.Value.ToLower() == "success"))
            {
                stock.TargetPrice = null;
                string orderId = GetOrderId(orderResponse);
                Events.RaiseAskForOrderSubscriptionEvent(orderId, true);
                stock.ParentOrderId = orderId;
                stock.LastOrderedQuantity = quantity;
                stock.ReversalNumber++;
                stock.OrderedQuantity = quantity;
                return true;
            }
            else
                return true;

        }

        private bool PlaceBracketOrder(RangeBreakOutStockConfig stock, OrderMode mode, int quantity)
        {
            
            var stopLoss = Math.Abs(stock.SellBreakOutPrice - stock.BuyBreakOutPrice);
            Dictionary<string, dynamic> orderResponse = new Dictionary<string, dynamic>();
            var triggerPrice = mode == OrderMode.BUY ? stock.BuyBreakOutPrice : stock.SellBreakOutPrice;
            var targetPrice = mode == OrderMode.BUY ? 
                                                  Math.Round(( stock.BuyBreakOutPrice + (stock.BuyBreakOutPrice * (stock.ProfitMargin / 100.0m))),2).GetNextValidPrice(true) :
                                                   Math.Round((stock.SellBreakOutPrice - (stock.SellBreakOutPrice * (stock.ProfitMargin / 100.0m))),2).GetNextValidPrice(true);

            var targetPoint = mode == OrderMode.BUY ? 
                (targetPrice - stock.BuyBreakOutPrice) : 
                (stock.SellBreakOutPrice - targetPrice);

            orderResponse = _kite.PlaceOrder(stock.Exchange, stock.Symbol, mode.ToString(), quantity, Product: Constants.PRODUCT_MIS, OrderType: Constants.ORDER_TYPE_LIMIT, Price: triggerPrice , Variety: Constants.VARIETY_BO, Validity: Constants.VALIDITY_DAY,SquareOffValue:targetPoint, StoplossValue:stopLoss);
            if (orderResponse.Any(s => s.Key.ToLower() == "status" && s.Value.ToLower() == "success"))
            {
                string orderId = GetOrderId(orderResponse);
                stock.AllOrderStatus[OrderType.BracketOrder] = new KeyValuePair<string, OrderStatus>(orderId, OrderStatus.Ordered);
                stock.AllOrderTargetPrice[OrderType.BracketOrder] = new KeyValuePair<OrderMode, decimal>(mode, targetPrice);
                Events.RaiseAskForOrderSubscriptionEvent(orderId, true);
                return true;
            }
            else
                return false;
        }

        private string GetOrderId(Dictionary<string, dynamic> orderResponse)
        {
            if (orderResponse.Any(s => s.Key.ToLower() == "status" && s.Value.ToLower() == "success"))
            {
                if (orderResponse.ContainsKey("data"))
                {
                    var data = orderResponse["data"];
                    if (data is ICollection)
                    {
                        foreach (var item in data)
                        {
                            if (item is KeyValuePair<string, object>)
                            {
                                if (item.Key == "order_id")
                                {
                                    return Convert.ToString(item.Value);
                                }
                            }
                        }
                    }
                    else
                        return null;
                    //var dat = data[]
                }
                else
                    return null;
            }
            return null;
        }

        private OrderMode? IsEligible(RangeBreakOutStockConfig stockConfiguration, decimal ltp)

        {

            if (stockConfiguration.OrderStatus != OrderStatus.BuyOrderInProgress && stockConfiguration.OrderStatus!= OrderStatus.SellOrderInProgress)
            {
                if (ltp > stockConfiguration.BuyBreakOutPrice)
                    return OrderMode.BUY;
                else if (ltp < stockConfiguration.SellBreakOutPrice)
                    return OrderMode.SELL;
            }
            else if(stockConfiguration.TargetPrice!=null &&( stockConfiguration.OrderStatus == OrderStatus.BuyOrderInProgress || stockConfiguration.OrderStatus == OrderStatus.SellOrderInProgress))
            {
                switch(stockConfiguration.OrderStatus)
                {
                    case OrderStatus.BuyOrderInProgress:
                        if(ltp >= stockConfiguration.TargetPrice)
                        {
                            stockConfiguration.OrderStatus = OrderStatus.TargetHit;
                            ExitOrder(stockConfiguration);
                        }
                        else if(ltp <= stockConfiguration.StopLoss)
                        {
                            stockConfiguration.TargetPrice = null;
                            stockConfiguration.OrderStatus = OrderStatus.SellOrderInProgress;
                            return OrderMode.SELL;
                        }
                        break;
                    case OrderStatus.SellOrderInProgress:
                        if(ltp <= stockConfiguration.TargetPrice)
                        {
                            stockConfiguration.OrderStatus = OrderStatus.TargetHit;
                            ExitOrder(stockConfiguration);
                        }
                        else if (ltp >= stockConfiguration.StopLoss)
                        {
                            stockConfiguration.TargetPrice = null;
                            stockConfiguration.OrderStatus = OrderStatus.BuyOrderInProgress;
                            return OrderMode.BUY;
                        }
                        break;
                }
            }

            return null;
        }

        private void RaiseStatusChangeEvent(RangeBreakOutStockConfig stock)
        {
            Events.RaiseUpdateObject(stock);
        }
        #endregion
    }
}
