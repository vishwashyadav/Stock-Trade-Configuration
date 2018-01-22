using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace StockTradeStrategy.BuySellOnSignal.Converters
{
    public class ProfitLossToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is decimal)
            {
                decimal val = 0;
                Decimal.TryParse(System.Convert.ToString(value), out val);
                if (val > 0)
                    return Brushes.Green;
                else if (val < 0)
                    return Brushes.Red;
            }
            else if(value is int)
            {
                int val = 0;
                int.TryParse(System.Convert.ToString(value), out val);
                if (val > 0)
                    return Brushes.Green;
                else if (val < 0)
                    return Brushes.Red;
            }
            return (SolidColorBrush)(new BrushConverter().ConvertFrom("#888")); ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
