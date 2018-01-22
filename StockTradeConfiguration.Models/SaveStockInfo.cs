using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public class SaveStockInfo : NotifyPropertyChanged
    {
        #region private variables
        private string _exchange;
        private DateTime? _lastUpdatedOn;
        private int _totalItems;
        private bool _isUpdatingStocks;
        private string _failedReason;

        #endregion

        #region Public Properties
        public string FailedReason
        {
            get { return _failedReason; }
            set { _failedReason = value; OnPropertyChanged("FailedReason"); }
        }
        public bool IsUpdatingStocks
        {
            get { return _isUpdatingStocks; }
            set { _isUpdatingStocks = value; OnPropertyChanged("IsUpdatingStocks"); }
        }
        public string Exchange
        {
            get { return _exchange; }
            set { _exchange = value; OnPropertyChanged("Exchange"); }
        }
        public DateTime? LastUpdatedOn
        {
            get { return _lastUpdatedOn; }
            set { _lastUpdatedOn = value; OnPropertyChanged("LastUpdatedOn"); }
        }
        public int TotalItems
        {
            get { return _totalItems; }
            set { _totalItems = value; OnPropertyChanged("TotalItems"); }
        }
        #endregion

    }
}
