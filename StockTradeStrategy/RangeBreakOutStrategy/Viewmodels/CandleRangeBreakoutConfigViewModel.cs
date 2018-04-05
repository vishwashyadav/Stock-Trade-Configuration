using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using StockTradeStrategy.RangeBreakOutStrategy.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockTradeStrategy.RangeBreakOutStrategy.Viewmodels
{
    public class CandleRangeBreakoutConfigViewModel : NotifyPropertyChanged
    {
        private const string BreakOutTimeZone = "BreakOutTimeZone";
        
        #region private variables
        Dictionary<string, RangeBreakOutStockConfig> _stockDictionary;
        private PreOpenStockSearchParameter _preOpenStockSearchParameter;
        private ObservableCollection<RangeBreakOutStockConfig> _stocks;
        private KeyValuePair<string, int> _selectedCandleDuration;
        #endregion

        #region public properties
        public KeyValuePair<string,int> SelectedCandleDuration
        {
            get { return _selectedCandleDuration; }
            set { _selectedCandleDuration = value; OnPropertyChanged("SelectedCandleDuration"); }
        }
        public List<KeyValuePair<string,int>> CandleDuration
        {
            get
            {
                return new List<KeyValuePair<string, int>>()
                {
                    new KeyValuePair<string, int>("1Minute",1),
                    new KeyValuePair<string, int>("5Minute",5),
                    new KeyValuePair<string, int>("15Minute",15),
                };
            }
        }

        public PreOpenStockSearchParameter PreOpenStockSearchParameter
        {
            get { return _preOpenStockSearchParameter; }
            set { _preOpenStockSearchParameter = value; OnPropertyChanged("PreOpenStockSearchParameter"); }
        }
        public DelegateCommand SearchStockCommand { get; set; }
        public DelegateCommand StartCommand { get; set; }
        public ObservableCollection<RangeBreakOutStockConfig> Stocks
        {
            get { return _stocks; }
            set { _stocks = value; OnPropertyChanged("Stocks"); }
        }
        #endregion

        #region constructor
        public CandleRangeBreakoutConfigViewModel()
        {
            SubscribeCommand();
            PreOpenStockSearchParameter = new PreOpenStockSearchParameter();
            Events.StockHighLowChangeEvent += Events_StockHighLowChangeEvent;
            Events.StockLTPChangedEvent += Events_StockLTPChangedEvent;
            Events.UpdateObjectEvent += Events_UpdateObjectEvent;
            Events.StatusChangedEvent += Events_StatusChangedEvent;
        }

        #endregion

        #region Command Implementation
        private void Events_UpdateObjectEvent(object obj)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                RangeBreakOutStockConfig config = obj as RangeBreakOutStockConfig;
                if(config!=null)
                {
                    if (Stocks == null)
                        return;
                    var localConfig = Stocks.FirstOrDefault(s => s.Exchange == config.Exchange && s.Symbol == config.Symbol);
                    if(localConfig!=null)
                    {
                        localConfig.TargetPrice = config.TargetPrice;
                        localConfig.OrderStatus = config.OrderStatus;
                        localConfig.ParentOrderId = config.ParentOrderId;
                        localConfig.TargetOrderId = config.TargetOrderId;
                        localConfig.StopLoss = config.StopLoss;
                        localConfig.LastOrderedQuantity = config.LastOrderedQuantity;
                    }
                }
            });
        }
        private void StartCommandExecute(object obj)
        {
            _stockDictionary = Stocks.ToDictionary(s => s.TradingSymbol, v => v);

            var instance = RangeBreakOutManager.Instance;
            var startTime = new TimeSpan(9,14,0);
            var min = startTime.Minutes + (SelectedCandleDuration.Value * PreOpenStockSearchParameter.NumberOfCandle);
            //var endTime = new TimeSpan(startTime.Hours + (min/60),(min > 60) ? (min%60):min , 0);
            var endTime = new TimeSpan(9, 30, 0);

            Events.RaiseSubscribeTimeZoneEvent(BreakOutTimeZone, startTime, endTime);
            Events.TimeZoneChangedEvent += Events_TimeZoneChangedEvent;
        }

        private void Events_TimeZoneChangedEvent(string timeZone, TimeZoneStatus status)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (timeZone == BreakOutTimeZone)
                {
                    if (status == TimeZoneStatus.Started)
                    {
                        var dictionary = Stocks.ToDictionary(s => s.TradingSymbol, val => val.PreOpenPrice);
                        StockHighLowWatchManager.Instance.WatchStocksForHighLow(dictionary);
                    }
                    else if (status == TimeZoneStatus.Stopped)
                    {
                        Stocks = new ObservableCollection<RangeBreakOutStockConfig>( Stocks.Where(s => s.ChangePercentage <= 1.5m));
                        StockHighLowWatchManager.Instance.Stop();
                        RangeBreakOutManager.Instance.WatchStocksForRangeBreakout(Stocks);
                    }
                }
            });
        }

        private void SearchStockCommandExecute(object obj)
        {
            var symbols = PreOpenStockPicker.GetStockSymbolsWithNSEOpenPrice(ConfigurationFileNames.PreOpenStockFileName, ConfigurationFileNames.TextToReplaceInPreOpenTextFile, ConfigurationFileNames.GapUpDownPercentageAtIndex, 1,PreOpenStockSearchParameter.MinGapupPercentage, PreOpenStockSearchParameter.MaxGapupPercentage, PreOpenStockSearchParameter.MinStockPrice,PreOpenStockSearchParameter.MaxStockPrice);
            Stocks = new ObservableCollection<RangeBreakOutStockConfig>( symbols.Select(s =>
            new RangeBreakOutStockConfig()
            {
                Exchange="NSE",
                Symbol = s.Key,
                InitialQuantity = PreOpenStockSearchParameter.InitialQuantity,
                Multiplier = PreOpenStockSearchParameter.SelectedMultiplier,
                PreOpenPrice = s.Value,
                OrderType = Models.OrderType.CoverAndBracketOrder,
                ProfitMargin = PreOpenStockSearchParameter.ProfitMarginPercentage,
                TradingSymbol = string.Format("{0}:{1}","NSE",s.Key)
            }));
            _stockDictionary = Stocks.ToDictionary(s => s.TradingSymbol, v => v);

        }

        #endregion

        #region private methods
        private void Events_StatusChangedEvent(string exchange, string symbol, string status)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Models.OrderStatus orderStatus = Models.OrderStatus.None;
                if (Enum.TryParse(status, out orderStatus))
                {
                    string tradingSymbol = !string.IsNullOrEmpty(exchange) ? string.Format("{0}:{1}", exchange, symbol) : symbol;
                    if (_stockDictionary.ContainsKey(tradingSymbol) && _stockDictionary[tradingSymbol].OrderStatus != orderStatus)
                    {
                        var stock = _stockDictionary[tradingSymbol];
                        stock.OrderStatus = orderStatus;
                    }
                }
            });
        }
        
        private void Events_StockLTPChangedEvent(string tradingSymbol, decimal lastPrice)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (_stockDictionary == null)
                    _stockDictionary = new Dictionary<string, RangeBreakOutStockConfig>();
                if (_stockDictionary.ContainsKey(tradingSymbol) && _stockDictionary[tradingSymbol].Price != lastPrice)
                {
                    var stock = _stockDictionary[tradingSymbol];
                    stock.LTP = lastPrice;
                    stock.Price = lastPrice;
                }
            });
        }

        private void SubscribeCommand()
        {
            SearchStockCommand = new DelegateCommand(SearchStockCommandExecute);
            StartCommand = new DelegateCommand(StartCommandExecute);
        }
        
        private void Events_StockHighLowChangeEvent(KeyValuePair<string, Tuple<decimal, decimal>> stockInfo)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
          
                if (_stockDictionary.ContainsKey(stockInfo.Key))
                {
                    var stock = _stockDictionary[stockInfo.Key];
                    stock.BuyBreakOutPrice =  Math.Max(stockInfo.Value.Item1, stock.BuyBreakOutPrice);
                    stock.SellBreakOutPrice = stockInfo.Value.Item2;
                    try
                    {
                        if (stock.OrderStatus != Models.OrderStatus.BuyOrderInProgress && stock.OrderStatus != Models.OrderStatus.SellOrderInProgress)
                        {
                            stock.ChangePercentage = Math.Round(((((stock.BuyBreakOutPrice - stock.SellBreakOutPrice) / stock.SellBreakOutPrice) * 100.0m)), 2);
                            stock.ProfitMargin = (stock.ChangePercentage / 2) < 0.5m ? 0.5m : (stock.ChangePercentage / 2);
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            });
        }
       
        #endregion
    }

    public class PreOpenStockSearchParameter : NotifyPropertyChanged
    {
        #region private variables
        private decimal _minStockPrice;
        private decimal _maxStockPrice;
        private decimal _minGapupPercentage;
        private decimal _maxGapupPercentage;
        private string _candleDuration;
        private int _numberOfCandle;
        private int _initialQuantity;
        private decimal _prorfitMarginPercentage;
        private int _selectedMultiplier;
        #endregion

        #region public properties
        public int InitialQuantity
        {
            get { return _initialQuantity; }
            set { _initialQuantity = value; OnPropertyChanged("InitialQuantity"); }
        }
        public decimal ProfitMarginPercentage
        {
            get { return _prorfitMarginPercentage; }
            set { _prorfitMarginPercentage = value; OnPropertyChanged("ProfitMarginPercentage"); }
        }
        public string CandleDuration
        {
            get { return _candleDuration; }
            set { _candleDuration = value; OnPropertyChanged("CandleDuration"); }
        }
        public int NumberOfCandle
        {
            get { return _numberOfCandle; }
            set { _numberOfCandle = value; OnPropertyChanged("NumberOfCandle"); }
        }
        public decimal MinStockPrice
        {
            get { return _minStockPrice; }
            set { _minStockPrice = value; OnPropertyChanged("MinStockPrice"); }
        }

        public decimal MaxStockPrice
        {
            get { return _maxStockPrice; }
            set { _maxStockPrice = value; OnPropertyChanged("MaxStockPrice"); }
        }

        public decimal MinGapupPercentage
        {
            get { return _minGapupPercentage; }
            set { _minGapupPercentage = value; OnPropertyChanged("MinGapupPercentage"); }
        }

        public decimal MaxGapupPercentage
        {
            get { return _maxGapupPercentage; }
            set { _maxGapupPercentage = value; OnPropertyChanged("MaxGapupPercentage"); }
        }

        public List<int> Multipliers
        {
            get { return new List<int>() { 1, 2, 3,4,5 }; }
        }
        public int SelectedMultiplier
        {
            get { return _selectedMultiplier; }
            set { _selectedMultiplier = value; OnPropertyChanged("SelectedMultiplier"); }
        }
        #endregion

        #region ctor
        public PreOpenStockSearchParameter()
        {
            ProfitMarginPercentage = 0.5m;
            MinGapupPercentage = 1.0m;
            MinStockPrice = 80.0m;
            MaxGapupPercentage = 4.0m;
            MaxStockPrice = 1800.0m;
            NumberOfCandle = 1;
            InitialQuantity = 1;
            SelectedMultiplier = 3;
        }
        #endregion
    }
}
