using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public class RangeBreakOutConfiguration : NotifyPropertyChanged
    {
        #region private variables
        private int _quantity;
        private string _stockSymbol;
        private decimal _stockCurrentPrice;
        private Condition _orderCondition;
        private decimal _breakOutPrice;
        private decimal _margin;
        private decimal _buySellPrice;
        private MarginType _marginType;
        private OrderType _orderType;
        private OrderMode _orderMode;
        private decimal _stopLoss;
        private string _summary;
        #endregion
        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value;  OnPropertyChanged("Quantity"); }
        }
        public string StockSymbol
        {
            get { return _stockSymbol; }
            set { _stockSymbol = value; OnPropertyChanged("StockSymbol"); }
        }
        public decimal StockCurrentPrice
        {
            get { return _stockCurrentPrice; }
            set { _stockCurrentPrice = value; OnPropertyChanged("StockCurrentPrice"); }
        }

        public Condition OrderCondition
        {
            get { return _orderCondition; }
            set { _orderCondition = value; OnPropertyChanged("OrderCondition"); }
        }

        public decimal BreakOutPrice
        {
            get { return _breakOutPrice; }
            set { _breakOutPrice = value; OnPropertyChanged("BreakOutPrice"); }
        }

        public decimal Margin
        {
            get { return _margin; }
            set { _margin = value; OnPropertyChanged("Margin"); }
        }

        public MarginType MarginType
        {
            get { return _marginType; }
            set { _marginType = value; OnPropertyChanged("MarginType"); }
        }

        public decimal BuySellPrice
        {
            get { return _buySellPrice; }
            set { _buySellPrice= value; OnPropertyChanged("BuySellPrice"); }
        }

        public decimal StopLoss
        {
            get { return _stopLoss; }
            set { _stopLoss = value; OnPropertyChanged("StopLoss"); }
        }

        public OrderType OrderType
        {
            get { return _orderType; }
            set { _orderType = value; OnPropertyChanged("OrderType"); }
        }

        public OrderMode OrderMode
        {
            get { return _orderMode; }
            set { _orderMode = value; OnPropertyChanged("OrderMode"); }
        }
        public OrderStatus OrderStatus { get; set; }

        public string Summary
        {
            get { return _summary; }
            set { _summary = value; OnPropertyChanged("Summary"); }
        }
        
        #region private methods
        public void UpdateSummary()
        {
            string format = "{0} {1} {2} shares when its price reaches {3} in {4} direction and {5} at price with margin of {6}%";
            Summary = string.Format(format, OrderMode, Quantity, StockSymbol, BreakOutPrice, OrderMode == OrderMode.BUY ? "Upwards" : "Downwards", OrderMode == OrderMode.BUY ? OrderMode.SELL : OrderMode.BUY, Margin);
        }
        #endregion
    }
    public enum MarginType
    {
        Absolute,
        Percentage
    }

    public enum OrderMode
    {
        BUY,
        SELL
    }

    public enum Condition
    {
        GreaterThanEqualTo,
        LessThanEqualTo
    }

    public enum OrderType
    {
        BracketOrder,
        CoverOrder
    }

    public enum OrderStatus
    {
        YetToPlaceOrder,
        OrderPlaced
    }
}
