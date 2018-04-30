using MailKit.Net.Imap;

namespace JobAlertManagerGUI.Model
{
    public static class AppConfig
    {
        public static ImapClient CurrentIMap { get; set; } = new ImapClient();

        public static bool? IsConsole { get; set; }
        public static string EmailSaveDirectory { get; set; }
    }
}