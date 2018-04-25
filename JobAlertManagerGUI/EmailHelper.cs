using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using JobAlertManagerGUI.Controller;
using JobAlertManagerGUI.Model;
using JobAlertManagerGUI.View;
using MailKit;
using MimeKit;

namespace JobAlertManagerGUI
{
    public class EmailHelper
    {
        private readonly MainWindow window;
        public EmailHelper(MainWindow window)
        {
            this.window = window;
        }

        // Object to store the current state, for passing to the caller.  
        //public class CurrentState
        //{
        //    public int LinesCounted;
        //    public int WordsMatched;
        //}

        //public string SourceFile;
        //public string CompareString;
        //private int WordCount;
        //private int LinesCounted;

        public void PopulateEmailList(
            //System.ComponentModel.BackgroundWorker worker,
            //System.ComponentModel.DoWorkEventArgs e
            )
        {
            // The Inbox folder is always available on all IMAP servers...
            var inbox = AppConfig.CurrentIMap.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            Console.WriteLine("Total messages: {0}", inbox.Count);
            Console.WriteLine("Recent messages: {0}", inbox.Recent);

            for (var x = 0; x < 50; x++)
            {
                if (AppConfig.CurrentIMap.IsConnected && AppConfig.CurrentIMap.IsAuthenticated && ThreadHelper.LogoutRequested == false)
                {
                    try
                    {
                        ThreadHelper.StallLogout = true;
                        var emailId = DownloadEmail(inbox, x);
                        var email = LoadEmail(emailId);
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            window.Emails.Add(email);
                        });
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


        UniqueId DownloadEmail(IMailFolder mailFolder, int index)
        {
            var emailId = mailFolder.Fetch(index, index, MessageSummaryItems.Full | MessageSummaryItems.UniqueId)[0].UniqueId;
            var fetchedEmail = new Email(emailId);

            Console.WriteLine("[summary] {0:D2}: {1}", index + 1, fetchedEmail.Message.Subject);
            string savePath = AppConfig.EmailSaveDirectory + fetchedEmail.Id + ".eml";
            File.WriteAllText(savePath, fetchedEmail.Message.ToString());
            return fetchedEmail.Id;
        }

        Email LoadEmail(UniqueId id)
        {
            return new Email(id, MimeMessage.Load(AppConfig.EmailSaveDirectory + id + ".eml"));
        }

        //public void CountWords(
        //    System.ComponentModel.BackgroundWorker worker,
        //    System.ComponentModel.DoWorkEventArgs e)
        //{
        //    // Initialize the variables.  
        //    CurrentState state = new CurrentState();
        //    string line = "";
        //    int elapsedTime = 20;
        //    DateTime lastReportDateTime = DateTime.Now;

        //    if (CompareString == null ||
        //        CompareString == System.String.Empty)
        //    {
        //        throw new Exception("CompareString not specified.");
        //    }

        //    // Open a new stream.  
        //    using (System.IO.StreamReader myStream = new System.IO.StreamReader(SourceFile))
        //    {
        //        // Process lines while there are lines remaining in the file.  
        //        while (!myStream.EndOfStream)
        //        {
        //            if (worker.CancellationPending)
        //            {
        //                e.Cancel = true;
        //                break;
        //            }
        //            else
        //            {
        //                line = myStream.ReadLine();
        //                WordCount += CountInString(line, CompareString);
        //                LinesCounted += 1;

        //                // Raise an event so the form can monitor progress.  
        //                int compare = DateTime.Compare(
        //                    DateTime.Now, lastReportDateTime.AddMilliseconds(elapsedTime));
        //                if (compare > 0)
        //                {
        //                    state.LinesCounted = LinesCounted;
        //                    state.WordsMatched = WordCount;
        //                    worker.ReportProgress(0, state);
        //                    lastReportDateTime = DateTime.Now;
        //                }
        //            }
        //            // Uncomment for testing.  
        //            //System.Threading.Thread.Sleep(5);  
        //        }

        //        // Report the final count values.  
        //        state.LinesCounted = LinesCounted;
        //        state.WordsMatched = WordCount;
        //        worker.ReportProgress(0, state);
        //    }
        //}
    }
}
