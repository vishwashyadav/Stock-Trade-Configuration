using Prism.Commands;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Stock_Trade_Configuration.ViewModels
{
    public class StockWatchViewModel : ViewModelBase
    {
        #region private variables
        private StockSymbols _selectedSymbol;
        private ObservableCollection<StockSymbols> _stockSymbols;
        #endregion
        
        public StockSymbols SelectedSymbol
        {
            get { return _selectedSymbol; }
            set { _selectedSymbol = value; OnPropertyChanged("SelectedSymbol"); }
        }
        public ObservableCollection<StockSymbols> StockSymbols
        {
            get {
                if (_stockSymbols == null)
                {
                    _stockSymbols = new ObservableCollection<StockTradeConfiguration.Models.StockSymbols>();
                    _stockSymbols.CollectionChanged += _rangeBreakOutConfigurations_CollectionChanged;
                }
                return _stockSymbols; }
            set { _stockSymbols = value; if(value!=null)value.CollectionChanged += _rangeBreakOutConfigurations_CollectionChanged; OnPropertyChanged("StockSymbols"); }
        }

        public StockWatchViewModel()
        {
            _stockSymbols = new ObservableCollection<StockTradeConfiguration.Models.StockSymbols>();
            SaveCommand = new StockTradeConfiguration.Models.DelegateCommand(SaveCommandExecute);
            LoadConfigurationFiles();
        }

        public void AddStockSymbols(IEnumerable<string> stockSymbols, bool isAppend=false)
        {
            if (stockSymbols == null)
                return;
            var symbols = new ObservableCollection<StockTradeConfiguration.Models.StockSymbols>(
                    stockSymbols.Select(s => new StockSymbols()
                    {
                        Category = "NSE",
                        StockSymbol = s,
                        DisplayName = string.Format("{0}:{1}", "NSE", s)
                    })
                    );
            if (!isAppend)
            {
                StockSymbols = symbols;
            }
            else
            {
                
                var stockWhichIsAlreadyExist = StockSymbols.Where(s => stockSymbols.Any(t => t.Equals(s.StockSymbol)));
                StockSymbols =  new ObservableCollection<StockTradeConfiguration.Models.StockSymbols>( StockSymbols.Except(stockWhichIsAlreadyExist));
                StockSymbols = new ObservableCollection<StockTradeConfiguration.Models.StockSymbols>( StockSymbols.Union(symbols));
            }
        }

        public void UpdateStockSymbolHighLow(KeyValuePair<string, Tuple<decimal,decimal>> stockHighLow)
        {
            var stock = StockSymbols.FirstOrDefault(s => s.DisplayName == stockHighLow.Key);
            if (stock != null)
            {
                if (stock.StockBreakOutBuyPrice < stockHighLow.Value.Item2)
                    stock.StockBreakOutBuyPrice = stockHighLow.Value.Item2;
                if (stock.StockBreakOutSellPrice==0 || stock.StockBreakOutSellPrice > stockHighLow.Value.Item1)
                    stock.StockBreakOutSellPrice = stockHighLow.Value.Item1;
            }
        }

        private void SaveCommandExecute(object obj)
        {
            this.Serializer.SaveConfiguration<ObservableCollection<StockSymbols>>(ConfigurationFileNames.StockWatchFileName, StockSymbols);
        }

        private void _rangeBreakOutConfigurations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AttachPropertyChanged();                        
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "DisplayName")
                (sender as StockSymbols).UpdateDisplayName();
        }

        private void AttachPropertyChanged()
        {
            foreach (var item in _stockSymbols)
            {
                (item as StockSymbols).PropertyChanged -= ItemPropertyChanged;
                (item as StockSymbols).PropertyChanged += ItemPropertyChanged;

            }


        }

        private async void LoadConfigurationFiles()
        {
            await Task.Factory.StartNew(() =>
            {
                return Serializer.GetConfiguration<ObservableCollection<StockSymbols>>(ConfigurationFileNames.StockWatchFileName);
            }).ContinueWith(result =>
            {
                StockSymbols = result.Result;
                AttachPropertyChanged();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

    }
}
