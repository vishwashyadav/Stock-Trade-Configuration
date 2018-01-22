using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StockTradeStrategy.BuySellOnSignal.Converters
{
    public class MaxProfitLossToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is decimal)
            {
                decimal val = System.Convert.ToDecimal(value);
                if (val == 0)
                    return "UNLIMITED";
                else
                    return val;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
