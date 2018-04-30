using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using AvalonDock;
using CryptoGateway.FileSystem.VShell.Interfaces;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    ///     Interaction logic for EMailThreadView.xaml
    /// </summary>
    public partial class EMailThreadView : DockableContent
    {
        private ThreadedMessage _root;

        public Action<bool, DockableContent> ClosedHandler = null;
        private bool IsTreeInitializing;

        public Action<string> MailSelected = null;

        public EMailThreadView()
        {
            InitializeComponent();
        }

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
                        TreeThrd.ItemsSource = new[] {value};
                        TreeThrd.EndInit();
                        IsTreeInitializing = false;
                    }
                }
                catch
                {
                }
            }
        }

        public bool IsInThread(string path)
        {
            if (_root == null)
                return false;
            var pnl = new List<ThreadedMessage>();
            var cnl = new List<ThreadedMessage>();
            pnl.Add(_root);
            while (true)
            {
                for (var i = 0; i < pnl.Count; i++)
                {
                    if (pnl[i].MsgDataPath is string && pnl[i].MsgDataPath.ToLower() == path.ToLower())
                    {
                        pnl[i].IsMsgNodeSelected = true;
                        return true;
                    }

                    foreach (var cmsg in pnl[i].ReplyMsgs)
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
                MailSelected((e.NewValue as ThreadedMessage).MsgDataPath);
        }


        private void OnClosing(object sender, CancelEventArgs e)
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