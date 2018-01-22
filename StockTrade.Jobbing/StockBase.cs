using KiteConnect;

using StockTradeConfiguration.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrade.Jobbing
{
    public abstract class JobbingStockBase  : NotifyPropertyChanged
    {
        private decimal _currentPrice;
        private JobbingStatus _status;
        private int _openPositionsCount;
        private string _pendingOrderId;
        private bool _isCancelled;
        public string JobbingType { get; set; }
        public MarginType MarginType { get; set; }
        public decimal LastExecutedOrderPrice { get; set; }
        protected Dictionary<OrderMode, decimal> StockActionPrice { get; set; }
        protected decimal OpenPositions { get; set; }
        protected JobbingDataSaver DataSaver { get; set; }
        protected OrderMode LastOrderMode { get; set; }
        public string SaveDirectoryName { get; set; }
        public int OpenPositionsCount
        {
            get { return _openPositionsCount; }
            set { _openPositionsCount = value; OnPropertyChanged("OpenPositionsCount"); }
        }
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public int StocksBuySellQuantityStart { get; set; }
        public decimal CurrentPrice
        {
            get { return _currentPrice; }
            set { _currentPrice = value; UpdateStockActionPrice(); }
        }
        public decimal Margin { get; set; }
        public decimal MaxLoss { get; set; }
        public JobbingStatus Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }
        public decimal MaxOpenPosition { get; set; }

        private void RaiseStatusPropertyChanged()
        {
            Events.RaiseStatusChangedEvent(this.Exchange, this.Symbol, this.Status.ToString());
        }

        protected void RaiseOpenPositionsChangedEvent()
        {
            Events.RaiseOpenPositionsChangedEvent(this.Exchange, this.Symbol, this.OpenPositionsCount);
        }


        public JobbingStockBase()
        {
            DataSaver = new JobbingDataSaver();
            StocksBuySellQuantityStart = 1;
            StockActionPrice = new Dictionary<OrderMode, decimal>();
            StockActionPrice[OrderMode.BUY] = 0;
            StockActionPrice[OrderMode.SELL] = 0;
        }

        private void UpdateStockActionPrice()
        {
            StockActionPrice[OrderMode.BUY] = (CurrentPrice - Margin).GetNextValidPrice(false);
            StockActionPrice[OrderMode.SELL] = (CurrentPrice + Margin).GetNextValidPrice(true);
        }

        public Kite Kite;

        #region Abstract Methods
        public abstract void BuySellStock(LTP ltp);
        public abstract void ExecutedOrder(Order order);
        #endregion

        public void Start(Kite kite)
        {
            _isCancelled = false;
            this.Status = JobbingStatus.Running;
            UpdateStockActionPrice();
            Kite = kite;
            RaiseStatusPropertyChanged();
            StartJobbing();
        }

        private async void WatchLTPAsync()
        {
            while (true)
            {
                WatchLTP();
                //await Task.Delay(500);
                if (_isCancelled)
                    break;
            }
        }
        private async void StartJobbing()
        {
            await Task.Run(() =>
            {
                WatchLTPAsync();
            });

        }

        public void Stop()
        {
            OpenPositionsCount = 0;
            _pendingOrderId = null;
            Status = JobbingStatus.NotStarted;
            _isCancelled = true;
            RaiseStatusPropertyChanged();
        }
        private void WatchLTP()
        {
            try
            {
                if (string.IsNullOrEmpty(_pendingOrderId))
                {
                    var getLTP = Kite.GetLTP(new string[] { string.Format("{0}:{1}", Exchange, Symbol) });
                    this.BuySellStock(getLTP.Values.FirstOrDefault());
                }
                else
                {
                    try
                    {
                        var order = Kite.GetOrder(_pendingOrderId);
                        if(order.Any(s => s.Status.Equals("complete", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            var orderObj = order.FirstOrDefault(s => s.Status.Equals("complete", StringComparison.CurrentCultureIgnoreCase));
                            LastExecutedOrderPrice = orderObj.AveragePrice;
                            _pendingOrderId = null;
                        }
                        else if((order.Any(s => s.Status.Equals("rejected", StringComparison.CurrentCultureIgnoreCase))))
                        {
                            _pendingOrderId = null;
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

        }
        
        protected void SaveData(OrderMode orderMode, int quantity, decimal price)
        {
            DataSaver.SaveData(this, orderMode, quantity, price, DateTime.Now);
            RaiseOpenPositionsChangedEvent();
        }

        protected void PlaceOrder(string orderMode, string quantity, string price, OrderOnPrice orderOnPrice = OrderOnPrice.LimitPrice)
        {
            var orderType = (orderOnPrice == OrderOnPrice.LimitPrice) ? "LIMIT" : "MARKET";
            var order = Kite.PlaceOrder(Exchange, Symbol, orderMode.ToString(), StocksBuySellQuantityStart.ToString(), Price: price.ToString(), Product: "MIS", OrderType: "LIMIT");
            if (order.Any(s => s.Key.ToLower() == "status" && s.Value.ToLower() == "success"))
            {
                if (order.ContainsKey("data"))
                {
                    var data = order["data"];
                    if (data is ICollection)
                    {
                        foreach (var item in data)
                        {
                            if (item is KeyValuePair<string, object>)
                            {
                                if (item.Key == "order_id")
                                {
                                    _pendingOrderId = Convert.ToString(item.Value);
                                    break;
                                }
                            }
                        }
                    }
                    else
                        _pendingOrderId = null;
                    //var dat = data[]
                }
                else
                    _pendingOrderId = null;
            }
            else
                _pendingOrderId = null;
        }

        private void AddOpenPositions(PositionalStockInfo stockInfo, OrderMode mode)
        {
           // OpenPositions[mode].Add(stockInfo);
        }
    }

    
}
