using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrentUtil;

namespace JobAlertManagerGUI.Model
{
    public class LogHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<string> logs = new ObservableCollection<string>();

        public ObservableCollection<string> Logs { get => logs; set => logs = value; }

        public void Clear() { logs.Clear(); }

        public LogHandler()
        {
            logs.CollectionChanged += Logs_Changed;
        }

        private void Logs_Changed(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (!File.Exists(@"C:\Users\Trent\Documents\JAM\Logs.txt"))
            {
                Directory.CreateDirectory(@"C:\Users\Trent\Documents\JAM");
            }
            FileIO.ExportStringsToFile(@"C:\Users\Trent\Documents\JAM\Logs.txt", logs);
        }
    }
}
