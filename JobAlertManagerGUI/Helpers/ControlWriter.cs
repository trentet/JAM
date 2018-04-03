using JobAlertManagerGUI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JobAlertManagerGUI.Helpers
{
    public class ControlWriter : TextWriter
    {
        private TextBlock textbox;

        public ControlWriter(TextBlock textblock)
        {
            this.textbox = textblock;
        }

        public override void Write(char value)
        {
            textbox.Text += value;
        }

        public override void Write(string value)
        {
            textbox.Text += value;
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
