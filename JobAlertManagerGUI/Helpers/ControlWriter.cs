using System.IO;
using System.Text;
using System.Windows.Controls;

namespace JobAlertManagerGUI.Helpers
{
    public class ControlWriter : TextWriter
    {
        private readonly TextBlock textbox;

        public ControlWriter(TextBlock textblock)
        {
            textbox = textblock;
        }

        public override Encoding Encoding => Encoding.ASCII;

        public override void Write(char value)
        {
            textbox.Text += value;
        }

        public override void Write(string value)
        {
            textbox.Text += value;
        }
    }
}