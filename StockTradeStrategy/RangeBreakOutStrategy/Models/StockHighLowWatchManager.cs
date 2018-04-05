using StockTradeConfiguration.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.RangeBreakOutStrategy.Models
{
    public class StockHighLowWatchManager
    {
        private ConcurrentDictionary<string, Tuple<decimal, decimal>> _stockHighLowDictionary = new ConcurrentDictionary<string, Tuple<decimal, decimal>>();
        private Dictionary<string, decimal> _stockOpenPriceDictionary = new Dictionary<string, decimal>();

        #region Singleton
        private bool _isWatchHighLowStart;
        private static StockHighLowWatchManager _instance = new StockHighLowWatchManager();
        public static StockHighLowWatchManager Instance
        {
            get { return _instance; }
        }

        private StockHighLowWatchManager()
        {
        }


        #endregion

        #region public methods
        public void WatchStocksForHighLow(Dictionary<string, decimal> _preOpenStocks)
        {
            Events.StockLTPChangedEvent += Events_StockLTPChangedEvent;
            _isWatchHighLowStart = true;
            foreach (var stockSymbol in _preOpenStocks)
            {
                _stockOpenPriceDictionary[stockSymbol.Key] = stockSymbol.Value;
                _stockHighLowDictionary[stockSymbol.Key] = new Tuple<decimal, decimal>(0,9999999m);
                Events.RaiseAskForStockSubscriptionEvent(stockSymbol.Key, StockSubscribeMode.LTP, true);
                Events.RaiseStatusChangedEvent(null, stockSymbol.Key, OrderStatus.HighLowScanning.ToString());
            }
            
        }
        public void Stop()
        {
            _isWatchHighLowStart = false;
            Events.StockLTPChangedEvent -= Events_StockLTPChangedEvent;
        }
        #endregion

        #region private methods
        private void Events_StockLTPChangedEvent(string tradingSymbol, decimal lastPrice)
        {
            
            if(_isWatchHighLowStart && _stockOpenPriceDictionary.ContainsKey(tradingSymbol) && _stockOpenPriceDictionary[tradingSymbol] != lastPrice)
            {
                _stockOpenPriceDictionary[tradingSymbol] = lastPrice;
                var highLow = _stockHighLowDictionary[tradingSymbol];

                var high = Math.Max(highLow.Item1, lastPrice);
                var low = Math.Min(highLow.Item2, lastPrice);
                _stockHighLowDictionary[tradingSymbol] = new Tuple<decimal, decimal>(high, low);
                Events.RaiseStockHighLowChangeEvent(new KeyValuePair<string, Tuple<decimal, decimal>>(tradingSymbol, new Tuple<decimal, decimal>(high, low)));
            }
        }
        #endregion
    }
}
