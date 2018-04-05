using KiteConnect;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.Singleton
{
    public class KiteInstance
    {
        private volatile bool _isLTPTickStart;
        private volatile bool _isOrderTickStart;
        private Dictionary<string, decimal> _cachedStockLtp = new Dictionary<string, decimal>();
        private ConcurrentBag<string> _configuredStocksForLTP = new ConcurrentBag<string>();
        private ConcurrentBag<string> _configuredOrderIdForOrderStatus = new ConcurrentBag<string>();
        // Initialize key and secret of your app
        string MyAPIKey = string.Empty;
        string MySecret = string.Empty;
        string MyUserId = string.Empty;
        public string RequestToken { get; set; }
        // persist these data in settings or db or file
        string MyPublicToken = "e7ost8r9p8l1c330";
        string MyAccessToken = "e7ost8r9p8l1c330";

        private static KiteInstance _instance = new KiteInstance();
        public KiteConnect.Kite Kite { get; set; }
        public static KiteInstance Instance
        {
            get { return _instance; }
        }

        private KiteInstance()
        {
            Events.AskForOrderSubscriptionEvent += Events_AskForOrderSubscriptionEvent;
            Events.AskForStockSubscriptionEvent += Events_AskForStockSubscriptionEvent;
            Events.AskForKiteInstance += Events_AskForKiteInstance;
        }

        private void Events_AskForKiteInstance()
        {
            Events.RaiseGiveKiteInstanceEvent(Kite);
        }

        public void SetInfo(UserInfo userInfo)
        {
            MyAPIKey = userInfo.APIKey;

            MySecret = userInfo.SecretKey;
            MyUserId = userInfo.UserId;
        }

        public Kite GetKite(UserInfo userInfo)
        {
            var kite = new Kite(userInfo.APIKey, Debug: false);
            kite.SetSessionExpiryHook(onTokenExpire);
            Kite.SetAccessToken(MyAccessToken);
            return kite;
        }

        public bool Login(string requestToken)
        {
            try
            {
                RequestToken = requestToken;
                User user = Kite.GenerateSession(requestToken, MySecret);
                MyPublicToken = user.PublicToken;
                MyAccessToken = user.AccessToken;
                Kite.SetAccessToken(MyAccessToken);
                //Kite.GetLTP(new string[] { "NSE:MEGH", "NSE:GTLINFRA" });
                //for (int i = 0; i < 2; i++)
                //{
                //    Kite k = new Kite(MyAPIKey, Debug:true);
                //    RequestToken = requestToken;
                //  //  User user1 = k.GenerateSession(requestToken, MySecret);
                //    k.SetAccessToken(MyAccessToken);
                //    k.GetLTP(new string[] { "NSE:SUBEX","NSE:SBI" });
                //}
                //StartPositionAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string GetLoginURL()
        {
            Kite = new Kite(MyAPIKey, Debug: true);
            Kite.SetSessionExpiryHook(onTokenExpire);
            var loginURL = Kite.GetLoginURL();
            return loginURL;
        }

        private void onTokenExpire()
        {
            
        }

        #region Event Handler
        private void Events_AskForStockSubscriptionEvent(string tradingSymbol, StockSubscribeMode mode, bool isSubscribe)
        {
            switch(mode)
            {
                case StockSubscribeMode.LTP:
                    if(isSubscribe)
                    {
                        if (!_configuredStocksForLTP.Any(s => s.Equals(tradingSymbol)))
                            _configuredStocksForLTP.Add(tradingSymbol);
                    }
                    else
                    {
                        var stock = _configuredStocksForLTP.FirstOrDefault(s => s.Equals(tradingSymbol));
                        if (stock != null)
                        {
                            _configuredStocksForLTP = new ConcurrentBag<string>(_configuredStocksForLTP.Except(new[] { tradingSymbol}));
                            if (_cachedStockLtp.ContainsKey(tradingSymbol))
                                _cachedStockLtp.Remove(tradingSymbol);
                        }
                    }
                    break;
            }

            StartStopLTP(_configuredStocksForLTP.Any());
        }

        private void Events_AskForOrderSubscriptionEvent(string orderId, bool isSubscribe)
        {
            if (isSubscribe)
            {
                if (!_configuredOrderIdForOrderStatus.Any(s => s.Equals(orderId)))
                    _configuredOrderIdForOrderStatus.Add(orderId);
            }
            else
            {
                var order = _configuredOrderIdForOrderStatus.FirstOrDefault(s => s.Equals(orderId));
                if (order != null)
                    _configuredOrderIdForOrderStatus.TryTake(out order);
            }
            StartStopOrderStatus(_configuredOrderIdForOrderStatus.Any());
        }

        #endregion

        #region LTP Ticker
        public void StartStopLTP(bool start)
        {
            if(start)
            {
                if (!_isLTPTickStart)
                {
                    _isLTPTickStart = true;
                    WatchLTPAsync();
                }
            }
            else
            {
                _isLTPTickStart = false;
            }
        }

        private async void WatchLTPAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                WatchWithTimer();
            });
        }

        private async void WatchWithTimer()
        {
            while (true)
            {
                WatchLTP();
                if (!_isLTPTickStart)
                    break;
                await Task.Delay(500);
            }
        }

        private void WatchLTP()
        {
            try
            {
                var ltps = Kite.GetLTP(_configuredStocksForLTP.ToArray());
                foreach (var item in ltps)
                {
                    try
                    {
                        if (_cachedStockLtp[item.Key] != item.Value.LastPrice)
                        {
                            Events.RaiseStockLTPChangedEvent(item.Key, item.Value.LastPrice);
                            _cachedStockLtp[item.Key] = item.Value.LastPrice;
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        Events.RaiseStockLTPChangedEvent(item.Key, item.Value.LastPrice);
                        _cachedStockLtp[item.Key] = item.Value.LastPrice;
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
        #endregion

        #region Order Status Ticker
        public void StartStopOrderStatus(bool start)
        {
            if (start)
            {
                if (!_isOrderTickStart)
                {
                    _isOrderTickStart = true;
                    WatchOrderAsync();
                }
            }
            else
            {
                _isOrderTickStart = false;
            }
        }

        private async void WatchOrderAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                WatchOrderWithTimer();
            });
        }

        private async void WatchOrderWithTimer()
        {
            while (true)
            {
                WatchOrder();
                if (!_isOrderTickStart)
                    break;
                await Task.Delay(500);
            }
        }

        private void WatchOrder()
        {
            try
            {
                var orders = Kite.GetOrders();
                foreach (var orrder in _configuredOrderIdForOrderStatus)
                {

                    var orderHistory = orders.Where(s => s.OrderId == orrder || s.ParentOrderId == orrder);
                    if (orderHistory.Any())
                    {
                        string symbol = string.Format("{0}:{1}", orderHistory.FirstOrDefault().Exchange, orderHistory.FirstOrDefault().Tradingsymbol);
                        Events.RaiseOrderHistoryFetchedEvent(symbol, orderHistory);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region GetPosition Ticker
        public async void StartPositionAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                WatchPosition();
            });
        }

        public async void WatchPosition()
        {
            decimal oldInActiveProfitLoss = 0;
            Dictionary<string, int> _stockPositionsDictionary = new Dictionary<string, int>();

            while (true)
            {
                try
                {
                    decimal profitLoss = 0;
                    var pos = Kite.GetPositions();
                    foreach (var item in pos.Day)
                    {
                        if (item.Quantity == 0)
                            profitLoss += item.PNL;
                        string tradintSymbol = string.Format("{0}:{1}", item.Exchange, item.TradingSymbol);
                        _stockPositionsDictionary[tradintSymbol] = item.Quantity;
                        {
                            _stockPositionsDictionary[tradintSymbol] = item.Quantity;
                            Events.RaisePositionUpdateEventEvent(tradintSymbol, item);
                        }
                    }

                    if (oldInActiveProfitLoss != profitLoss)
                    {
                        oldInActiveProfitLoss = profitLoss;
                        Events.RaiseInactiveStockPNLChangedEvent(profitLoss);
                    }

                }
                catch (Exception)
                {

                }
                await Task.Delay(1000);
            }
        }
        
        #endregion


    }
}
