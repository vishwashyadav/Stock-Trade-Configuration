using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public class ProfitMarginRange
    {
        public decimal From { get; set; }
        public decimal To { get; set; }
        public decimal ProfitMargin { get; set; }
    }
}
