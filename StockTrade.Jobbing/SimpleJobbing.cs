using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KiteConnect;
using StockTradeConfiguration.Models;

namespace StockTrade.Jobbing
{
    [JobbingType(Name ="Simple Jobbing", index =1)]
    public class SimpleJobbing : JobbingStockBase
    {
        #region Override methods
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
            else if(lastPrice>= StockActionPrice[OrderMode.SELL])
            {
                //Place Sell Order
                shouldPlaceOrder = true;
                orderMode = OrderMode.SELL;
            }

            if (shouldPlaceOrder)
            {
                var price = lastPrice.GetNextValidPrice(orderMode == OrderMode.BUY ? false : true);
                 this.PlaceOrder(orderMode.ToString(), StocksBuySellQuantityStart.ToString(), price.ToString());
                CurrentPrice = lastPrice;
                SaveData(orderMode, StocksBuySellQuantityStart, price);
            }
        }

        public override void ExecutedOrder(Order order)
        {
            
        }

        #endregion
    }
}
