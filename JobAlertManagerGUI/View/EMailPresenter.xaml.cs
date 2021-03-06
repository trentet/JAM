﻿using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CryptoGateway.FileSystem.VShell.Interfaces;
using JobAlertManagerGUI.Model;
using LumiSoft.Net.Mime;

namespace JobAlertManagerGUI.View
{
    [Export(typeof(IEMailReader))]
    public partial class EMailPresenter : UserControl, IEMailReader
    {
        public static readonly DependencyProperty SourceUriProperty =
            DependencyProperty.Register("SourceUri", typeof(Uri), typeof(EMailPresenter),
                new UIPropertyMetadata(null, (o, e) => { (o as EMailPresenter).SourceUri = e.NewValue as Uri; }));

        public static readonly DependencyProperty IsAlreadyExpandedProperty =
            DependencyProperty.Register("IsAlreadyExpanded", typeof(bool), typeof(EMailPresenter),
                new UIPropertyMetadata(false,
                    (o, e) => { (o as EMailPresenter).IsAlreadyExpanded = (bool) e.NewValue; }));

        private byte[] _sourceData;

        private bool _threadWindowOpened = true;

        private bool ActiveShowThreadWindow;

        private byte[] CurrSourceData;

        private bool DataPending;

        private bool IsExpandedViewOpened;

        private string prev_uri;
        private bool RefreshThreadPending;

        private bool StartBrief = true;
        private bool ThreadSelection;

        /*
        private void OnSelectAttachment(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;
            MimeEntity de = e.AddedItems[0] as MimeEntity;
            TransCompSelectFolderEventArgs ex = new TransCompSelectFolderEventArgs(ComponentEvents.TransCompSelectFolderEvent);
            ex.EventTitle = Properties.Resources.AttachmentSaveDirSelWord;
            ex.InitialFolderPath = LastDepositFolder;
            RaiseEvent(ex);
            if (ex.Handled && ex.IsSelectionMade && !string.IsNullOrEmpty(ex.SelectedFolderPath))
            {
                if (Directory.Exists(ex.SelectedFolderPath))
                {
                    LastDepositFolder = ex.SelectedFolderPath;
                    SaveFile(ex.SelectedFolderPath, de.ContentDisposition_FileName, de);
                }
                else if (ex.SelectedFolderPath.LastIndexOf('\\') != -1)
                {
                    string dir = ex.SelectedFolderPath.Substring(0, ex.SelectedFolderPath.LastIndexOf('\\') + 1);
                    if (Directory.Exists(dir))
                        SaveFile(dir, ex.SelectedFolderPath.Substring(dir.Length), de);
                }
            }
            else
                CmbAttachs.SelectedIndex = -1;
        }

        private void SaveFile(string dir, string fname, MimeEntity entity)
        {
            string filename = dir.TrimEnd('\\') + "\\" + fname;
            bool save = true;
            if (File.Exists(filename))
            {
                MessageBoxResult mr = MessageBox.Show(Properties.Resources.FileExistWarningWords, Properties.Resources.WarningWord, MessageBoxButton.YesNo);
                if (mr != MessageBoxResult.Yes)
                    save = false;
            }
            if (save)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(entity.Data, 0, entity.Data.Length);
                }
            }
        }
        */

        private DataTemplate tmpl_a;
        private DataTemplate tmpl_aa;
        private DataTemplate tmpl_aad;
        private DataTemplate tmpl_ad;
        private DataTemplate tmpl_b;
        private DataTemplate tmpl_bb;
        private DataTemplate tmpl_bbd;
        private DataTemplate tmpl_bd;
        private DataTemplate tmpl_c;
        private DataTemplate tmpl_cc;
        private DataTemplate tmpl_ccd;
        private DataTemplate tmpl_cd;

        private bool UpdateModePending;

        public EMailPresenter()
        {
            InitializeComponent();
            ChkBriefView.Content = Properties.Resources.BriefViewWord;
        }

        public byte[] SourceData
        {
            set
            {
                _sourceData = value;
                DataPending = value != null;
            }
        }

        public bool IsAlreadyExpanded
        {
            get => (bool) GetValue(IsAlreadyExpandedProperty);
            set
            {
                if (IsAlreadyExpanded != value)
                    SetValue(IsAlreadyExpandedProperty, value);
                if (value)
                {
                    BtnFullView.Visibility = Visibility.Collapsed;
                    BtnThreadView.Visibility = Visibility.Visible;
                }
                else
                {
                    BtnFullView.Visibility = Visibility.Visible;
                    BtnThreadView.Visibility = Visibility.Collapsed;
                }
            }
        }

        internal RootModel Data
        {
            get
            {
                if (RootModel.CurrentModel == null)
                {
                    var _data = new RootModel();
                    _data.TextHeaderStr = Properties.Resources.PlainTextWord;
                    _data.HtmlHeaderStr = Properties.Resources.RichTextWord;
                    _data.RawHeaderStr = Properties.Resources.RawTextWord;
                    _data.OtherAspectsHeaderStr = Properties.Resources.MailPropertiesWord;
                    _data.SaveAttachmentToolTip = Properties.Resources.SaveAttachmentWord;
                    RootModel.CurrentModel = _data;
                }

                return RootModel.CurrentModel;
            }
            set
            {
                RootModel.CurrentModel = value;
                if (UpdateModePending)
                    OnCheckBriefView(this, null);
            }
        }

        private string LastDepositFolder => RootModel.LastDepositFolder;

        public bool ThreadWindowOpened
        {
            get => _threadWindowOpened;
            set
            {
                if (_threadWindowOpened != value)
                {
                    _threadWindowOpened = value;
                    BtnThreadView.IsEnabled = !value;
                }
            }
        }

        public Uri SourceUri
        {
            get => (Uri) GetValue(SourceUriProperty);
            set
            {
                if (SourceUri == null && value != null || SourceUri != null && value == null ||
                    SourceUri.ToString() != value.ToString())
                    SetValue(SourceUriProperty, value);
                if (prev_uri == value.ToString())
                    return;
                prev_uri = value.ToString();
                if (IsAlreadyExpanded || !IsExpandedViewOpened)
                {
                    if (IsAlreadyExpanded)
                        if (!ThreadSelection && RootModel.ThreadViewer != null &&
                            !RootModel.ThreadViewer.IsInThread(value.LocalPath))
                            RefreshThreadPending = true;
                    DisplayMsg();
                }
                else
                {
                    RootModel.ReaderWindow.SourceUri = value;
                }
            }
        }

        public QueryMessageThreadHanlder QueryThread { get; set; }

        public void UpdateState()
        {
            OnCheckBriefView(this, null);
        }

        public void UpdateMode()
        {
            UpdateModePending = true;
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            DataContext = Data;
            BtnThreadView.ToolTip = Properties.Resources.ShowMessageThreadDDDToolTip;
            BtnFullView.ToolTip = Properties.Resources.OpenViewWindowDDDToolTip;
            BtnSubDoc.ToolTip = Properties.Resources.SubMessageFoundDDDTip;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataPending)
            {
                DataPending = false;
                DisplayMsg(_sourceData);
            }
        }

        private void OnExpandView(object sender, RoutedEventArgs e)
        {
            if (!IsExpandedViewOpened)
            {
                var isfirst = RootModel.ReaderWindow == null;
                if (isfirst)
                {
                    RootModel.ReaderWindow = new EMailReader();
                    RootModel.ReaderWindow.Title = Properties.Resources.CGWEMailReaderTitleWord;
                    RootModel.ReaderWindow.ClosedHandler = OnFullViewClosed;
                    RootModel.ReaderWindow.Owner = Window.GetWindow(this);
                    RootModel.ReaderWindow.QueryThread = QueryThread;
                    RootModel.ReaderWindow.HostPresenter = this;
                }
                else
                {
                    RootModel.ReaderWindow.UpdateMode();
                }

                //RootModel.ReaderWindow.Data = Data;
                RootModel.ReaderWindow.SourceUri = SourceUri;
                StartBrief = Data.BriefView;
                RootModel.ReaderWindow.Show();
                Visibility = Visibility.Collapsed;
                IsExpandedViewOpened = true;
                if (isfirst)
                    RootModel.ReaderWindow.SourceUri = SourceUri;
            }
        }

        private void OnOpenSubDoc(object sender, RoutedEventArgs e)
        {
            var reader = new EMailReader();
            reader.ForceClose = true;
            reader.SourceData = CurrSourceData;
            reader.Title += "/" + Properties.Resources.SubMessagesWord;
            reader.HostPresenter = this;
            reader.ShowDialog();
        }

        private void OnFullViewClosed(bool IsClosed, Window reader)
        {
            if (!IsClosed)
            {
                Visibility = Visibility.Visible;
                IsExpandedViewOpened = false;
                if (StartBrief != Data.BriefView)
                    OnCheckBriefView(this, null);
            }
        }

        private void OnCheckBriefView(object sender, RoutedEventArgs e)
        {
            if (Data.Entity != null)
            {
                var hasAttachments = Data.EntityRoot.Attachments != null && Data.EntityRoot.Attachments.Length > 0;
                if (!Data.HasAlternativeParts)
                    if (Data.Entity.ContentType == MediaType_enum.Text_plain ||
                        Data.Entity.ContentType == MediaType_enum.NotSpecified)
                        view.ContentTemplate = Data.BriefView
                            ? (hasAttachments ? GetTemplate("aad") : GetTemplate("aa"))
                            : (hasAttachments ? GetTemplate("ad") : GetTemplate("a"));
                    else
                        view.ContentTemplate = Data.BriefView
                            ? (hasAttachments ? GetTemplate("bbd") : GetTemplate("bb"))
                            : (hasAttachments ? GetTemplate("bd") : GetTemplate("b"));
                else
                    view.ContentTemplate = Data.BriefView
                        ? (hasAttachments ? GetTemplate("ccd") : GetTemplate("cc"))
                        : (hasAttachments ? GetTemplate("cd") : GetTemplate("c"));
                UpdateModePending = false;
            }
        }

        private void DisplayMsg()
        {
            if (SourceUri == null)
            {
                ClearView();
                return;
            }

            if (File.Exists(SourceUri.LocalPath))
            {
                var sr = new StreamReader(SourceUri.LocalPath);
                Data.RawText = sr.ReadToEnd();
                sr.Close();
                var bf = Encoding.ASCII.GetBytes(Data.RawText);
                DisplayMsg(bf);
            }
            else
            {
                ClearView();
            }
        }

        private void DisplayMsg(byte[] bf)
        {
            Mime m;
            try
            {
                m = Mime.Parse(bf);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(SourceUri);
                Trace.WriteLine(ex.Message);
                return;
            }

            Data.EntityRoot = m;
            if (GetSubDocData(m.MainEntity))
                BtnSubDoc.Visibility = Visibility.Visible;
            else
                BtnSubDoc.Visibility = Visibility.Collapsed;
            var hasAttachments = m.Attachments != null && m.Attachments.Length > 0;
            /*
            if (hasAttachments)
            {
                DPAttach.Visibility = System.Windows.Visibility.Visible;
                CmbAttachs.SelectedIndex = -1;
                CmbAttachs.ItemsSource = attachments;
            }
            else
            {
                DPAttach.Visibility = System.Windows.Visibility.Collapsed;
                CmbAttachs.ItemsSource = null;
            }
            */
            if (RefreshThreadPending)
            {
                ShowMsgThreadView();
                RefreshThreadPending = false;
            }

            var centity = GetDocEntity(m.MainEntity);
            if (centity == null) centity = m.MainEntity;
            Data.HasAlternativeParts = centity != null && centity.ParentEntity != null &&
                                       centity.ParentEntity.ContentType == MediaType_enum.Multipart_alternative &&
                                       centity.ParentEntity.ChildEntities.Count > 1;
            if (!Data.HasAlternativeParts)
            {
                if (centity.ContentType == MediaType_enum.Text_plain ||
                    centity.ContentType == MediaType_enum.NotSpecified)
                {
                    view.ContentTemplate = Data.BriefView
                        ? (hasAttachments ? GetTemplate("aad") : GetTemplate("aa"))
                        : (hasAttachments ? GetTemplate("ad") : GetTemplate("a"));
                    view.Content = DataContext;
                    Data.PlainText = m.BodyText;
                    Data.Entity = centity;
                }
                else
                {
                    MimeEntity tentity = null;
                    if (centity.ContentType != MediaType_enum.Multipart_related)
                        Data.Entity = centity;
                    else
                        foreach (MimeEntity ce in centity.ChildEntities)
                            if (ce.ContentType == MediaType_enum.Multipart_alternative)
                            {
                                tentity = centity;
                                Data.HasAlternativeParts = true;
                                foreach (MimeEntity cce in ce.ChildEntities)
                                    if (cce.ContentType == MediaType_enum.Text_plain)
                                        tentity = cce;
                                    else if (IsContentBlock(cce))
                                        Data.Entity = cce;
                                    else if (ce.ContentType == MediaType_enum.Multipart_related)
                                        foreach (MimeEntity ccce in cce.ChildEntities)
                                            if (IsContentBlock(ccce))
                                                Data.Entity = ccce;
                            }
                            else
                            {
                                if (ce.ContentID == null && IsContentBlock(ce))
                                {
                                    Data.Entity = ce;
                                    break;
                                }
                            }

                    if (Data.HasAlternativeParts)
                        view.ContentTemplate = Data.BriefView
                            ? (hasAttachments ? GetTemplate("ccd") : GetTemplate("cc"))
                            : (hasAttachments ? GetTemplate("cd") : GetTemplate("c"));
                    else
                        view.ContentTemplate = Data.BriefView
                            ? (hasAttachments ? GetTemplate("bbd") : GetTemplate("bb"))
                            : (hasAttachments ? GetTemplate("bd") : GetTemplate("b"));
                    view.Content = DataContext;
                    Data.PlainText =
                        tentity != null && (tentity.ContentType == MediaType_enum.Text_plain ||
                                            tentity.ContentType == MediaType_enum.NotSpecified)
                            ? tentity.DataText
                            : "";
                    ServerHtml();
                }
            }
            else
            {
                var tentity = centity;
                foreach (MimeEntity ce in centity.ParentEntity.ChildEntities)
                    if (ce.ContentType == MediaType_enum.Text_plain)
                        tentity = ce;
                    else if (IsContentBlock(ce))
                        Data.Entity = ce;
                    else if (ce.ContentType == MediaType_enum.Multipart_related)
                        foreach (MimeEntity cce in ce.ChildEntities)
                            if (IsContentBlock(cce))
                                Data.Entity = cce;
                view.ContentTemplate = Data.BriefView
                    ? (hasAttachments ? GetTemplate("ccd") : GetTemplate("cc"))
                    : (hasAttachments ? GetTemplate("cd") : GetTemplate("c"));
                view.Content = DataContext;
                Data.PlainText =
                    tentity != null && (tentity.ContentType == MediaType_enum.Text_plain ||
                                        tentity.ContentType == MediaType_enum.NotSpecified)
                        ? tentity.DataText
                        : "";
                ServerHtml();
            }
        }

        private bool IsContentBlock(MimeEntity entity)
        {
            return entity.ContentType == MediaType_enum.Text_html ||
                   entity.ContentType == MediaType_enum.Text_plain ||
                   entity.ContentType == MediaType_enum.Text_rtf ||
                   entity.ContentType == MediaType_enum.Text_xml ||
                   entity.ContentType == MediaType_enum.NotSpecified;
        }

        private bool GetSubDocData(MimeEntity entity)
        {
            if (entity.ContentType == MediaType_enum.Message_rfc822 &&
                entity.ContentDisposition == ContentDisposition_enum.Inline)
            {
                CurrSourceData = entity.Data;
                return true;
            }

            foreach (MimeEntity ce in entity.ChildEntities)
                if (GetSubDocData(ce))
                    return true;
            return false;
        }

        private MimeEntity GetDocEntity(MimeEntity entity)
        {
            if (IsContentBlock(entity) || entity.ContentType == MediaType_enum.Multipart_related)
                return entity;
            foreach (MimeEntity ce in entity.ChildEntities)
            {
                var centity = GetDocEntity(ce);
                if (centity != null)
                    return centity;
            }

            return null;
        }

        private DataTemplate GetTemplate(string kind)
        {
            switch (kind.ToLower())
            {
                case "a":
                    if (tmpl_a == null)
                        tmpl_a = FindResource("A") as DataTemplate;
                    return tmpl_a;
                case "ad":
                    if (tmpl_ad == null)
                        tmpl_ad = FindResource("AD") as DataTemplate;
                    return tmpl_ad;
                case "aa":
                    if (tmpl_aa == null)
                        tmpl_aa = FindResource("AA") as DataTemplate;
                    return tmpl_aa;
                case "aad":
                    if (tmpl_aad == null)
                        tmpl_aad = FindResource("AAD") as DataTemplate;
                    return tmpl_aad;
                case "b":
                    if (tmpl_b == null)
                        tmpl_b = FindResource("B") as DataTemplate;
                    return tmpl_b;
                case "bd":
                    if (tmpl_bd == null)
                        tmpl_bd = FindResource("BD") as DataTemplate;
                    return tmpl_bd;
                case "bb":
                    if (tmpl_bb == null)
                        tmpl_bb = FindResource("BB") as DataTemplate;
                    return tmpl_bb;
                case "bbd":
                    if (tmpl_bbd == null)
                        tmpl_bbd = FindResource("BBD") as DataTemplate;
                    return tmpl_bbd;
                case "c":
                    if (tmpl_c == null)
                        tmpl_c = FindResource("C") as DataTemplate;
                    return tmpl_c;
                case "cd":
                    if (tmpl_cd == null)
                        tmpl_cd = FindResource("CD") as DataTemplate;
                    return tmpl_cd;
                case "cc":
                    if (tmpl_cc == null)
                        tmpl_cc = FindResource("CC") as DataTemplate;
                    return tmpl_cc;
                case "ccd":
                    if (tmpl_ccd == null)
                        tmpl_ccd = FindResource("CCD") as DataTemplate;
                    return tmpl_ccd;
            }

            return null;
        }

        private void ServerHtml()
        {
            HttpContentServer.State = HttpContentServerState.Message;
            if (!HttpContentServer.IsServing)
            {
                Action act = () => { HttpContentServer.Start(); };
                act.BeginInvoke(ar => { SetBrowseUri(); }, null);
            }
            else
            {
                Data.HtmlUri = new Uri(HttpContentServer.Prefix);
            }
        }

        private void SetBrowseUri()
        {
            if (!Dispatcher.CheckAccess())
            {
                while (!HttpContentServer.IsServing) Thread.Sleep(10);
                Dispatcher.Invoke(new Action(SetBrowseUri), null);
            }
            else
            {
                if (Data.HtmlUri == null || Data.HtmlUri.ToString().ToLower() != HttpContentServer.Prefix.ToLower())
                    Data.HtmlUri = new Uri(HttpContentServer.Prefix);
                else
                    Data.HtmlRefreshTrigger = !Data.HtmlRefreshTrigger;
            }
        }

        private void ClearView()
        {
            Data.PlainText = "";
            Data.HtmlUri = null;
            Data.RawText = "";
            view.Content = null;
            //DPAttach.Visibility = System.Windows.Visibility.Collapsed;
        }


        private void OnShowThreadView(object sender, RoutedEventArgs e)
        {
            ActiveShowThreadWindow = true;
            ShowMsgThreadView();
            ActiveShowThreadWindow = false;
        }

        public void ShowMsgThreadView()
        {
            if (QueryThread != null)
            {
                ThreadedMessage start = null;
                if (QueryThread(Data.EntityRoot.MainEntity.MessageID, out start))
                {
                    BtnThreadView.IsEnabled = start.ReplyMsgs.Count() > 0;
                    if (BtnThreadView.IsEnabled)
                        if (RootModel.ThreadViewer.MailSelected == null)
                        {
                            RootModel.ThreadViewer.MailSelected = path =>
                            {
                                if (!string.IsNullOrEmpty(path))
                                {
                                    ThreadSelection = true;
                                    SourceUri = new Uri(path);
                                    ThreadSelection = false;
                                }
                            };
                            RootModel.ThreadViewer.ClosedHandler = (b, w) =>
                            {
                                BtnThreadView.IsEnabled = true;
                                ThreadWindowOpened = false;
                            };
                        }

                    RootModel.ThreadViewer.Root = start;
                    if (!ThreadWindowOpened)
                        if (ActiveShowThreadWindow)
                        {
                            RootModel.ThreadViewer.Show();
                            BtnThreadView.IsEnabled = false;
                            ThreadWindowOpened = true;
                        }
                }
            }
        }
    }
}