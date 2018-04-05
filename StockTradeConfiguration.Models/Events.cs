
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
    public delegate void TimeZoneChangedEventHandler(string timeZone, TimeZoneStatus status);
    public delegate void FileContentChangedEventHandler(string filePath);
    public delegate void OpenHighStockListChangedEventHandler(List<OpenHighLowData> stockList);
    public delegate void StatusChangedEventHandler(string exchange, string symbol, string status);
    public delegate void OpenPositionsChangedEventHandler(string exchange, string symbol, int openPositions);
    public delegate void PositionUpdateEventHandler(string tradingSymbol, Position position);
    public delegate void StockLastPriceChangeEventHandler(string exchange, string symbol, decimal lastPrice);
    public delegate void TargetStopLossChangeEventHandler(string exchange, string symbol, decimal targetPrice, decimal stopLossPrice);
    public delegate void TargetStopLossHitEventHandler(string exchange, string symbol, int targetHitCount, int stopLossHitCount);
    public delegate void AskForStockSubscriptionHandler(string tradingSymbol, StockSubscribeMode mode, bool isSubscribe);
    public delegate void AskForOrderSubscriptionHandler(string orderId, bool isSubscribe);
    public delegate void StockLTPChangedEventHandler(string tradingSymbol, decimal lastPrice);
    public delegate void OrderHistoryFetchedEventHandler(string tradingSymbol, IEnumerable<Order> orderHistory);
    public delegate void SubscribeTimeZoneHandler(string timeZoneName, TimeSpan startTime, TimeSpan endTime);
    public delegate void InactiveStockPNLChangedHandler(decimal pnl);
    public delegate void DayProfitLossChangeEventHandler(decimal pnl);
    public delegate void UpdateObjectEventHandler(object obj);
    public delegate void AskForKiteInstanceHandler();
    public delegate void GiveKiteInstanceHandler(Kite kite);
    public static class Events
    {
        public static event UpdateObjectEventHandler UpdateObjectEvent;
        public static event AskForKiteInstanceHandler AskForKiteInstance;
        public static event GiveKiteInstanceHandler GiveKiteInstanceEvent;
        public static event StockHighLowChangeEventHandler StockHighLowChangeEvent;
        public static event TimeZoneChangedEventHandler TimeZoneChangedEvent;
        public static event FileContentChangedEventHandler FileContentChangedEvent;
        public static event OpenHighStockListChangedEventHandler OpenHighStockListChangedEvent;
        public static event StatusChangedEventHandler StatusChangedEvent;
        public static event OpenPositionsChangedEventHandler OpenPositionsChangedEvent;
        public static event PositionUpdateEventHandler PositionUpdateEvent;
        public static event StockLastPriceChangeEventHandler StockLastPriceChangeEvent;
        public static event TargetStopLossChangeEventHandler TargetStopLossChangeEvent;
        public static event TargetStopLossHitEventHandler TargetStopLossHitEvent;
        public static event AskForStockSubscriptionHandler AskForStockSubscriptionEvent;
        public static event StockLTPChangedEventHandler StockLTPChangedEvent;
        public static event AskForOrderSubscriptionHandler AskForOrderSubscriptionEvent;
        public static event OrderHistoryFetchedEventHandler OrderHistoryFetchedEvent;
        public static event SubscribeTimeZoneHandler SubscribeTimeZoneEvent;
        public static event InactiveStockPNLChangedHandler InactiveStockPNLChanged;
        public static event DayProfitLossChangeEventHandler DayProfitLossChanged;
        
        public static void RaiseUpdateObject(object obj)
        {
            if (UpdateObjectEvent != null)
                UpdateObjectEvent(obj);
        }
        public static void RaiseGiveKiteInstanceEvent(Kite kite)
        {
            if (GiveKiteInstanceEvent != null)
                GiveKiteInstanceEvent(kite);
        }
        public static void RaiseAskForKiteInstanceEvent()
        {
            if(AskForKiteInstance!=null)
            {
                AskForKiteInstance();
            }
        }
        public static void RaiseDayProfitLossChanged(decimal pnl)
        {
            if(DayProfitLossChanged!=null)
            {
                DayProfitLossChanged(pnl);
            }
        }
        public static void RaiseInactiveStockPNLChangedEvent(decimal pnl)
        {
            if(InactiveStockPNLChanged!=null)
            {
                InactiveStockPNLChanged(pnl);
            }
        }
        public static void RaiseSubscribeTimeZoneEvent(string timeZoneName, TimeSpan startTime, TimeSpan endTime)
        {
            if (SubscribeTimeZoneEvent != null)
                SubscribeTimeZoneEvent(timeZoneName, startTime, endTime);
        }
        public static void RaiseOrderHistoryFetchedEvent(string tradingSymbol, IEnumerable<Order> orderHistory)
        {
            if (OrderHistoryFetchedEvent != null)
                OrderHistoryFetchedEvent(tradingSymbol, orderHistory);
        }
        public static void RaiseAskForOrderSubscriptionEvent(string orderId, bool isSubscribe)
        {
            if (AskForOrderSubscriptionEvent != null)
                AskForOrderSubscriptionEvent(orderId, isSubscribe);
        }
        public static void RaiseStockLTPChangedEvent(string tradingSymbol, decimal lastPrice)
        {
            if (StockLTPChangedEvent != null)
                StockLTPChangedEvent(tradingSymbol, lastPrice);
        }
        public static void RaiseAskForStockSubscriptionEvent(string tradingSymbol, StockSubscribeMode mode, bool isSubscribe)
        {
            if (AskForStockSubscriptionEvent != null)
                AskForStockSubscriptionEvent(tradingSymbol, mode, isSubscribe);
        }
        public static void RaiseStatusChangedEvent(string exchange, string symbol, string status)
        {
            if (StatusChangedEvent != null)
                StatusChangedEvent(exchange, symbol, status);
        }

        public static void RaiseTargetStopLossHitEvent(string exchange, string symbol, int targetHitCount, int stopLossHitCount)
        {
            if (TargetStopLossHitEvent != null)
                TargetStopLossHitEvent(exchange, symbol, targetHitCount, stopLossHitCount);
        }

        public static void RaiseTargetStopLossChangeEvent(string exchange, string symbol, decimal targetPrice, decimal stopLossPrice)
        {
            if (TargetStopLossChangeEvent != null)
                TargetStopLossChangeEvent(exchange, symbol, targetPrice,stopLossPrice);
        }

        public static void RaiseStockLastPriceChangeEvent(string exchange, string symbol, decimal lastPrice)
        {
            if (StockLastPriceChangeEvent != null)
                StockLastPriceChangeEvent(exchange, symbol, lastPrice);
        }

        public static void RaisePositionUpdateEventEvent(string tradingSymbol, Position position)
        {
            if (PositionUpdateEvent != null)
                PositionUpdateEvent(tradingSymbol, position);
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


        public static void RaiseTimeZoneChangedEvent(string timeZone, TimeZoneStatus status)
        {
            if (TimeZoneChangedEvent != null)
            {
                TimeZoneChangedEvent(timeZone,status);
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
