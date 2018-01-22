using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.ViewModels
{
    public class ConfigurationFileNames
    {
        public static string RangeBreakOutOrderConfigurationFileName = "RangeBreakOutOrderConfiguration.config";
        public static string StockWatchFileName = "StockWatch.config";
        public static string RequestToken = "RequestToken.txt";
        public static string PreOpenStockFileName = "PreOpenStock.txt";
        public static string JobbingStockDir = "JobbingStock";
        public static string AllNSEInstruments = "NSEInstruments.txt";
        public static string ValidStocks = "ValidStocks.txt";
        public static string UserSpecificFile = "User.config";
        public static string[] TextToReplaceInPreOpenTextFile = new string[] {"Corporate Action", "Sparkline Graph" };
        public static int GapUpDownPercentageAtIndex =4;
        public static string HighLowWatchTimeStartHour = "HighLowWatchTimeStartHour";
        public static string HighLowWatchTimeStartMin = "HighLowWatchTimeStartMin";
        public static string HighLowWatchTimeEndMin = "HighLowWatchTimeEndMin";
        public static string IntradaySquareOffHour = "IntradaySquareOffHour";
        public static string IntradaySquareOffMin = "IntradaySquareOffMin";
        public static string ProfitMargin = "ProfitMargin";

        public static string GapUpDownPickMax = "GapUpDownPickMax";
        public static string GapUpDownPickMin = "GapUpDownPickMin";
        public static string PicKStockWhosePriceMin = "PicKStockWhosePriceMin";
        public static string PicKStockWhosePriceMax = "PicKStockWhosePriceMax";
    }
}
