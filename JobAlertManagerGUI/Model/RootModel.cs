using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AvalonDock;
using JobAlertManagerGUI.Properties;
using JobAlertManagerGUI.View;
using LumiSoft.Net.Mime;

namespace JobAlertManagerGUI.Model
{
    internal class RootModel : DependencyObject
    {
        public static RootModel CurrentModel = null;

        internal static EMailReader ReaderWindow = null;

        public static readonly DependencyProperty MsgSubjectProperty =
            DependencyProperty.Register("MsgSubject", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty MsgFromProperty =
            DependencyProperty.Register("MsgFrom", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty MsgToProperty =
            DependencyProperty.Register("MsgTo", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty MsgCcProperty =
            DependencyProperty.Register("MsgCc", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty MsgBccProperty =
            DependencyProperty.Register("MsgBcc", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty ReceivedDateTimeProperty =
            DependencyProperty.Register("ReceivedDateTime", typeof(DateTime?), typeof(RootModel),
                new UIPropertyMetadata(default(DateTime?)));

        public static readonly DependencyProperty EntityRootProperty =
            DependencyProperty.Register("EntityRoot", typeof(Mime), typeof(RootModel),
                new UIPropertyMetadata(null, (o, e) => { (o as RootModel).EntityRoot = e.NewValue as Mime; }));

        public static readonly DependencyProperty TextHeaderStrProperty =
            DependencyProperty.Register("TextHeaderStr", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty HtmlHeaderStrProperty =
            DependencyProperty.Register("HtmlHeaderStr", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty RawHeaderStrProperty =
            DependencyProperty.Register("RawHeaderStr", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty OtherAspectsHeaderStrProperty =
            DependencyProperty.Register("OtherAspectsHeaderStr", typeof(string), typeof(RootModel),
                new UIPropertyMetadata(""));

        public static readonly DependencyProperty BriefViewProperty =
            DependencyProperty.Register("BriefView", typeof(bool), typeof(RootModel), new UIPropertyMetadata(true));

        public static readonly DependencyProperty HasAlternativePartsProperty =
            DependencyProperty.Register("HasAlternativeParts", typeof(bool), typeof(RootModel),
                new UIPropertyMetadata(false));

        public static readonly DependencyProperty PlainTextProperty =
            DependencyProperty.Register("PlainText", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty HtmlUriProperty =
            DependencyProperty.Register("HtmlUri", typeof(Uri), typeof(RootModel), new UIPropertyMetadata(null));

        public static readonly DependencyProperty HtmlRefreshTriggerProperty =
            DependencyProperty.Register("HtmlRefreshTrigger", typeof(bool), typeof(RootModel),
                new UIPropertyMetadata(false));

        public static readonly DependencyProperty RawTextProperty =
            DependencyProperty.Register("RawText", typeof(string), typeof(RootModel), new UIPropertyMetadata(""));

        public static readonly DependencyProperty EntryUriProperty = DependencyProperty.RegisterAttached("EntryUri",
            typeof(Uri), typeof(RootModel),
            new FrameworkPropertyMetadata(null, OnEntryUriChanged));

        public static readonly DependencyProperty RefreshProperty = DependencyProperty.RegisterAttached("Refresh",
            typeof(bool), typeof(RootModel),
            new FrameworkPropertyMetadata(false, OnRefreshPropertyChanged));

        private static readonly Dictionary<WebBrowser, bool> brw_state = new Dictionary<WebBrowser, bool>();

        public static string LastDepositFolder
        {
            get => LastDepositFolder;
            set => LastDepositFolder = value;
        }

        internal static DockingManager MainDockMgr => ReaderWindow?.MainDockMgr;

        internal static EMailThreadView ThreadViewer => ReaderWindow?.ThreadView;

        public string MsgAttachmentsWord => Resources.AttachmentsWordQ;

        public string MsgSubjectWord => Resources.MsgSubjectQWord;

        public string MsgSubject
        {
            get => (string) GetValue(MsgSubjectProperty);
            set => SetValue(MsgSubjectProperty, value);
        }

        public string MsgFromWord => Resources.MsgFromQWord;

        public string MsgFrom
        {
            get => (string) GetValue(MsgFromProperty);
            set => SetValue(MsgFromProperty, value);
        }

        public string MsgToWord => Resources.MsgToQWord;

        public string MsgTo
        {
            get => (string) GetValue(MsgToProperty);
            set => SetValue(MsgToProperty, value);
        }

        public string MsgCcWord => Resources.MsgCcQWord;

        public string MsgCc
        {
            get => (string) GetValue(MsgCcProperty);
            set => SetValue(MsgCcProperty, value);
        }

        public string MsgBccWord => Resources.MsgBccQWord;

        public string MsgBcc
        {
            get => (string) GetValue(MsgBccProperty);
            set => SetValue(MsgBccProperty, value);
        }

        public string ReceivedDateTimeWord => Resources.ReceivedDateTimeQWord;

        public DateTime? ReceivedDateTime
        {
            get => (DateTime?) GetValue(ReceivedDateTimeProperty);
            set => SetValue(ReceivedDateTimeProperty, value);
        }

        public Mime EntityRoot
        {
            get => (Mime) GetValue(EntityRootProperty);
            set
            {
                if (EntityRoot != value)
                    SetValue(EntityRootProperty, value);
                //..
                if (value != null)
                {
                    var hc = value.MainEntity.Header;
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
                    var bundisc = false;
                    if (hc.GetFirst("To:") != null)
                    {
                        MsgTo = hc.GetFirst("To:").Value;
                        bundisc = MsgTo.StartsWith("undisclosed") &&
                                  MsgTo.TrimEnd(" ;".ToArray()).EndsWith("recipients:");
                    }

                    if (string.IsNullOrEmpty(MsgTo) || bundisc)
                        if (hc.GetFirst("X-Original-To:") != null)
                            MsgTo = hc.GetFirst("X-Original-To:").Value + "*";
                        else if (hc.GetFirst("Delivered-To:") != null)
                            MsgTo = hc.GetFirst("Delivered-To:").Value + "*"; //??..
                        else if (hc.GetFirst("X-Rcpt-To:") != null)
                            MsgTo = hc.GetFirst("X-Rcpt-To:").Value + "*";
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
                        var strdate = hc.GetFirst("Date:").Value;
                        ReceivedDateTime = MimeUtils.ParseDate(strdate);
                    }
                    else
                    {
                        ReceivedDateTime = default(DateTime?);
                    }
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

        public MimeEntity Entity
        {
            get => HttpContentServer.CurrentEntity;
            set => HttpContentServer.CurrentEntity = value;
        }

        public string TextHeaderStr
        {
            get => (string) GetValue(TextHeaderStrProperty);
            set => SetValue(TextHeaderStrProperty, value);
        }

        public string HtmlHeaderStr
        {
            get => (string) GetValue(HtmlHeaderStrProperty);
            set => SetValue(HtmlHeaderStrProperty, value);
        }

        public string RawHeaderStr
        {
            get => (string) GetValue(RawHeaderStrProperty);
            set => SetValue(RawHeaderStrProperty, value);
        }

        public string OtherAspectsHeaderStr
        {
            get => (string) GetValue(OtherAspectsHeaderStrProperty);
            set => SetValue(OtherAspectsHeaderStrProperty, value);
        }

        public string SaveAttachmentToolTip { get; set; }

        public bool BriefView
        {
            get => (bool) GetValue(BriefViewProperty);
            set => SetValue(BriefViewProperty, value);
        }

        public bool HasAlternativeParts
        {
            get => (bool) GetValue(HasAlternativePartsProperty);
            set => SetValue(HasAlternativePartsProperty, value);
        }

        public string PlainText
        {
            get => (string) GetValue(PlainTextProperty);
            set => SetValue(PlainTextProperty, value);
        }

        public Uri HtmlUri
        {
            get => (Uri) GetValue(HtmlUriProperty);
            set => SetValue(HtmlUriProperty, value);
        }

        public bool HtmlRefreshTrigger
        {
            get => (bool) GetValue(HtmlRefreshTriggerProperty);
            set => SetValue(HtmlRefreshTriggerProperty, value);
        }

        public string RawText
        {
            get => (string) GetValue(RawTextProperty);
            set => SetValue(RawTextProperty, value);
        }

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
            var brw = d as WebBrowser;
            try
            {
                brw.Source = e.NewValue as Uri;
            }
            catch
            {
            }
        }

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
                var brw = d as WebBrowser;
                brw.Refresh();
            }
            catch
            {
            }
        }
    }

    public class MimeWrapper : DependencyObject
    {
        public MimeWrapper(MimeEntity entity)
        {
            Entity = entity;
        }

        public MimeEntity Entity { get; set; }

        public string SaveStr
        {
            get => Resources.SaveWord;
            set { }
        }

        public string ButtonTip
        {
            get => Resources.SaveAttachmentWord;
            set { }
        }
    }
}