using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public class TradeType
    {
        public string Delivery="CNC";
        public string IntraDay = "MIS";
    }

    public class TradeMode
    {
        public string Buy = "BUY";
        public string Sell = "SELL";
    }

    public class OrderOptions
    {
        public string Regular = "Regular";
        public string BracketOrder = "BO";
        public string CoverOrder = "CO";
        public string AfterMarketOrder = "AMO";
    }
}
