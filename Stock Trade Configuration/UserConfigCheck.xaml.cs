using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for UserConfigCheck.xaml
    /// </summary>
    public partial class UserConfigCheck : Window
    {
        string invalidLicense = "Key is either invalid or expired";
        string validLicense = "Key is valid till {0}";
        public UserConfigCheck()
        {
            InitializeComponent();
            this.Loaded += UserConfigCheck_Loaded;
        }

        private void UserConfigCheck_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(ConfigurationFileNames.UserSpecificFile))
            {
                var userConfig = XSerializer.Instance.GetConfiguration<StockTradeConfiguration.Models.UserInfo>(ConfigurationFileNames.UserSpecificFile);
                txtSecret.Text = userConfig.SecretKey;
                txtUserId.Text = userConfig.UserId;
                txtApi.Text = userConfig.APIKey;
                txtKey.Text = userConfig.Key;
            }
        }

        private void txtKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateLicenseKey(GetUserConfig());
         }

        private void ValidateLicenseKey(UserInfo userInfo)
        {
            bool isActiveLicense = false;
            DateTime validTill = default(DateTime);
            if (!string.IsNullOrEmpty(userInfo.UserId))
            {
                try
                {
                    isActiveLicense = SerialKeyManager.IsLicenseActive(userInfo.Key, userInfo.UserId, DateTime.Now, out validTill);
                }
                catch (Exception)
                {
                    isActiveLicense = false;
                }
            }
            if (isActiveLicense)
            {
                userInfo.IsValidKey = true;
                txtinfo.Text = string.Format(validLicense, validTill.ToShortDateString());
                txtinfo.Foreground = Brushes.Green;
            }
            else
            {
                userInfo.IsValidKey = false;
                txtinfo.Text = invalidLicense;
                txtinfo.Foreground = Brushes.Red;
            }

        }

        private UserInfo GetUserConfig()
        {
            UserInfo userInfo = new UserInfo();
            userInfo.APIKey = txtApi.Text;
            userInfo.Key = txtKey.Text;
            userInfo.SecretKey = txtSecret.Text;
            userInfo.UserId = txtUserId.Text;
            return userInfo;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var userInfo = GetUserConfig();
            ValidateLicenseKey(userInfo);
            if(userInfo.IsValidKey)
            {
                XSerializer.Instance.SaveConfiguration<UserInfo>(ConfigurationFileNames.UserSpecificFile,userInfo);
                LoginAuthentication window = new LoginAuthentication(userInfo);
                window.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please update valid Key");
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
