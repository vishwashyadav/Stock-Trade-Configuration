using KiteConnect;
using Stock_Trade_Configuration.Singleton;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            dynamic activeX = this.webBrowser.GetType().InvokeMember("ActiveXInstance",
                   BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                   null, this.webBrowser, new object[] { });

            if (activeX != null)
                activeX.Silent = true;

            webBrowser.Navigated += WebBrowser_Navigated;
            
            KiteInstance.Instance.SetInfo(_userInfo);
            var url = KiteInstance.Instance.GetLoginURL();
            webBrowser.Navigate(new Uri(url));
         }

        private void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //UpstoxNet.Upstox up = new UpstoxNet.Upstox();
            //up.Api_Key = "jxZSWJP6YU3BlxUyR02Y89OPkenca2Rk5FO25wvG";
            //up.Api_Secret = "3vw2gzqbt3";
            
           // up.Login();
            string requestToke = "request_token";
            if (e.Uri.ToString().Contains(requestToke))
            {
                var split = e.Uri.ToString().Split(new char[] { '&' });
                var str = split.FirstOrDefault(s => s.Contains(requestToke));
                RequestToken = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            
           // else
               // webBrowser.Navigate(e.Uri);
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
