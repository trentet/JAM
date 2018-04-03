using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CryptoGateway.FileSystem.VShell
{
    public delegate void FileNodeProgressRoutedEventHandler(object sender, FileNodeProgressRoutedEventArgs e);

    public class FileNodeProgressRoutedEventArgs : RoutedEventArgs
    {
        public FileNodeProgressRoutedEventArgs()
            : base()
        {
        }

        public FileNodeProgressRoutedEventArgs(RoutedEvent ev) : base(ev) { }

        public FileNodeProgressRoutedEventArgs(RoutedEvent ev, object source) : base(ev, source) { }

        public string EventContext
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }
       
    }
}
