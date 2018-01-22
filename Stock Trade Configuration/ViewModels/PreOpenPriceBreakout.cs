using StockTradeConfiguration.Data;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.ViewModels
{
    public class PreOpenPriceBreakoutViewModel : ViewModelBase
    {
        #region private variables
        private int _quantity=10;
        private decimal _minStockPrice=80;
        private decimal _maxStockPrice=2000;
        private decimal _minChangePercent=1;
        private decimal _maxChangePercent=5;
        private ObservableCollection<OpenBreakOutConfig> _stocks;
        private PreOpenBreakOutStockOrder _preOpenBreakOutStockOrder;

        #endregion

        #region public properties
        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; OnPropertyChanged("Quantity"); }
        }
        public decimal MinStockPrice
        {
            get { return _minStockPrice; }
            set { _minStockPrice = value; OnPropertyChanged("MinStockPrice"); }
        }
        public decimal MaxStockPrice
        {
            get { return _maxStockPrice; }
            set { _maxStockPrice = value; OnPropertyChanged("MaxStockPrice"); }
        }
        public decimal MinChangePercent
        {
            get { return _minChangePercent; }
            set { _minChangePercent= value; OnPropertyChanged("MinChangePercent"); }
        }
        public decimal MaxChangePercent
        {
            get { return _maxChangePercent; }
            set { _maxChangePercent = value; OnPropertyChanged("MaxChangePercent"); }
        }
        public ObservableCollection<OpenBreakOutConfig> Stocks
        {
            get { return _stocks; }
            set { _stocks = value; OnPropertyChanged("Stocks"); }
        }
        #endregion

        public PreOpenPriceBreakoutViewModel()
        {
        }

        private Dictionary<string, Tuple<decimal, decimal>> _openValues = new Dictionary<string, Tuple<decimal, decimal>>();
        #region public properties
        public async void OrderStockWhenItBreaksRange()
        {
            if (Stocks == null)
                return;
            var stocks = Stocks.Select(s => (OpenBreakOutConfig)s.CloneObject());
            var symbols = stocks.Select(s => string.Format("{0}:{1}", s.Exchange, s.Symbol)).ToList();
            await Task.Factory.StartNew(() =>
            {
                Try:
                try
                {
                    if (symbols.Count == 0)
                        return;

                    var ohlcs = KiteInstance.Kite.GetLTP(symbols.ToArray());

                    foreach (var item in ohlcs)
                    {
                        if (!_openValues.ContainsKey(item.Key))
                            _openValues[item.Key] = new Tuple<decimal, decimal>(item.Value.LastPrice, 0);
                        else
                            _openValues[item.Key] = new Tuple<decimal, decimal>(_openValues[item.Key].Item1, item.Value.LastPrice);
                    }

                    int count = 0;
                    foreach (var item in ohlcs)
                    {
                        if (_openValues.ContainsKey(item.Key))
                        {
                            var tuple = _openValues[item.Key];

                            try
                            {
                                File.AppendAllLines(item.Key + ".txt", new List<string> { tuple.Item1 + "$" + tuple.Item2 + "$" + DateTime.Now });
                            }
                            catch (Exception)
                            {

                            }
                            OrderMode? mode = null;

                             if (tuple.Item2 == 0 || tuple.Item1 == tuple.Item2)
                            {
                                continue;
                            }
                            else if (tuple.Item2 > tuple.Item1)
                            {
                                mode = OrderMode.BUY;
                            }
                            else if (tuple.Item2 < tuple.Item1)
                                mode = OrderMode.SELL;

                            if (mode == null)
                                continue;

                            var stock = Stocks.FirstOrDefault(s => string.Format("{0}:{1}", s.Exchange, s.Symbol) == item.Key);
                            if (stock != null)
                            {
                                var orderResponse = KiteInstance.Kite.PlaceOrder(stock.Exchange, stock.Symbol, mode.ToString(), Quantity.ToString(),Price: tuple.Item2.ToString(), Product: "MIS", OrderType: "LIMIT");
                                symbols.RemoveAll(s => s == string.Format("{0}:{1}", stock.Exchange, stock.Symbol));

                                goto Try;
                            }
                        }
                    }

                    if(symbols.Count>0)
                        goto Try;
                }
                catch (Exception)
                {
                    goto Try;
                }
            });

        }

        public void SearchStocks()
        {
            try
            {
                var symbols = PreOpenStockPicker.GetStockSymbolsWithNSEOpenPrice(ViewModels.ConfigurationFileNames.PreOpenStockFileName, ViewModels.ConfigurationFileNames.TextToReplaceInPreOpenTextFile, ViewModels.ConfigurationFileNames.GapUpDownPercentageAtIndex, 1, MinChangePercent, MaxChangePercent, MinStockPrice, MaxStockPrice);
                Stocks = new ObservableCollection<OpenBreakOutConfig>(symbols);
            }
            catch (Exception)
            {
                
            }
        }
        #endregion
    }
}
