using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using StockTradeConfiguration.Models;
using KiteConnect;

namespace Stock_Trade_Configuration.ViewModels
{
    public class RangeBreakOutOrderConfigurationViewModel : ViewModelBase
    {
        #region private variables
        private ObservableCollection<RangeBreakOutConfiguration> _rangeBreakOutConfigurations;
        #endregion

        #region public properties
        public Kite Kite { get; set; }
        public ObservableCollection<RangeBreakOutConfiguration> RangeBreakOutConfigurations
        {
            get { return _rangeBreakOutConfigurations; }
            set { _rangeBreakOutConfigurations = value; OnPropertyChanged("RangeBreakOutConfigurations"); }
        }
        #endregion

        #region Constructor
        public RangeBreakOutOrderConfigurationViewModel()
        {
            SaveCommand = new  DelegateCommand(SaveConfigurationFiles);
            _rangeBreakOutConfigurations = new ObservableCollection<RangeBreakOutConfiguration>();
            _rangeBreakOutConfigurations.CollectionChanged += _rangeBreakOutConfigurations_CollectionChanged;
            LoadConfigurationFiles();
        }


        #endregion

      

        #region private Methods
        private void _rangeBreakOutConfigurations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newItem = e.NewItems[0];
            (newItem as RangeBreakOutConfiguration).PropertyChanged += (s, e1) =>
            {
                if (e1.PropertyName != "Summary")
                    (s as RangeBreakOutConfiguration).UpdateSummary();

                if(e1.PropertyName=="StockSymbol")
                {
                    try
                    {
                        var kiteLTP = Kite.GetLTP(new string[] { "NSE:" + (s as RangeBreakOutConfiguration).StockSymbol });
                        (s as RangeBreakOutConfiguration).StockCurrentPrice = kiteLTP.FirstOrDefault().Value.LastPrice;
                    }
                    catch(Exception ex)
                    {

                    }
                }
            };
        }
        private async void LoadConfigurationFiles()
        {
            await Task.Factory.StartNew(() =>
            {
                return Serializer.GetConfiguration<ObservableCollection<RangeBreakOutConfiguration>>(ConfigurationFileNames.RangeBreakOutOrderConfigurationFileName);
            }).ContinueWith(result =>
            {
                RangeBreakOutConfigurations = result.Result;
            },TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void SaveConfigurationFiles(object obj)
        {
            await Task.Factory.StartNew(() =>
            {
                Serializer.SaveConfiguration<ObservableCollection<RangeBreakOutConfiguration>>(ConfigurationFileNames.RangeBreakOutOrderConfigurationFileName, RangeBreakOutConfigurations);
            });
        }
        #endregion

    }
}
