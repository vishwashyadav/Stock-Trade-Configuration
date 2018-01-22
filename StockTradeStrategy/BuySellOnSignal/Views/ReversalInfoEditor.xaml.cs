using StockTradeStrategy.BuySellOnSignal.Models;
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
using StockTradeConfiguration.Models;
using System.Windows.Forms;

namespace StockTradeStrategy.BuySellOnSignal.Views
{
    /// <summary>
    /// Interaction logic for ReversalConfig.xaml
    /// </summary>
    public partial class ReversalInfoEditor : Window
    {
        ViewModels.ReversalInfoEditorViewModel viewModel;
        public DialogResult Result { get; set; }
        public ReversalInfoEditor(ReversalConfig config)
        {
            InitializeComponent();
            viewModel = this.DataContext as ViewModels.ReversalInfoEditorViewModel;
            viewModel.IsEditMode = (config != null);
            viewModel.SelectedReversalConfig = config ==null ? new ReversalConfig() { Key = Guid.NewGuid() } : config.DeepCopy<ReversalConfig>();
            viewModel.CloseWindowAction = CloseWindow;
        }


        public void CloseWindow()
        {
            Result = viewModel.Result;
            this.Close();
        }


    }
}
