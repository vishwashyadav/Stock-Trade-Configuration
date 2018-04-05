using Microsoft.Win32;
using Stock_Trade_Configuration.ViewModels;

using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Stock_Trade_Configuration
{
    public class StockBreakOutTicker
    {
      
        private static StockBreakOutTicker _instance = new StockBreakOutTicker();
        private bool _isCancelled;
        public static StockBreakOutTicker Instance
        {
            get { return _instance; }
        }

        private StockBreakOutTicker()
        {
        }

     
        DispatcherTimer _timer;
        KiteConnect.Kite _kite;
        string[] _stockSymbols;
        Dictionary<string, Tuple<decimal, decimal>> _stockSybolsHighLow;

        public async void Start(TimeSpan interval, KiteConnect.Kite kite,IEnumerable<StockSymbols> stockSymbols)
        {
            _stockSybolsHighLow = new Dictionary<string, Tuple<decimal, decimal>>();

            _kite = kite;
            _stockSymbols = stockSymbols.Distinct().Select(s => s.DisplayName).ToArray();
            _stockSybolsHighLow = _stockSymbols.ToDictionary(key => key, value => new Tuple<decimal, decimal>(0, 0));
            if (!_isCancelled)
            {
                _isCancelled = true;
                await CheckHighLowTick();
            }
        }


        public async Task CheckHighLowTick()
        {

            await Task.Run(() =>
            {
                while (true)
                {
                    CheckHighLow();

                    Task.Delay(1000);
                    if (!_isCancelled)
                        break;
                }
            });

        }

        private Dictionary<string, decimal> _breakOutPrice = new Dictionary<string, decimal>();
        public void Stop()
        {
            _isCancelled = false;
        }

        private void CheckHighLow()
        {
            try
            {
                var stockLastPrice = _kite.GetLTP(_stockSymbols);
                foreach (var stockLTP in stockLastPrice)
                {
                    if (_stockSybolsHighLow.ContainsKey(stockLTP.Key))
                    {
                        if(!_breakOutPrice.ContainsKey(stockLTP.Key))
                        {
                            _breakOutPrice[stockLTP.Key] = stockLTP.Value.LastPrice;
                            continue;
                        }

                        //if (_breakOutPrice[stockLTP.Key] == stockLTP.Value.LastPrice)
                          //  continue;

                        _breakOutPrice[stockLTP.Key] = stockLTP.Value.LastPrice;
                        var highLow = _stockSybolsHighLow[stockLTP.Key];
                        decimal min = highLow.Item1 == 0 ? stockLTP.Value.LastPrice : Math.Min(highLow.Item1, stockLTP.Value.LastPrice);
                        decimal max = highLow.Item2 == 0 ? stockLTP.Value.LastPrice : Math.Max(highLow.Item2, stockLTP.Value.LastPrice);
                        _stockSybolsHighLow[stockLTP.Key] = new Tuple<decimal, decimal>(min, max);
                        Events.RaiseStockHighLowChangeEvent(new KeyValuePair<string, Tuple<decimal, decimal>>(stockLTP.Key, new Tuple<decimal, decimal>(min, max)));
                    }
                }
            }
            catch (Exception ex)
            {
                // Stop();
            }
        }
    }
}
