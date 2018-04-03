using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CryptoGateway.FileSystem.VShell
{
    public class TransCompSelectFolderEventArgs : RoutedEventArgs
    {
        public TransCompSelectFolderEventArgs()
            : base()
        {
        }

        public TransCompSelectFolderEventArgs(RoutedEvent ev) : base(ev) { }

        public TransCompSelectFolderEventArgs(RoutedEvent ev, object source) : base(ev, source) { }

        public string EventTitle
        {
            get;
            set;
        }

        public string InitialFolderPath
        {
            get;
            set;
        }

        public string SelectedFolderPath
        {
            get;
            set;
        }

        public bool IsSelectionMade = false;
    }

    public delegate void TransCompSelectFolderEventHandler(object sender, TransCompSelectFolderEventArgs e);

    public class ComponentEvents
    {
        public static readonly RoutedEvent MediaFinishedEvent = EventManager.RegisterRoutedEvent("MediaFinishedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ComponentEvents));
        public static readonly RoutedEvent TransCompSelectFolderEvent = EventManager.RegisterRoutedEvent("TransCompSelectFolderEvent", RoutingStrategy.Bubble, typeof(TransCompSelectFolderEventHandler), typeof(ComponentEvents));
    }
}
