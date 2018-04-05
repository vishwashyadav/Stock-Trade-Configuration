using Microsoft.Win32;
using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using StockTradeConfigurationCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StockTradeStrategy.BuySellOnSignal.Views
{
    /// <summary>
    /// Interaction logic for BuySellSignalConfiguration.xaml
    /// </summary>
    [View(Name = "Buy Sell On Signal", Description = "Jobbing Description")]
    public partial class BuySellSignalConfiguration : UserControl
    {
        ViewModels.BuySellSignalConfigurationViewmodel _dataContext;
        public KiteConnect.Kite Kite { get; set; }
        public BuySellSignalConfiguration()
        {
            InitializeComponent();
            _dataContext = this.DataContext as ViewModels.BuySellSignalConfigurationViewmodel;
            LoadSavedFilePath();
        }

        private void BuySellSignalConfiguration_Unloaded(object sender, RoutedEventArgs e)
        {
           // var file = new KeyValue() { Key = txtFolderPath.Name, Value = txtFolderPath.Text };
          //  XSerializer.Instance.SaveConfiguration<object>(txtFolderPath.Name + ".txt", file);
        }

        private void txtFolderPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK )
            {
                txtFolderPath.Text = dialog.SelectedPath;
            }
        }

        private void btnReverConfig_Click(object sender, RoutedEventArgs e)
        {
            ReversalList editor = new ReversalList();
            editor.ShowDialog();
            (this.DataContext as ViewModels.BuySellSignalConfigurationViewmodel).LoadReversalConfigSetting();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BuySellOnSignal.Models.BuySellOnSignalSymbolConfig configItem = (sender as Button).DataContext as BuySellOnSignal.Models.BuySellOnSignalSymbolConfig;
            if (configItem.Status == StockTradeConfiguration.Models.StrategyStockStatus.Running)
            {
                var result = MessageBox.Show("Deleting this item will exit from all open positions. Do you want to continue?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    this._dataContext.DeleteItem((sender as Button).DataContext as BuySellOnSignal.Models.BuySellOnSignalSymbolConfig);
                }
            }
            else
                this._dataContext.DeleteItem((sender as Button).DataContext as BuySellOnSignal.Models.BuySellOnSignalSymbolConfig);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
   
            if (_dataContext.BuySellOnSignalSymbolConfigs != null && _dataContext.BuySellOnSignalSymbolConfigs.Any())
            {
                SaveFileDialog dialog = new SaveFileDialog();
                
                dialog.Filter = "Stock Template Files (*.bss)|*.bss;";
                dialog.ShowDialog();
                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    string fileName = dialog.FileName;
                    XSerializer.Instance.SaveConfiguration<ObservableCollection<Models.BuySellOnSignalSymbolConfig>>(fileName, _dataContext.BuySellOnSignalSymbolConfigs);
                    MessageBox.Show("Saved...");
                }
            }
            else
            {
                MessageBox.Show("At least one stock to be added in configured scrip in order to save as Template.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            bool canLoad = false;
            if (_dataContext.BuySellOnSignalSymbolConfigs != null && _dataContext.BuySellOnSignalSymbolConfigs.Any())
            {
                var result = MessageBox.Show("There is already a list of stocks configured. Loading from saved template will overwrite all currently configured scrip. Do you want to continue?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
                canLoad = result == MessageBoxResult.Yes;
            }
            else
                canLoad = true;
            
            if(canLoad)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Stock Template Files (*.bss)|*.bss;";
                dialog.ShowDialog();
                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    var list = XSerializer.Instance.GetConfiguration<ObservableCollection<Models.BuySellOnSignalSymbolConfig>>(dialog.FileName);
                    _dataContext.BuySellOnSignalSymbolConfigs = list;
                }
            }

        }

        private void LoadSavedFilePath()
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (File.Exists("FilePath.txt"))
                {
                    var keyValue = XSerializer.Instance.GetConfiguration<KeyValue>("FilePath.txt");
                    txtFolderPath.Text = keyValue.Value; 
                }
            });
        }

        private void txtFolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                XSerializer.Instance.SaveConfiguration<KeyValue>("FilePath.txt", new KeyValue() { Key = "FilePath", Value = txtFolderPath.Text});
            });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private void btnPartialSqureOff_Click(object sender, RoutedEventArgs e)
        {
            
            Button btn = sender as Button;
            Models.BuySellOnSignalSymbolConfig config = btn.DataContext as Models.BuySellOnSignalSymbolConfig;

            Predicate<object> validator = (obj) =>
            {
                int count = 0;
                if (string.IsNullOrEmpty(Convert.ToString(obj)))
                {
                    throw new Exception("Number of position should not be empty");
                }
                else if(!int.TryParse(Convert.ToString(obj), out count))
                {
                    throw new Exception("Invalid input");
                }
                else if (int.TryParse(Convert.ToString(obj), out count) && (count > Math.Abs(config.OpenPosition) || count<0))
                {
                    throw new Exception(string.Format("Value should be in range from {0} to {1}",1,Math.Abs(config.OpenPosition)));
                }
                else if (int.TryParse(Convert.ToString(obj), out count) && count ==0)
                {
                    throw new Exception("Value should not be zero");
                }
                return true;
            };
            string title = "No of position you want to square off.";
            UserInputWindow window = new UserInputWindow(title, "No of Position", "Ok", validator);
            window.ShowDialog();
            if(window.Result == MessageBoxResult.OK)
            {
                _dataContext.SquareOffPosition(config, Convert.ToInt32(window.Value));
            }
        }

        private void btnAppSetting_Click(object sender, RoutedEventArgs e)
        {
            SingalParameterSetting setting = new SingalParameterSetting();
            setting.ShowDialog();
            _dataContext.LoadAppSetting();
        }
    }
}
