using Microsoft.Win32;
using StockTradeConfiguration.Data;
using StockTradeConfigurationCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    }
}
