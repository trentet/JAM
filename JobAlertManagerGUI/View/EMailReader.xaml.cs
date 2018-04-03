using System;
using System.IO;
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
using System.Xml;
using CryptoGateway.FileSystem.VShell.Interfaces;
using CryptoGateway.FileSystem.VShell;
using AvalonDock;
using JobAlertManagerGUI.Model;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    /// Interaction logic for EMailReader.xaml
    /// </summary>
    public partial class EMailReader : Window
    {
        public Uri SourceUri
        {
            get { return presenter.SourceUri; }
            set
            {
                if (value != null)
                    presenter.UpdateState();
                presenter.SourceUri = value;
            }
        }

        public byte[] SourceData
        {
            set { presenter.SourceData = value; }
        }

        internal RootModel Data
        {
            get
            {
                return presenter.Data;
            }
            set
            {
                presenter.Data = value;
                presenter.DataContext = value;
                PanelHeader.DataContext = value;
            }
        }

        public EMailPresenter HostPresenter
        {
            get { return _hostPresenter == null ? null : _hostPresenter.Target as EMailPresenter; }
            set { _hostPresenter = new WeakReference(value); }
        }
        private WeakReference _hostPresenter = null;

        public EMailThreadView ThreadView
        {
            get { return ThrdViewer; }
        }

        public DockingManager MainDockMgr
        {
            get { return _MainDockMgr; }
        }

        public QueryMessageThreadHanlder QueryThread
        {
            set { presenter.QueryThread = value; }
        }

        public Action<bool, Window> ClosedHandler = null;

        public bool ForceClose = false;

        public EMailReader()
        {
            InitializeComponent();
            AddHandler(ComponentEvents.TransCompSelectFolderEvent, new TransCompSelectFolderEventHandler(OnDepositionSelectFolder));
            Title = Properties.Resources.CGWEMailReaderTitleWord;
        }

        public void UpdateMode()
        {
            presenter.UpdateMode();
        }

        private void OnDepositionSelectFolder(object sender, TransCompSelectFolderEventArgs e)
        {
            if (HostPresenter != null)
                HostPresenter.RaiseEvent(e);
        }

        private void OnThreadViewStateChanged(object sender, RoutedEventArgs e)
        {
            if (ThrdViewer.State == DockableContentState.Hidden)
                presenter.ThreadWindowOpened = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PanelHeader.DataContext = presenter.Data;
            DocContent.Title = "test"; //Properties.Resources.ContentWord;
        }

        private void MainDockManagerLoaded(object sender, RoutedEventArgs e)
        {
            string sdoc = ""/*Properties.Settings.Default.EMailReaderLayout*/;
            if (!string.IsNullOrEmpty(sdoc))
            {
                StringReader sr = new StringReader(sdoc);
                try
                {
                    _MainDockMgr.RestoreLayout(sr);
                }
                catch
                {
                }
                finally
                {
                    sr.Close();
                }
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ForceClose)
            {
                Hide();
                e.Cancel = true;
            }
            else
                e.Cancel = false;
            ClosedHandler?.Invoke(ForceClose, this);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            _MainDockMgr.SaveLayout(sw);
            //Properties.Settings.Default.EMailReaderLayout = sb.ToString();
            //Properties.Settings.Default.Save();
        }

    }
}
