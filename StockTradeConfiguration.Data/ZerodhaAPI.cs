using KiteConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Data
{
    public class ZerodhaAPI : StockAPIBase
    {
        public Kite Kite { get; set; }

        public override void UpdateSubscription()
        {
            if(SubscribedStocks!=null && SubscribedStocks.Any())
            {
                var ltpStocks = SubscribedStocks.Where(s => s.Mode.HasFlag(SubscribeMode.LTP));
                var quoteStocks = SubscribedStocks.Where(s => s.Mode.HasFlag(SubscribeMode.Quote));
                var ohlcStocks = SubscribedStocks.Where(s => s.Mode.HasFlag(SubscribeMode.OHLC));

                if (ltpStocks.Any())
                {
                        SubscribedStocksInfo[SubscribeMode.LTP] = ltpStocks.Select(s=> s.ToTradingSymbol( Models.ApiClient.Zerodha)).ToArray();
                }
                if(quoteStocks.Any())
                {
                    SubscribedStocksInfo[SubscribeMode.Quote] = quoteStocks.Select(s => s.ToTradingSymbol(Models.ApiClient.Zerodha)).ToArray();
                }
                if(ohlcStocks.Any())
                {
                    SubscribedStocksInfo[SubscribeMode.OHLC] = ohlcStocks.Select(s => s.ToTradingSymbol(Models.ApiClient.Zerodha)).ToArray();
                }
            }
        }

        public override void WatchLTP()
        {
            throw new NotImplementedException();
        }

        public override void WatchOHLC()
        {
            throw new NotImplementedException();
        }

        public override void WatchQuote()
        {
            throw new NotImplementedException();
        }
    }
}
