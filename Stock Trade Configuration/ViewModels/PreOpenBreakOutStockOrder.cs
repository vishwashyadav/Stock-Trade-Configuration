using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.ViewModels
{
    public class PreOpenBreakOutStockOrder
    {
        public List<OpenBreakOutConfig> Stocks { get; set; }
        private static PreOpenBreakOutStockOrder _instance = new PreOpenBreakOutStockOrder();
        private bool _isCancelled = true;
        public static PreOpenBreakOutStockOrder Instance
        {
            get { return _instance; }
        }


        private PreOpenBreakOutStockOrder()
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

        private void OrderStockWhenItBreaksRange()
        {
            var symbols = Stocks.Select(s => string.Format("{0}:{1}", s.Exchange, s.Symbol));
            var ohlcs = _kite.GetOHLC(symbols.ToArray());

        }

        public void Stop()
        {
            _isCancelled = true;
        }
    }
}
