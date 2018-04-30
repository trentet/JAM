using System;
using System.Collections.Generic;
using System.IO;
using MailKit;

namespace JobAlertManagerGUI.Model
{
    public static class Extensions
    {
        public static UniqueId[] ToUniqueIdArray(this IEnumerable<string> uidStrings)
        {
            var uniqueIds = new List<UniqueId>();
            foreach (var uidString in uidStrings)
                if (uint.TryParse(uidString, out var uid))
                    uniqueIds.Add(new UniqueId(uid));
                else
                    Console.WriteLine("Failed to parse UniqueID. UniqueID string value: " + uidString);

            return uniqueIds.ToArray();
        }

        public static void DownloadEmail(this Email email, IMailFolder mailFolder)
        {
            try
            {
                Console.WriteLine("[Downloading Email] {0:D2} ", email.Id);
                var savePath = AppConfig.EmailSaveDirectory + email.Id + ".eml";
                if (!File.Exists(savePath))
                    lock (mailFolder.SyncRoot)
                    {
                        File.WriteAllText(savePath, mailFolder.GetMessage(email.Id).ToString());
                        Console.WriteLine("[Email Downloaded] {0:D2}: {1}", email.Id, savePath);
                    }
                else
                    Console.WriteLine("[Download Cancelled] EML {0:D2} Already Exists: {1}", email.Id, savePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to download email of ID {" + email.Id + "}.");
                Console.WriteLine(e.StackTrace);
                throw;
                //return null;
            }
        }
    }
}