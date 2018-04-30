using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    ///     Interaction logic for LargeTextBox.xaml
    /// </summary>
    public partial class LargeTextBox : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(LargeTextBox), new UIPropertyMetadata("",
                (o, e) => { (o as LargeTextBox).Text = e.NewValue as string; }));

        private string _txtval;

        public LargeTextBox()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set
            {
                if (IsVisible)
                    ShowText(value);
                else
                    _txtval = value;
            }
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
                var sr = new StringReader(value);
                var s = new Paragraph();
                var line = sr.ReadLine();
                do
                {
                    var r = new Run(line);
                    var lb = new LineBreak();
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