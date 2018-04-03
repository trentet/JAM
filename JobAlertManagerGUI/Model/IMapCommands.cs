//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace JobAlertManagerGUI.Model
//{
//    public static class IMapCommands
//    {
//        private static List<MenuCommand> commands = new List<MenuCommand>()
//            {
//                new MenuCommand("Login", Login),
//                new MenuCommand("Select Folder", SelectFolder),
//                new MenuCommand("Examine Folder", SelectFolder),
//                new MenuCommand("Search", Search),
//                new MenuCommand("Fetch Header", FetchMessageHeader),
//                new MenuCommand("Move Message", MoveMessage),
//                new MenuCommand("Delete Message", DeleteMessage),
//                new MenuCommand("Mark Message Unread", MarkAsUnread),
//                new MenuCommand("Get Quota", GetQuota),
//                new MenuCommand("Get Message Size", GetMessageSize),
//                new MenuCommand("Logout", Logout),
//                new MenuCommand("Exit", Exit)
//            };

//        public static List<MenuCommand> Commands { get => commands; set => commands = value; }

//        public static CommandResult<Unit> Login()
//        {
//            if (AppConfig.IsConsole == true || AppConfig.IsConsole == null)
//            {
//                Console.Write("Host[imap.gmail.com]:");
//                string sHost = Console.ReadLine();
//                if (sHost.Length < 1)
//                {
//                    sHost = "imap.gmail.com";
//                }

//                Credentials.Host = sHost;

//                Console.Write("Port [993]:");
//                string sPort = Console.ReadLine();
//                if (sPort.Length < 1)
//                {
//                    sPort = "993";
//                }

//                Credentials.Port = Convert.ToUInt16(sPort);

//                Console.Write("SSLEnabled[True]:");
//                string sSslEnabled = Console.ReadLine();
//                bool bSSL = true;
//                if ((sSslEnabled.Length < 1) || (String.Compare(sSslEnabled, "yes", true) == 0) || (String.Compare(sSslEnabled, "true", true) == 0))
//                {

//                    bSSL = true;
//                }
//                else
//                {
//                    bSSL = false;
//                }

//                Credentials.Ssl = bSSL;

//                Console.Write("User[]:");
//                string sUser = Console.ReadLine();
//                if (sUser.Length < 1)
//                {
//                    sUser = "trentfromrid@gmail.com";
//                }

//                Credentials.Username = sUser;

//                Console.Write("Password []:");
//                string sPwd = "eeeuuyjkjjrwxjsk";
//                //string sPwd = ReadPassword();
//                if (sPwd.Length < 1)
//                {
//                    sPwd = "";
//                }

//                Credentials.Password = sPwd;

//                Console.WriteLine("########################################");
//                Console.WriteLine("Host:{0}", sHost);
//                Console.WriteLine("Port:{0}", sPort);
//                Console.WriteLine("SSLEnabled:{0}", bSSL.ToString());
//                Console.WriteLine("User:{0}", sUser);
//                Console.WriteLine("########################################");
//            }

//            AppConfig.CurrentIMap.Login(Credentials.Host, Credentials.Port,
//                Credentials.Username, Credentials.Password, Credentials.Ssl);
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }

//        public static CommandResult<Unit> Login(string host, bool ssl, UInt16 port, string username, string password)
//        {
//            AppConfig.CurrentIMap.Login(host, port, username, password, ssl);
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }
//        public static CommandResult<Unit> SelectFolder()
//        {
//            string sInbox = "INBOX";
//            if (AppConfig.IsConsole == true || AppConfig.IsConsole == null)
//            {
//                Console.Write("Folder: [INBOX]");
//                sInbox = Console.ReadLine();
//                if (sInbox.Length < 1) { sInbox = "INBOX"; }
//            }
             

//            AppConfig.CurrentIMap.SelectFolder(sInbox);
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }
//        public static CommandResult<Unit> ExamineFolder()
//        {
//            Console.Write("Folder: [INBOX]");
//            string sInbox = Console.ReadLine();
//            if (sInbox.Length < 1) if (sInbox.Length < 1) { sInbox = "INBOX"; }
//            AppConfig.CurrentIMap.ExamineFolder(sInbox);
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }
//        public static CommandResult<List<string>> Search()
//        {
//            string sSearch = "ALL";
//            string[] saSearchData = new String[1];
//            if (AppConfig.IsConsole == true || AppConfig.IsConsole == null)
//            {
//                Console.Write("Search String:");
//                sSearch = Console.ReadLine();
//            }
//            saSearchData[0] = sSearch;
//            List<string> searchResultUIDs = AppConfig.CurrentIMap.SearchMessage(saSearchData, false);
//            CommandResult<List<string>> result = new CommandResult<List<string>>(searchResultUIDs);
//            return result;
//        }
//        public static CommandResult<Unit> FetchMessageHeader()
//        {
//            Console.Write("Message UID[]:");
//            string sUid = Console.ReadLine();
//            if (sUid.Length < 1)
//                sUid = "";
//            Console.Write("Fetch Body:[false]");
//            string sFetchBody = Console.ReadLine();
//            bool bFetchBody = sFetchBody.ToLower() == "true";
//            List<string> saArray = new List<string>();
//            string sFileName = sUid + ".xml";
//            XmlTextWriter oXmlWriter = new XmlTextWriter(sFileName, System.Text.Encoding.UTF8)
//            {
//                Formatting = Formatting.Indented
//            };
//            oXmlWriter.WriteStartDocument(true);
//            oXmlWriter.WriteStartElement("Message");
//            oXmlWriter.WriteAttributeString("UID", sUid);
//            AppConfig.CurrentIMap.FetchMessage(sUid, oXmlWriter, bFetchBody);
//            oXmlWriter.WriteEndElement();
//            oXmlWriter.WriteEndDocument();
//            oXmlWriter.Flush();
//            oXmlWriter.Close();
//            //AppConfig.CurrentIMap.FetchPartHeader(sUid, sPart, saArray);
//            //c.GetHeader(oImap, sUid, sPart);
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }

//        public static CommandResult<string> FetchMessageHeader(string uid)
//        {
//            Console.WriteLine("Message UID[]: " + uid);
//            //uid = Console.ReadLine();
//            if (uid.Length < 1)
//                uid = "";
//            bool bFetchBody = true;//sFetchBody.ToLower() == "true";
//            Console.WriteLine("Fetch Body:" + bFetchBody);
//            //string sFetchBody = Console.ReadLine();
            
//            List<string> saArray = new List<string>();
//            string sFileName = AppConfig.EmailSaveDirectory + uid + ".xml";
//            if (!File.Exists(sFileName))
//            {
//                Directory.CreateDirectory(AppConfig.EmailSaveDirectory);
//            }
//            XmlTextWriter oXmlWriter = new XmlTextWriter(sFileName, System.Text.Encoding.UTF8)
//            {
//                Formatting = Formatting.Indented
//            };
//            oXmlWriter.WriteStartDocument(true);
//            oXmlWriter.WriteStartElement("Message");
//            oXmlWriter.WriteAttributeString("UID", uid);
//            var test = AppConfig.CurrentIMap.FetchMessage(uid, oXmlWriter, bFetchBody);
//            oXmlWriter.WriteEndElement();
//            oXmlWriter.WriteEndDocument();
//            oXmlWriter.Flush();
//            oXmlWriter.Close();
//            //AppConfig.CurrentIMap.FetchPartHeader(sUid, sPart, saArray);
//            //c.GetHeader(oImap, sUid, sPart);

//            //XmlDocument document = new XmlDocument();
//            //document.Load(sFileName);
//            CommandResult<string> result = new CommandResult<string>(test);
//            return result;
//        }

//        public static CommandResult<Unit> MoveMessage()
//        {
//            Console.Write("Message UID:");
//            string sUid = Console.ReadLine();
//            Console.Write("Folder To Move:");
//            string sFolder = Console.ReadLine();
//            AppConfig.CurrentIMap.MoveMessage(sUid, sFolder);
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }
//        public static CommandResult<Unit> DeleteMessage()
//        {
//            Console.Write("Message UID:");
//            string sUid = Console.ReadLine();
//            AppConfig.CurrentIMap.SetFlag(sUid, "\\Deleted");
//            AppConfig.CurrentIMap.Expunge();
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }
//        public static CommandResult<Unit> MarkAsUnread()
//        {
//            Console.Write("Message UID:");
//            string sUid = Console.ReadLine();
//            AppConfig.CurrentIMap.SetFlag(sUid, "\\Seen", true);
//            AppConfig.CurrentIMap.Expunge();
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }
//        public static CommandResult<Quota> GetQuota()
//        {
//            //bool bUnlimitedQuota = false;
//            //int nUsedKBytes = 0;
//            //int nTotalKBytes = 0;

//            Quota quota = AppConfig.CurrentIMap.GetQuota("inbox");//, ref bUnlimitedQuota, ref nUsedKBytes, ref nTotalKBytes);
//            Console.WriteLine("Unlimitedquota:{0}, UsedKBytes:{1}, TotalKBytes:{2}",
//                quota.BUnlimitedQuota, quota.NUsedKBytes, quota.NTotalKBytes);
//            CommandResult<Quota> result = new CommandResult<Quota>(quota);
//            return result;

//        }
//        public static CommandResult<long> GetMessageSize()
//        {
//            Console.Write("Message UID:");
//            string sUid = Console.ReadLine();

//            long size = AppConfig.CurrentIMap.GetMessageSize(sUid);
//            CommandResult<long> result = new CommandResult<long>(size);
//            return result;
//        }
//        public static CommandResult<Unit> Logout()
//        {
//            AppConfig.CurrentIMap.LogOut();
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }
//        public static CommandResult<Unit> Exit()
//        {
//            if (AppConfig.CurrentIMap.IsLoggedIn)
//            {
//                AppConfig.CurrentIMap.LogOut();
//            }
//            CommandResult<Unit> result = new CommandResult<Unit>(Unit.Value);
//            return result;
//        }

//        /// <summary>
//        /// Read password
//        /// </summary>
//        /// <returns></returns>
//        internal static string ReadPassword()
//        {
//            string password = "";
//            ConsoleKeyInfo info = Console.ReadKey(true);
//            while (info.Key != ConsoleKey.Enter)
//            {
//                if (info.Key != ConsoleKey.Backspace)
//                {
//                    Console.Write("*");
//                    password += info.KeyChar;
//                }
//                else if (info.Key == ConsoleKey.Backspace)
//                {
//                    if (!string.IsNullOrEmpty(password))
//                    {
//                        // remove one character from the list of password characters
//                        password = password.Substring(0, password.Length - 1);
//                        // get the location of the cursor
//                        int pos = Console.CursorLeft;
//                        // move the cursor to the left by one character
//                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
//                        // replace it with space
//                        Console.Write(" ");
//                        // move the cursor to the left by one character again
//                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
//                    }
//                }
//                info = Console.ReadKey(true);
//            }

//            // add a new line because user pressed enter at the end of their password
//            Console.WriteLine();
//            return password;
//        }
//    }
//}
