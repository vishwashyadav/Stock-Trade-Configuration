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
        private OrderMode? nextOrderMode;
        private int _transactionCount = 0;
        public TargetStatus? TargetStatus { get; set; }
        public StockIncrementalMethod IncrementalMethod { get; set; }
        public int IncrementalNumber { get; set; }
        public OrderOnPrice OrderOnPriceType { get; set; }
        public bool OpenForOrder { get; set; }
        public decimal PivotPrice { get; set; }
        private OrderMode? lastOrder { get; set; }

        #region Override Methods
       private int GetQuantity()
        {
            int quantity = (_transactionCount == 0 || IncrementalNumber == 0) ? StocksBuySellQuantityStart :
               (IncrementalMethod == StockIncrementalMethod.Addition ? (_transactionCount + IncrementalNumber) :
               (_transactionCount * IncrementalNumber));
            return quantity;
        }
        public override void BuySellStock(Quote quote)
        {
            OrderMode? orderMode;
            var lastPrice = quote.LastPrice;
            //bool canPlaceOrder = CanPlaceOrder(lastPrice, ref orderMode);
            TargetStatus = null;
            if(nextOrderMode.HasValue && OpenPositions==0)
            {
                PlaceOrder(nextOrderMode.Value.ToString(), GetQuantity(), quote.LastPrice);
                return;
            }

            if (!lastOrder.HasValue || OpenPositions==0)
            {
                if(quote.LastPrice > PivotPrice)
                {
                    PlaceOrder(OrderMode.BUY.ToString(), GetQuantity(), quote.LastPrice);
                }
                else if(quote.LastPrice < PivotPrice)
                {
                    PlaceOrder(OrderMode.SELL.ToString(), GetQuantity(), quote.LastPrice);
                }
            }
            else if (lastOrder.HasValue && OpenPositions != 0)
            {
                //Target met Condition
                if (lastOrder.Value == OrderMode.BUY && quote.Bids.Any(s => s.Price >= TargetPrice))
                {
                    TargetStatus = StockTradeConfiguration.Models.TargetStatus.TargetHit;
                    PlaceOrder(OrderMode.SELL.ToString(), Convert.ToInt32( Math.Abs(OpenPositions)), quote.LastPrice);
                }
                //Target met condition
                else if (lastOrder.Value == OrderMode.SELL && quote.Offers.Any(s => s.Price <= TargetPrice))
                {
                    TargetStatus = StockTradeConfiguration.Models.TargetStatus.TargetHit;
                    PlaceOrder(OrderMode.BUY.ToString(), Convert.ToInt32(Math.Abs(OpenPositions)), quote.LastPrice);
                }
                //Stop Loss condition
                else if (lastOrder.Value == OrderMode.BUY && quote.LastPrice < StopLossPrice)
                {
                    TargetStatus = StockTradeConfiguration.Models.TargetStatus.StopLossHit;
                    PlaceOrder(OrderMode.SELL.ToString(), Convert.ToInt32(Math.Abs(OpenPositions)), quote.LastPrice);
                }
                //Stop Loss condition
                else if(lastOrder.Value == OrderMode.SELL && quote.LastPrice > StopLossPrice)
                {
                    TargetStatus = StockTradeConfiguration.Models.TargetStatus.StopLossHit;
                    PlaceOrder(OrderMode.BUY.ToString(), Convert.ToInt32(Math.Abs(OpenPositions)), quote.LastPrice);
                }
            }

          
        }
        public override void ExecutedOrder(Order order)
        {
            nextOrderMode = null;
            if (order.Status.Equals("complete", StringComparison.InvariantCultureIgnoreCase))
            {
                OrderMode mode = order.TransactionType.Equals("buy", StringComparison.InvariantCultureIgnoreCase) ? OrderMode.BUY : OrderMode.SELL;
                if(mode == OrderMode.BUY)
                {
                    OpenPositions = OpenPositions + order.Quantity;
                }
                else
                {
                    OpenPositions = OpenPositions - order.Quantity;
                }

                if (TargetStatus == null)
                {
                    var buyTargetPriceMargin = (MarginType == MarginType.Absolute) ?
                          (Margin).GetNextValidPrice(true) :
                          (((order.AveragePrice * Margin) / 100.0m)).GetNextValidPrice(true);

                    var sellTargetPriceMargin = (MarginType == MarginType.Absolute) ?
                                            (Margin).GetNextValidPrice(false) :
                                            (((order.AveragePrice * Margin) / 100.0m)).GetNextValidPrice(false);

                    if (mode == OrderMode.BUY)
                    {
                        TargetPrice = order.AveragePrice +  buyTargetPriceMargin;
                        StopLossPrice = order.AveragePrice - (sellTargetPriceMargin*2);
                        lastOrder = OrderMode.BUY;
                    }
                    else if (mode == OrderMode.SELL)
                    {
                        TargetPrice = order.AveragePrice - sellTargetPriceMargin;
                        lastOrder = OrderMode.SELL;
                        StopLossPrice = order.AveragePrice + (buyTargetPriceMargin*2);
                    }
                    if (OpenPositions != 0)
                    {
                        _transactionCount++;
                    }
                }
                else
                {
                    if(TargetStatus == StockTradeConfiguration.Models.TargetStatus.TargetHit)
                    {
                        TargetStatus = null;
                        TargetHitCount++;
                        if (mode == OrderMode.BUY)
                        {
                            PlaceOrder(OrderMode.SELL.ToString(), GetQuantity(), order.AveragePrice, OrderOnPrice.MarketPrice);
                        }
                        else
                        {
                            PlaceOrder(OrderMode.BUY.ToString(), GetQuantity(), order.AveragePrice, OrderOnPrice.MarketPrice);
                        }
                    }
                    else
                    {
                        TargetStatus = null;
                        StopLossHitCount--;
                        PlaceOrder(mode.ToString(), GetQuantity(), order.AveragePrice, OrderOnPrice.MarketPrice);
                    }
                }
            }
            RaiseTargetStopLossEvent();
            RaiseTargetStopLossHitEvent();
            Events.RaiseOpenPositionsChangedEvent(Exchange, Symbol, Convert.ToInt32(OpenPositions));
        }
        #endregion
        

        private void RaiseTargetStopLossEvent()
        {
            Events.RaiseTargetStopLossChangeEvent(Exchange, Symbol, TargetPrice, StopLossPrice);
        }

        private void RaiseTargetStopLossHitEvent()
        {
            Events.RaiseTargetStopLossHitEvent(Exchange, Symbol, TargetHitCount, StopLossHitCount);
        }
    }
}
