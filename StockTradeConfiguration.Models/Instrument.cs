using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    /// <summary>
    /// Instrument structure
    /// </summary>
    public class Instrument
    {
        public string TradingSymbol { get; set; }
        public string Name { get; set; }
        public string Expiry { get; set; }
        public string Exchange { get; set; }
        public string Segment { get; set; }
        public int LotSize { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Expiry))
                return TradingSymbol;
            else
                return string.Format("{0}({1})", TradingSymbol, Expiry);
        }
    }
}
