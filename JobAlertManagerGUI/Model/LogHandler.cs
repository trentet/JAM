using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using TrentUtil;

namespace JobAlertManagerGUI.Model
{
    public class LogHandler : INotifyPropertyChanged
    {
        public LogHandler()
        {
            Logs.CollectionChanged += Logs_Changed;
        }

        public ObservableCollection<string> Logs { get; set; } = new ObservableCollection<string>();
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Clear()
        {
            Logs.Clear();
        }

        private void Logs_Changed(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (!File.Exists(@"C:\Users\Trent\Documents\JAM\Logs.txt"))
                Directory.CreateDirectory(@"C:\Users\Trent\Documents\JAM");
            FileIO.ExportStringsToFile(@"C:\Users\Trent\Documents\JAM\Logs.txt", Logs);
        }
    }
}