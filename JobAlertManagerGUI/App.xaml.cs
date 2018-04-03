using JobAlertManagerGUI.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace JobAlertManagerGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private LogHandler logHandler = new LogHandler();

        public LogHandler Logger { get => logHandler; set => logHandler = value; }


    }
}
