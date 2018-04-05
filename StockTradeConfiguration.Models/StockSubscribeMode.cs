using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    [Flags]
    public enum StockSubscribeMode
    {
        LTP = 0x01,
        OHLC = 0x02,
        Quote = 0x04
    }
}
