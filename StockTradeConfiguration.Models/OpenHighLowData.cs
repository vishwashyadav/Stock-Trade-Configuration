using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public class OpenHighLowData : NotifyPropertyChanged
    {
        private string _exchange;
        private string _symbol;
        private decimal _open;
        private decimal _high;
        private decimal _low;
        private Status _status;

        public string Exchange
        {
            get { return _exchange; }
            set { _exchange = value; OnPropertyChanged("Exchange"); }
        }

        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; OnPropertyChanged("Symbol"); }
        }

        public decimal Open
        {
            get { return _open; }
            set { _open = value; OnPropertyChanged("Open"); }
        }

        public decimal High
        {
            get { return _high; }
            set { _high = value; OnPropertyChanged("High"); }
        }

        public decimal Low
        {
            get { return _low; }
            set { _low = value; OnPropertyChanged("Low"); }
        }

        public Status Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }
    }

    public enum Status
    {
        UnKnown,
        OpenHigh,
        OpenLow
    }

}
