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
    public class BuySellSignalConfigurationViewmodel : NotifyPropertyChanged
    {
        #region private variables
        private decimal _trailingStopLoss;
        private TimeSpan _startTime;
        private Dictionary<string, BuySellOnSignalSymbolConfig> _configuredStocksDictionary;
        private string _debugInfo;
        private GlobalProfitLossSetting _globalProfitLossSetting;
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
        private decimal _tickProfit;
        private int _contractSize = 1;
        private SignalProfitType _signalProfitType;
        private ObservableCollection<ReversalConfig> _reversalConfigurations;
        private Models.SignalSettingInfo _setting;

        private ObservableCollection<BuySellOnSignalSymbolConfig> _buySellOnSignalSymbolConfigs;
        #endregion

        #region public Properties
        public Models.SignalSettingInfo Setting
        {
            get { return _setting; }
            set { _setting = value; OnPropertyChanged("Setting"); }
        }
        public decimal TrailingStopLoss
        {
            get { return _trailingStopLoss; }
            set { _trailingStopLoss = value; OnPropertyChanged("TrailingStopLoss"); }
        }
        public TimeSpan StartTime
        {
            get { return _startTime; }
            set { _startTime = value; OnPropertyChanged("StartTime"); }
        }
        public string DebugInfo
        {
            get { return _debugInfo; }
            set { _debugInfo = value; OnPropertyChanged("DebugInfo"); }
        }
        public SignalProfitType SignalProfitType
        {
            get { return _signalProfitType; }
            set { _signalProfitType = value; OnPropertyChanged("SignalProfitType"); }
        }
        public decimal TickProfit
        {
            get { return _tickProfit; }
            set { _tickProfit = value; OnPropertyChanged("TickProfit"); }
        }
        public int ContractSize
        {
            get { return _contractSize; }
            set { _contractSize = value; OnPropertyChanged("ContractSize"); }
        }
        public DelegateCommand CancelGlobalProfitCommand { get; set; }
        public DelegateCommand EditGlobalProfitCommand{get;set;}
        public DelegateCommand ApplyGlobalProfitCommand { get; set; }
        public DelegateCommand SetGlobalProfitLossCommand { get; set; }
        public GlobalProfitLossSetting GlobalProfitLossSetting
        {
            get { return _globalProfitLossSetting; }
            set { _globalProfitLossSetting = value; OnPropertyChanged("GlobalProfitLossSetting"); }
        }

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
            Events.RaiseAskForStockSubscriptionEvent(string.Format("{0}:{1}", buySellOnSignalSymbolConfig.Exchange, buySellOnSignalSymbolConfig.Symbol), StockSubscribeMode.LTP, false);
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
            LoadAppSetting();
            _configuredStocksDictionary = new Dictionary<string, BuySellOnSignalSymbolConfig>();
            EditGlobalProfitCommand = new DelegateCommand(EditGlobalProfitCommandExecute);
            CancelGlobalProfitCommand = new DelegateCommand(CancelGlobalProfitCommandExecute);
            ApplyGlobalProfitCommand = new DelegateCommand(ApplyGlobalProfitCommandExecute, ApplyGlobalProfitCommandCanExecute);
            SetGlobalProfitLossCommand = new DelegateCommand(SetGlobalProfitLossCommandExecute);
            MaxProfitEditCommand = new DelegateCommand(MaxProfitEditCommandExecute);
            MaxLossEditCommand = new DelegateCommand(MaxProfitLossCommandExecute);
            AddCommand = new DelegateCommand(AddCommandExecute, AddCommandCanExecute);
            SquareOffCommand = new DelegateCommand(SquareOffCommandExecute);
            StartCommand = new DelegateCommand(StartCommandExecute);
            _tradingSymbolManager = new TradingSymbolManager();
            Exchanges = _tradingSymbolManager.GetExchanges().ToList();
            Events.PositionUpdateEvent += Events_PositionUpdateEvent;
            Events.StatusChangedEvent += Events_StatusChangedEvent;
            Events.InactiveStockPNLChanged += Events_InactiveStockPNLChanged;
            LoadReversalConfigSetting();
            this.PropertyChanged += BuySellSignalConfigurationViewmodel_PropertyChanged;
            Events.TargetStopLossChangeEvent += Events_TargetStopLossChangeEvent;
            Events.StockLTPChangedEvent += Events_StockLTPChangedEvent;
            StartTime = DateTime.Now.TimeOfDay;
        }

        private bool ApplyGlobalProfitCommandCanExecute(object obj)
        {
            return GlobalProfitLossSetting != null && GlobalProfitLossSetting.MaxLossEdit != 0 && GlobalProfitLossSetting.MaxProfitEdit != 0;
        }

        private void ApplyGlobalProfitCommandExecute(object obj)
        {
            if (GlobalProfitLossSetting != null )
            {
                GlobalProfitLossSetting.IsMaxProfitEditMode = false;
                GlobalProfitLossSetting.IsMaxLossEditMode = false;
                GlobalProfitLossSetting.MaxLoss = GlobalProfitLossSetting.MaxLossEdit;
                GlobalProfitLossSetting.MaxProfit = GlobalProfitLossSetting.MaxProfitEdit;
                GlobalProfitLossSetting.UpdateMinMax(true);
                BuySellSignalOrderManager.Instance.SetDayProfitLoss(GlobalProfitLossSetting.MaxProfit, GlobalProfitLossSetting.MaxLoss);
            }
        }

        private void CancelGlobalProfitCommandExecute(object obj)
        {
            if (GlobalProfitLossSetting != null)
            {
                GlobalProfitLossSetting.IsMaxProfitEditMode = false;
                GlobalProfitLossSetting.IsMaxLossEditMode = false;
                GlobalProfitLossSetting.MaxLossEdit = GlobalProfitLossSetting.MaxLoss;
                GlobalProfitLossSetting.MaxProfitEdit = GlobalProfitLossSetting.MaxProfit;
            }
        }

        private void EditGlobalProfitCommandExecute(object obj)
        {
            if(GlobalProfitLossSetting!=null)
            {
                GlobalProfitLossSetting.IsMaxProfitEditMode = true;
                GlobalProfitLossSetting.IsMaxLossEditMode = true;
                GlobalProfitLossSetting.MaxLossEdit = GlobalProfitLossSetting.MaxLoss;
                GlobalProfitLossSetting.MaxProfitEdit = GlobalProfitLossSetting.MaxProfit;
            }
        }

        private void UpdateDayProfitLoss()
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var activeStockProfitLoss = BuySellOnSignalSymbolConfigs.Where(s => s.OpenPosition != 0).Sum(s => s.CurrentProfitLoss);
                if (GlobalProfitLossSetting == null)
                    return;
                var dayProfitLoss = GlobalProfitLossSetting.InactiveDayProfitLoss + activeStockProfitLoss;
                if (dayProfitLoss != GlobalProfitLossSetting.CurrentProfitLoss)
                {
                    GlobalProfitLossSetting.CurrentProfitLoss = dayProfitLoss;
                    Events.RaiseDayProfitLossChanged(GlobalProfitLossSetting.CurrentProfitLoss);
                }
                
            });
        }

        private void SetGlobalProfitLossCommandExecute(object obj)
        {
            if(GlobalProfitLossSetting==null)
            {
                GlobalProfitLossSetting = new GlobalProfitLossSetting() { IsMaxLossEditMode = true, IsMaxProfitEditMode=true };
                GlobalProfitLossSetting.PropertyChanged += (s, e) =>
                 {
                     ApplyGlobalProfitCommand.RaiseCanExecuteChanged();
                 };
            }
        }

        private void MaxProfitLossCommandExecute(object obj)
        {
            var config = obj as BuySellOnSignalSymbolConfig;
            if (config.MaxLossEditMode)
            {
                config.MaxLoss = config.MaxLossEdit;
                BuySellSignalOrderManager.Instance.UpdateMaxLoss(config);
                config.UpdateMinMax(true);
            }
            config.MaxLossEdit = config.MaxLoss;
            config.MaxLossEditMode = !config.MaxLossEditMode;
        }

        private void MaxProfitEditCommandExecute(object obj)
        {
            var config = obj as BuySellOnSignalSymbolConfig;
            if(config.MaxProfitEditMode)
            {
                if (config.SignalProfitType == SignalProfitType.Absolute)
                {
                    config.MaxProfit = config.MaxProfitEdit;
                    BuySellSignalOrderManager.Instance.UpdateMaxProfit(config);
                    config.UpdateMinMax(true);
                }
                else if(config.SignalProfitType == SignalProfitType.TickProfit)
                {
                    config.TickProfit = config.TickProfitEdit;
                    config.UpdateMax();
                }
            }
            config.MaxProfitEdit = config.MaxProfit;
            config.TickProfitEdit = config.TickProfit;
            config.MaxProfitEditMode = !config.MaxProfitEditMode;
        }

        private void SquareOffCommandExecute(object obj)
        {
            var config = obj as BuySellOnSignalSymbolConfig;
            SquareOffPosition(config, Math.Abs(config.OpenPosition));
        }

        public void SquareOffPosition (BuySellOnSignalSymbolConfig config, int count)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show("Performing this action will close all open position and will stop Robot Transaction. Do you want to continue?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    if (config != null)
                    {
                        BuySellSignalOrderManager.Instance.SquareOffAllPositions(config,count);
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
        public void LoadAppSetting()
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var savedData = XSerializer.Instance.GetConfiguration<SignalSettingInfo>(ConfigFileNames.SignalParameterSettingFileName);

                //Save data with default data
                if (savedData == null)
                {
                    Setting = new SignalSettingInfo();
                    Setting.Version++;
                    XSerializer.Instance.SaveConfiguration<SignalSettingInfo>(ConfigFileNames.SignalParameterSettingFileName, Setting);
                }
                else
                    Setting = savedData;

                BuySellSignalOrderManager.Instance.UpdateSettingInfo(Setting);
            });
        }
      

        private void StartCommandExecute(object obj)
        {
            var item = obj as BuySellOnSignalSymbolConfig;
            if (item != null)
            {
                if (item.Status != StrategyStockStatus.Running)
                    BuySellSignalOrderManager.Instance.Start(item, Kite,Setting);
                else if(item.Status == StrategyStockStatus.Running)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        bool canStop = true;
                        if (item.OpenPosition != 0)
                        {
                            var result = MessageBox.Show("Stoping this service will square off all position and stop further trades. Are you sure want to continue?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            canStop = result == MessageBoxResult.Yes;
                        }
                        if(canStop)
                        {
                            BuySellSignalOrderManager.Instance.Stop(item);
                        }
                    });
                    //BuySellSignalOrderManager.Instance.sto(item, Kite);
                }
            }
        }
        private void AddCommandExecute(object obj)
        {
            if (System.IO.Directory.Exists(FolderPath))
            {
                if (BuySellOnSignalSymbolConfigs == null)
                    BuySellOnSignalSymbolConfigs = new ObservableCollection<BuySellOnSignalSymbolConfig>();

                if (!BuySellOnSignalSymbolConfigs.Any(s => s.Exchange == SelectedExchange && s.Symbol == SelectedInstrument.TradingSymbol))
                {
                    Random r = new Random();
                    BuySellOnSignalSymbolConfig config = new BuySellOnSignalSymbolConfig()
                    {
                        
                        Seperator = ",",
                        SignalProfitType = SignalProfitType,
                        StartTime = StartTime,
                        TrailingStopLoss = TrailingStopLoss,
                        ContractSize = ContractSize,
                        TickProfit = TickProfit,
                        Exchange = SelectedExchange,
                        Symbol = SelectedInstrument.TradingSymbol,
                        MaxLoss = MaxLoss,
                        Extension = SelectedFileFormat,
                        LotSize = SelectedInstrument.LotSize,
                        MaxProfit = MaxProfit,
                        MappedSymbolName = MappedSymbolName,
                        DataDirectoryPath = FolderPath,
                        DataFileExtesnion = SelectedFileFormat,
                    };
                    string tradingSymbol = string.Format("{0}:{1}", config.Exchange, config.Symbol);
                    if (SelectedReversalConfig != null)
                        config.ReversalInfoes = SelectedReversalConfig.ReversalInfoes;
                    BuySellOnSignalSymbolConfigs.Add(config);
                    Events.RaiseAskForStockSubscriptionEvent(tradingSymbol, StockSubscribeMode.LTP, true);
                    _configuredStocksDictionary[tradingSymbol] = config;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(string.Format("Symbol '{0}' is already added to the list. Try adding different symbol!", SelectedInstrument.TradingSymbol), "Duplicate Stock", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Please enter valid director path.", "Invalid Folder Path", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region Event Handlers
        private void Events_InactiveStockPNLChanged(decimal pnl)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (GlobalProfitLossSetting != null)
                {
                    GlobalProfitLossSetting.InactiveDayProfitLoss = pnl;
                }
            });
        }

        private void Events_StockLTPChangedEvent(string tradingSymbol, decimal lastPrice)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (_configuredStocksDictionary.ContainsKey(tradingSymbol))
                {
                    var stock = _configuredStocksDictionary[tradingSymbol];
                    stock.LTP = lastPrice;
                    if (stock.SellValue != 0 || stock.BuyValue != 0)
                    {
                        stock.CurrentProfitLoss = Math.Round(((stock.SellValue - stock.BuyValue) + (stock.NetQuantity.Value * stock.LTP * stock.Multiplier)), 2);
                        if (stock.MaxLoss != 0 && stock.TrailingStopLoss != 0)
                        {
                            if (stock.CurrentProfitLoss - stock.LastTrailPoint > stock.TrailingStopLoss)
                            {
                                stock.MaxLoss -= stock.TrailingStopLoss;
                                stock.LastTrailPoint = stock.CurrentProfitLoss;
                                BuySellSignalOrderManager.Instance.UpdateMaxLoss(stock);
                            }
                        }
                    }
                    BuySellSignalOrderManager.Instance.UpdateCurrentProfitLoss(stock);

                    UpdateDayProfitLoss();
                }
            });
        }

        private void Events_TargetStopLossChangeEvent(string exchange, string symbol, decimal targetPrice, decimal stopLossPrice)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                DebugInfo += "\r\n" + exchange + " " + DateTime.Now;
            });
        }


        private void Events_StatusChangedEvent(string exchange, string symbol, string status)
        {
            StrategyStockStatus strategyStatus = StrategyStockStatus.Added;
            if (BuySellOnSignalSymbolConfigs == null)
                return;
            var stock = BuySellOnSignalSymbolConfigs.FirstOrDefault(s => s.Exchange == exchange && symbol == s.Symbol);

            if (BuySellOnSignalSymbolConfigs != null && Enum.TryParse(status, out strategyStatus))
            {
                if (stock != null)
                {
                    stock.Status = strategyStatus;
                }
            }
            else if(stock != null)
            {
                stock.DebugStatus = status;
            }

        }
        private void Events_PositionUpdateEvent(string tradingSymbol, Position position)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (BuySellOnSignalSymbolConfigs == null)
                    return;
                var stock = BuySellOnSignalSymbolConfigs.FirstOrDefault(s => string.Format("{0}:{1}", s.Exchange, s.Symbol) == tradingSymbol);

                if (stock != null && position.Product == "MIS" && position.Quantity != stock.NetQuantity)
                {
                    stock.CurrentProfitLoss = position.PNL;
                    stock.BuyValue = position.BuyValue;
                    stock.SellValue = position.SellValue;
                    stock.NetQuantity = position.Quantity;
                    stock.Multiplier = position.Multiplier;
                    stock.OpenPosition = position.Quantity;
                }
            });
        }


        #endregion
    }
}
