using System.Windows;
using JobAlertManagerGUI.Model;

namespace JobAlertManagerGUI
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public LogHandler Logger { get; set; } = new LogHandler();
    }
}