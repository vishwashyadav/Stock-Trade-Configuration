using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using StockTradeStrategy.BuySellOnSignal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace StockTradeStrategy.BuySellOnSignal.ViewModels
{
    public class ReversalInfoEditorViewModel : NotifyPropertyChanged
    {
        #region private variables
        private bool _isLoadingInformation;
        private const string duplicateRecordMessage = "An item with name '{0}' is already exist, please try supplying another name.";
        private ObservableCollection<ReversalConfig> _reversalConfigurations;
        private string _name;
        private ReversalConfig _selectedReversalConfig;
        private ObservableCollection<ReversalInfo> _reversalInfoes;
        #endregion

        #region public properties
        public DialogResult Result { get; set; }
      public Action CloseWindowAction { get; set; }
        public bool IsLoadingInformation
        {
            get { return _isLoadingInformation; }
            set { _isLoadingInformation = value; OnPropertyChanged("IsLoadingInformation"); }
        }
        public ObservableCollection<ReversalConfig> ReversalConfigurations
        {
            get { return _reversalConfigurations; }
            set { _reversalConfigurations = value; OnPropertyChanged("ReversalConfigurations"); }
        }
        public bool IsEditMode { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); SaveCommand.RaiseCanExecuteChanged(); }
        }
        public ReversalConfig SelectedReversalConfig
        {
            get { return _selectedReversalConfig; }
            set {
                _selectedReversalConfig = value;
                if(value!=null)
                {
                    Name = value.Name;
                    ReversalInfoes = new ObservableCollection<ReversalInfo>(value.ReversalInfoes);
                }
                OnPropertyChanged("SelectedReversalConfig");
            }
        }
        public ObservableCollection<ReversalInfo> ReversalInfoes
        {
            get { return _reversalInfoes; }
            set { _reversalInfoes = value; OnPropertyChanged("ReversalInfoes"); }
        }
        #endregion

        #region Constructor
        public ReversalInfoEditorViewModel()
        {
            SaveCommand = new DelegateCommand(SaveCommandExecute, SaveCommandCanExecute);
            ReversalInfoes = new ObservableCollection<ReversalInfo>();
        }

        #endregion

        #region Command Implementation
        private bool SaveCommandCanExecute(object obj)
        {
            return !string.IsNullOrEmpty(Name);
        }

        private void Save(List<ReversalConfig> config)
        {
            XSerializer.Instance.SaveConfiguration<List<ReversalConfig>>(ConfigFileNames.ReversalInforFileName,config);
            if(CloseWindowAction!=null)
            {
                Result = DialogResult.Yes;
                CloseWindowAction.Invoke();
            }
        }

        private void SaveCommandExecute(object obj)
        {
            var data = XSerializer.Instance.GetConfiguration<List<ReversalConfig>>(ConfigFileNames.ReversalInforFileName);
            if (data != null)
            {
                if (IsEditMode)
                {
                    var currentData = data.FirstOrDefault(s => s.Key == SelectedReversalConfig.Key);

                    if (data.Any(s => s.Name == Name && s.Key!= currentData.Key))
                    {
                        System.Windows.MessageBox.Show(string.Format(duplicateRecordMessage, Name), "Duplicate Item", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                         currentData.Name = Name;
                        currentData.ReversalInfoes = ReversalInfoes.ToList();
                        Save(data);
                    }
                }
                else
                {
                    if (!data.Any(s => s.Name == Name))
                    {
                        data.Add(new ReversalConfig()
                        {
                            Key = Guid.NewGuid(),
                            Name = Name,
                            ReversalInfoes = ReversalInfoes.ToList()
                        });
                        Save(data);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(duplicateRecordMessage, "Duplicate Item", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
            else
            {
                data = new List<ReversalConfig>();

                {
                    data.Add(new ReversalConfig()
                    {
                        Key = Guid.NewGuid(),
                        Name = Name,
                        ReversalInfoes = ReversalInfoes.ToList()
                    });
                    Save(data);
                }

            }
        }

        #endregion

       
    }
}
