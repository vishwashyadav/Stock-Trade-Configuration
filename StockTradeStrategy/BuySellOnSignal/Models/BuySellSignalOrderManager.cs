using KiteConnect;
using StockTrade.Jobbing;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeStrategy.BuySellOnSignal.Models
{
    public class BuySellSignalOrderManager
    {

        private KiteConnect.Kite _kite;
        private static BuySellSignalOrderManager _instance = new BuySellSignalOrderManager();
        public static BuySellSignalOrderManager Instance
        {
            get { return _instance; }
        }

        private BuySellSignalOrderManager()
        {
            
        }
        
        List<BuySellStockOnSignal> _configuresStocks = new List<BuySellStockOnSignal>();
        public void Start(BuySellOnSignalSymbolConfig stock, KiteConnect.Kite kite)
        {
            var clonnedStock = stock.DeepCopy<BuySellOnSignalSymbolConfig>();
            BuySellStockOnSignal signal = new BuySellStockOnSignal(clonnedStock, kite);
            _kite = kite;
            signal.Start();
            _configuresStocks.Add(signal);
            if (_configuresStocks.Count == 1)
            {
                CheckAsync();
                Events.PositionUpdateEvent += Events_PositionUpdateEvent;
            }
        }

        private void Events_PositionUpdateEvent(string exchange, string symbol, KiteConnect.Position position)
        {
            if(_configuresStocks!=null && _configuresStocks.Any())
            {
                var stock = _configuresStocks.FirstOrDefault(s => s.IsMatchSymbol(exchange, symbol));
                if(stock!=null)
                {
                    if(stock.CheckMaxProfitLossAndClosePosition(position))
                    {
                        _configuresStocks.Remove(stock);
                        if (!_configuresStocks.Any())
                            Events.PositionUpdateEvent -= Events_PositionUpdateEvent;
                    }
                }
            }
        }

        private async void CheckAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                check();
            });
        }

        async void check()
        {
            while (true)
            {
                WatchPosition();
                await Task.Delay(1000);
            }
        }

        internal void SquareOffAllPositions(BuySellOnSignalSymbolConfig buySellOnSignalSymbolConfig)
        {
            if (_configuresStocks != null && _configuresStocks.Any())
            {
                var stock = _configuresStocks.FirstOrDefault(s => s.IsMatchSymbol(buySellOnSignalSymbolConfig.Exchange, buySellOnSignalSymbolConfig.Symbol));
                if (stock != null)
                {
                    stock.SquareOffAllPositions();
                }
            }
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
                    stock.UpdateMaxLoss(buySellOnSignalSymbolConfig.MaxProfit);
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
                    stock.CloseAllPosition();
                    _configuresStocks.Remove(stock);
                }
            }
        }

       

        private void WatchPosition()
        {
            try
            {
                var positions = _kite.GetPositions();
                foreach (var position in positions.Day)
                {
                    Events.RaisePositionUpdateEventEvent(position.Exchange, position.TradingSymbol, position);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    public class BuySellStockOnSignal
    {
        int _currentOpenPosition = 0;
        int _countOfContiniousExecutionInOneDirection=0;
        private KiteConnect.Kite _kite;
        Dictionary<int, int> _reversalMultiplier = new Dictionary<int, int>();
        private BuySellOnSignalSymbolConfig _config;
        OrderMode? _oldOrderMode;
        FileSystemWatcher fileWather = new FileSystemWatcher();

        public BuySellStockOnSignal(BuySellOnSignalSymbolConfig config, KiteConnect.Kite kite)
        {
            _config = config;
            _kite = kite; 
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
                _config.Status = StrategyStockStatus.Running;
                Events.RaiseStatusChangedEvent(_config.Exchange, _config.Symbol, _config.Status.ToString());

            }
            catch (Exception ex)
            {
                
            }
        }

        private void FileWather_Changed(object sender, FileSystemEventArgs e)
        {
            Try:
            try
            {
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
                        var latestLines = firstLine.Split(new string[] { _config.Seperator.Trim() }, StringSplitOptions.RemoveEmptyEntries);
                        OrderMode? mode = latestLines[0].Trim().Equals(OrderMode.BUY.ToString(), StringComparison.InvariantCultureIgnoreCase) ? OrderMode.BUY : (latestLines[0].Equals(OrderMode.SELL.ToString(), StringComparison.InvariantCultureIgnoreCase) ? (OrderMode?)OrderMode.SELL : null);
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
            if (reversal.Key != 0)
            {
                quantity = (_config.LotSize * reversal.Value) + _currentOpenPosition;
                _currentOpenPosition = quantity - _currentOpenPosition;
            }
            else
            {
                quantity = _config.LotSize;
                _currentOpenPosition = quantity;
            }
                
            _countOfContiniousExecutionInOneDirection++;

            _oldOrderMode = orderMode;
            try
            {
                var orderResponse = _kite.PlaceOrder(_config.Exchange, _config.Symbol, orderMode.ToString(), quantity.ToString(), Product: "MIS", OrderType: "MARKET");
            }
            catch(Exception ex)
            {

            }
        }
       
        public bool IsMatchSymbol(string exchange, string symbol)
        {
            return _config.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase) && _config.Exchange.Equals(exchange, StringComparison.InvariantCultureIgnoreCase);
        }
        
        public void CloseAllPosition()
        {
            if (_oldOrderMode.HasValue)
            {
                _kite.PlaceOrder(_config.Exchange, _config.Symbol, _oldOrderMode.Value == OrderMode.BUY ? OrderMode.SELL.ToString() : OrderMode.BUY.ToString(), Math.Abs(_currentOpenPosition).ToString(), Product: "MIS", OrderType: "MARKET");
            }
            fileWather.EnableRaisingEvents = false;
            fileWather.Changed -= FileWather_Changed;
        }

        /// <summary>
        /// Returns true if it reaches max loss and max profit
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool CheckMaxProfitLossAndClosePosition(Position position)
        {
            try
            {
                if ((position.PNL >= _config.MaxProfit && _config.MaxProfit != 0 ) || (position.PNL <= -(_config.MaxLoss) && _config.MaxLoss !=0))
                {
                    OrderMode mode = OrderMode.BUY;
                    if (position.Quantity > 0)
                    {
                        mode = OrderMode.SELL;
                    }
                    else
                    {
                        mode = OrderMode.BUY;
                    }
                    var status = _kite.PlaceOrder(_config.Exchange, _config.Symbol, mode.ToString(), Math.Abs(position.Quantity).ToString(), Product: "MIS", OrderType: "MARKET");
                    var orderPlacedSuccessfully = status.Any(s => s.Key.ToLower() == "status" && s.Value.ToLower() == "success");
                    if (orderPlacedSuccessfully)
                    {
                        if (position.PNL > _config.MaxProfit)
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

        internal void SquareOffAllPositions()
        {
            try
            {
                if (_oldOrderMode.HasValue)
                {
                    _kite.PlaceOrder(_config.Exchange, _config.Symbol, _oldOrderMode.Value == OrderMode.BUY ? OrderMode.SELL.ToString() : OrderMode.BUY.ToString(), Math.Abs(_currentOpenPosition).ToString(), Product: "MIS", OrderType: "MARKET");
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
