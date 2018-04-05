using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.RangeBreakOutStrategy.Models
{
    public enum OrderStatus
    {
        None,
        HighLowScanning,
        CheckOrderStarted,
        Ordered,
        BuyOrderInProgress,
        SellOrderInProgress,
        TargetHit,
        StopLossHit
    }
}
