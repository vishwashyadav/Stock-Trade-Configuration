using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public class StockSymbols : NotifyPropertyChanged
    {
        private StockSymbolStatus _stockSymbolStatus;
        private string _stockSymbol { get; set; }
        private string _category { get; set; }
        private string _displayName { get; set; }
        private decimal _stockBreakOutBuyPrice;
        private decimal _stockBreakOutSellPrice;
        public decimal _profitMargin;

        public string ProfitMarginStr
        {
            get { return _profitMargin == 0 ? "NA" : _profitMargin + " %"; }
        }
        public decimal ProfitMargin
        {
            get { return _profitMargin; }
            set { _profitMargin = value; OnPropertyChanged("ProfitMargin"); }
        }

        public decimal StockBreakOutBuyPrice
        {
            get { return _stockBreakOutBuyPrice; }
            set { if (value != _stockBreakOutBuyPrice)
                {
                    _stockBreakOutBuyPrice = value; OnPropertyChanged("StockBreakOutBuyPrice");
                }
            }
        }

        public decimal StockBreakOutSellPrice
        {
            get { return _stockBreakOutSellPrice; }
            set { _stockBreakOutSellPrice = value; OnPropertyChanged("StockBreakOutSellPrice"); }
        }

        public StockSymbolStatus StockSymbolStatus
        {
            get { return _stockSymbolStatus; }
            set { _stockSymbolStatus = value; OnPropertyChanged("StockSymbolStatus"); }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; OnPropertyChanged("DisplayName"); }
        }
        public string StockSymbol
        {
            get { return _stockSymbol; }
            set { _stockSymbol = value; OnPropertyChanged("StockSymbol"); }
        }


        public string Category
        {
            get { return _category; }
            set { _category = value; OnPropertyChanged("Category"); }
        }
        public void UpdateDisplayName()
        {
            DisplayName = string.Format("{0}:{1}", Category, StockSymbol);
        }

        private static decimal GetNextValidPrice(decimal value, bool up)
        {
            if (value.ToString().Contains("."))
            {
                var val = value.ToString().Split(new char[] { '.' });
                var last = Convert.ToDecimal(val[1]);
                Divide:
                if (last % 5 != 0)
                {
                    if (up)
                        last++;
                    else last--;
                    goto Divide;
                }
                return Convert.ToDecimal(val[0] + "." + last);
            }
            return value;
        }

    }

    public enum StockSymbolStatus
    {
        UnKnown,
        InValid,
        Valid
    }
}
