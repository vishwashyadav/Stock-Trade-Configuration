using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfigurationCommon
{
   public class ViewAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Icon { get; set; }
    }
}
