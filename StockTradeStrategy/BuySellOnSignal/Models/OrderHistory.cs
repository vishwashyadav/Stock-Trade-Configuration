using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.BuySellOnSignal.Models
{
    public struct OrderHistory
    {
        public string BuySellSignal { get; set; }
        public string Stock { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public string SystemTime { get; set; }
        public string StockLTP { get; set; }
        public string SignalGeneratedTime { get; set; }
        public string SignalGeneratedPrice { get; set; }
        public string Reason { get; set; }
    }
}
