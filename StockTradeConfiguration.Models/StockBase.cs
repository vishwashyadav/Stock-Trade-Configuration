using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public class StockBase : NotifyPropertyChanged
    {
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public string TradingSymbol { get; set; }
        public decimal Price { get; set; }
    }
}
