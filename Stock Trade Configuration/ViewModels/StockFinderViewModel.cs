using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockTradeConfiguration.Models;
using System.Threading.Tasks;
using StockTrade.Jobbing;
using System.Collections.ObjectModel;

namespace Stock_Trade_Configuration.ViewModels
{
    public class StockFinderViewModel : ViewModelBase
    {
        #region private variables
        private bool _findingStocks;
        private decimal _minPrice;
        private decimal _maxPrice;
        private ObservableCollection<StockBase> _stocks;
        #endregion

        #region public properties
        public ObservableCollection<StockBase> Stocks
        {
            get { return _stocks; }
            set { _stocks = value; OnPropertyChanged("Stocks"); }
        }
        public decimal MinPrice
        {
            get { return _minPrice; }
            set { _minPrice = value; OnPropertyChanged("MinPrice"); }
        }
        public decimal MaxPrice
        {
            get { return _maxPrice; }
            set { _maxPrice = value; OnPropertyChanged("MaxPrice"); }
        }

        public bool FindingStocks
        {
            get { return _findingStocks; }
            set { _findingStocks = value; OnPropertyChanged("FindingStocks"); }
        }
        #endregion

        #region private methods
        public void FindStocks()
        {
            FindingStocks = true;
            Task.Factory.StartNew(() =>
            {

                var stocks = new List<StockBase>();
                var allStocks = this.Serializer.GetConfiguration<List<string>>(ConfigurationFileNames.ValidStocks);
                var stocksInChunk = allStocks.GetListItemsInChunk<string>(200);
                foreach (var item in stocksInChunk)
                {
                    try
                    {
                        var ltps = KiteInstance.Kite.GetLTP(item.ToArray());
                        stocks.AddRange( ltps.Where(s => s.Value.LastPrice <= MaxPrice && s.Value.LastPrice >= MinPrice).
                         Select(s => new StockBase()
                        {
                            Exchange = s.Key.Split(new char[] { ':' })[0],
                            Price = s.Value.LastPrice,
                            Symbol = s.Key.Split(new char[] {':' })[1]
                        }));
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return stocks;
            }).ContinueWith((res)=>
            {
                Stocks = new ObservableCollection<StockBase>(res.Result);
                FindingStocks = false;
            });
        }
        #endregion
    }
}
