using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using JobAlertManagerGUI.Model;

namespace JobAlertManagerGUI.View
{
    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        public LoginWindow()
        {
            InitializeComponent();
            if (File.Exists(@"C:\Users\Trent\Documents\JAM\Credentials.txt"))
            {
                Credentials.ImportFromFile(@"C:\Users\Trent\Documents\JAM\Credentials.txt");
                txtHost.Text = Credentials.Host;
                checkBoxSSL.IsChecked = Credentials.Ssl;
                txtPort.Text = Credentials.Port.ToString();
                txtEmail.Text = Credentials.Username;
                txtPassword.Text = Credentials.Password;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void MenuItem_Click1(object sender, RoutedEventArgs e)
        {
            LoginLayer.Visibility = Visibility.Visible;
        }

        private void Login_Click(object sender, RoutedEventArgs ev)
        {
            Credentials.Host = txtHost.Text;
            Credentials.Ssl = true;
            if (checkBoxSSL.IsChecked ?? false)
                Credentials.Ssl = true;
            else
                Credentials.Ssl = false;
            Credentials.Port = Convert.ToUInt16(txtPort.Text);
            Credentials.Username = txtEmail.Text;
            Credentials.Password = txtPassword.Text;


            AppConfig.CurrentIMap.ServerCertificateValidationCallback = (s, c, h, e) => true;

            AppConfig.CurrentIMap.Connect(Credentials.Host, Credentials.Port, Credentials.Ssl);

            AppConfig.CurrentIMap.Authenticate(Credentials.Username, Credentials.Password);

            //IMapCommands.Login(Credentials.Host, Credentials.Ssl, Credentials.Port, Credentials.Username, Credentials.Password);

            if (AppConfig.CurrentIMap.IsAuthenticated && AppConfig.CurrentIMap.IsConnected)
            {
                LogBox.Text = "Login successful";
                Credentials.SaveToFile(@"C:\Users\Trent\Documents\JAM\Credentials.txt");
                Close();
            }
            else
            {
                LogBox.Text = "Login unsuccessful";
            }
        }

        internal void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}