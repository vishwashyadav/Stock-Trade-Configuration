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
    /// Interaction logic for ReversalList.xaml
    /// </summary>
    public partial class ReversalList : Window
    {
        ViewModels.ReversalListViewmodel viewmodel;
        public ReversalList()
        {
            InitializeComponent();
            viewmodel = this.DataContext as ViewModels.ReversalListViewmodel;
            viewmodel.AddCommandAction = AddConfig;
            viewmodel.EditCommandAction = EditConfig;
        }

        public void AddConfig()
        {
            ReversalInfoEditor editor = new ReversalInfoEditor(null);
            editor.ShowDialog();
            RefreshConfig(editor);
        }
        public void RefreshConfig(ReversalInfoEditor editor)
        {
            if(editor.Result == System.Windows.Forms.DialogResult.Yes)
            {
                viewmodel.LoadReversalConfigs();
            }
        }
        public void EditConfig()
        {
            ReversalInfoEditor editor = new ReversalInfoEditor(viewmodel.SelectedReversalConfig);
            editor.ShowDialog();
            RefreshConfig(editor);
        }
    }
}
