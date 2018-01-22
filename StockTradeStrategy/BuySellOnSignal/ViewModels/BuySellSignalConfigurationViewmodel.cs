using KiteConnect;
using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using StockTradeStrategy.BuySellOnSignal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockTradeStrategy.BuySellOnSignal.ViewModels
{
    public class BuySellSignalConfigurationViewmodel:NotifyPropertyChanged
    {
        #region private variables
        private string _selectedExchange;
        private List<StockTradeConfiguration.Models.Instrument> _instruments;
        private TradingSymbolManager _tradingSymbolManager;
        private List<string> _exchanges;
        private StockTradeConfiguration.Models.Instrument _selectedInstrument;
        private string _folderPath;
        private string _mappedSymbolName;
        private decimal _maxProfit;
        private decimal _maxLoss;
        private ReversalConfig _selectedReversalConfig;
        private string _selectedFileFormat;
        private ObservableCollection<ReversalConfig> _reversalConfigurations;
        private ObservableCollection<BuySellOnSignalSymbolConfig> _buySellOnSignalSymbolConfigs;
        #endregion

        #region public Properties
        public DelegateCommand MaxProfitEditCommand { get; set; }
        public DelegateCommand MaxLossEditCommand { get; set; }
        public DelegateCommand SquareOffCommand { get; set; }
        public ReversalConfig SelectedReversalConfig
        {
            get { return _selectedReversalConfig; }
            set { _selectedReversalConfig = value; OnPropertyChanged("SelectedReversalConfig"); }
        }
        public ObservableCollection<ReversalConfig> ReversalConfigurations
        {
            get { return _reversalConfigurations; }
            set { _reversalConfigurations = value; OnPropertyChanged("ReversalConfigurations"); }
        }
        public DelegateCommand StartCommand { get; set; }
        public ObservableCollection<BuySellOnSignalSymbolConfig> BuySellOnSignalSymbolConfigs
        {
            get { return _buySellOnSignalSymbolConfigs; }
            set { _buySellOnSignalSymbolConfigs = value; OnPropertyChanged("BuySellOnSignalSymbolConfigs"); }
        }

        internal void DeleteItem(BuySellOnSignalSymbolConfig buySellOnSignalSymbolConfig)
        {
            BuySellSignalOrderManager.Instance.Stop(buySellOnSignalSymbolConfig);
            BuySellOnSignalSymbolConfigs.Remove(buySellOnSignalSymbolConfig);
        }

        public string FolderPath
        {
            get { return _folderPath; }
            set { _folderPath = value; OnPropertyChanged("FolderPath"); }
        }
        public DelegateCommand AddCommand { get; set; }
        public string SelectedFileFormat
        {
            get { return _selectedFileFormat; }
            set { _selectedFileFormat = value; OnPropertyChanged("SelectedFileFormat"); }
        }
        public List<string> FileFormats
        {
            get { return new  List<string>(){"CSV","TXT" }; }
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
        public string MappedSymbolName
        {
            get { return _mappedSymbolName; }
            set { _mappedSymbolName = value; OnPropertyChanged("MappedSymbolName"); }
        }
      
        public StockTradeConfiguration.Models.Instrument SelectedInstrument
        {
            get { return _selectedInstrument; }
            set { _selectedInstrument = value; OnPropertyChanged("SelectedInstrument"); }
        }
        public List<StockTradeConfiguration.Models.Instrument> Instruments
        {
            get { return _instruments; }
            set { _instruments = value; OnPropertyChanged("Instruments"); }
        }
        public string SelectedExchange
        {
            get { return _selectedExchange; }
            set { _selectedExchange = value; LoadInstrumentsByExchange(); OnPropertyChanged("SelectedExchange"); }
        }
        public List<string> Exchanges
        {
            get { return _exchanges; }
            set { _exchanges = value; OnPropertyChanged("Exchanges"); }
        }

        #endregion

        #region constructor
        public BuySellSignalConfigurationViewmodel()
        {
            MaxProfitEditCommand = new DelegateCommand(MaxProfitEditCommandExecute);
            MaxLossEditCommand = new DelegateCommand(MaxProfitLossCommandExecute);
            AddCommand = new DelegateCommand(AddCommandExecute, AddCommandCanExecute);
            SquareOffCommand = new DelegateCommand(SquareOffCommandExecute);
            StartCommand = new DelegateCommand(StartCommandExecute);
            _tradingSymbolManager = new TradingSymbolManager();
            Exchanges = _tradingSymbolManager.GetExchanges().ToList();
            Events.PositionUpdateEvent += Events_PositionUpdateEvent;
            Events.StatusChangedEvent += Events_StatusChangedEvent;
            LoadReversalConfigSetting();
            this.PropertyChanged += BuySellSignalConfigurationViewmodel_PropertyChanged;
        }

        private void MaxProfitLossCommandExecute(object obj)
        {
            var config = obj as BuySellOnSignalSymbolConfig;
            if (config.MaxLossEditMode)
            {
                config.MaxLoss = config.MaxLossEdit;
                BuySellSignalOrderManager.Instance.UpdateMaxLoss(config);
            }
            config.MaxLossEdit = config.MaxLoss;
            config.MaxLossEditMode = !config.MaxLossEditMode;
        }

        private void MaxProfitEditCommandExecute(object obj)
        {
            var config = obj as BuySellOnSignalSymbolConfig;
            if(config.MaxProfitEditMode)
            {
                config.MaxProfit = config.MaxProfitEdit;
                BuySellSignalOrderManager.Instance.UpdateMaxProfit(config);
            }
            config.MaxProfitEdit = config.MaxProfit;
            config.MaxProfitEditMode = !config.MaxProfitEditMode;
        }

        private void SquareOffCommandExecute(object obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show("Performing this action will close all open position and will stop Robot Transaction. Do you want to continue?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    var config = obj as BuySellOnSignalSymbolConfig;
                    if (config != null)
                    {
                        BuySellSignalOrderManager.Instance.SquareOffAllPositions(config);
                    }
                }
            });
        }

        private void BuySellSignalConfigurationViewmodel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AddCommand.RaiseCanExecuteChanged();
        }

        private bool AddCommandCanExecute(object obj)
        {
            return !string.IsNullOrEmpty(MappedSymbolName) && !string.IsNullOrEmpty(FolderPath) && SelectedInstrument != null && !string.IsNullOrEmpty(SelectedFileFormat);
        }

        #endregion

        #region private methods
        private void Events_StatusChangedEvent(string exchange, string symbol, string status)
        {
            StrategyStockStatus strategyStatus = StrategyStockStatus.Added;
            if (BuySellOnSignalSymbolConfigs!=null && Enum.TryParse(status, out strategyStatus))
            {
                var stock = BuySellOnSignalSymbolConfigs.FirstOrDefault(s => s.Exchange == exchange && symbol == s.Symbol);
                if (stock != null)
                {
                    stock.Status = strategyStatus;
                }
            }
        }
        private void Events_PositionUpdateEvent(string exchange, string symbol, Position position)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var stock = BuySellOnSignalSymbolConfigs.FirstOrDefault(s => s.Exchange == exchange && s.Symbol == symbol);
                if(stock!= null)
                {
                    stock.CurrentProfitLoss = position.PNL;
                    stock.OpenPosition = position.Quantity;
                }
            });
        }
        private void StartCommandExecute(object obj)
        {
            var item = obj as BuySellOnSignalSymbolConfig;
            if (item != null)
            {
                if (item.Status == StrategyStockStatus.Added)
                    BuySellSignalOrderManager.Instance.Start(item, Kite);
                else if(item.Status == StrategyStockStatus.Running)
                {
                    //BuySellSignalOrderManager.Instance.sto(item, Kite);
                }
            }
        }
        private void AddCommandExecute(object obj)
        {
            if (BuySellOnSignalSymbolConfigs == null)
                BuySellOnSignalSymbolConfigs = new ObservableCollection<BuySellOnSignalSymbolConfig>();

            if (!BuySellOnSignalSymbolConfigs.Any(s => s.Exchange == SelectedExchange && s.Symbol == SelectedInstrument.TradingSymbol))
            {
                Random r = new Random();
                BuySellOnSignalSymbolConfig config = new BuySellOnSignalSymbolConfig()
                {
                    Seperator = ",",
                    Exchange = SelectedExchange,
                    Symbol = SelectedInstrument.TradingSymbol,
                    MaxLoss = MaxLoss,
                    Extension = SelectedFileFormat,
                    LotSize = SelectedInstrument.LotSize,
                    MaxProfit = MaxProfit,
                    MappedSymbolName = MappedSymbolName,
                    DataDirectoryPath = FolderPath,
                    DataFileExtesnion = SelectedFileFormat

                };

                if (SelectedReversalConfig != null)
                    config.ReversalInfoes = SelectedReversalConfig.ReversalInfoes;
                BuySellOnSignalSymbolConfigs.Add(config);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(string.Format("Symbol '{0}' is already added to the list. Try adding different symbol!", SelectedInstrument.TradingSymbol), "Duplicate Stock", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
        private void LoadInstrumentsByExchange()
        {
            Instruments = new List<StockTradeConfiguration.Models.Instrument>() { new StockTradeConfiguration.Models.Instrument() { Name = "Loading..." } };
            Task.Factory.StartNew(() =>
            {
                var instruments = _tradingSymbolManager.GetInstruments(ApiClient.Zerodha, SelectedExchange);
                return instruments;
            }).ContinueWith((res) =>
            {
                Instruments = res.Result;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        public void LoadReversalConfigSetting()
        {
            Task.Factory.StartNew(() =>
            {
                var data = XSerializer.Instance.GetConfiguration<List<ReversalConfig>>(ConfigFileNames.ReversalInforFileName);
                return data;
            }).ContinueWith((res)=>
            {
                ReversalConfigurations = new ObservableCollection<ReversalConfig>(res.Result);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        #endregion
    }
}
