using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrade.Jobbing
{
    public class JobbingDataSaver
    {
        public void SaveData(JobbingStockBase stockBase, OrderMode orderMode, int quantity, decimal price,DateTime dtTime)
        {
            if (!Directory.Exists(stockBase.SaveDirectoryName))
                Directory.CreateDirectory(stockBase.SaveDirectoryName);

            var dateDir = string.Format("{0}_{1}_{2}", dtTime.Year, dtTime.Month, dtTime.Day);
            string dirPath = stockBase.SaveDirectoryName + "\\" + dateDir;
            if(!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            string fileName = string.Format("{0}_{1}", stockBase.Exchange, stockBase.Symbol);
            string dataToSave = string.Format("{0}${1}${2}${3}\r\n", orderMode, quantity, price, dtTime.ToString());

            string filePath = dirPath + "\\" + fileName;
            if (File.Exists(filePath))
            {
                using (var writer = File.AppendText(filePath))
                {
                    writer.Write(dataToSave);
                }
            }
            else
            {
                File.WriteAllText(filePath, dataToSave);
            }
            //string format = "{0}:{1}$
        }
        
        public static List<JobbingStockDataInfo> GetData(string exchange, string symbol, DateTime dateTime, string parentDirectoryName)
        {
            List<JobbingStockDataInfo> info = new List<JobbingStockDataInfo>();
            if (Directory.Exists(parentDirectoryName))
            {
                var dateDir = string.Format("{0}_{1}_{2}", dateTime.Year, dateTime.Month, dateTime.Day);
                string dirPath = parentDirectoryName + "\\" + dateDir;
                if (Directory.Exists(dirPath))
                {
                    string fileName = string.Format("{0}_{1}", exchange, symbol);

                    string filePath = dirPath + "\\" + fileName;
                    if (File.Exists(filePath))
                    {
                        var lines = File.ReadAllLines(filePath);
                        foreach (var item in lines)
                        {
                            var splitData = item.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splitData.Length == 4)
                            {
                                var orderMode = splitData[0];
                                var quantity = splitData[2];
                                var price = splitData[2];
                                var date = splitData[3];
                                info.Add(new JobbingStockDataInfo()
                                {
                                    TransactionType = orderMode,
                                    Price = price,
                                    Quantity = quantity,
                                    TransactionTime = date,
                                });
                            }
                        }
                    }
                }
            }
            return info;
        }
    }
}
