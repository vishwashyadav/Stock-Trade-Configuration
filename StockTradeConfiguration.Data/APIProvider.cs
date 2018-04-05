using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Data
{
    
    [Flags]
    public enum SubscribeMode
    {
        LTP= 0x01,
        OHLC= 0x02,
        Quote= 0x04
    }

    public class SubscribedStockInfo
    {
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public SubscribeMode Mode { get; set; }

        public string ToTradingSymbol(ApiClient apiClient)
        {
            switch(apiClient)
            {
                case ApiClient.Upstox:
                    return null;
                case ApiClient.Zerodha:
                    return string.Format("{0}:{1}", Exchange, Symbol);
            }
            return null;
        }
    }

    public struct StockLTP
    {
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public decimal LastPrice { get; set; }

        StockLTP ToStockLTP(string exchange, string symbol, decimal lastPrice)
        {
            return new StockLTP() { Exchange = exchange, Symbol = symbol, LastPrice = lastPrice };
        }
    }
}
