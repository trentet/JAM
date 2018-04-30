using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using AvalonDock;
using CryptoGateway.FileSystem.VShell;
using CryptoGateway.FileSystem.VShell.Interfaces;
using JobAlertManagerGUI.Model;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    ///     Interaction logic for EMailReader.xaml
    /// </summary>
    public partial class EMailReader : Window
    {
        private WeakReference _hostPresenter;

        public Action<bool, Window> ClosedHandler = null;

        public bool ForceClose = false;

        public EMailReader()
        {
            InitializeComponent();
            AddHandler(ComponentEvents.TransCompSelectFolderEvent,
                new TransCompSelectFolderEventHandler(OnDepositionSelectFolder));
            Title = Properties.Resources.CGWEMailReaderTitleWord;
        }

        public Uri SourceUri
        {
            get => presenter.SourceUri;
            set
            {
                if (value != null)
                    presenter.UpdateState();
                presenter.SourceUri = value;
            }
        }

        public byte[] SourceData
        {
            set => presenter.SourceData = value;
        }

        internal RootModel Data
        {
            get => presenter.Data;
            set
            {
                presenter.Data = value;
                presenter.DataContext = value;
                PanelHeader.DataContext = value;
            }
        }

        public EMailPresenter HostPresenter
        {
            get => _hostPresenter == null ? null : _hostPresenter.Target as EMailPresenter;
            set => _hostPresenter = new WeakReference(value);
        }

        public EMailThreadView ThreadView => ThrdViewer;

        public DockingManager MainDockMgr => _MainDockMgr;

        public QueryMessageThreadHanlder QueryThread
        {
            set => presenter.QueryThread = value;
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
            var sdoc = "" /*Properties.Settings.Default.EMailReaderLayout*/;
            if (!string.IsNullOrEmpty(sdoc))
            {
                var sr = new StringReader(sdoc);
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

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!ForceClose)
            {
                Hide();
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }

            ClosedHandler?.Invoke(ForceClose, this);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            _MainDockMgr.SaveLayout(sw);
            //Properties.Settings.Default.EMailReaderLayout = sb.ToString();
            //Properties.Settings.Default.Save();
        }
    }
}