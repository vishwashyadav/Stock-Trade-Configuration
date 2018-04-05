using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockTradeConfiguration.Models;
using KiteConnect;

namespace StockTradeConfiguration.Data
{
    public abstract class StockAPIBase
    {
        #region Public Properties
        public Dictionary<SubscribeMode, string[]> SubscribedStocksInfo { get; set; }
        public StockLastPriceChangeEventHandler LastPriceChanged { get; set; }

        public List<SubscribedStockInfo> SubscribedStocks { get; set; }

        #endregion

        #region abstract methods
        public abstract void WatchLTP();
        public abstract void WatchOHLC();
        public abstract void WatchQuote();
        public abstract void UpdateSubscription();
        #endregion

        #region Constructor
        public StockAPIBase()
        {
            SubscribedStocksInfo = new Dictionary<SubscribeMode, string[]>();
        }
        #endregion

        #region Methods
        public void PlaceOrder(string exchange, string symbol, int quantity, decimal price, decimal targetPoint, decimal stopLossPoint, OrderMode orderMode)
        {

        }

        public void SubscribeStock(string exchange, string symbol, SubscribeMode mode)
        {
            if (SubscribedStocks == null)
                SubscribedStocks = new List<SubscribedStockInfo>();

            if (!SubscribedStocks.Any(s => s.Exchange == exchange && s.Symbol == symbol))
            {
                SubscribedStocks.Add(new SubscribedStockInfo()
                {
                    Exchange = exchange,
                    Symbol = symbol,
                    Mode = mode
                });
            }
        }

        public async void StartLTP()
        {
            await Task.Factory.StartNew(() =>
           {
               WatchLTPAsync();
           });
        }

        public void WatchLTPAsync()
        {
            while(true)
            {
                WatchLTP();
                Task.Delay(1000);
            }
        }

        public async void StartOHLC()
        {
            await Task.Factory.StartNew(() =>
            {
                WatchOHLCAsync();
            });
        }

        public void WatchOHLCAsync()
        {
            while (true)
            {
                WatchOHLC();
                Task.Delay(1000);
            }
        }

        public async void StartQuote()
        {
            await Task.Factory.StartNew(() =>
            {
                WatchQuoteAsync();
            });
        }

        public void WatchQuoteAsync()
        {
            while (true)
            {
                WatchQuote();
                Task.Delay(1000);
            }
        }
        #endregion

    }
}
