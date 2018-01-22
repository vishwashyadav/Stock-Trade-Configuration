using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KiteConnect;
using StockTradeConfiguration.Models;

namespace StockTrade.Jobbing
{
    [JobbingType(Name ="Pivot Jobbing")]
    public class PivotJobbing : JobbingStockBase
    {
        private int _currentOpenPosition = 0;
        private int _transactionCount = 0;
        public StockIncrementalMethod IncrementalMethod { get; set; }
        public int IncrementalNumber { get; set; }
        public OrderOnPrice OrderOnPriceType { get; set; }
        public bool OpenForOrder { get; set; }
        public decimal PivotPrice { get; set; }
        public decimal TargetPrice { get; set; }
        private OrderMode? lastOrder { get; set; }

        #region Override Methods
        public override void BuySellStock(LTP ltp)
        {
            OrderMode orderMode = OrderMode.BUY;
            var lastPrice = ltp.LastPrice;
            bool canPlaceOrder = CanPlaceOrder(lastPrice, ref orderMode);
            if (canPlaceOrder)
            {
                int quantity = (_transactionCount == 0 || IncrementalNumber == 0) ? StocksBuySellQuantityStart :
                    (IncrementalMethod == StockIncrementalMethod.Addition ? (_transactionCount + IncrementalNumber) :
                    (_transactionCount * IncrementalNumber));
                if (!lastOrder.HasValue)
                {
                    PlaceOrder(orderMode.ToString(), quantity.ToString(), lastPrice.ToString(), OrderOnPrice.MarketPrice);
                    _currentOpenPosition += StocksBuySellQuantityStart;
                }
                else
                {
                    if(lastOrder.Value == orderMode)
                    {
                        //1. Close Previouse Position
                        var closePositionOrder = orderMode == OrderMode.BUY ? OrderMode.SELL : OrderMode.BUY;
                        PlaceOrder(closePositionOrder.ToString(), _currentOpenPosition.ToString(), lastPrice.ToString(), OrderOnPrice.MarketPrice);
                        PlaceOrder(closePositionOrder.ToString(), quantity.ToString(), lastPrice.ToString(), OrderOnPrice.MarketPrice);
                        PivotPrice = lastPrice;
                        orderMode = closePositionOrder;
                    }
                    else
                    {
                        PlaceOrder(orderMode.ToString(), quantity.ToString(), lastPrice.ToString(), OrderOnPrice.MarketPrice);
                    }
                }
                _currentOpenPosition = quantity;
                lastOrder = orderMode;
                _transactionCount++;
            }
        }
        public override void ExecutedOrder(Order order)
        {
            if(order.Status.Equals("completed", StringComparison.InvariantCultureIgnoreCase))
            {
                if(lastOrder.Value == OrderMode.BUY)
                {
                    TargetPrice = (MarginType == MarginType.Absolute) ?
                        (order.AveragePrice + Margin).GetNextValidPrice(true) :
                        (((order.AveragePrice * Margin) / 100.0m) + order.AveragePrice).GetNextValidPrice(true);
                    
                                 
                }
                else if(lastOrder.Value == OrderMode.SELL)
                {
                    TargetPrice = (MarginType == MarginType.Absolute) ?
                        (order.AveragePrice - Margin).GetNextValidPrice(false) :
                        (order.AveragePrice -  ((order.AveragePrice * Margin) / 100.0m)).GetNextValidPrice(false);

                }
                PivotPrice = order.AveragePrice;
            }
        }
        #endregion
        private bool CanPlaceOrder(decimal lastPrice, ref OrderMode orderMode)
        {
            bool shouldPlaceOrder = false;

            if (Math.Abs(OpenPositions) > 0)
            {
                if (lastPrice >= TargetPrice)
                {
                    //Place Buy Order
                    shouldPlaceOrder = true;
                    orderMode = OrderMode.BUY;
                }
                else if (lastPrice < PivotPrice)
                {
                    //Place Sell Order
                    shouldPlaceOrder = true;
                    orderMode = OrderMode.SELL;
                }
            }
            else
            {
                if (lastPrice > PivotPrice)
                {
                    orderMode = OrderMode.BUY;
                }
                else if (lastPrice < PivotPrice)
                {
                    orderMode = OrderMode.SELL;
                }
            }
            return shouldPlaceOrder;
        }
        
    }
}
