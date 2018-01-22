using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public class OpenBreakOutConfig
    {
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public decimal PreOpenMarketOpenPrice { get; set; }

    }
}
