using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using StockTradeStrategy.BuySellOnSignal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.BuySellOnSignal.ViewModels
{
    public class ReversalListViewmodel : NotifyPropertyChanged
    {
        #region private variables
        private Models.ReversalConfig _selectedReversalConfig;
        private ObservableCollection<Models.ReversalConfig> _reversalConfigs;
        #endregion

        #region public properties
        public Action AddCommandAction { get; set; }
        public Action EditCommandAction { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public ReversalConfig SelectedReversalConfig
        {
            get { return _selectedReversalConfig; }
            set { _selectedReversalConfig = value; OnPropertyChanged("SelectedReversalConfig"); }
        }

        public ObservableCollection<ReversalConfig> ReversalConfigs
        {
            get { return _reversalConfigs; }
            set { _reversalConfigs = value; OnPropertyChanged("ReversalConfigs"); }
        }
        #endregion

        #region constructor
        public ReversalListViewmodel()
        {
            AddCommand = new DelegateCommand(AddCommandExecute, AddCommandCanExecute);
            EditCommand = new DelegateCommand(EditCommandExecute, EditCommandCanExecute);
            DeleteCommand = new DelegateCommand(DeleteCommandExecute, DeleteCommandCanExecute);
            this.PropertyChanged += ReversalListViewmodel_PropertyChanged;
            LoadReversalConfigs();
        }

        private void ReversalListViewmodel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AddCommand.RaiseCanExecuteChanged();
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region Command Implementation
        private void EditCommandExecute(object obj)
        {
            if(EditCommandAction!=null)
            {
                EditCommandAction.Invoke();
            }
        }

        private void DeleteCommandExecute(object obj)
        {
            
        }

        private bool DeleteCommandCanExecute(object obj)
        {
            return SelectedReversalConfig != null;
        }

        private bool EditCommandCanExecute(object obj)
        {
            return SelectedReversalConfig != null;
        }

        private bool AddCommandCanExecute(object obj)
        {
            return true;
        }

        private void AddCommandExecute(object obj)
        {
            if (AddCommandAction != null)
                AddCommandAction.Invoke();
        }
        #endregion

        #region private methods
        public void LoadReversalConfigs()
        {
            Task.Factory.StartNew(() =>
            {
                var data = XSerializer.Instance.GetConfiguration<List<ReversalConfig>>(ConfigFileNames.ReversalInforFileName);
                return data;
            }).ContinueWith((res) =>
            {
                if (res.Result != null)
                {
                    ReversalConfigs = new ObservableCollection<ReversalConfig>(res.Result);
                }
                else
                    ReversalConfigs = new ObservableCollection<ReversalConfig>();
            }, TaskScheduler.FromCurrentSynchronizationContext()); ;
        }
        #endregion
    }
}
