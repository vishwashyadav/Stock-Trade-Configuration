using Stock_Trade_Configuration.ViewModels;

using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
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

namespace Stock_Trade_Configuration
{
    /// <summary>
    /// Interaction logic for StockWatch.xaml
    /// </summary>
    public partial class StockWatch : UserControl
    {
        ViewModels.StockWatchViewModel _viewModel;
        public StockWatch()
        {
            InitializeComponent();
            _viewModel = this.DataContext as ViewModels.StockWatchViewModel;
            this.Loaded += StockWatch_Loaded;
            this.Unloaded += StockWatch_Unloaded;
        }

        private void StockWatch_Unloaded(object sender, RoutedEventArgs e)
        {
            Events.StockHighLowChangeEvent -= _viewModel.UpdateStockSymbolHighLow;
        }

        private void StockWatch_Loaded(object sender, RoutedEventArgs e)
        {
            Events.StockHighLowChangeEvent += _viewModel.UpdateStockSymbolHighLow;
        }
       

        private void SearchStockBtn_Click(object sender, RoutedEventArgs e)
        {
            AddStocks();
        }

        public void AddStocks(bool isAppend = false)
        {
            var symbols = PreOpenStockPicker.GetStockSymbols(ViewModels.ConfigurationFileNames.PreOpenStockFileName, ViewModels.ConfigurationFileNames.TextToReplaceInPreOpenTextFile, ViewModels.ConfigurationFileNames.GapUpDownPercentageAtIndex,1,ViewModels.SettingValues.Instance.GapUpDownPickMin,ViewModels.SettingValues.Instance.GapUpDownPickMax,ViewModels.SettingValues.Instance.PicKStockWhosePriceMin,ViewModels.SettingValues.Instance.PicKStockWhosePriceMax);
                _viewModel.AddStockSymbols(symbols,isAppend);
        }

        private void SearchStockAndAppendBtn_Click(object sender, RoutedEventArgs e)
        {
            AddStocks(true);
        }
    }
}
