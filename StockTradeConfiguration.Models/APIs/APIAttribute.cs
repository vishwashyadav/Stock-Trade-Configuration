using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models.APIs
{
    public class APIAttribute : Attribute
    {
        public ApiClient ApiClient { get; set; }
        public string Description { get; set; }
    }
}
