
using KiteConnect;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public delegate void StockHighLowChangeEventHandler(KeyValuePair<string, Tuple<decimal, decimal>> stockInfo);
    public delegate void TimeZoneChangedEventHandler(StockTimeZone timeZone);
    public delegate void FileContentChangedEventHandler(string filePath);
    public delegate void OpenHighStockListChangedEventHandler(List<OpenHighLowData> stockList);
    public delegate void StatusChangedEventHandler(string exchange, string symbol, string status);
    public delegate void OpenPositionsChangedEventHandler(string exchange, string symbol, int openPositions);
    public delegate void PositionUpdateEventHandler(string exchange, string symbol, Position position);
    public static class Events
    {
        public static event StockHighLowChangeEventHandler StockHighLowChangeEvent;
        public static event TimeZoneChangedEventHandler TimeZoneChangedEvent;
        public static event FileContentChangedEventHandler FileContentChangedEvent;
        public static event OpenHighStockListChangedEventHandler OpenHighStockListChangedEvent;
        public static event StatusChangedEventHandler StatusChangedEvent;
        public static event OpenPositionsChangedEventHandler OpenPositionsChangedEvent;
        public static event PositionUpdateEventHandler PositionUpdateEvent;
        public static void RaiseStatusChangedEvent(string exchange, string symbol, string status)
        {
            if (StatusChangedEvent != null)
                StatusChangedEvent(exchange, symbol, status);
        }

        public static void RaisePositionUpdateEventEvent(string exchange, string symbol, Position position)
        {
            if (PositionUpdateEvent != null)
                PositionUpdateEvent(exchange, symbol, position);
        }

        public static void RaiseOpenPositionsChangedEvent(string exchange, string symbol, int openPositions)
        {
            if (OpenPositionsChangedEvent != null)
                OpenPositionsChangedEvent(exchange, symbol, openPositions);
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
