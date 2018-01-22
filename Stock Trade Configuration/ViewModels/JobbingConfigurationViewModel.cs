using KiteConnect;

using StockTrade.Jobbing;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.ViewModels
{
    public class JobbingConfigurationViewModel : ViewModelBase
    {
        #region private variables
        private ObservableCollection<KeyValuePair<string, decimal>> _pullBackPercentange;
        KeyValuePair<string, decimal> _selectedPullBackPercentage;
        private ProfitMarginType _profitMarginType;
        private string _exchange;
        private string _symbol;
        private decimal _margin;
        private List<string> _exchanges;
        private ObservableCollection<string> _symbols;
        ObservableCollection<KeyValuePair<string, Type>> _jobbingTypes;
        Dictionary<string, List<string>> _allInstruments;
        KeyValuePair<string, Type> _selectedJobbingType;
        private ObservableCollection<JobbingStockBase> _jobbingStocks;
        private StockIncrementalMethod _incrementalMethod;
        private int _incrementalNumber;

        #endregion

        #region Public Properties
        public StockIncrementalMethod IncrementalMethod
        {
            get { return _incrementalMethod; }
            set { _incrementalMethod = value; OnPropertyChanged("IncrementalMethod"); }
        }
        public int IncrementalNumber
        {
            get { return _incrementalNumber; }
            set { _incrementalNumber = value; OnPropertyChanged("IncrementalNumber"); }
        }
        public KeyValuePair<string,decimal> SelectedPullBackPercentage
        {
            get { return _selectedPullBackPercentage; }
            set { _selectedPullBackPercentage = value; OnPropertyChanged("SelectedPullBackPercentage"); }
        }
        public ObservableCollection<KeyValuePair<string,decimal>> PullBackPercentage
        {
            get { return _pullBackPercentange; }
            set { _pullBackPercentange=value; OnPropertyChanged("PullBackPercentage"); }
        }
        public ObservableCollection<JobbingStockBase> JobbingStocks
        {
            get { return _jobbingStocks; }
            set { _jobbingStocks = value; OnPropertyChanged("JobbingStocks"); }
        }
        public decimal Margin
        {
            get { return _margin; }
            set { _margin = value; OnPropertyChanged("Margin"); }
        }
        public KeyValuePair<string,Type> SelectedJobbingType
        {
            get { return _selectedJobbingType; }
            set { _selectedJobbingType = value; OnPropertyChanged("SelectedJobbingType"); }
        }
        public ProfitMarginType ProfitMarginType
        {
            get { return _profitMarginType; }
            set { _profitMarginType = value; OnPropertyChanged("ProfitMarginType"); }
        }
        public ObservableCollection<KeyValuePair<string, Type>> JobbingTypes
        {
            get { return _jobbingTypes; }
            set { _jobbingTypes = value; OnPropertyChanged("JobbingTypes"); }
        }
        public string Exchange
        {
            get { return _exchange; }
            set
            {
                _exchange = value;
                if (!string.IsNullOrEmpty(value))
                    Symbols = _allInstruments.ContainsKey(value) ? new ObservableCollection<string>(_allInstruments[value]) : null;
                else
                    Symbols = new ObservableCollection<string>();
                OnPropertyChanged("Exchange");
            }

        }

        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; OnPropertyChanged("Symbol"); }
        }

        public List<string> Exchanges
        {
            get { return _exchanges; }
            set { _exchanges = value; OnPropertyChanged("Exchanges"); }
        }

        public ObservableCollection<string> Symbols
        {
            get { return _symbols; }
            set { _symbols = value; OnPropertyChanged("Symbols"); }
        }
        #endregion

        #region constructor
        public JobbingConfigurationViewModel()
        {
            LoadPullBackPercentage();
            JobbingStocks = new ObservableCollection<JobbingStockBase>();
            LoadSymbols();
            LoadAllJobbingType();
            Events.StatusChangedEvent+= Events_JobbingStatusChangedEvent;
            Events.OpenPositionsChangedEvent += Events_OpenPositionsChangedEvent;
        }

        private void Events_OpenPositionsChangedEvent(string exchange, string symbol, int openPositions)
        {
            var stock = JobbingStocks.FirstOrDefault(s => s.Exchange == exchange && symbol == s.Symbol);
            if (stock != null)
            {
                stock.OpenPositionsCount = openPositions;
            }
        }

        #endregion

        private void Events_JobbingStatusChangedEvent(string exchange, string symbol, string status)
        {
            JobbingStatus jobbingStatus = JobbingStatus.NotStarted;
            if (Enum.TryParse(status, out jobbingStatus))
            {
                var stock = JobbingStocks.FirstOrDefault(s => s.Exchange == exchange && symbol == s.Symbol);
                if (stock != null)
                {
                    stock.Status = jobbingStatus;
                }
            }
        }

        private void LoadPullBackPercentage()
        {
            PullBackPercentage = new ObservableCollection<KeyValuePair<string, decimal>>();
            PullBackPercentage.Add(new KeyValuePair<string, decimal>("0.1%", 0.1m));
            PullBackPercentage.Add(new KeyValuePair<string, decimal>("0.2%", 0.2m));
            PullBackPercentage.Add(new KeyValuePair<string, decimal>("0.3%", 0.3m));
            PullBackPercentage.Add(new KeyValuePair<string, decimal>("0.4%", 0.4m));
            PullBackPercentage.Add(new KeyValuePair<string, decimal>("0.5%", 0.5m));
        }

        private void LoadSymbols()
        {
            var symbols = Serializer.GetConfiguration<List<string>>(ConfigurationFileNames.ValidStocks);
            var groupy = symbols.GroupBy(s => s.Split(':')[0]);
            _allInstruments = groupy.ToDictionary(k => k.Key, v => v.Select(s => s.Split(':')[1]).OrderBy(s=>s).ToList());

            Exchanges = _allInstruments.Select(s => s.Key).ToList();
            Exchange = Exchanges.FirstOrDefault();
            
        }

        private void LoadAllJobbingType()
        { 
            Task.Factory.StartNew(() =>
            {
                Dictionary<string, Type> jobbingTypeWithName = new Dictionary<string, Type>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var types = assembly.GetTypes().Where(s => s.CustomAttributes.Any(c => c.AttributeType == typeof(JobbingTypeAttribute)));
                    foreach (var jobbingTpe in types)
                    {
                        var attrib = jobbingTpe.GetCustomAttributes(true).FirstOrDefault(s => s.GetType() == typeof(JobbingTypeAttribute)) as JobbingTypeAttribute;
                        if (attrib != null)
                        {
                            jobbingTypeWithName[attrib.Name] = jobbingTpe;
                        }
                    }
                }
                return jobbingTypeWithName;
            }).ContinueWith((result) =>
            {
                JobbingTypes =new ObservableCollection<KeyValuePair<string, Type>>( result.Result.Select(s=>s));
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void AddStockForJobbing()
        {
            JobbingStockBase stock = Activator.CreateInstance(SelectedJobbingType.Value) as JobbingStockBase;
            stock.Exchange = Exchange;
            stock.Symbol = Symbol;
            stock.SaveDirectoryName = ConfigurationFileNames.JobbingStockDir;
            try
            {
                var ltp = KiteInstance.Kite.GetLTP(new string[] { string.Format("{0}:{1}", Exchange, Symbol) });
                stock.CurrentPrice = ltp.FirstOrDefault().Value.LastPrice;
                if(SelectedJobbingType.Value == typeof(StrongPullBackJobbing))
                {
                    (stock as StrongPullBackJobbing).PullBackPercentage = Convert.ToDouble( SelectedPullBackPercentage.Value);
                }
                else if(SelectedJobbingType.Value == typeof(PivotJobbing))
                {
                    (stock as PivotJobbing).IncrementalMethod = IncrementalMethod;
                    (stock as PivotJobbing).IncrementalNumber = IncrementalNumber;
                }
                stock.JobbingType = SelectedJobbingType.Key;
                switch (ProfitMarginType)
                {
                    case ProfitMarginType.Percentage:
                        stock.Margin = stock.CurrentPrice.FindPercentagValue(Margin);
                        break;
                    case ProfitMarginType.Absolute:
                        stock.Margin = Margin;
                        break;
                }
                JobbingStocks.Add(stock); 
            }
            catch(Exception ex)
            {

            }
        }

        public void Start(JobbingStockBase stock)
        {
            StockJobber.Instance.Start(stock, KiteInstance.Kite);
        }
    }
}
