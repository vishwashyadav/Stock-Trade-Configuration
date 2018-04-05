using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.BuySellOnSignal.Models
{
    public class SignalSettingInfo : NotifyPropertyChanged
    {
        #region private variables
        private string _seperator;
        private int _timeIndex;
        private int _priceIndex;
        private int _buySellSignalIndex;
        private int _timeDifferenceBetweenSystemAndSignal;
        private int _timeBufferToTakeOrder;
        private decimal _priceBufferToAcceptOrder;
        private string _timeFormat;
        #endregion

        #region public properties
        public int Version { get; set; }
        public decimal PriceBufferToAcceptOrder
        {
            get { return _priceBufferToAcceptOrder; }
            set { _priceBufferToAcceptOrder = value; OnPropertyChanged("PriceBufferToAcceptOrder"); }
        }
        public int TimeDifferenceBetweenSystemAndSignal
        {
            get { return _timeDifferenceBetweenSystemAndSignal; }
            set { _timeDifferenceBetweenSystemAndSignal = value; OnPropertyChanged("TimeDifferenceBetweenSystemAndSignal"); }
        }

        public int TimeBufferToTakeOrder
        {
            get { return _timeBufferToTakeOrder; }
            set { _timeBufferToTakeOrder = value; OnPropertyChanged("TimeBufferToTakeOrder"); }
        }
        public string TimeFormat
        {
            get { return _timeFormat; }
            set { _timeFormat = value; OnPropertyChanged("TimeFormat"); }
        }
        public string Seperator
        {
            get { return _seperator; }
            set { _seperator = value;  OnPropertyChanged("Seperator"); }
        }

        public int TimeIndex
        {
            get { return _timeIndex; }
            set { _timeIndex = value; OnPropertyChanged("TimeIndex"); }
        }

        public int PriceIndex
        {
            get { return _priceIndex; }
            set { _priceIndex = value; OnPropertyChanged("PriceIndex"); }
        }

        public int BuySellSignalIndex
        {
            get { return _buySellSignalIndex; }
            set { _buySellSignalIndex = value; OnPropertyChanged("BuySellSignalIndex"); }
        }


        #endregion

        #region constructor
        public SignalSettingInfo()
        {
            _seperator = ",";
            _timeIndex = 2;
            _priceIndex = 1;
            _buySellSignalIndex = 0;
            _timeBufferToTakeOrder = 10;
            _timeDifferenceBetweenSystemAndSignal=0;
            _priceBufferToAcceptOrder = 0;
            _timeFormat = "yyyy.MM.dd HH:mm:ss";
        }
        #endregion

        
    }
}
