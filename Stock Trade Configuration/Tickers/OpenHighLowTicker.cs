using KiteConnect;
using Stock_Trade_Configuration.ViewModels;

using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.Tickers
{
    public class OpenHighLowTicker
    {
        private static OpenHighLowTicker _instance = new OpenHighLowTicker();
        private bool _isCancelled=true;
        public static OpenHighLowTicker Instance
        {
            get { return _instance; }
        }

        private OpenHighLowTicker()
        {
        }

        KiteConnect.Kite _kite;
        List<string> instruments = new List<string>();
        List<List<string>> symbolsInChunlk = new List<List<string>>();
        public async void Start(TimeSpan interval, KiteConnect.Kite kite, IEnumerable<string> instruments)
        {
            _kite = kite;
            this.instruments = instruments.ToList();
            symbolsInChunlk = GetStockSymbolsInChunk(this.instruments, 200);
            if (_isCancelled)
            {
                _isCancelled = false;
                await CheckHighLowTick();
            }
        }

        public void Stop()
        {
            _isCancelled = true;
        }

        private List<List<string>> GetStockSymbolsInChunk(IEnumerable<string> symbols, int chunkSize)
        {
            List<List<string>> stockSymbolsList = new List<List<string>>();
            for (int i = 0; i < (symbols.Count() / chunkSize) + 1; i++)
            {
                var tempsymbols = symbols.Skip((i * chunkSize)).Take(chunkSize);
                stockSymbolsList.Add(new List<string>(tempsymbols.Select(s => s)));
            }
            return stockSymbolsList;
        }

        private async Task CheckHighLowTick()
        {
            await Task.Run(() =>
            {
                CheckHighLowTickFun();
                while (true)
                {   
                    Task.Delay(1000);
                    if (_isCancelled)
                        break;
                }
            });
        }

        private void CheckHighLowTickFun()
        {
            var openHighLowStock = new List<OpenHighLowData>();
            int count = 0;
            List<string> validStocks = new List<string>();
            List<string> inValidStock = new List<string>();
            foreach (var symbols in symbolsInChunlk)
            {
                try
                {
                        try
                        {
                        var openHighLowData = _kite.GetOHLC(symbols.ToArray());
                        var onlyOpenHighLow = openHighLowData.Where(s => s.Value.Open == s.Value.High || s.Value.Open == s.Value.Low).
                            Select(s => new OpenHighLowData()
                            {
                                Exchange = "NSE",
                                Open = s.Value.Open,
                                Low = s.Value.Low,
                                High = s.Value.High,
                                Symbol = s.Key,
                                Status = s.Value.Open == s.Value.High ? Status.OpenHigh : Status.OpenLow
                            });
                        openHighLowStock.AddRange(onlyOpenHighLow);
                        }
                        catch (Exception)
                        {
                            
                        }
                }
                catch (Exception)
                {
                    
                }
            }
            Events.RaiseOpenHighStockListChangedEvent(openHighLowStock);

        }
    }
}
