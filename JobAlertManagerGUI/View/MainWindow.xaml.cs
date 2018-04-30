using JobAlertManagerGUI.Model;
using JobAlertManagerGUI.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using MailKit;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CryptoGateway.FileSystem.VShell.Interfaces;
using JobAlertManagerGUI.Controller;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        internal void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Import(typeof(IEMailReader))]
        private IEMailReader Reader
        {
            get;
            set;
        }

        //private ObservableCollection<Email> _emails = new ObservableCollection<Email>();

        public ObservableCollection<Email> Emails { get; set; }

        public Email SelectedEmail { get; set; }

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
            AppConfig.EmailSaveDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), @"Documents\JAM\Emails\");
            Console.SetOut(new MultiTextWriter(new LogWriter(), Console.Out));
            UpdateLoggedInUser();
        }

        private void LoadReader(string pluginDir)
        {
            DirectoryCatalog categ = new DirectoryCatalog(pluginDir);
            string cname = AttributedModelServices.GetContractName(typeof(IEMailReader));
            System.Linq.Expressions.Expression<Func<ExportDefinition, bool>> exp = a => a.ContractName == cname;
            ImportDefinition id = new ImportDefinition(exp, cname, ImportCardinality.ExactlyOne, true, true);
            List<Tuple<ComposablePartDefinition, ExportDefinition>> l = categ.GetExports(id).ToList();
            if (l.Count == 1)
            {
                var cc = new CompositionContainer(categ);
                cc.ComposeParts(this);
                creader.Content = Reader;
                Reader.QueryThread = OnQueryThread;
            }
            else if (l.Count == 0)
            {
                creader.Visibility = System.Windows.Visibility.Collapsed;
                System.Windows.MessageBox.Show("Failed to find any plugin!");
            }
        }

        public void UpdateUI()
        {
            var timeNow = DateTime.Now;

            if ((timeNow - _previousTime).Milliseconds <= 50) return;

            _synchronizationContext.Post(new SendOrPostCallback(o =>
            {
                EmailList.ItemsSource = Emails;
            }), Emails);

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

                string pluginDir = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + @"Plugins\");
                if (System.IO.Directory.Exists(pluginDir))
                {
                    LoadReader(pluginDir);
                }

                if (AppConfig.CurrentIMap.IsAuthenticated && AppConfig.CurrentIMap.IsConnected)
                {
                    await Task.Run(() =>
                    {
                        PopulateEmailList(AppConfig.CurrentIMap.Inbox);
                    });
                }
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
                Dispatcher.Invoke((Action)delegate
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
                System.Windows.MessageBox.Show("The reader has not been loaded.");
                return;
            }
            Reader.SourceUri = new Uri(AppConfig.EmailSaveDirectory + email.FileName);
        }

        private bool OnQueryThread(string MsgID, out ThreadedMessage StartMsg)
        {
            StartMsg = null;
            return false;
        }

        void EmailList_SelectionChanged(object sender, RoutedEventArgs e)
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

            var emailSummaries = mailFolder.Fetch(0, 49, MessageSummaryItems.UniqueId | MessageSummaryItems.PreviewText | MessageSummaryItems.Envelope);

            Console.WriteLine("Total messages: {0}", mailFolder.Count);
            Console.WriteLine("Recent messages: {0}", mailFolder.Recent);

            foreach (var emailSummary in emailSummaries)
            {
                if (AppConfig.CurrentIMap.IsConnected && AppConfig.CurrentIMap.IsAuthenticated && ThreadHelper.LogoutRequested == false)
                {
                    try
                    {
                        Email email = null;
                        ThreadHelper.StallLogout = true;

                        if (emailSummary.TextBody != null)
                        {
                            email = new Email(emailSummary);
                        }
                        else if (emailSummary.HtmlBody != null)
                        {
                            email = new Email(emailSummary, mailFolder.GetMessage(emailSummary.UniqueId));
                        }

                        if (email != null && email.Id != null && email.MessageSummary != null)
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Emails.Add(email);
                                Console.WriteLine("[Email Added] {0:D2}: {1}", email.Id, email.MessageSummary.Envelope.Subject);
                            });
                        }
                        else
                        {
                            string idState = "NOT NULL";
                            string messageState = "NOT NULL";
                            if (email.Id == null)
                            {
                                idState = "NULL";
                            }

                            if (email.MessageSummary == null)
                            {
                                messageState = "NULL";
                            }

                            Console.WriteLine("Email not added successfully.\nId {" + idState + "}\nMessage {" + messageState + "}");
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
                }
                else
                {
                    break;
                }
            }
        }

        void OnEmailCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null || e.OldItems != null)
            {
                UpdateUI();
            }
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
