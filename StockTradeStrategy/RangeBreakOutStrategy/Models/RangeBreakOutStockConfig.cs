using KiteConnect;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.RangeBreakOutStrategy.Models
{
    public class RangeBreakOutStockConfig : StockTradeConfiguration.Models.StockBase
    {
        #region private variables
        private decimal _buyBreakOutPrice;
        private decimal _sellBreakOutPrice;
        private OrderStatus _orderStatus;
        private decimal _stopLoss;
        private decimal? _targetPrice;
        public decimal _ltp;
        private string _parentOrderId;
        private string _targetOrderId;
        private decimal _changePercentage;
        private decimal _profitMargin;
        #endregion

        #region public properties
        public Dictionary<OrderType?, KeyValuePair<OrderMode, decimal>> AllOrderTargetPrice { get; set; }
        public Dictionary<OrderType?, KeyValuePair<string, OrderStatus>> AllOrderStatus { get; set; }
        public OrderType OrderType { get; set; }
        public decimal ChangePercentage
        {
            get { return _changePercentage; }
            set { _changePercentage = value; OnPropertyChanged("ChangePercentage"); }
        }
        public string Variety { get; set; }
        public decimal LTP
        {
            get { return _ltp; }
            set
            {
                if (_ltp != value)
                {
                    _ltp = value;
                    OnPropertyChanged("LTP");
                }
            }
        }
        public decimal ProfitMargin
        {
            get { return _profitMargin; }
            set {
                _profitMargin = value;
                if (value > 0.75m)
                {
                    Events.RaiseAskForStockSubscriptionEvent(this.TradingSymbol, StockSubscribeMode.LTP, false);
                }
                OnPropertyChanged("ProfitMargin"); }
        }
        public decimal StopLoss
        {
            get { return _stopLoss; }
            set { _stopLoss = value; OnPropertyChanged("StopLoss"); }
        }
        public volatile bool IsCheckInProgress;
        public string ParentOrderId
        {
            get { return _parentOrderId; }
            set { _parentOrderId = value; OnPropertyChanged("ParentOrderId"); }
        }
        public string TargetOrderId
        {
            get { return _targetOrderId; }
            set { _targetOrderId = value; OnPropertyChanged("TargetOrderId"); }
        }
        public int LastOrderedQuantity { get; set; }
        public int ReversalNumber { get; set; }
        public int OrderedQuantity { get; set; }
        public int Multiplier { get; set; }
        public OrderStatus OrderStatus
        {
            get { return _orderStatus; }
            set
            {
                if (_orderStatus != value)
                {
                    _orderStatus = value;
                    if(value == OrderStatus.TargetHit)
                        Events.RaiseAskForStockSubscriptionEvent(this.TradingSymbol, StockSubscribeMode.LTP, false);

                    OnPropertyChanged("OrderStatus");
                }
            }
        }
        public decimal PreOpenPrice { get; set; }
        public decimal BuyBreakOutPrice
        {
            get { return _buyBreakOutPrice; }
            set
            {
                if (value != _buyBreakOutPrice)
                {
                    _buyBreakOutPrice = value; OnPropertyChanged("BuyBreakOutPrice");
                }
            }
        }
        public decimal SellBreakOutPrice
        {
            get { return _sellBreakOutPrice; }
            set
            {
                if (value != _sellBreakOutPrice)
                {
                    _sellBreakOutPrice = value;
                    OnPropertyChanged("SellBreakOutPrice");
                }
            }
        }
        public decimal? TargetPrice
        {
            get { return _targetPrice; }
            set { _targetPrice = value; OnPropertyChanged("TargetPrice"); }
        }
        public int InitialQuantity
        {
            get;set;
        }
        #endregion

        public RangeBreakOutStockConfig()
        {
            this.AllOrderStatus = new Dictionary<OrderType?, KeyValuePair<string, OrderStatus>>();
            this.AllOrderTargetPrice = new Dictionary<OrderType?, KeyValuePair<OrderMode, decimal>>();
            Variety = Constants.VARIETY_CO;
        }

        #region Public Method
        public int GetQuantity()
        {
            if (ReversalNumber == 0)
                return InitialQuantity == 0 ? 1 : InitialQuantity;
            else
                return OrderedQuantity * Multiplier;
        }
        #endregion


    }

    public enum OrderType
    {
        CoverOrder,
        BracketOrder,
        CoverAndBracketOrder
    }

    
}
