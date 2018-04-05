using StockTradeConfiguration.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Stock_Trade_Configuration
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            DateTime dateTime = DateTime.Now;
            bool isValidLicense = false;
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    MainWindow window = new Stock_Trade_Configuration.MainWindow(null);
            //    window.Show();
            //});
            //return;
            //if (File.Exists(ViewModels.ConfigurationFileNames.UserSpecificFile))
            //{
            //    try
            //    {
            //        var userConfig = XSerializer.Instance.GetConfiguration<StockTradeConfiguration.Models.UserInfo>(ViewModels.ConfigurationFileNames.UserSpecificFile);
            //        isValidLicense = SerialKeyManager.IsLicenseActive(userConfig.Key, userConfig.UserId, DateTime.Now, out dateTime);
            //        if (isValidLicense)
            //        {
            //            Application.Current.Dispatcher.Invoke(() =>
            //            {
            //                LoginAuthentication mainWindow = new LoginAuthentication(userConfig);
            //                mainWindow.Show();
            //            });
            //        }
            //    }
            //    catch (Exception)
            //    {
                    
            //    }
            //}

            if (!isValidLicense)
            {
                UserConfigCheck config = new UserConfigCheck();
                config.Show();
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
           

           
        }
    }
}
