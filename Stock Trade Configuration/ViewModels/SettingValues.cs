using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.ViewModels
{
    public class SettingValues
    {
        public TimeSpan IntradaySquareOffTime { get; set; }
        public TimeSpan IntradayTradeTimeStart { get; set; }
        public TimeSpan IntradayTradeTimeEnd { get; set; }
        public TimeSpan StockHighLowWatchTimeStart { get; set; }
        public TimeSpan StockHighLowWatchTimeEnd { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal GapUpDownPickMax { get; set; }
        public decimal GapUpDownPickMin { get; set; }
        public decimal PicKStockWhosePriceMin { get; set; }
        public decimal PicKStockWhosePriceMax { get; set; }

        private static SettingValues _instance = new SettingValues();

        public static SettingValues Instance
        {
            get { return _instance; }
        }


        private SettingValues()
        {
            LoadStockHighLowTimeZone();
            LoadIntradaySquareOffTime();
            LoadIntradayTradeTimeZone();
            LoadProfitMargin();
            LoadStockPickWithGapUpValue();
            LoadStockPickSettingValue();
        }

        private void LoadProfitMargin()
        {
            try
            {
                ProfitMargin = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.ProfitMargin) ? Convert.ToDecimal(ConfigurationManager.AppSettings[ConfigurationFileNames.ProfitMargin]) : 0.3m;
            }
            catch(Exception)
            {
                ProfitMargin = 0.3m;
            }
        }

        public void LoadStockHighLowTimeZone()
        {
            try
            {
                var stockHighLowWathHour = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.HighLowWatchTimeStartHour) ? ConfigurationManager.AppSettings[ConfigurationFileNames.HighLowWatchTimeStartHour] : null;
                if (stockHighLowWathHour != null)
                {
                    var stockHighLowWathStartMin = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.HighLowWatchTimeStartMin) ? ConfigurationManager.AppSettings[ConfigurationFileNames.HighLowWatchTimeStartMin] : null;
                    var stockHighLowWathEndMin = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.HighLowWatchTimeEndMin) ? ConfigurationManager.AppSettings[ConfigurationFileNames.HighLowWatchTimeEndMin] : null;
                    StockHighLowWatchTimeStart = new TimeSpan(Convert.ToInt32(stockHighLowWathHour), Convert.ToInt32(stockHighLowWathStartMin), 0);
                    StockHighLowWatchTimeEnd = new TimeSpan(Convert.ToInt32(stockHighLowWathHour), Convert.ToInt32(stockHighLowWathEndMin), 0);
                }
                else
                {
                    StockHighLowWatchTimeStart = new TimeSpan(9, 15, 0);
                    StockHighLowWatchTimeEnd = new TimeSpan(9, 20, 0);
                }
            }
            catch (Exception)
            {
                StockHighLowWatchTimeStart = new TimeSpan(9, 15, 0);
                StockHighLowWatchTimeEnd = new TimeSpan(9, 20, 0);
            }
        }

        public void LoadIntradaySquareOffTime()
        {
            try
            {
                var intradaySquareOffHour = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.IntradaySquareOffHour) ? ConfigurationManager.AppSettings[ConfigurationFileNames.IntradaySquareOffHour] : null;
                if (intradaySquareOffHour != null)
                {
                    var intradaySquareOffMin = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.IntradaySquareOffMin) ? ConfigurationManager.AppSettings[ConfigurationFileNames.IntradaySquareOffMin] : null;
                    IntradaySquareOffTime = new TimeSpan(Convert.ToInt32(intradaySquareOffHour), Convert.ToInt32(intradaySquareOffMin), 0);
                }
                else
                {
                    IntradaySquareOffTime = new TimeSpan(15, 30, 1);
                }
            }
            catch (Exception)
            {
                IntradaySquareOffTime = new TimeSpan(15, 30, 1);
            }
        }

        public void LoadIntradayTradeTimeZone()
        {
            IntradayTradeTimeStart = new TimeSpan(StockHighLowWatchTimeEnd.Hours, StockHighLowWatchTimeEnd.Minutes, 1);
        }

        public void LoadStockPickWithGapUpValue()
        {
            try
            {
                GapUpDownPickMax = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.GapUpDownPickMax) ? Convert.ToDecimal(ConfigurationManager.AppSettings[ConfigurationFileNames.GapUpDownPickMax]) : 2.0m;
            }
            catch (Exception)
            {

                GapUpDownPickMax = 2.0m;
            }

            try
            {
                GapUpDownPickMin = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.GapUpDownPickMin) ? Convert.ToDecimal(ConfigurationManager.AppSettings[ConfigurationFileNames.GapUpDownPickMin]) : 1.0m;
            }
            catch (Exception)
            {

                GapUpDownPickMin = 1.0m;
            }
        }

        public void LoadStockPickSettingValue()
        {
            try
            {
                PicKStockWhosePriceMax = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.PicKStockWhosePriceMax) ? Convert.ToDecimal(ConfigurationManager.AppSettings[ConfigurationFileNames.PicKStockWhosePriceMax]) : 2.0m;
            }
            catch (Exception)
            {

                PicKStockWhosePriceMax = 1000.0m;
            }

            try
            {
                PicKStockWhosePriceMin = ConfigurationManager.AppSettings.AllKeys.Contains(ConfigurationFileNames.GapUpDownPickMin) ? Convert.ToDecimal(ConfigurationManager.AppSettings[ConfigurationFileNames.PicKStockWhosePriceMin]) : 1.0m;
            }
            catch (Exception)
            {

                PicKStockWhosePriceMin = 150.0m;
            }
        }
    }
}
