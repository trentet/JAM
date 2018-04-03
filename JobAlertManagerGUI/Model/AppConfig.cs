using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;

namespace JobAlertManagerGUI.Model
{
    public static class AppConfig
    {
        private static ImapClient currentIMap = new ImapClient();
        public static ImapClient CurrentIMap { get => currentIMap; set => currentIMap = value; }

        public static bool? IsConsole { get; set; }
        public static string EmailSaveDirectory { get; set; }


    }
}
