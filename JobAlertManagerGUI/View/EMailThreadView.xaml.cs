using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CryptoGateway.FileSystem.VShell.Interfaces;
using AvalonDock;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    /// Interaction logic for EMailThreadView.xaml
    /// </summary>
    public partial class EMailThreadView : DockableContent
    {
        public ThreadedMessage Root
        {
            set
            {
                _root = value;
                try
                {
                    if (TreeThrd.IsInitialized)
                    {
                        IsTreeInitializing = true;
                        TreeThrd.BeginInit();
                        TreeThrd.ItemsSource = new ThreadedMessage[] { value };
                        TreeThrd.EndInit();
                        IsTreeInitializing = false;
                    }
                }
                catch
                {
                }
            }
        }
        private ThreadedMessage _root = null;
        private bool IsTreeInitializing = false;

        public Action<string> MailSelected = null;

        public Action<bool, DockableContent> ClosedHandler = null;

        public EMailThreadView()
        {
            InitializeComponent();
        }

        public bool IsInThread(string path)
        {
            if (_root == null)
                return false;
            List<ThreadedMessage> pnl = new List<ThreadedMessage>();
            List<ThreadedMessage> cnl = new List<ThreadedMessage>();
            pnl.Add(_root);
            while (true)
            {
                for (int i = 0; i < pnl.Count; i++)
                {
                    if ((pnl[i].MsgDataPath is string) && (pnl[i].MsgDataPath as string).ToLower() == path.ToLower())
                    {
                        pnl[i].IsMsgNodeSelected = true;
                        return true;
                    }
                    foreach (ThreadedMessage cmsg in pnl[i].ReplyMsgs)
                        cnl.Add(cmsg);
                }
                if (cnl.Count == 0)
                    break;
                pnl = cnl;
                cnl = new List<ThreadedMessage>();
            }
            return false;
        }

        private void OnItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsTreeInitializing)
                return;
            if (e.NewValue is ThreadedMessage && MailSelected != null)
            {
                MailSelected((e.NewValue as ThreadedMessage).MsgDataPath);
            }
        }

        
        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ClosedHandler != null)
                ClosedHandler(false, this);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            //Properties.Settings.Default.Save();
        }

        private void OnShowSeqNumber(object sender, RoutedEventArgs e)
        {
            ThreadedMessage[] tml;
            _root.UpdateBranchSeqNumber(!_root.ShowTimeSeqNumber, out tml);
        }
    }
}
