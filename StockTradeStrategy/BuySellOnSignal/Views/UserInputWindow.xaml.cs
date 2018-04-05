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
    /// Interaction logic for UserInputWindow.xaml
    /// </summary>
    public partial class UserInputWindow : Window
    {
        public string ContentMessage { get; set; }
        public string OkButtonText { get; set; }
        public string WindowTitle { get; set; }
        public object Value { get; set; }
        public MessageBoxResult Result { get; set; }
        Predicate<object> ValidateFunc { get; set; }
        public UserInputWindow(string contentMessage, string title, string okButtonText,Predicate<object> validateObject)
        {
            InitializeComponent();
            ValidateFunc = validateObject;
            ContentMessage = contentMessage;
            WindowTitle = title;
            OkButtonText = okButtonText;
            this.DataContext = this;
            okButton.Focus();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.PreviewKeyDown += GenericWindow_PreviewKeyDown;
            okButton.IsEnabled = false;
            okButton.Click += OkButton_Click;
            txtValue.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            this.Close();
        }

        private void GenericWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if ((e.Key == Key.Enter || e.Key == Key.Return) && okButton.IsEnabled)
            {
                OkButton_Click(null, null);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string errorMessage = string.Empty;
            try
            {
                okButton.IsEnabled = ValidateFunc.Invoke((sender as TextBox).Text);
                txtError.Text = string.Empty;
            }
            catch(Exception ex)
            {
                okButton.IsEnabled = false;
                txtError.Text = ex.Message;
            }
        } 
        
    }
}
