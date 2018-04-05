using KiteConnect;
using StockTrade.Jobbing;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockTradeStrategy.BuySellOnSignal.Models
{
    public class BuySellSignalOrderManager
    {
        public decimal DayMaxProfit { get; set; }
        public decimal DayMaxLoss { get; set; }
        private SignalSettingInfo _signalSettingInfo { get; set; }
        public decimal DayCurrentProfitLoss { get; set; }

        public decimal CurrentProfitLoss { get; set; }
        List<BuySellStockOnSignal> _configuresStocks = new List<BuySellStockOnSignal>();
        private KiteConnect.Kite _kite;
        private static BuySellSignalOrderManager _instance = new BuySellSignalOrderManager();
        public static BuySellSignalOrderManager Instance
        {
            get { return _instance; }
        }
        
        private BuySellSignalOrderManager()
        {
            Events.DayProfitLossChanged += Events_DayProfitLossChanged;
        }

        private void Events_DayProfitLossChanged(decimal pnl)
        {
            CurrentProfitLoss = pnl;
            CheckGlobalProfitLoss();
        }

        public async void UpdateCurrentProfitLoss(BuySellOnSignalSymbolConfig stock)
        {
            await Task.Factory.StartNew(() =>
            {
                int quantity = stock.NetQuantity.HasValue ?  stock.NetQuantity.Value:0;
                var pnl = stock.CurrentProfitLoss;

                if (_configuresStocks != null && _configuresStocks.Any())
                {
                    var configuredstock = _configuresStocks.FirstOrDefault(s => s.IsMatchSymbol(stock.Exchange, stock.Symbol));
                    if (configuredstock != null)
                    {
                        if (configuredstock.CheckMaxProfitLossAndClosePosition(quantity, pnl))
                        {
                            _configuresStocks.Remove(configuredstock);
                        }
                    }
                }
            });
        }

        public void Start(BuySellOnSignalSymbolConfig stock, KiteConnect.Kite kite, SignalSettingInfo settingInfo)
        {
            var clonnedStock = stock.DeepCopy<BuySellOnSignalSymbolConfig>();
            clonnedStock.StartTime = stock.StartTime;
            var clonnedSetting = settingInfo != null ? settingInfo.DeepCopy<SignalSettingInfo>() : new SignalSettingInfo();
            BuySellStockOnSignal signal = new BuySellStockOnSignal(clonnedStock, kite,clonnedSetting);

            var addedstock = _configuresStocks.FirstOrDefault(s => s.IsMatchSymbol(clonnedStock.Exchange, clonnedStock.Symbol));
            if (addedstock != null)
            {
                addedstock.Reset();
                _configuresStocks.Remove(addedstock);
            }
            _kite = kite;
            signal.Start();

            _configuresStocks.Add(signal);
        
        }

        public void SetDayProfitLoss(decimal maxProfit, decimal maxLoss)
        {
            DayMaxProfit = maxProfit;
            DayMaxLoss = maxLoss;
        }

        internal void SquareOffAllPositions(BuySellOnSignalSymbolConfig buySellOnSignalSymbolConfig,int count)
        {
            if (_configuresStocks != null && _configuresStocks.Any())
            {
                var stock = _configuresStocks.FirstOrDefault(s => s.IsMatchSymbol(buySellOnSignalSymbolConfig.Exchange, buySellOnSignalSymbolConfig.Symbol));
                if (stock != null)
                {
                    stock.SquareOffAllPositions(count);
                }
            }
        }

        internal void UpdateSettingInfo(SignalSettingInfo settingInfo)
        {
            _configuresStocks.AsParallel().ForAll(s =>
            {
                s.UpdateSettingInfo(settingInfo);
            });
        }

        public void UpdateMaxProfit(BuySellOnSignalSymbolConfig buySellOnSignalSymbolConfig)
        {
            if (_configuresStocks != null && _configuresStocks.Any())
            {
                var stock = _configuresStocks.FirstOrDefault(s => s.IsMatchSymbol(buySellOnSignalSymbolConfig.Exchange, buySellOnSignalSymbolConfig.Symbol));
                if (stock != null)
                {
                    stock.UpdateMaxProfit(buySellOnSignalSymbolConfig.MaxProfit);
                }
            }
        }

        public void UpdateMaxLoss(BuySellOnSignalSymbolConfig buySellOnSignalSymbolConfig)
        {
            if (_configuresStocks != null && _configuresStocks.Any())
            {
                var stock = _configuresStocks.FirstOrDefault(s => s.IsMatchSymbol(buySellOnSignalSymbolConfig.Exchange, buySellOnSignalSymbolConfig.Symbol));
                if (stock != null)
                {
                    stock.UpdateMaxLoss(buySellOnSignalSymbolConfig.MaxLoss);
                }
            }
        }
     
        internal void Stop(BuySellOnSignalSymbolConfig buySellOnSignalSymbolConfig)
        {
            if (_configuresStocks != null && _configuresStocks.Any())
            {
                var stock = _configuresStocks.FirstOrDefault(s => s.IsMatchSymbol(buySellOnSignalSymbolConfig.Exchange, buySellOnSignalSymbolConfig.Symbol));
                if (stock != null)
                {
                    stock.CloseAllPosition(StrategyStockStatus.Stopped);
                    //_configuresStocks.Remove(stock);
                }
            }
        }
        Dictionary<string, int> _openPositions = new Dictionary<string, int>();
        private decimal _oldInactivePnl;
        
        private async void CheckGlobalProfitLoss()
        {
            await Task.Factory.StartNew(() =>
            {
                if (DayMaxLoss != 0 && DayMaxProfit != 0)
                {
                    var shouldClosePosition = false;
                    StrategyStockStatus status = StrategyStockStatus.Added;
                    if (CurrentProfitLoss > 0 && CurrentProfitLoss >= DayMaxProfit)
                    {
                        shouldClosePosition = true;
                        status = StrategyStockStatus.DayProfitReached;
                    }
                    else if (CurrentProfitLoss < 0 && Math.Abs(CurrentProfitLoss) >= DayMaxLoss)
                    {
                        shouldClosePosition = true;
                        status = StrategyStockStatus.DayLossReached;
                    }

                    if (shouldClosePosition)
                    {
                        _configuresStocks.ForEach(s => s.CloseAllPosition(status));
                    }
                }
            });
        }
    }

    public class BuySellStockOnSignal
    {
        #region private variables
        private string _tradingSymbol;
        int _currentOpenPosition = 0;
        int _countOfContiniousExecutionInOneDirection=0;
        private KiteConnect.Kite _kite;
        Dictionary<int, int> _reversalMultiplier = new Dictionary<int, int>();
        private BuySellOnSignalSymbolConfig _config;
        OrderMode? _oldOrderMode;
        FileSystemWatcher fileWather = new FileSystemWatcher();
        SignalSettingInfo _signalSettingInfo;
        #endregion

        public BuySellStockOnSignal(BuySellOnSignalSymbolConfig config, KiteConnect.Kite kite, SignalSettingInfo signalSettingInfo)
        {
            _config = config;
            _kite = kite;
            _signalSettingInfo = signalSettingInfo;
            if(config.ReversalInfoes!=null && config.ReversalInfoes.Any())
            {
                _reversalMultiplier = config.ReversalInfoes.GroupBy(s => s.ReversalNumber).ToDictionary(key => key.Key, value => value.FirstOrDefault().Multiplier);
            }
        }

        public void Start()
        {
            try
            {
                var directiory = _config.DataDirectoryPath + "\\" + _config.MappedSymbolName;
                string directoryToLookFor = string.Empty;
                if (Directory.Exists(directiory))
                {
                    directoryToLookFor = directiory;
                }
                else
                {
                    directoryToLookFor = _config.DataDirectoryPath;
                }
                var filter = !_config.DataFileExtesnion.StartsWith("*.") ? "*." + _config.DataFileExtesnion.Trim(new char[] { '*', '.' }) : _config.DataFileExtesnion;
                fileWather = new FileSystemWatcher()
                {
                    Path = directoryToLookFor,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    Filter = filter,
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                };

                fileWather.Changed += FileWather_Changed;
                _tradingSymbol = string.Format("{0}:{1}", _config.Exchange, _config.Symbol);
                _config.Status = StrategyStockStatus.Running;
                Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, _config.Status.ToString());
                Events.PositionUpdateEvent += Events_PositionUpdateEvent;
                Events.StockLTPChangedEvent += Events_StockLTPChangedEvent;
            }
            catch (Exception ex)
            {
                
            }
        }

        private void Events_StockLTPChangedEvent(string tradingSymbol, decimal lastPrice)
        {
            if(_config!=null && tradingSymbol == _tradingSymbol)
            {
                _config.LTP = lastPrice;
            }
        }

        private void Events_PositionUpdateEvent(string tradingSymbol, Position position)
        {
            if(string.Format("{0}:{1}",_config.Exchange,_config.Symbol)==tradingSymbol)
            {
                _currentOpenPosition = position.Quantity;
            }
        }

        private void FileWather_Changed(object sender, FileSystemEventArgs e)
        {
            Try:
            try
            {
                if (DateTime.Now.TimeOfDay < _config.StartTime)
                {
                    Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, "Rejected because signal not yet synced");
                    return;
                }
                fileWather.EnableRaisingEvents = false;
                string fileName = Path.GetFileNameWithoutExtension(e.FullPath);
                if (fileName.Equals(_config.MappedSymbolName, StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        string filePath = e.FullPath;
                        var lines = File.ReadAllLines(filePath);
                        if (!lines.Any())
                            return;
                        var firstLine = lines.FirstOrDefault();
                        var mode = IsEligibleToPlaceOrder(firstLine, _oldOrderMode);
                        var latestLines = firstLine.Split(new string[] { _config.Seperator.Trim() }, StringSplitOptions.RemoveEmptyEntries);
                        //OrderMode? mode = latestLines[0].Trim().Equals(OrderMode.BUY.ToString(), StringComparison.InvariantCultureIgnoreCase) ? OrderMode.BUY : (latestLines[0].Equals(OrderMode.SELL.ToString(), StringComparison.InvariantCultureIgnoreCase) ? (OrderMode?)OrderMode.SELL : null);
                        if (mode.HasValue)
                        {
                            try
                            {
                                var price = Convert.ToDecimal(latestLines[1]);
                                if (price != 0)
                                {
                                    PlaceOrder(mode.Value, price);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        goto Try;
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            finally
            {
                fileWather.EnableRaisingEvents = true;
            }
        }

        private void PlaceOrder(OrderMode orderMode,decimal price)
        {
            int quantity = _config.LotSize;

            if (_oldOrderMode.HasValue && _oldOrderMode.Value == orderMode)
                return;

            var reversal = _reversalMultiplier.Where(s => s.Key <= _countOfContiniousExecutionInOneDirection).OrderByDescending(s => s.Key).FirstOrDefault();
            if (reversal.Value != 0)
            {
                quantity = (_config.LotSize * reversal.Value) + Math.Abs(_currentOpenPosition);
            }
            else
            {
                quantity = _config.LotSize;
            }
                
            _countOfContiniousExecutionInOneDirection++;

            _oldOrderMode = orderMode;
            try
            {
                var orderResponse = _kite.PlaceOrder(_config.Exchange, _config.Symbol, orderMode.ToString(), quantity, Product: "MIS", OrderType: "MARKET");
                Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, orderMode + " order executed");
            }
            catch(Exception ex)
            {

            }
        }
       
        public bool IsMatchSymbol(string exchange, string symbol)
        {
            return _config.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase) && _config.Exchange.Equals(exchange, StringComparison.InvariantCultureIgnoreCase);
        }
        
        public void CloseAllPosition(StrategyStockStatus status)
        {
            try
            {
                if (_oldOrderMode.HasValue && _currentOpenPosition != 0)
                {
                    _kite.PlaceOrder(_config.Exchange, _config.Symbol, _oldOrderMode.Value == OrderMode.BUY ? OrderMode.SELL.ToString() : OrderMode.BUY.ToString(), Convert.ToInt32(Math.Abs(_currentOpenPosition)), Product: "MIS", OrderType: "MARKET");
                }
            }
            catch (Exception)
            {
                
            }

            fileWather.EnableRaisingEvents = false;
            fileWather.Changed -= FileWather_Changed;
            _currentOpenPosition = 0;
            _countOfContiniousExecutionInOneDirection = 0;
            _oldOrderMode = null;
            Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, status.ToString());
        }

        /// <summary>
        /// Returns true if it reaches max loss and max profit
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool CheckMaxProfitLossAndClosePosition(int quantity, decimal PNL)
        {
            try
            {
                _config.NetQuantity = quantity;
                _config.UpdateMax();
                if ((PNL >= _config.MaxProfit && _config.MaxProfit != 0 ) || (PNL <= -(_config.MaxLoss) && _config.MaxLoss !=0))
                {
                    OrderMode mode = OrderMode.BUY;
                    if (quantity > 0)
                    {
                        mode = OrderMode.SELL;
                    }
                    else
                    {
                        mode = OrderMode.BUY;
                    }
                    var status = _kite.PlaceOrder(_config.Exchange, _config.Symbol, mode.ToString(), Convert.ToInt32(Math.Abs(quantity)), Product: "MIS", OrderType: "MARKET");
                    var orderPlacedSuccessfully = status.Any(s => s.Key.ToLower() == "status" && s.Value.ToLower() == "success");
                    if (orderPlacedSuccessfully)
                    {
                        if (PNL > _config.MaxProfit)
                            Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, StrategyStockStatus.MaxProfitReached.ToString());
                        else
                            Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, StrategyStockStatus.MaxLossReached.ToString());

                        fileWather.Changed -= FileWather_Changed;
                        fileWather.EnableRaisingEvents = false;
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal void UpdateMaxProfit(decimal value)
        {
            _config.MaxProfit = value;
        }

        internal void UpdateMaxLoss(decimal value)
        {
            _config.MaxLoss = value;
        }

        internal void SquareOffAllPositions(int count)
        {
            try
            {
                if (_oldOrderMode.HasValue)
                {
                    _kite.PlaceOrder(_config.Exchange, _config.Symbol, _oldOrderMode.Value == OrderMode.BUY ? OrderMode.SELL.ToString() : OrderMode.BUY.ToString(), count, Product: "MIS", OrderType: "MARKET");
                }
            }
            catch (Exception)
            {
                
            }
        }

        internal void Reset()
        {
            fileWather.Changed -= FileWather_Changed;
            fileWather.EnableRaisingEvents = false;
        }

        private OrderMode? IsEligibleToPlaceOrder(string signal, OrderMode? oldOrderMode)
        {
            if (_signalSettingInfo != null)
            {
                string seperator = string.IsNullOrEmpty(_signalSettingInfo.Seperator) ? _signalSettingInfo.Seperator.Trim() : ",";
                int priceIndex = _signalSettingInfo.PriceIndex;

                decimal priceBuffer = _signalSettingInfo.PriceBufferToAcceptOrder;
                int timeIndex = _signalSettingInfo.TimeIndex;

                var splitData = signal.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries);

                OrderMode mode = splitData[_signalSettingInfo.BuySellSignalIndex].Trim().ToLower().Equals("buy", StringComparison.InvariantCultureIgnoreCase) ? OrderMode.BUY : OrderMode.SELL;

                if (oldOrderMode.HasValue && oldOrderMode.Value == mode)
                {
                    Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, "Current generated order is same as last order : " + mode.ToString());
                    //MessageBox.Show("Current generated order is same as last order : " + mode.ToString());
                    return null;
                }

                var price = Convert.ToDecimal(splitData[priceIndex]);

                //Check Price Buffer
                if (_config.LTP < (Math.Abs(price - priceBuffer)) || _config.LTP > (Math.Abs(price + priceBuffer)))
                {
                    Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, "Rejected because buffer price didn't match : " + "LTP:" + _config.LTP + " $ Price:" + price + " $PriceBuffer:" + priceBuffer + " $Start:" + Math.Abs(price - priceBuffer) + " $End:" + Math.Abs(price + priceBuffer));
                    //MessageBox.Show("Rejected because buffer price didn't match : " + "LTP:"+_config.LTP+" $ Price:"+price + " $PriceBuffer:"+priceBuffer+" $Start:"+ Math.Abs(price - priceBuffer)+" $End:"+ Math.Abs(price + priceBuffer));
                    return null;
                }
                try
                {
                    DateTime dt = DateTime.ParseExact(splitData[timeIndex], _signalSettingInfo.TimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                    var signalTimeSpan = dt.TimeOfDay;
                    var signalTimeSpanMinutes = (signalTimeSpan.TotalMinutes + _signalSettingInfo.TimeDifferenceBetweenSystemAndSignal);

                    var currentTimeSpanMinutes = DateTime.Now.TimeOfDay.TotalMinutes;

                    if (signalTimeSpanMinutes < Math.Abs(currentTimeSpanMinutes - _signalSettingInfo.TimeBufferToTakeOrder) || currentTimeSpanMinutes > Math.Abs(signalTimeSpanMinutes + _signalSettingInfo.TimeBufferToTakeOrder))
                    {
                        Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, "Rejected because buffer time didn't match : " + "Current Time:" + DateTime.Now.TimeOfDay + " $ Generated Time:" + signalTimeSpan + " $ Buffer Time:" + TimeSpan.FromMinutes(signalTimeSpanMinutes));
                        // MessageBox.Show("Rejected because buffer time didn't match : " + "Current Time:" + DateTime.Now.TimeOfDay + " $ Generated Time:" + signalTimeSpan + " $ Buffer Time:" + TimeSpan.FromMinutes(signalTimeSpanMinutes));
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, ex.Message + " " + splitData[timeIndex] + _signalSettingInfo.TimeFormat);
                    return null;
                    //MessageBox.Show(ex.Message + " " + splitData[timeIndex] + _signalSettingInfo.TimeFormat);
                }
                return mode;

            }
            else
            {
                var splitData = signal.Split(new string[] { ",", }, StringSplitOptions.RemoveEmptyEntries);
                OrderMode mode = splitData[_signalSettingInfo.BuySellSignalIndex].Equals("buy", StringComparison.InvariantCultureIgnoreCase) ? OrderMode.BUY : OrderMode.SELL;

                if (oldOrderMode.HasValue && oldOrderMode.Value == mode)
                    return null;
                else
                    return mode;
            }

        }

        internal void UpdateSettingInfo(SignalSettingInfo settingInfo)
        {
            _signalSettingInfo = settingInfo;
        }
    }

}
