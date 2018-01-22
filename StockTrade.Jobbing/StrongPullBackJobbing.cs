using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KiteConnect;
using StockTradeConfiguration.Models;

namespace StockTrade.Jobbing
{
    [JobbingType(Name = "String Pull Back Jobbing", index = 1)]
    public class StrongPullBackJobbing : JobbingStockBase
    {

        public double PullBackPercentage { get; set; }

        private int StocksCountToBuyOrCell()
        {
            if (OpenPositionsCount == 0)
                return StocksBuySellQuantityStart;
            else
            {
                var count = OpenPositionsCount * ((PullBackPercentage / 100.0) * 1);
                return Convert.ToInt32(Math.Round(count));
            }
        }

        #region Override Methods
        public override void BuySellStock(LTP ltp)
        {
            bool shouldPlaceOrder = false;
            OrderMode orderMode = OrderMode.BUY;
            var lastPrice = ltp.LastPrice;

            if (lastPrice <= StockActionPrice[OrderMode.BUY])
            {
                //Place Buy Order
                shouldPlaceOrder = true;
                orderMode = OrderMode.BUY;
            }
            else if (lastPrice >= StockActionPrice[OrderMode.SELL])
            {
                //Place Sell Order
                shouldPlaceOrder = true;
                orderMode = OrderMode.SELL;
            }


            if (shouldPlaceOrder)
            {
                int quantity = (OpenPositionsCount != 0 && LastOrderMode != orderMode) ? StocksCountToBuyOrCell() : StocksBuySellQuantityStart;

                var price = lastPrice.GetNextValidPrice(orderMode == OrderMode.BUY ? false : true);
                this.PlaceOrder(orderMode.ToString(), quantity.ToString(), price.ToString());
                CurrentPrice = lastPrice;
                OpenPositionsCount = (OpenPositionsCount != 0 && LastOrderMode != orderMode) ? (OpenPositionsCount - quantity) : (OpenPositionsCount + quantity);
                SaveData(orderMode, quantity, price);
            }


            LastOrderMode = orderMode;
        }

        public override void ExecutedOrder(Order order)
        {
        }

        #endregion
    }
}
