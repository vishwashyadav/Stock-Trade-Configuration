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
using System.Windows.Shapes;

namespace Stock_Trade_Configuration
{
    /// <summary>
    /// Interaction logic for JobbingTransactionHistory.xaml
    /// </summary>
    public partial class JobbingTransactionHistory : Window
    {
        public JobbingTransactionHistory(List<JobbingStockDataInfo> stocks)
        {
            InitializeComponent();
            lstView.ItemsSource = stocks;
        }
    }
}
