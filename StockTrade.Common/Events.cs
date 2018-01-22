using StockTrade.Jobbing;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrade.Common
{
    public delegate void StockHighLowChangeEventHandler(KeyValuePair<string, Tuple<decimal, decimal>> stockInfo);
    public delegate void TimeZoneChangedEventHandler(StockTimeZone timeZone);
    public delegate void FileContentChangedEventHandler(string filePath);
    public delegate void OpenHighStockListChangedEventHandler(List<OpenHighLowData> stockList);
    public delegate void JobbingStatusChangedEventHandler(string exchange, string symbol, JobbingStatus status);
    public static class Events
    {
        public static event StockHighLowChangeEventHandler StockHighLowChangeEvent;
        public static event TimeZoneChangedEventHandler TimeZoneChangedEvent;
        public static event FileContentChangedEventHandler FileContentChangedEvent;
        public static event OpenHighStockListChangedEventHandler OpenHighStockListChangedEvent;
        public static event JobbingStatusChangedEventHandler JobbingStatusChangedEvent;

        public static void RaiseJobbingStatusChangedEvent(string exchange, string symbol, string status)
        {
            if (JobbingStatusChangedEvent != null)
                JobbingStatusChangedEvent(exchange, symbol, status);
        }

        public static void RaiseStockHighLowChangeEvent(KeyValuePair<string, Tuple<decimal, decimal>> stockInfo)
        {
            if (StockHighLowChangeEvent != null)
                StockHighLowChangeEvent(stockInfo);
        }


        public static void RaiseTimeZoneChangedEvent(StockTimeZone timeZone)
        {
            if (TimeZoneChangedEvent != null)
            {
                TimeZoneChangedEvent(timeZone);
            }
        }

        public static void RaiseOpenHighStockListChangedEvent(List<OpenHighLowData> stockList)
        {
            if (OpenHighStockListChangedEvent != null)
            {
                OpenHighStockListChangedEvent(stockList);
            }
        }
        public static void RaiseFileContentChangedEvent(string filePath)
        {
            if (FileContentChangedEvent != null)
            {
                FileContentChangedEvent(filePath);
            }
        }
    }
}
