using JobAlertManagerGUI.Model;
using JobAlertManagerGUI.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MailKit.Search;
using MailKit;
using System.ComponentModel;
using System.Collections.ObjectModel;
using MimeKit;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        internal void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private LoginWindow login = new LoginWindow();

        private ObservableCollection<Email> emails = new ObservableCollection<Email>();

        private Email selectedEmail;

        public ObservableCollection<Email> Emails { get => emails; set => emails = value; }
        public Email SelectedEmail { get => selectedEmail; set => selectedEmail = value; }

        public MainWindow()
        {
            InitializeComponent();
            AppConfig.IsConsole = false;
            AppConfig.EmailSaveDirectory = @"C:\Users\Trent\Documents\JAM\Emails\";
            Console.SetOut(new MultiTextWriter(new LogWriter(), Console.Out));
            UpdateLoggedInUser();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                login.ShowDialog();
                UpdateLoggedInUser();
                if (AppConfig.CurrentIMap.IsAuthenticated && AppConfig.CurrentIMap.IsConnected)
                {
                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = AppConfig.CurrentIMap.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    Console.WriteLine("Total messages: {0}", inbox.Count);
                    Console.WriteLine("Recent messages: {0}", inbox.Recent);

                    var fetchedEmails = inbox.Fetch(0, 49, MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
                    for (int x = 0; x < 50; x++)// var email in inbox.Fetch(0, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId))
                    {
                        Console.WriteLine("[summary] {0:D2}: {1}", fetchedEmails[x].Index, fetchedEmails[x].Envelope.Subject);
                        Emails.Add(new Email(fetchedEmails[x]));
                        string preview = Emails[x].Preview;
                    }
                    EmailSubjectList.ItemsSource = Emails;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Message: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppConfig.CurrentIMap.Disconnect(true);
                UpdateLoggedInUser();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Message: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
            }
        }

        private void EmailSubjectList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SelectedEmail = Emails[EmailSubjectList.SelectedIndex];
            EmailContent.DataContext = SelectedEmail;
        }

        private void UpdateLoggedInUser()
        {
            if (AppConfig.CurrentIMap.IsConnected && AppConfig.CurrentIMap.IsAuthenticated)
            {
                LoggedInUser.Text = Credentials.Username;
            }
            else
            {
                LoggedInUser.Text = "Not logged in";
            }
        }
    }
}
