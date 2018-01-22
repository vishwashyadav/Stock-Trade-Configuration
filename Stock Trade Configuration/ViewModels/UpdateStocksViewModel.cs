using Stock_Trade_Configuration.Singleton;
using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.ViewModels
{
    public class UpdateStocksViewModel : ViewModelBase
    {
        #region private variables
        private ObservableCollection<SaveStockInfo> _stocksInfoes;
        private bool _isLoadingStocksInfo;

        #endregion

        #region public properties
        public DelegateCommand StartCommand { get; set; }
        public bool IsLoadingStocksInfo
        {
            get { return _isLoadingStocksInfo; }
            set { _isLoadingStocksInfo = value; OnPropertyChanged("IsLoadingStocksInfo"); }
        }
        public ObservableCollection<SaveStockInfo> StocksInfoes
        {
            get { return _stocksInfoes; }
            set { _stocksInfoes = value; OnPropertyChanged("StocksInfoes"); }
        }
        #endregion

        #region constructor
        public UpdateStocksViewModel()
        {
            LoadStocksInfo();
            StartCommand = new DelegateCommand(StartCommandExecute, StartCommandCanExecute);
        }

        private void UpdateStocks(SaveStockInfo s)
        {
            s.IsUpdatingStocks = true;
            Task.Factory.StartNew(() =>
            {
                var count = new ExchangeSymbolUpdate().UpdateSymbol(this.CurrentApiClient, KiteInstance.Kite, s.Exchange);
                return count;
            }).ContinueWith((res) =>
            {
                switch (res.Status)
                {
                    case TaskStatus.RanToCompletion:
                        s.LastUpdatedOn = DateTime.Now;
                        s.FailedReason = string.Empty;
                        s.TotalItems = res.Result;
                        break;
                    case TaskStatus.Faulted:
                        s.FailedReason = res.Exception.Message;
                        break;
                }
                s.IsUpdatingStocks = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void StartCommandExecute(object obj)
        {
            for (int i = 0; i < _stocksInfoes.Count; i++)
            {
                UpdateStocks(_stocksInfoes[i]);
            }
        }

        private bool StartCommandCanExecute(object obj)
        {
            return true;            
        }

        private void LoadStocksInfo()
        {
            IsLoadingStocksInfo = true;
            Task.Factory.StartNew(() =>
            {
                var stocksInfos = new TradingSymbolManager().GetSavedStockSymbolInfo(this.CurrentApiClient);
                return stocksInfos;
            }).ContinueWith((res) =>
            {
                StocksInfoes = new ObservableCollection<SaveStockInfo>( res.Result);
                IsLoadingStocksInfo = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        #endregion


    }
}
