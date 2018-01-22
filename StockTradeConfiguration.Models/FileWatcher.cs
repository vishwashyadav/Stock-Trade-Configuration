using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Models
{
    public class FileWatcher
    {
        public delegate void FileContentChangedEventHandler(string filePath);
        public event FileContentChangedEventHandler FileContentChangedEvent;

        private string FolderPath { get; set; }
        public string FileExtenstion { get; private set; }

        private FileSystemWatcher _watcher = new FileSystemWatcher();

        public FileWatcher(string folderPath, string extenstion)
        {
            FolderPath = folderPath;
            FileExtenstion = extenstion;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if(FileContentChangedEvent!= null)
            {
                FileContentChangedEvent(e.FullPath);
            }
        }

        public void StartWatch()
        {
            _watcher = new FileSystemWatcher()
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Path = FolderPath,
                Filter = FileExtenstion.StartsWith("*.") ? "*." + FileExtenstion.Trim(new char[] { '*', '.' }) : FileExtenstion
            };
            _watcher.EnableRaisingEvents = true;
            _watcher.Changed += Watcher_Changed;
        }

        internal void StopWatch()
        {
            if (_watcher != null)
            {
                _watcher.Changed -= Watcher_Changed;
            }
        }
    }
}
