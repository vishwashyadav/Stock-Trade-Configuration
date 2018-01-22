using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Data
{
    public class PreOpenStockPicker
    {
        public static IEnumerable<string> GetStockSymbols(string filePath, IEnumerable<string> stringsToReplace,int changeIndex, int priceIndex,decimal gapUpMin, decimal gapUpMax, decimal stockPriceMin, decimal stockPriceMax)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string allText = File.ReadAllText(filePath);
                    foreach (var stringToReplace in stringsToReplace)
                    {
                        allText = allText.Replace(stringToReplace, "");
                    }

                    string[] lines = allText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    Dictionary<string, Tuple<decimal, decimal>> stocks = new Dictionary<string, Tuple<decimal, decimal>>();
                    foreach (var line in lines)
                    {
                        try
                        {
                            var arrayBySpace = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrayBySpace.Count() >= changeIndex)
                            {
                                stocks[arrayBySpace[0]] = new Tuple<decimal, decimal>(Convert.ToDecimal(arrayBySpace[changeIndex - 1]), Convert.ToDecimal(arrayBySpace[priceIndex]));
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    return stocks.Where(s => (Math.Abs(s.Value.Item1) >= gapUpMin && Math.Abs(s.Value.Item1) <= gapUpMax) && (Math.Abs(s.Value.Item2) >= stockPriceMin && Math.Abs(s.Value.Item2) <= stockPriceMax)).Select(s => s.Key);
                }
            }
            catch (Exception)
            {
                
            }
            return null;
        }

        public static IEnumerable<OpenBreakOutConfig> GetStockSymbolsWithNSEOpenPrice(string filePath, IEnumerable<string> stringsToReplace, int changeIndex, int priceIndex, decimal gapUpMin, decimal gapUpMax, decimal stockPriceMin, decimal stockPriceMax)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string allText = File.ReadAllText(filePath);
                    foreach (var stringToReplace in stringsToReplace)
                    {
                        allText = allText.Replace(stringToReplace, "");
                    }

                    string[] lines = allText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    Dictionary<string, Tuple<decimal, decimal>> stocks = new Dictionary<string, Tuple<decimal, decimal>>();
                    foreach (var line in lines)
                    {
                        try
                        {
                            var arrayBySpace = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrayBySpace.Count() >= changeIndex)
                            {
                                stocks[arrayBySpace[0]] = new Tuple<decimal, decimal>(Convert.ToDecimal(arrayBySpace[changeIndex - 1]), Convert.ToDecimal(arrayBySpace[priceIndex]));
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    return stocks.Where(s => (Math.Abs(s.Value.Item1) >= gapUpMin && Math.Abs(s.Value.Item1) <= gapUpMax) && (Math.Abs(s.Value.Item2) >= stockPriceMin && Math.Abs(s.Value.Item2) <= stockPriceMax)).Select(s => new OpenBreakOutConfig() { Exchange = "NSE", Symbol = s.Key, PreOpenMarketOpenPrice = s.Value.Item2 });
                }
            }
            catch (Exception)
            {

            }
            return null;
        }
    }
}
