using Stock_Trade_Configuration.ViewModels;

using StockTradeConfiguration.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Stock_Trade_Configuration
{
    public struct TimeZoneStartEnd
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeZoneStatus? Status { get; set; }
    }
    public class StockTimeZomeTicker
    {
        private ConcurrentDictionary<string, TimeZoneStartEnd> _timeZoneStartEndDictionary;
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
            _timeZoneDictionary = new Dictionary<string, TimeZoneStatus?>();
            _timeZoneStartEndDictionary = new ConcurrentDictionary<string, TimeZoneStartEnd>();
            _timeZoneStartEndDictionary[StockTimeZone.EquityMarket.ToString()] = new TimeZoneStartEnd() { StartTime=Time(9,15,0), EndTime = Time(15,20,0)};
            Events.SubscribeTimeZoneEvent += Events_SubscribeTimeZoneEvent;
        }

        private void Events_SubscribeTimeZoneEvent(string timeZoneName, TimeSpan startTime, TimeSpan endTime)
        {
            _timeZoneStartEndDictionary[timeZoneName] = new TimeZoneStartEnd() { StartTime = startTime, EndTime=endTime };
        }

        private TimeSpan Time(int hour, int min, int second)
        {
            return new TimeSpan(hour, min, second);
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

        private Dictionary<string, TimeZoneStatus?> _timeZoneDictionary;

        private void CheckTimeZone()
        {
            foreach (var item in _timeZoneStartEndDictionary)
            {
                TimeZoneStatus? status = null;
                TimeSpan currentTime = DateTime.Now.TimeOfDay;

                if (currentTime >= item.Value.StartTime && currentTime < item.Value.EndTime)
                    status = TimeZoneStatus.Started;
                else if (currentTime > item.Value.EndTime)
                    status = TimeZoneStatus.Stopped;

                if(status.HasValue && (!_timeZoneDictionary.ContainsKey(item.Key) || _timeZoneDictionary[item.Key] != status))
                {
                    _timeZoneDictionary[item.Key] = status;
                    Events.RaiseTimeZoneChangedEvent(item.Key, status.Value);
                }
            }
        }

        public void Stop()
        {
            _isCancelled = true;
        }
    }
}
