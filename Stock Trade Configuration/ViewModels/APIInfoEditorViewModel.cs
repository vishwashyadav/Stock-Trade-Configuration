using StockTradeConfiguration.Models;
using StockTradeConfiguration.Models.APIs;
using StockTradeConfiguration.Models.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.ViewModels
{
    public class APIInfoEditorViewModel : ViewModelBase
    {
        #region private variables
        private ApiClient _selectedApiClient;
        private object _selectedApiInstance;
        private List<KeyValuePair<APIAttribute, Type>> apiAttributes; 
        #endregion

        #region public properties
        public object SelectedApiInstance
        {
            get { return _selectedApiInstance; }
            set { _selectedApiInstance = value;  OnPropertyChanged("SelectedApiInstance"); }
        }
        public ApiClient SelectedApiClient
        {
            get { return _selectedApiClient; }
            set { _selectedApiClient = value; SelectApiInstance(); OnPropertyChanged("SelectedApiClient"); }
        }
        #endregion

        #region constructor
        public APIInfoEditorViewModel()
        {
            LoadAllApiType();
        }
        #endregion

        #region private methods
        private void LoadAllApiType()
        {
            apiAttributes = AppDomain.CurrentDomain.GetAssemblies().GetAllClassByAttribute<APIAttribute>();
        }
        private void SelectApiInstance()
        {
            if(apiAttributes!=null)
            {
                var attrib = apiAttributes.FirstOrDefault(s => s.Key.ApiClient == SelectedApiClient);
                if (attrib.Key == null)
                    return;
                SelectedApiInstance = Activator.CreateInstance(attrib.Value);
            }
        }
        #endregion
    }
}
