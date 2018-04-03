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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LumiSoft.Net.Mime;
using CryptoGateway.FileSystem.VShell;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    /// Interaction logic for EMailMetaInfo.xaml
    /// </summary>
    public partial class EMailMetaDetails : UserControl
    {
        public EMailMetaDetails()
        {
            InitializeComponent();
            GBA.Header = Properties.Resources.ReplyToWord;
            GBC.Header = Properties.Resources.RoutingInfoWord;
            GBD.Header = Properties.Resources.DispositionNotificationWord;
            GBE.Header = Properties.Resources.OtherInfoWord;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LumiSoft.Net.Mime.Mime m = DataContext as LumiSoft.Net.Mime.Mime;
            if (m != null)
            {
                if (m.MainEntity.ReplyTo != null)
                {
                    TxtReplyTo.Text = m.MainEntity.ReplyTo.ToAddressListString();
                    GBA.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    GBA.Visibility = System.Windows.Visibility.Collapsed;
                /*
                var attachments = m.Attachments == null ? null : from d in m.Attachments where !string.IsNullOrEmpty(d.ContentDisposition_FileName) select d;
                if (attachments == null || attachments.Count() == 0)
                    GBB.Visibility = System.Windows.Visibility.Collapsed;
                else
                {
                    GBB.Visibility = System.Windows.Visibility.Visible;
                    LstAttach.ItemsSource = from d in attachments select new MimeWrapper(d);
                }
                */
                string srt = "";
                if (m.MainEntity.Header.GetFirst("Return-Path:") != null)
                    srt += "Return-Path: " + m.MainEntity.Header.GetFirst("Return-Path:").Value;
                foreach (HeaderField hf in m.MainEntity.Header)
                {
                    if (hf.Name == "Received:")
                        srt += hf.Name + " " + hf.Value + "\r\n";
                }
                if (!string.IsNullOrEmpty(srt))
                {
                    TxtRouting.Text = srt.TrimEnd(" \t\r\n".ToCharArray());
                    GBC.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    GBC.Visibility = System.Windows.Visibility.Collapsed;
                if (m.MainEntity.DSN != null)
                {
                    TxtMDN.Text = m.MainEntity.DSN;
                    GBD.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    GBD.Visibility = System.Windows.Visibility.Collapsed;
               
            }
        }

    }
}
