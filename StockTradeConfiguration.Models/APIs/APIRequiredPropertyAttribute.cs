using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models.APIs
{
    public class APIRequiredPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}
