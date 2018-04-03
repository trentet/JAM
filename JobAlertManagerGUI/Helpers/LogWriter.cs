using JobAlertManagerGUI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JobAlertManagerGUI.Helpers
{
    public class LogWriter : TextWriter
    {
        private string line;
        public LogWriter()
        {
            this.NewLine = "/r/n";
        }

        public override void Write(char value)
        {
            line += value;
            if (value == '\r')
            {
                //Console.WriteLine();
            }
            if (value == '\n')
            {
                (Application.Current as App).Logger.Logs.Add(line);
                line = "";
            }
            
        }

        public override void Write(string value)
        {
            (Application.Current as App).Logger.Logs.Add(value);
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
