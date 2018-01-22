using Stock_Trade_Configuration.ViewModels;

using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Stock_Trade_Configuration
{
    public class StockTimeZomeTicker
    {
        
        StockTimeZone? _timeZone;
        DispatcherTimer _timer;
        private static StockTimeZomeTicker _instance = new StockTimeZomeTicker();
        private bool _isCancelled=true;

        public static StockTimeZomeTicker Instance
        {
            get { return _instance; }
        }


        private StockTimeZomeTicker()
        {
            
        }

        public async void Start()
        {
            if (_isCancelled)
            {
                _isCancelled = false;
                await CheckTimeZoneTick();
            }
        }

        async void check()
        {
            while (true)
            {
                CheckTimeZone();

                await Task.Delay(1000);
                if (_isCancelled)
                    break;
            }
        }

        public async Task CheckTimeZoneTick()
        {

            await Task.Run(() => 
            {
                 check();
            });

        }

        private void CheckTimeZone()
        {
            StockTimeZone timeZone = StockTimeZone.Idle;
            ViewModels.SettingValues settingValues = ViewModels.SettingValues.Instance;

            TimeSpan currentTimeSpan = DateTimeOffset.Now.TimeOfDay;
            if (currentTimeSpan >= settingValues.StockHighLowWatchTimeStart && currentTimeSpan <= settingValues.StockHighLowWatchTimeEnd)
            {
                timeZone = StockTimeZone.HighLowWatch;
            }
            else if (currentTimeSpan >= settingValues.IntradayTradeTimeStart && currentTimeSpan <= settingValues.IntradaySquareOffTime)
                timeZone = StockTimeZone.TradeTime;
            else
                timeZone = StockTimeZone.Idle;

            if( _timeZone == null || _timeZone!=timeZone)
            {
                Events.RaiseTimeZoneChangedEvent(timeZone);
                _timeZone = timeZone;
            }
        }

        public void Stop()
        {
            _isCancelled = true;
        }
    }
}
