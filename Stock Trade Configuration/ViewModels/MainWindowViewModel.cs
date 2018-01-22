using StockTradeConfigurationCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region private variables
        private ObservableCollection<ViewInfo> _allViews;
        #endregion

        #region public properties
        public ObservableCollection<ViewInfo> AllViews
        {
            get { return _allViews; }
            set { _allViews = value; OnPropertyChanged("AllViews"); }
        }
        #endregion

        #region constructor
        public MainWindowViewModel()
        {
            LoadAllViews();
        }
        #endregion

        #region private methods
        private void LoadAllViews()
        {
            List<ViewInfo> viewInfoes = new List<ViewInfo>();
            Dictionary<string, Type> views = new Dictionary<string, Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(s => s.CustomAttributes.Any(c => c.AttributeType == typeof(ViewAttribute)));
                foreach (var jobbingTpe in types)
                {
                    var attrib = jobbingTpe.GetCustomAttributes(true).FirstOrDefault(s => s.GetType() == typeof(ViewAttribute)) as ViewAttribute;
                    if (attrib != null)
                    {
                        ViewInfo info = new ViewInfo()
                        {
                            Name = attrib.Name,
                            Description = attrib.Description,
                            UserControl = jobbingTpe
                        };
                        viewInfoes.Add(info);
                    }
                }
            }
            AllViews = new ObservableCollection<ViewInfo>(viewInfoes);
        }
        #endregion

    }
}
