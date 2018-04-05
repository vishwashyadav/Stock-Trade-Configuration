using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using StockTradeStrategy.BuySellOnSignal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockTradeStrategy.BuySellOnSignal.ViewModels
{
    public class SignalparameterSettingViewmodel : NotifyPropertyChanged
    {
        #region private varibles
        private SignalSettingInfo _settingInfo;
       
        #endregion

        #region public properties
        public DelegateCommand SaveCommand { get; set; }
        public Action CloseWindowAction { get; set; }
        public bool DataSaved { get; set; }
        public SignalSettingInfo SettingInfo
        {
            get { return _settingInfo; }
            set { _settingInfo = value; OnPropertyChanged("SettingInfo"); }
        }
        #endregion

        #region constructor
        public SignalparameterSettingViewmodel()
        {
            LoadFile();
            SaveCommand = new DelegateCommand(SaveCommandExecute);
        }
        #endregion

        #region commandImplementation
        public void LoadFile()
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var savedData = XSerializer.Instance.GetConfiguration<SignalSettingInfo>(ConfigFileNames.SignalParameterSettingFileName);

                //Save data with default data
                if (savedData == null)
                {
                    SettingInfo = new SignalSettingInfo();
                    SettingInfo.Version++;
                    XSerializer.Instance.SaveConfiguration<SignalSettingInfo>(ConfigFileNames.SignalParameterSettingFileName, SettingInfo);
                }
                else
                    SettingInfo = savedData;
            });
        }
        public bool SaveCommandCanExecute(object param)
        {
            return true;
            //return SettingInfo!= null &&
            //       SettingInfo.BuySellSignalIndex>=0 &&
            //       SettingInfo.PriceIndex >=0 &&
            //       SettingInfo.
        }
        public void SaveCommandExecute(object param)
        {
            SettingInfo.Version++;
           XSerializer.Instance.SaveConfiguration<SignalSettingInfo>(ConfigFileNames.SignalParameterSettingFileName, SettingInfo);
            if (CloseWindowAction != null)
                CloseWindowAction();
        }

        public void CancelCommandExecute(object param)
        {
            if (CloseWindowAction != null)
                CloseWindowAction();
        }
        #endregion
    }
}
