using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.BuySellOnSignal.Models
{
    public class GlobalProfitLossSetting : NotifyPropertyChanged
    {
        #region private variables
        private bool _isMaxProfitEditMode;
        private bool _isMaxLossEditMode;
        private decimal _maxProfitEdit;
        private decimal _maxLossEdit;
        private decimal _maxProfit;
        private decimal _maxLoss;
        private decimal _currentProfitLoss;
        private decimal _min;
        private decimal _max;
        private string _profitLossStatus;
        private decimal _absoluteProfitLoss;
        #endregion

        #region Public Properties
        public decimal InactiveDayProfitLoss { get; set; }
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

        public decimal CurrentProfitLoss
        {
            get { return _currentProfitLoss; }
            set
            {
                if (value != _currentProfitLoss)
                {
                    _currentProfitLoss = value;
                    UpdateMinMax(false);                   
                    OnPropertyChanged("CurrentProfitLoss");
                }
            }
        }
        public decimal MaxProfit
        {
            get { return _maxProfit; }
            set { _maxProfit = value; OnPropertyChanged("MaxProfit"); }
        }

        public decimal MaxLoss
        {
            get { return _maxLoss; }
            set { _maxLoss = value; OnPropertyChanged("MaxLoss"); }
        }

        public decimal MaxProfitEdit
        {
            get { return _maxProfitEdit; }
            set { _maxProfitEdit = value; OnPropertyChanged("MaxProfitEdit"); }
        }
        public decimal MaxLossEdit
        {
            get { return _maxLossEdit; }
            set { _maxLossEdit = value; OnPropertyChanged("MaxLossEdit"); }
        }
        public bool IsMaxProfitEditMode
        {
            get { return _isMaxProfitEditMode; }
            set { _isMaxProfitEditMode = value; OnPropertyChanged("IsMaxProfitEditMode"); }
        }

        public bool IsMaxLossEditMode
        {
            get { return _isMaxLossEditMode; }
            set { _isMaxLossEditMode = value; OnPropertyChanged("IsMaxLossEditMode"); }
        }

        public decimal AbsoluteProfitLoss
        {
            get { return _absoluteProfitLoss; }
            set { _absoluteProfitLoss = value; OnPropertyChanged("AbsoluteProfitLoss"); }
        }
        #endregion

        #region private methods
        public void UpdateMinMax(bool updateForce = false)
        {
            AbsoluteProfitLoss = Math.Abs(CurrentProfitLoss);
            var status = "";
            if (CurrentProfitLoss > 0)
            {
                status = "profit";

            }
            else if (CurrentProfitLoss < 0)
            {
                status = "loss";

            }

            if (status != _profitLossStatus || updateForce)
            {
                if (status == "profit")
                {
                    CurrentProfitLoss = 1;
                    Max = MaxProfit;
                }
                else if (status == "loss")
                {
                    Min = 0;
                    Max = Math.Abs(MaxLoss);
                }
            }

            _profitLossStatus = status;

            AbsoluteProfitLoss = Math.Abs(CurrentProfitLoss);
        }
        #endregion
    }
}
