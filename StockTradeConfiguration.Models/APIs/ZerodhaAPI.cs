using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models.APIs
{
    [API(ApiClient = ApiClient.Zerodha)]
    public class ZerodhaAPI
    {
        [APIRequiredProperty]
        public string UserId { get; set; }

        [APIRequiredProperty]
        public string APIKey { get; set; }

        [APIRequiredProperty]
        public string SecretKey { get; set; }
    }
}
