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
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using MimeKit;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CryptoGateway.FileSystem.VShell.Interfaces;
using TrentUtil;
using JobAlertManagerGUI.Controller;

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

        [Import(typeof(IEMailReader))]
        private IEMailReader reader
        {
            get;
            set;
        }

        private ObservableCollection<Email> emails = new ObservableCollection<Email>();

        public ObservableCollection<Email> Emails { get => emails; set => emails = value; }

        private Email selectedEmail;

        public Email SelectedEmail { get => selectedEmail; set => selectedEmail = value; }

        private readonly SynchronizationContext synchronizationContext;
        private DateTime previousTime = DateTime.Now;

        //private bool logoutRequested = false;
        //private bool stallLogout = false;

        public MainWindow()
        {
            InitializeComponent();
            synchronizationContext = SynchronizationContext.Current;
            Emails.CollectionChanged += OnEmailCollectionChanged;
            AppConfig.IsConsole = false;
            AppConfig.EmailSaveDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), @"Documents\JAM\Emails\");
            Console.SetOut(new MultiTextWriter(new LogWriter(), Console.Out));
            UpdateLoggedInUser();
        }

        public void UpdateUI()
        {
            var timeNow = DateTime.Now;

            if ((timeNow - previousTime).Milliseconds <= 50) return;

            synchronizationContext.Post(new SendOrPostCallback(o =>
            {
                EmailList.ItemsSource = Emails;
            }), Emails);

            previousTime = timeNow;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.LogoutRequested = false;
            try
            {
                var login = new LoginWindow();
                login.ShowDialog();
                UpdateLoggedInUser();
                if (AppConfig.CurrentIMap.IsAuthenticated && AppConfig.CurrentIMap.IsConnected)
                {
                    await Task.Run(() =>
                    {
                        EmailHelper emailHelper = new EmailHelper(this);
                        emailHelper.PopulateEmailList();
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
                    Emails.Clear();
                    EmailList.SelectedIndex = -1;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Message: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
            }
        }

        void EmailList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (EmailList.SelectedIndex > -1)
            {
                SelectedEmail = Emails[EmailList.SelectedIndex];
                EmailContent.DataContext = SelectedEmail;
            }
            else
            {
                EmailContent.DataContext = null;
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

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // This event handler is called when the background thread finishes.  
            // This method runs on the main thread.  
            if (e.Error != null)
                MessageBox.Show("Error: " + e.Error.Message);
            else if (e.Cancelled)
                MessageBox.Show("Word counting canceled.");
            else
                MessageBox.Show("Finished counting words.");
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This method runs on the main thread.  
            Words.CurrentState state =
                (Words.CurrentState)e.UserState;
            //this.LinesCounted.Text = state.LinesCounted.ToString();
            //this.WordsCounted.Text = state.WordsMatched.ToString();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // This event handler is where the actual work is done.  
            // This method runs on the background thread.  

            // Get the BackgroundWorker object that raised this event.  
            System.ComponentModel.BackgroundWorker worker;
            worker = (System.ComponentModel.BackgroundWorker)sender;

            // Get the Words object and call the main method.  
            Words WC = (Words)e.Argument;
            WC.CountWords(worker, e);
        }

        private void StartThread()
        {
            // This method runs on the main thread.  
            //this.WordsCounted.Text = "0";

            // Initialize the object that the background worker calls.  
            Words WC = new Words();
            //WC.CompareString = this.CompareString.Text;
            //WC.SourceFile = this.SourceFile.Text;

            // Start the asynchronous operation.  
            //backgroundWorker1.RunWorkerAsync(WC);
        }
    }
}
