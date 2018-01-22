using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StockTradeConfiguration.Models
{
    public class UserInfo
    {
        public string UserId { get; set; }
        public string APIKey { get; set; }
        public string SecretKey { get; set; }
        public string Key { get; set; }
        [XmlIgnore]
        public bool IsValidKey { get; set; }
    }
}
