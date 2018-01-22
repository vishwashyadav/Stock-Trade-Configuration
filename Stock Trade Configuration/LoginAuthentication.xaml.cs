using KiteConnect;
using Stock_Trade_Configuration.Singleton;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Stock_Trade_Configuration
{
    /// <summary>
    /// Interaction logic for LoginAuthentication.xaml
    /// </summary>
    public partial class LoginAuthentication : Window
    {
        UserInfo _userInfo;
        public string RequestToken = string.Empty;
        Kite kite;
        public LoginAuthentication(UserInfo userInfo)
        {
            _userInfo = userInfo;
            InitializeComponent();
            this.Loaded += LoginAuthentication_Loaded;
           
        }

        private void LoginAuthentication_Loaded(object sender, RoutedEventArgs e)
        {
            KiteInstance.Instance.SetInfo(_userInfo);
            var url = KiteInstance.Instance.GetLoginURL();
            webBrowser.Navigate(new Uri(url));
            webBrowser.Navigated += WebBrowser_Navigated;
        }

        private void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //UpstoxNet.Upstox up = new UpstoxNet.Upstox();
            //up.Api_Key = "jxZSWJP6YU3BlxUyR02Y89OPkenca2Rk5FO25wvG";
            //up.Api_Secret = "3vw2gzqbt3";
            
           // up.Login();
            string requestToke = "request_token";
            if (e.Uri.ToString().Contains(requestToke))
                RequestToken =  e.Uri.ToString().Split(new string[] { requestToke + "=" }, StringSplitOptions.RemoveEmptyEntries)[1];
            if(!string.IsNullOrEmpty(RequestToken))
            {
                try
                {
                    KiteInstance.Instance.Login(RequestToken);
                    MainWindow window = new MainWindow(_userInfo);
                    window.Show();
                    this.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void webBrowser_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }
    }
}
