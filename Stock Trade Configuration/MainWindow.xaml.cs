using KiteConnect;
using Microsoft.Win32;
using Stock_Trade_Configuration.Singleton;
using Stock_Trade_Configuration.ViewModels;

using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Stock_Trade_Configuration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        
        Dictionary<string, List<string>> _stocksByExchanges = new Dictionary<string, List<string>>();
        ViewModels.StockWatchViewModel _stockWatchViewModel;
        DispatcherTimer timer;
        // instances of Kite and Ticker
        Kite kite;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Kite Kite
        {
            get { return kite; }
            set { kite = value; OnPropertyChanged("Kite"); }
        }
        public MainWindow(UserInfo userInfo)
        {
            InitializeComponent();
            txtAPIKey.Text = userInfo.APIKey;
            txtSecretKey.Text = userInfo.SecretKey;
            txtUserId.Text = userInfo.UserId;
            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
            Kite = KiteInstance.Instance.Kite;
            Events.OpenHighStockListChangedEvent += Events_OpenHighStockListChangedEvent;
            FileWatcher.Instance.StartWatch();
            this.DataContext = new ViewModels.MainWindowViewModel();
            this.Title = "Auto Trading Software (" + userInfo.UserId + ")";
            //var request = kite.PlaceOrder("NFO", "NIFTY18FEBFUT", "BUY", "2", OrderType: "MARKET",  Product: "MIS");
            (ctrlBuySellSignal.DataContext as NotifyPropertyChanged).Kite = kite;
        }

        private void Events_OpenHighStockListChangedEvent(List<OpenHighLowData> stockList)
        {
           // Application.Current.Dispatcher.Invoke(()=> { openHighLowGrid.ItemsSource = null; openHighLowGrid.ItemsSource = stockList;  });
        }
        
        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            FileWatcher.Instance.StopWatch();
            ResetTimer();
        }

        private static void ResetTimer()
        {
            StockBreakOutTicker.Instance.Stop();
            StockOrderTicker.Instance.Stop();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Events.TimeZoneChangedEvent += Events_TimeZoneChangedEvent;
        }

        private void Events_TimeZoneChangedEvent(StockTimeZone timeZone)
        {
            ResetTimer();
            switch(timeZone)
            {
                case StockTimeZone.HighLowWatch:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                      //  (viewOpenPriceBreakout.DataContext as ViewModels.PreOpenPriceBreakoutViewModel).OrderStockWhenItBreaksRange();

                    }));
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        SaveStockSymbols();
                    }));
                    var stockSymbols = GetAllStocksToWatch();
                    if (stockSymbols.Any())
                        StockBreakOutTicker.Instance.Start(new TimeSpan(1000), KiteInstance.Instance.Kite, stockSymbols.Where(s => s.StockSymbolStatus == StockSymbolStatus.Valid));
                   
                    break;
                case StockTimeZone.TradeTime:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        SaveAndGetStockBreakOutConfiguration();
                    }));
                    var configurations = GetAllBreakOutConfigurations();
                        StockOrderTicker.Instance.Start(new TimeSpan(1000), kite, configurations);
                    break;
            }
        }
       
        private void save_Click(object sender, RoutedEventArgs e)
        {
            var content = (tabControl.SelectedContent as FrameworkElement).DataContext as ViewModels.ViewModelBase;
            if(content!= null)
            {
                content.SaveCommand.Execute(null);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StockTimeZomeTicker.Instance.Start();
            MessageBox.Show("Started...");
            //var symbols = SaveAndGetStockBreakOutConfiguration();
            //StockOrderTicker.Instance.Start(new TimeSpan(1000), kite, symbols);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            
        }

        private IEnumerable<StockSymbols> ValidateSymbols()
        {
            var stockSymbols = (stockWatch.DataContext as ViewModels.StockWatchViewModel).StockSymbols;
            foreach (var item in stockSymbols)
            {
                try
                {
                    var ltp = kite.GetLTP(new string[] { item.DisplayName });
                    item.StockSymbolStatus = StockSymbolStatus.Valid;
                }
                catch (Exception ex)
                {
                    item.StockSymbolStatus = StockSymbolStatus.InValid;
                }
            }
            var stockSymbolsInChunk = GetStockSymbolsInChunk(stockSymbols.Where(s => s.StockSymbolStatus == StockSymbolStatus.Valid), 50);
            //Assign High Low
            foreach (var item in stockSymbolsInChunk)
            {
                try
                {
                    var highLowItems = kite.GetOHLC(item.ToArray());
                    foreach (var highLowItem in highLowItems)
                    {
                        var stock = stockSymbols.FirstOrDefault(s => s.DisplayName == highLowItem.Key);
                        if (stock != null)
                        {
                            stock.StockBreakOutBuyPrice = highLowItem.Value.High;
                            stock.StockBreakOutSellPrice = highLowItem.Value.Low;
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return stockSymbols;
        }

        private List<List<string>> GetStockSymbolsInChunk(IEnumerable<StockSymbols> symbols, int chunkSize)
        {
         
            List<List<string>> stockSymbolsList = new List<List<string>>();
            for (int i = 0; i < (symbols.Count() / chunkSize) + 1; i++)
            {
                var tempsymbols = symbols.Skip((i * chunkSize)).Take(chunkSize);
                stockSymbolsList.Add(new List<string>(symbols.Select(s => s.DisplayName)));
            }
            return stockSymbolsList;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var stockSymbols = ValidateSymbols();  
            StockBreakOutTicker.Instance.Start(new TimeSpan(2000), kite, stockSymbols.Where(s => s.StockSymbolStatus == StockSymbolStatus.Valid));

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ValidateSymbols();
        }

        private void SaveStockSymbols()
        {
            //stockWatch.AddStocks();
            //ValidateSymbols();
            //(stockWatch.DataContext as ViewModels.StockWatchViewModel).SaveCommand.Execute(null);
        }

        private void SaveAndGetStockBreakOutConfiguration()
        {
            int quantity = 1;
            (stockWatch.DataContext as ViewModels.StockWatchViewModel).SaveCommand.Execute(null);
            var stockSymbols = (stockWatch.DataContext as ViewModels.StockWatchViewModel).StockSymbols;
            var configurations = new List<RangeBreakOutConfiguration>();
            foreach (var item in stockSymbols)
            {
                var margin = item.StockBreakOutBuyPrice > 1000 ? 0.2m : 0.3m;

                RangeBreakOutConfiguration buy = new RangeBreakOutConfiguration()
                {
                    BreakOutPrice = item.StockBreakOutBuyPrice,
                    MarginType = MarginType.Percentage,
                    Margin = margin,
                    BuySellPrice = item.StockBreakOutBuyPrice * (1 + (margin / 100.0m)),
                    OrderCondition = StockTradeConfiguration.Models.Condition.GreaterThanEqualTo,
                    OrderMode = OrderMode.BUY,
                    OrderType = OrderType.BracketOrder,
                    Quantity = quantity,
                    StockSymbol = item.DisplayName,
                    StopLoss = Math.Abs(item.StockBreakOutBuyPrice - item.StockBreakOutSellPrice)
                };

                RangeBreakOutConfiguration sell = new RangeBreakOutConfiguration()
                {
                    BreakOutPrice = item.StockBreakOutSellPrice,
                    MarginType = MarginType.Percentage,
                    Margin = margin,
                    BuySellPrice = item.StockBreakOutSellPrice * (1 - (margin / 100.0m)),
                    OrderCondition = StockTradeConfiguration.Models.Condition.LessThanEqualTo,
                    OrderMode = OrderMode.SELL,
                    OrderType = OrderType.BracketOrder,
                    Quantity = quantity,
                    StockSymbol = item.DisplayName,
                    StopLoss = Math.Abs(item.StockBreakOutBuyPrice - item.StockBreakOutSellPrice)
                };

                buy.UpdateSummary();
                sell.UpdateSummary();
                configurations.Add(buy);
                configurations.Add(sell);
            }
            XSerializer.Instance.SaveConfiguration<List<RangeBreakOutConfiguration>>(ViewModels.ConfigurationFileNames.RangeBreakOutOrderConfigurationFileName, configurations);
        }

        private List<RangeBreakOutConfiguration> GetAllBreakOutConfigurations()
        {
            return XSerializer.Instance.GetConfiguration<List<RangeBreakOutConfiguration>>(ViewModels.ConfigurationFileNames.RangeBreakOutOrderConfigurationFileName);
        }

        private List<StockSymbols> GetAllStocksToWatch()
        {
            return XSerializer.Instance.GetConfiguration<List<StockSymbols>>(ViewModels.ConfigurationFileNames.StockWatchFileName);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            StockTimeZomeTicker.Instance.Start();
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            new SettingValues().ShowDialog();
        }

        private void GetAndSaveAllInstruments()
        {
            //var instruments = kite.GetInstruments("NFO");
            //XSerializer.Instance.SaveConfiguration<List<Instrument>>("NFO.Config", instruments);
        }

        private async void btnLoadAllInstruments_Click(object sender, RoutedEventArgs e)
        {
            btnLoadAllInstruments.IsEnabled = false;
            await Task.Factory.StartNew(GetAndSaveAllInstruments);
            btnLoadAllInstruments.IsEnabled = true;
        }

        private void btnWatchHighLow_Click(object sender, RoutedEventArgs e)
        {
            var instruments = XSerializer.Instance.GetConfiguration<List<string>>(ConfigurationFileNames.ValidStocks);
            Tickers.OpenHighLowTicker.Instance.Start(new TimeSpan(1000), kite, instruments);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
            StockTimeZomeTicker.Instance.Stop();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            UpdateStocks stock = new UpdateStocks();
            stock.ShowDialog();
        }
    }
}
