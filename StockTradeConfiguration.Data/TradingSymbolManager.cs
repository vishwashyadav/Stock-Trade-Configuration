using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Data
{
    public class TradingSymbolManager
    {
        private Dictionary<string, List<Instrument>> _instruments = new Dictionary<string, List<Instrument>>();
        private static string[] _exchanges = new string[] { "NSE","BSE","CDS","MCX","NFO"};
        public IEnumerable<string> GetExchanges()
        {
            return _exchanges;
        }

        public List<Instrument> GetInstruments(ApiClient client,string exchange)
        {
            if (!_instruments.ContainsKey(exchange))
            {
                string filePath = "Stocks\\"+client +"\\"+exchange + "_Stocks.config";
                if (File.Exists(filePath))
                {
                    var data = XSerializer.Instance.GetConfiguration<List<Instrument>>(filePath);
                    var stocks = data.Where(s => !s.TradingSymbol.Contains("-")).Distinct().ToList();
                    _instruments[exchange] = stocks.OrderBy(s => s.TradingSymbol).ThenBy(s => s.Expiry).ToList();
                    return _instruments[exchange];
                }
                return null;
            }
            else
            {
                return _instruments[exchange];
            }
        }

        public List<SaveStockInfo> GetSavedStockSymbolInfo(ApiClient client)
        {
            List<SaveStockInfo> infoes = new List<SaveStockInfo>();
            string rootPath = "Stocks\\" + client.ToString();
            if (Directory.Exists(rootPath))
            {
                foreach (var item in _exchanges)
                {
                    SaveStockInfo stockInfo;
                    var filePath = rootPath + "\\" + item + "_Stocks.config";
                    if(File.Exists(filePath))
                    {
                        FileInfo filInfo = new FileInfo(filePath);
                        try
                        {
                            var stocks = XSerializer.Instance.GetConfiguration<List<Instrument>>(filePath);
                            stockInfo = new SaveStockInfo()
                            {
                                Exchange = item,
                                LastUpdatedOn = filInfo.LastWriteTime,
                                TotalItems = stocks.Count
                            };
                        }
                        catch (Exception)
                        {
                            stockInfo = new SaveStockInfo()
                            {
                                Exchange = item
                            };
                        }

                    }
                    else
                    {
                        stockInfo = new SaveStockInfo() { Exchange = item };
                    }
                    infoes.Add(stockInfo);
                }
            }
            else
            {
               infoes =  _exchanges.Select(s => new SaveStockInfo()
                {
                    Exchange = s,
                    LastUpdatedOn  =null,
                    TotalItems=0
                }).ToList();
            }
            return infoes;
        }

    }
}
