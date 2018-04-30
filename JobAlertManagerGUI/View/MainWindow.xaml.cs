using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CryptoGateway.FileSystem.VShell.Interfaces;
using JobAlertManagerGUI.Controller;
using JobAlertManagerGUI.Helpers;
using JobAlertManagerGUI.Model;
using MailKit;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly SynchronizationContext _synchronizationContext;
        private DateTime _previousTime = DateTime.Now;

        //private bool logoutRequested = false;
        //private bool stallLogout = false;

        public MainWindow()
        {
            InitializeComponent();
            _synchronizationContext = SynchronizationContext.Current;
            //Emails.CollectionChanged += OnEmailCollectionChanged;
            AppConfig.IsConsole = false;
            AppConfig.EmailSaveDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"),
                @"Documents\JAM\Emails\");
            Console.SetOut(new MultiTextWriter(new LogWriter(), Console.Out));
            UpdateLoggedInUser();
        }

        [Import(typeof(IEMailReader))] private IEMailReader Reader { get; set; }

        //private ObservableCollection<Email> _emails = new ObservableCollection<Email>();

        public ObservableCollection<Email> Emails { get; set; }

        public Email SelectedEmail { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void LoadReader(string pluginDir)
        {
            var categ = new DirectoryCatalog(pluginDir);
            var cname = AttributedModelServices.GetContractName(typeof(IEMailReader));
            Expression<Func<ExportDefinition, bool>> exp = a => a.ContractName == cname;
            var id = new ImportDefinition(exp, cname, ImportCardinality.ExactlyOne, true, true);
            var l = categ.GetExports(id).ToList();
            if (l.Count == 1)
            {
                var cc = new CompositionContainer(categ);
                cc.ComposeParts(this);
                creader.Content = Reader;
                Reader.QueryThread = OnQueryThread;
            }
            else if (l.Count == 0)
            {
                creader.Visibility = Visibility.Collapsed;
                MessageBox.Show("Failed to find any plugin!");
            }
        }

        public void UpdateUI()
        {
            var timeNow = DateTime.Now;

            if ((timeNow - _previousTime).Milliseconds <= 50) return;

            _synchronizationContext.Post(o => { EmailList.ItemsSource = Emails; }, Emails);

            _previousTime = timeNow;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.LogoutRequested = false;
            try
            {
                var login = new LoginWindow();
                login.ShowDialog();
                UpdateLoggedInUser();

                Emails = new ObservableCollection<Email>();
                Emails.CollectionChanged += OnEmailCollectionChanged;

                var pluginDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + @"Plugins\");
                if (Directory.Exists(pluginDir)) LoadReader(pluginDir);

                if (AppConfig.CurrentIMap.IsAuthenticated && AppConfig.CurrentIMap.IsConnected)
                    await Task.Run(() => { PopulateEmailList(AppConfig.CurrentIMap.Inbox); });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Message: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
            }
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.LogoutRequested = true;
            while (ThreadHelper.StallLogout)
            {
                await Task.Delay(100);
                Console.WriteLine("Stalling logout...");
            }

            try
            {
                AppConfig.CurrentIMap.Disconnect(true);
                UpdateLoggedInUser();
                Dispatcher.Invoke(delegate
                {
                    Emails = null;
                    EmailList.ItemsSource = Emails;
                    EmailList.SelectedIndex = -1;
                    Reader = null;
                    creader.Content = Reader;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Message: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
            }
        }

        private void LoadMimeFile(Email email)
        {
            if (Reader == null)
            {
                MessageBox.Show("The reader has not been loaded.");
                return;
            }

            Reader.SourceUri = new Uri(AppConfig.EmailSaveDirectory + email.FileName);
        }

        private bool OnQueryThread(string MsgID, out ThreadedMessage StartMsg)
        {
            StartMsg = null;
            return false;
        }

        private void EmailList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (EmailList.SelectedIndex > -1)
            {
                SelectedEmail = Emails[EmailList.SelectedIndex];
                SelectedEmail.DownloadEmail(AppConfig.CurrentIMap.Inbox);
                EmailContent.DataContext = SelectedEmail;
                LoadMimeFile(SelectedEmail);
            }
            else
            {
                EmailContent.DataContext = null;
            }
        }

        public void PopulateEmailList(IMailFolder mailFolder)
        {
            // The Inbox folder is always available on all IMAP servers...
            mailFolder.Open(FolderAccess.ReadOnly);

            var emailSummaries = mailFolder.Fetch(0, 49,
                MessageSummaryItems.UniqueId | MessageSummaryItems.PreviewText | MessageSummaryItems.Envelope);

            Console.WriteLine("Total messages: {0}", mailFolder.Count);
            Console.WriteLine("Recent messages: {0}", mailFolder.Recent);

            foreach (var emailSummary in emailSummaries)
                if (AppConfig.CurrentIMap.IsConnected && AppConfig.CurrentIMap.IsAuthenticated &&
                    ThreadHelper.LogoutRequested == false)
                    try
                    {
                        Email email = null;
                        ThreadHelper.StallLogout = true;

                        if (emailSummary.TextBody != null)
                            email = new Email(emailSummary);
                        else if (emailSummary.HtmlBody != null)
                            lock (mailFolder.SyncRoot)
                            {
                                email = new Email(emailSummary, mailFolder.GetMessage(emailSummary.UniqueId));
                            }

                        if (email != null && email.Id != null && email.MessageSummary != null)
                        {
                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                Emails.Add(email);
                                Console.WriteLine("[Email Added] {0:D2}: {1}", email.Id,
                                    email.MessageSummary.Envelope.Subject);
                            });
                        }
                        else
                        {
                            var idState = "NOT NULL";
                            var messageState = "NOT NULL";
                            if (email.Id == null) idState = "NULL";

                            if (email.MessageSummary == null) messageState = "NULL";

                            Console.WriteLine("Email not added successfully.\nId {" + idState + "}\nMessage {" +
                                              messageState + "}");
                        }

                        ThreadHelper.StallLogout = false;
                    }
                    catch (Exception exception)
                    {
                        if (ThreadHelper.LogoutRequested)
                        {
                            Console.WriteLine("Logout requested during email collection. ");
                            ThreadHelper.StallLogout = false;
                        }
                        else
                        {
                            Console.WriteLine(exception);
                            throw;
                        }
                    }
                else
                    break;
        }

        private void OnEmailCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null || e.OldItems != null) UpdateUI();
        }

        private void UpdateLoggedInUser()
        {
            if (AppConfig.CurrentIMap.IsConnected && AppConfig.CurrentIMap.IsAuthenticated)
                LoggedInUser.Text = Credentials.Username;
            else
                LoggedInUser.Text = "Not logged in";
        }
    }
}