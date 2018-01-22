using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrade.Jobbing
{
    public class JobbingTypeAttribute : Attribute
    {
        public string Name { get; set; }
        public int index { get; set; }
    }
}
