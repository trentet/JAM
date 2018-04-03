using System;
using System.Collections.Generic;
using System.ComponentModel;
using TrentUtil;

namespace JobAlertManagerGUI.Model
{
    public static class Credentials
    {
        private static string host;
        private static UInt16 port;
        private static bool ssl;
        private static string username;
        private static string password;

        public static string Host { get => host; set => host = value; }
        public static UInt16 Port { get => port; set => port = value; }
        public static bool Ssl { get => ssl; set => ssl = value; }
        public static string Username { get => username; set => username = value; }
        public static string Password { get => password; set => password = value; }

        public static void SaveToFile(string filepath)
        {
            List<string> credentialsList = new List<string>() { "Host: " + Host, "Port: " + Port, "SSL: " + Ssl, "Username: " + Username, "Password: " + Password };
            FileIO.ExportStringsToFile(filepath, credentialsList);
        }

        public static void ImportFromFile(string filepath)
        {
            List<string> credentialsList = FileIO.ImportFileToStringList(filepath);
            foreach (string line in credentialsList)
            {
                if (line.StartsWith("Host: "))
                {
                    Host = line.Remove(0, "Host: ".Length);
                }
                else if (line.StartsWith("Port: "))
                {
                    Port = Convert.ToUInt16(line.Remove(0, "Port: ".Length));
                }
                else if (line.StartsWith("SSL: "))
                {
                    Ssl = Convert.ToBoolean(line.Remove(0, "SSL: ".Length));
                }
                else if (line.StartsWith("Username: "))
                {
                    Username = line.Remove(0, "Username: ".Length);
                }
                else if (line.StartsWith("Password: "))
                {
                    Password = line.Remove(0, "Password: ".Length);
                }
            }
        }
    }
}
