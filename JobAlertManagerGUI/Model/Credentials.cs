using System;
using System.Collections.Generic;
using TrentUtil;

namespace JobAlertManagerGUI.Model
{
    public static class Credentials
    {
        public static string Host { get; set; }

        public static ushort Port { get; set; }

        public static bool Ssl { get; set; }

        public static string Username { get; set; }

        public static string Password { get; set; }

        public static void SaveToFile(string filepath)
        {
            var credentialsList = new List<string>
            {
                "Host: " + Host,
                "Port: " + Port,
                "SSL: " + Ssl,
                "Username: " + Username,
                "Password: " + Password
            };
            FileIO.ExportStringsToFile(filepath, credentialsList);
        }

        public static void ImportFromFile(string filepath)
        {
            var credentialsList = FileIO.ImportFileToStringList(filepath);
            foreach (var line in credentialsList)
                if (line.StartsWith("Host: "))
                    Host = line.Remove(0, "Host: ".Length);
                else if (line.StartsWith("Port: "))
                    Port = Convert.ToUInt16(line.Remove(0, "Port: ".Length));
                else if (line.StartsWith("SSL: "))
                    Ssl = Convert.ToBoolean(line.Remove(0, "SSL: ".Length));
                else if (line.StartsWith("Username: "))
                    Username = line.Remove(0, "Username: ".Length);
                else if (line.StartsWith("Password: ")) Password = line.Remove(0, "Password: ".Length);
        }
    }
}