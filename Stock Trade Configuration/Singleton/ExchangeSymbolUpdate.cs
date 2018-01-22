using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.Singleton
{
    public class ExchangeSymbolUpdate
    {
        public int UpdateSymbol(ApiClient apiClient, KiteConnect.Kite kite, string exchange)
        {
            string directory = "Stocks";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string clientDirectory = directory + "\\" + apiClient;

            if (!Directory.Exists(clientDirectory))
                Directory.CreateDirectory(clientDirectory);

            string filePath = clientDirectory + "\\" + exchange + "_Stocks.config";
            var instruments = kite.GetInstruments(exchange);
            XSerializer.Instance.SaveConfiguration<List<KiteConnect.Instrument>>(filePath, instruments);
            return instruments.Count;

        }
    }
}
