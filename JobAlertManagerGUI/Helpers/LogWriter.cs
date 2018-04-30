using System.IO;
using System.Text;
using System.Windows;

namespace JobAlertManagerGUI.Helpers
{
    public class LogWriter : TextWriter
    {
        private string line;

        public LogWriter()
        {
            NewLine = "/r/n";
        }

        public override Encoding Encoding => Encoding.ASCII;

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
    }
}