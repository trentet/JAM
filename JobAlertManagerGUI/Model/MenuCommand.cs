using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlertManagerGUI.Model
{
    public class MenuCommand
    {
        private string displayName;
        private Func<object> execute;

        public string DisplayName { get => displayName; set => displayName = value; }
        public Func<object> Execute { get => execute; set => execute = value; }

        public MenuCommand(string displayName, Func<object> function)
        {
            DisplayName = displayName;
            Execute = function;
        }
    }
}
