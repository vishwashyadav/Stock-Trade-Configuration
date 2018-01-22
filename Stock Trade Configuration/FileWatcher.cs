
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration
{
    public class FileWatcher
    {
        private FileSystemWatcher _watcher = new FileSystemWatcher();
        private static FileWatcher _instance = new FileWatcher();
       public static FileWatcher Instance
        {
            get { return _instance; }
        }

        private FileWatcher()
        {
          
        }

       

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var path = Path.GetFileName(e.FullPath);
            Events.RaiseFileContentChangedEvent(path);
        }

        internal void StartWatch()
        {
            _watcher = new FileSystemWatcher()
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Path = AppDomain.CurrentDomain.BaseDirectory,
                Filter ="*.txt"
            };
            _watcher.EnableRaisingEvents = true;
            _watcher.Changed += Watcher_Changed;
        }

        internal void StopWatch()
        {
         if(_watcher!=null)
            {
                _watcher.Changed -= Watcher_Changed;
            }
        }
    }
}
