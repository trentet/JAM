using JobAlertManagerGUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobAlertManagerGUI.Model
{
    public class MyMenuItem
    {
        public string Header { get; set; }
        public List<MyMenuItem> Children { get; set; }
        public RelayCommand Command { get; set; }
    }
}
