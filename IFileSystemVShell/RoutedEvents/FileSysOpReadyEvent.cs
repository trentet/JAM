using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CryptoGateway.FileSystem.VShell
{
    public delegate void FileSysOpReadyRoutedEventHandler(object sender, FileSysOpReadyEventArgs e);

    public class FileSysOpReadyEventArgs : RoutedEventArgs
    {
        public FileSysOpReadyEventArgs()
            : base()
        {
        }

        public FileSysOpReadyEventArgs(RoutedEvent ev) : base(ev) { }

        public FileSysOpReadyEventArgs(RoutedEvent ev, object source) : base(ev, source) { }

    }
}
