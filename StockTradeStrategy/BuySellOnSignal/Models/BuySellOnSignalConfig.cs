using KiteConnect;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StockTradeStrategy.BuySellOnSignal.Models
{

    public class BuySellOnSignalSymbolConfig : NotifyPropertyChanged
    {
        #region private variables
        private decimal _currentProfitLoss;
        private decimal _min;
        private decimal _max;
        private decimal _absoluteProfitLoss;
        private decimal _ltp;
        private string _profitLossStatus;
        private StrategyStockStatus _status;
        private decimal _priceBuffer;
        private int _openPosition;
        private bool _maxProfitEditMode;
        private bool _maxLossEditMode;
        private decimal _maxProfit;
        private decimal _maxLoss;
        private decimal _maxProfitEdit;
        private decimal _maxLossEdit;
        private decimal _tickProfit;
        private decimal _tickProfitEdit;
        private string _debugStatus;
        #endregion
        public string DebugStatus
        {
            get { return _debugStatus; }
            set { _debugStatus = value; OnPropertyChanged("DebugStatus"); }
        }
        public decimal PriceBuffer
        {
            get { return _priceBuffer; }
            set { _priceBuffer = value; OnPropertyChanged("PriceBuffer"); }
        }
        public decimal TrailingStopLoss { get; set; }
        public decimal SellValue { get; set; }
        public decimal BuyValue { get; set; }
        public int? NetQuantity { get; set; }
        public decimal Multiplier { get; set; } 
        public TimeSpan StartTime { get; set; }
        public decimal LTP
        {
            get { return _ltp; }
            set { _ltp = value; OnPropertyChanged("LTP"); }
        }
        public decimal TickProfitEdit
        {
            get { return _tickProfitEdit; }
            set { _tickProfitEdit = value; OnPropertyChanged("TickProfitEdit"); }
        }
        public decimal TickProfit
        {
            get { return _tickProfit; }
            set { _tickProfit = value; OnPropertyChanged("TickProfit"); }
        }
        public SignalProfitType SignalProfitType { get; set; }
        public int ContractSize { get; set; }
        [XmlIgnore]
        public decimal MaxProfitEdit
        {
            get { return _maxProfitEdit; }
            set { _maxProfitEdit = value; OnPropertyChanged("MaxProfitEdit"); }
        }
        [XmlIgnore]
        public decimal MaxLossEdit
        {
            get { return _maxLossEdit; }
            set { _maxLossEdit = value; OnPropertyChanged("MaxLossEdit"); }
        }
        [XmlIgnore]
        public bool MaxProfitEditMode
        {
            get { return _maxProfitEditMode; }
            set { _maxProfitEditMode = value; OnPropertyChanged("MaxProfitEditMode"); }
        }

        [XmlIgnore]
        public bool MaxLossEditMode
        {
            get { return _maxLossEditMode; }
            set { _maxLossEditMode = value; OnPropertyChanged("MaxLossEditMode"); }
        }
        [XmlIgnore]
        public int OpenPosition
        {
            get { return _openPosition; }
            set
            {
                if (_openPosition != value)
                {
                    _openPosition = value;
                    UpdateMax();
                    OnPropertyChanged("OpenPosition");

                }
            }
        }

        public void UpdateMax()
        {
            if (SignalProfitType == SignalProfitType.TickProfit && Status == StrategyStockStatus.Running)
            {
                if (NetQuantity.HasValue)
                {
                    MaxProfit = Math.Abs(NetQuantity.Value) * ((Multiplier) * TickProfit);
                    UpdateMinMax(true);
                }
            }
        }

        [XmlIgnore]
        public StrategyStockStatus Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }
        public int LotSize { get; set; }

        [XmlIgnore]
        public decimal AbsoluteProfitLoss
        {
            get { return _absoluteProfitLoss; }
            set { _absoluteProfitLoss = value; OnPropertyChanged("AbsoluteProfitLoss"); }
        }
        public decimal Min
        {
            get { return _min; }
            set { _min = value; OnPropertyChanged("Min"); }
        }

        public decimal Max
        {
            get { return _max; }
            set { _max = value; OnPropertyChanged("Max"); }
        }
        public string Exchange { get; set; }
        public string DataDirectoryPath { get; set; }
        public string Symbol { get; set; }
        public string Seperator { get; set; }
        public string MappedSymbolName { get; set; }
        public string Extension { get; set; }
        public decimal MaxProfit
        {
            get { return _maxProfit; }
            set { _maxProfit = value;  OnPropertyChanged("MaxProfit"); }
        }
        public decimal MaxLoss
        {
            get { return _maxLoss; }
            set { _maxLoss = value; OnPropertyChanged("MaxLoss"); }
        }
        public decimal LastTrailPoint { get; set; }
        [XmlIgnore]
        public decimal CurrentProfitLoss
        {
            get { return _currentProfitLoss; }
            set { _currentProfitLoss = value; UpdateMinMax(); OnPropertyChanged("CurrentProfitLoss"); }
        }
        public string DataFileExtesnion { get; set; }
        public List<ReversalInfo> ReversalInfoes { get; set; }

        public BuySellOnSignalSymbolConfig()
        {
          
        }

        private void FileWatcher_FileContentChangedEvent(string filePath)
        {
            if(File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                var latestLine = lines.FirstOrDefault();
                var splitData = latestLine.Split(new string[] { Seperator }, StringSplitOptions.RemoveEmptyEntries);
                OrderMode? mode = null;
                decimal price = 0;
                if (decimal.TryParse(splitData[1], out price))
                {
                    if (splitData[0].Equals("buy", StringComparison.InvariantCultureIgnoreCase))
                    {
                        mode = OrderMode.BUY;
                    }
                    else if (splitData[0].Equals("sell", StringComparison.InvariantCultureIgnoreCase))
                    {
                        mode = OrderMode.SELL;
                    }
                }
            }
        }

        public void UpdateMinMax(bool updateForce=false)
        {
            var status="";
            if(CurrentProfitLoss>0)
            {
                status = "profit";
                
            }
            else if(CurrentProfitLoss<0)
            {
                status = "loss";
                
            }

            if (status != _profitLossStatus || updateForce)
            {
                if(status =="profit")
                {
                    Min = 1;
                    Max = MaxProfit;
                }
                else if(status == "loss")
                {
                    Min = 0;
                    Max = Math.Abs(MaxLoss);
                }
            }

            _profitLossStatus = status;

            AbsoluteProfitLoss = Math.Abs(CurrentProfitLoss);
        }
        
    }
}
