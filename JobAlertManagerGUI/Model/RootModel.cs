using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using LumiSoft.Net.Mime;
using AvalonDock;
using JobAlertManagerGUI.View;

namespace JobAlertManagerGUI.Model
{
    internal class RootModel : DependencyObject
    {
        public static RootModel CurrentModel = null;

        public static string LastDepositFolder
        {
            get 
            {
                //string folderDir = @"C:\";
                return LastDepositFolder;
            }
            set 
            {
                //if (value != LastDepositFolder)
                //{
                    LastDepositFolder = value;//Properties.Settings.Default.LastDepositeFolder = value;
                    //Properties.Settings.Default.Save();
                //}
            }
        }

        internal static EMailReader ReaderWindow = null;

        internal static DockingManager MainDockMgr
        {
            get { return ReaderWindow?.MainDockMgr; }
        }

        internal static EMailThreadView ThreadViewer
        {
            get { return ReaderWindow?.ThreadView; }
        }

        public string MsgAttachmentsWord
        {
            get { return Properties.Resources.AttachmentsWordQ; }
        }

        public string MsgSubjectWord
        {
            get { return Properties.Resources.MsgSubjectQWord; }
        }

        public string MsgSubject
        {
            get { return (string)GetValue(MsgSubjectProperty); }
            set { SetValue(MsgSubjectProperty, value); }
        }
        public static readonly DependencyProperty MsgSubjectProperty =
            DependencyProperty.Register("MsgSubject", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public string MsgFromWord
        {
            get { return Properties.Resources.MsgFromQWord; }
        }

        public string MsgFrom
        {
            get { return (string)GetValue(MsgFromProperty); }
            set { SetValue(MsgFromProperty, value); }
        }
        public static readonly DependencyProperty MsgFromProperty =
            DependencyProperty.Register("MsgFrom", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public string MsgToWord
        {
            get { return Properties.Resources.MsgToQWord; }
        }

        public string MsgTo
        {
            get { return (string)GetValue(MsgToProperty); }
            set { SetValue(MsgToProperty, value); }
        }
        public static readonly DependencyProperty MsgToProperty =
            DependencyProperty.Register("MsgTo", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public string MsgCcWord
        {
            get { return Properties.Resources.MsgCcQWord; }
        }

        public string MsgCc
        {
            get { return (string)GetValue(MsgCcProperty); }
            set { SetValue(MsgCcProperty, value); }
        }
        public static readonly DependencyProperty MsgCcProperty =
            DependencyProperty.Register("MsgCc", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public string MsgBccWord
        {
            get { return Properties.Resources.MsgBccQWord; }
        }

        public string MsgBcc
        {
            get { return (string)GetValue(MsgBccProperty); }
            set { SetValue(MsgBccProperty, value); }
        }
        public static readonly DependencyProperty MsgBccProperty =
            DependencyProperty.Register("MsgBcc", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public string ReceivedDateTimeWord
        {
            get { return Properties.Resources.ReceivedDateTimeQWord; }
        }

        public DateTime? ReceivedDateTime
        {
            get { return (DateTime?)GetValue(ReceivedDateTimeProperty); }
            set { SetValue(ReceivedDateTimeProperty, value); }
        }
        public static readonly DependencyProperty ReceivedDateTimeProperty =
            DependencyProperty.Register("ReceivedDateTime", typeof(DateTime?), typeof(RootModel), new UIPropertyMetadata(default(DateTime?)));

        public LumiSoft.Net.Mime.Mime EntityRoot
        {
            get { return (LumiSoft.Net.Mime.Mime)GetValue(EntityRootProperty); }
            set 
            {
                if (EntityRoot != value)
                    SetValue(EntityRootProperty, value);
                //..
                if (value != null)
                {
                    HeaderFieldCollection hc = value.MainEntity.Header;
                    if (hc.GetFirst("Subject:") != null)
                        MsgSubject = hc.GetFirst("Subject:").Value;
                    else
                        MsgSubject = "";
                    if (hc.GetFirst("From:") != null)
                        MsgFrom = hc.GetFirst("From:").Value;
                    else
                        MsgFrom = "";
                    //if (MsgFrom.Contains("simon") && MsgFrom.Contains("dornoo"))
                    //{
                    //    int brk = 0;
                    //}
                    MsgTo = "";
                    bool bundisc = false;
                    if (hc.GetFirst("To:") != null)
                    {
                        MsgTo = hc.GetFirst("To:").Value;
                        bundisc = MsgTo.StartsWith("undisclosed") && MsgTo.TrimEnd(" ;".ToArray()).EndsWith("recipients:");
                    }
                    if (string.IsNullOrEmpty(MsgTo) || bundisc)
                    {
                        if (hc.GetFirst("X-Original-To:") != null)
                            MsgTo = hc.GetFirst("X-Original-To:").Value + "*";
                        else if (hc.GetFirst("Delivered-To:") != null)
                            MsgTo = hc.GetFirst("Delivered-To:").Value + "*"; //??..
                        else if (hc.GetFirst("X-Rcpt-To:") != null)
                            MsgTo = hc.GetFirst("X-Rcpt-To:").Value + "*";
                    }
                    if (hc.GetFirst("Cc:") != null)
                        MsgCc = hc.GetFirst("Cc:").Value;
                    else
                        MsgCc = "";
                    if (hc.GetFirst("Bcc:") != null)
                        MsgBcc = hc.GetFirst("Bcc:").Value;
                    else
                        MsgBcc = "";
                    if (hc.GetFirst("Date:") != null)
                    {
                        string strdate = hc.GetFirst("Date:").Value;
                        ReceivedDateTime = MimeUtils.ParseDate(strdate);
                    }
                    else
                        ReceivedDateTime = default(DateTime?);
                }
                else
                {
                    MsgSubject = "";
                    MsgFrom = "";
                    MsgTo = "";
                    MsgCc = "";
                    ReceivedDateTime = default(DateTime?);
                }
                //..
            }
        }
        public static readonly DependencyProperty EntityRootProperty =
            DependencyProperty.Register("EntityRoot", typeof(LumiSoft.Net.Mime.Mime), typeof(RootModel), new UIPropertyMetadata(null, (o, e) => {
                (o as RootModel).EntityRoot = e.NewValue as LumiSoft.Net.Mime.Mime;
            }));

        public LumiSoft.Net.Mime.MimeEntity Entity
        {
            get { return HttpContentServer.CurrentEntity; }
            set { HttpContentServer.CurrentEntity = value; }
        }

        public string TextHeaderStr
        {
            get { return (string)GetValue(TextHeaderStrProperty); }
            set { SetValue(TextHeaderStrProperty, value); }
        }
        public static readonly DependencyProperty TextHeaderStrProperty =
            DependencyProperty.Register("TextHeaderStr", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public string HtmlHeaderStr
        {
            get { return (string)GetValue(HtmlHeaderStrProperty); }
            set { SetValue(HtmlHeaderStrProperty, value); }
        }
        public static readonly DependencyProperty HtmlHeaderStrProperty =
            DependencyProperty.Register("HtmlHeaderStr", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public string RawHeaderStr
        {
            get { return (string)GetValue(RawHeaderStrProperty); }
            set { SetValue(RawHeaderStrProperty, value); }
        }
        public static readonly DependencyProperty RawHeaderStrProperty =
            DependencyProperty.Register("RawHeaderStr", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public string OtherAspectsHeaderStr
        {
            get { return (string)GetValue(OtherAspectsHeaderStrProperty); }
            set { SetValue(OtherAspectsHeaderStrProperty, value); }
        }
        public static readonly DependencyProperty OtherAspectsHeaderStrProperty =
            DependencyProperty.Register("OtherAspectsHeaderStr", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public string SaveAttachmentToolTip
        {
            get;
            set;
        }

        public bool BriefView
        {
            get { return (bool)GetValue(BriefViewProperty); }
            set { SetValue(BriefViewProperty, value); }
        }
        public static readonly DependencyProperty BriefViewProperty =
            DependencyProperty.Register("BriefView", typeof(bool), typeof(RootModel), new UIPropertyMetadata(true));
       
        public bool HasAlternativeParts
        {
            get { return (bool)GetValue(HasAlternativePartsProperty); }
            set { SetValue(HasAlternativePartsProperty, value); }
        }
        public static readonly DependencyProperty HasAlternativePartsProperty =
            DependencyProperty.Register("HasAlternativeParts", typeof(bool), typeof(RootModel), new UIPropertyMetadata(false));

        public string PlainText
        {
            get { return (string)GetValue(PlainTextProperty); }
            set { SetValue(PlainTextProperty, value); }
        }
        public static readonly DependencyProperty PlainTextProperty =
            DependencyProperty.Register("PlainText", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public Uri HtmlUri
        {
            get { return (Uri)GetValue(HtmlUriProperty); }
            set { SetValue(HtmlUriProperty, value); }
        }
        public static readonly DependencyProperty HtmlUriProperty =
            DependencyProperty.Register("HtmlUri", typeof(Uri), typeof(RootModel), new UIPropertyMetadata(null));

        public bool HtmlRefreshTrigger
        {
            get { return (bool)GetValue(HtmlRefreshTriggerProperty); }
            set { SetValue(HtmlRefreshTriggerProperty, value); }
        }
        public static readonly DependencyProperty HtmlRefreshTriggerProperty =
            DependencyProperty.Register("HtmlRefreshTrigger", typeof(bool), typeof(RootModel), new UIPropertyMetadata(false));

        public string RawText
        {
            get { return (string)GetValue(RawTextProperty); }
            set { SetValue(RawTextProperty, value); }
        }
        public static readonly DependencyProperty RawTextProperty =
            DependencyProperty.Register("RawText", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty EntryUriProperty = DependencyProperty.RegisterAttached("EntryUri", 
            typeof(Uri), typeof(RootModel),
            new FrameworkPropertyMetadata(null, OnEntryUriChanged));

        public static void SetEntryUri(WebBrowser browser, Uri value)
        {
            try
            {
                browser.Source = value;
            }
            catch
            {
            }
        }

        public static Uri GetEntryUri(WebBrowser browser)
        {
            return browser.Source;
        }

        private static void OnEntryUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            WebBrowser brw = d as WebBrowser;
            try
            {
                brw.Source = e.NewValue as Uri;
            }
            catch
            {
            }
        }

        public static readonly DependencyProperty RefreshProperty = DependencyProperty.RegisterAttached("Refresh",
            typeof(bool), typeof(RootModel),
            new FrameworkPropertyMetadata(false, OnRefreshPropertyChanged));

        private static Dictionary<WebBrowser, bool> brw_state = new Dictionary<WebBrowser, bool>();

        public static void SetRefresh(WebBrowser browser, bool value)
        {
            brw_state[browser] = value;
        }

        public static bool GetRefresh(WebBrowser browser)
        {
            return brw_state.ContainsKey(browser) ? brw_state[browser] : false;
        }

        private static void OnRefreshPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                WebBrowser brw = d as WebBrowser;
                brw.Refresh();
            }
            catch
            { 
            }
        }
    }

    public class MimeWrapper : DependencyObject
    {
        public MimeWrapper(LumiSoft.Net.Mime.MimeEntity entity)
        {
            Entity = entity;
        }

        public LumiSoft.Net.Mime.MimeEntity Entity
        {
            get;
            set;
        }

        public string SaveStr
        {
            get { return Properties.Resources.SaveWord; }
            set { }
        }

        public string ButtonTip
        {
            get { return Properties.Resources.SaveAttachmentWord; }
            set { }
        }
    }
}
