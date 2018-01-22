using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.BuySellOnSignal.Models
{
    public class ReversalInfo
    {
        public int ReversalNumber { get; set; }
        public int Multiplier { get; set; }
    }

    public class ReversalConfig
    {
        public string Name { get; set; }
        public Guid Key { get; set; }
        public List<ReversalInfo> ReversalInfoes { get; set; }
        public ReversalConfig()
        {
            ReversalInfoes = new List<ReversalInfo>();
        }
    }
}
