using StockTradeConfiguration.Data;
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

namespace KeyGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            txtKey.Text = SerialKeyManager.GetKey(txtUserId.Text, dtStart.SelectedDate.Value, dtTo.SelectedDate.Value);
            var isLicenseActive = SerialKeyManager.IsLicenseActive(txtKey.Text, txtUserId.Text,DateTime.Now, out dt);
        }
    }
}
