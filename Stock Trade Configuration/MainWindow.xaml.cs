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
            Kite = KiteInstance.Instance.Kite;
            this.DataContext = new ViewModels.MainWindowViewModel();
            this.Title = "Auto Trading Software (" + userInfo.UserId + ")";
            //var request = kite.PlaceOrder("NFO", "NIFTY18FEBFUT", "BUY", "2", OrderType: "MARKET",  Product: "MIS");
            (ctrlBuySellSignal.DataContext as NotifyPropertyChanged).Kite = kite;
            StockTimeZomeTicker.Instance.Start();
        }
       
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            UpdateStocks stock = new UpdateStocks();
            stock.ShowDialog();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
