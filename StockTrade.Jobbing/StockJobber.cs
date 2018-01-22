using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrade.Jobbing
{
    public class StockJobber
    {
        private static StockJobber _instance = new StockJobber();
        
        public static StockJobber Instance
        {
            get { return _instance; }
        }

        private StockJobber()
        {
        }

        List<JobbingStockBase> _jobbingStocks = new List<JobbingStockBase>();

        public void AddStocks(JobbingStockBase stockBase)
        {
            
        }

        public void Start(JobbingStockBase stockBase, KiteConnect.Kite kite)
        {
            if (stockBase.Status == JobbingStatus.NotStarted)
            {
                var clonedObject = stockBase.CloneObject() as JobbingStockBase;
                if (!_jobbingStocks.Any(s => s.Symbol == clonedObject.Symbol && s.Exchange == clonedObject.Exchange))
                {
                    _jobbingStocks.Add(clonedObject);
                }


                var stock = _jobbingStocks.FirstOrDefault(s => s.Symbol == clonedObject.Symbol && s.Exchange == clonedObject.Exchange);
                if (stock != null)
                {
                    stock.Start(kite);
                }
            }
            else
            {
                Stop(stockBase.Symbol, stockBase.Exchange);
            }
        }

        public void Stop(string symbol, string exchange)
        {
            var stock = _jobbingStocks.FirstOrDefault(s => s.Symbol == symbol && s.Exchange == exchange);
            if (stock != null)
            {

                stock.Stop();
            }
        }


    }
}
