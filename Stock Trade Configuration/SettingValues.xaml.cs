using StockTradeConfiguration.Models;
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

namespace Stock_Trade_Configuration
{
    /// <summary>
    /// Interaction logic for SettingValues.xaml
    /// </summary>
    public partial class SettingValues : Window
    {
        private ObservableCollection<KeyValue> _settingKeyValues;
        public ObservableCollection<KeyValue> SettingKeyValues
        {
            get { return _settingKeyValues; }
            set { _settingKeyValues = value; }
        }
        public SettingValues()
        {
            InitializeComponent();
            InitializeSettingKeyValues();
            this.DataContext = this;
        }

        private void InitializeSettingKeyValues()
        {
            var settingValues = ViewModels.SettingValues.Instance;
            var properties = settingValues.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            SettingKeyValues = new ObservableCollection<KeyValue>();
            foreach (var item in properties)
            {
                SettingKeyValues.Add(new KeyValue()
                {
                    Key = item.Name,
                    Value = item.GetValue(settingValues)
                });
            }
        }

        private void Save()
        {
            var settingValues = ViewModels.SettingValues.Instance;
            var properties = settingValues.GetType().GetProperties( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var item in properties)
            {
                var keyvalue = SettingKeyValues.FirstOrDefault(s => s.Key == item.Name);
                if (keyvalue != null)
                {
                    try
                    {
                        object val = null;
                        if(item.PropertyType == typeof(TimeSpan))
                        {
                            val = TimeSpan.Parse(keyvalue.Value);
                        }
                        else
                        val = Convert.ChangeType(keyvalue.Value, item.PropertyType);
                        item.SetValue(settingValues, val);
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
            }

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
            MessageBox.Show("Saved...");
        }
    }
}
