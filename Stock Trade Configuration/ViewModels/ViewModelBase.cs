using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using StockTradeConfiguration.Data;
using Prism.Commands;
using System.Configuration;
using Stock_Trade_Configuration.Singleton;
using StockTradeConfiguration.Models;

namespace Stock_Trade_Configuration.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public StockTradeConfiguration.Models.DelegateCommand SaveCommand { get; set; }
        public ApiClient CurrentApiClient { get; set; }
        public KiteInstance KiteInstance { get; set; }
        public XSerializer Serializer { get; set; }
        public SettingValues SettingValues { get; set; }
        
        public ViewModelBase()
        {
            Serializer = XSerializer.Instance;
            SettingValues = SettingValues.Instance;
            KiteInstance = KiteInstance.Instance;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
       
    }
}
