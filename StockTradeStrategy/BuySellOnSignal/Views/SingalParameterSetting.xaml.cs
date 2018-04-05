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

namespace StockTradeStrategy.BuySellOnSignal.Views
{
    /// <summary>
    /// Interaction logic for SingalParameterSetting.xaml
    /// </summary>
    public partial class SingalParameterSetting : Window
    {
        public SingalParameterSetting()
        {
            InitializeComponent();
            (this.DataContext as ViewModels.SignalparameterSettingViewmodel).CloseWindowAction = CloseWindow;
        }

        private void CloseWindow()
        {
            this.Close();
        }
    }
}
