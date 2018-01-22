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
        private string _profitLossStatus;
        private StrategyStockStatus _status;
        private int _openPosition;
        private bool _maxProfitEditMode;
        private bool _maxLossEditMode;
        private decimal _maxProfit;
        private decimal _maxLoss;
        #endregion
        public decimal MaxProfitEdit { get; set; }
        public decimal MaxLossEdit { get; set; }
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
            set { _openPosition = value; OnPropertyChanged("OpenPosition"); }
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

        [XmlIgnore]
        public decimal CurrentProfitLoss
        {
            get { return _currentProfitLoss; }
            set { _currentProfitLoss = value; UpdateMinMax(); OnPropertyChanged("CurrentProfitLoss"); }
        }
        public string DataFileExtesnion { get; set; }
        public List<ReversalInfo> ReversalInfoes { get; set; }

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

        private void UpdateMinMax()
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

            if (status != _profitLossStatus)
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
