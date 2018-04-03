using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    /// Interaction logic for LargeTextBox.xaml
    /// </summary>
    public partial class LargeTextBox : UserControl
    {
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                if (IsVisible)
                    ShowText(value);
                else
                    _txtval = value;
            }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(LargeTextBox), new UIPropertyMetadata("",
                (o, e) =>
                {
                    (o as LargeTextBox).Text = e.NewValue as string;
                }));
        private string _txtval;

        public LargeTextBox()
        {
            InitializeComponent();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible && _txtval != null)
                ShowText(_txtval);
        }

        private void ShowText(string value)
        {
            txt.Document.Blocks.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                StringReader sr = new StringReader(value);
                Paragraph s = new Paragraph();
                string line = sr.ReadLine();
                do
                {
                    Run r = new Run(line);
                    LineBreak lb = new LineBreak();
                    s.Inlines.Add(r);
                    s.Inlines.Add(lb);
                    line = sr.ReadLine();
                } while (line != null);
                txt.Document.Blocks.Add(s);
            }
            _txtval = null;
        }
    }
}
