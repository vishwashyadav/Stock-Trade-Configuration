using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public enum StrategyStockStatus
    {
        Added,
        Running,
        MaxLossReached,
        MaxProfitReached,
        ManuallySquaredOff
    }
}
