using StockTrade.Jobbing;
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
    /// Interaction logic for JobbingConfiguration.xaml
    /// </summary>
    [View(Name ="Jobbing", Description ="Jobbing Description")]
    public partial class JobbingConfiguration : UserControl
    {
        ViewModels.JobbingConfigurationViewModel _dataContext;
        public JobbingConfiguration()
        {
            InitializeComponent();
            _dataContext = this.DataContext as ViewModels.JobbingConfigurationViewModel;
        }

        private void btnAddStock_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.AddStockForJobbing();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if(control!=null)
            {
                _dataContext.Start(control.DataContext as JobbingStockBase);
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        { 
            Hyperlink control = sender as Hyperlink;
            if (control != null)
            {
                var st = control.DataContext as JobbingStockBase;
                var info = JobbingDataSaver.GetData(st.Exchange, st.Symbol, DateTime.Now, st.SaveDirectoryName);
                JobbingTransactionHistory history = new JobbingTransactionHistory(info);
                history.ShowDialog();
            }
        }
    }
}
