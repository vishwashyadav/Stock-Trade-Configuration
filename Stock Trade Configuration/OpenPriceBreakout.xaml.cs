using StockTradeConfigurationCommon;
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
    /// Interaction logic for OpenPriceBreakout.xaml
    /// </summary>
    [View(Name = "Open Price Breakout", Description = "Jobbing Description")]
    public partial class OpenPriceBreakout : UserControl
    {
        public OpenPriceBreakout()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as ViewModels.PreOpenPriceBreakoutViewModel).SearchStocks();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as ViewModels.PreOpenPriceBreakoutViewModel).OrderStockWhenItBreaksRange();
        }
    }
}
