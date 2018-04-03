using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobAlertManagerGUI.View;

namespace JobAlertManagerGUI.Model
{
    public class MenuModel
    {
        public MenuModel(List<MenuCommand> commands)
        {
            Commands = commands;
            BuildMenu();
        }
        private List<MenuCommand> commands;
        private Dictionary<int, MenuCommand> options = new Dictionary<int, MenuCommand>();
        private bool isActive = false;

        public bool IsActive { get => isActive; set => isActive = value; }
        public List<MenuCommand> Commands { get => commands; set => commands = value; }
        public Dictionary<int, MenuCommand> Options { get => options; set => options = value; }

        public void BuildMenu()
        {
            Dictionary<int, MenuCommand> options = new Dictionary<int, MenuCommand>();
            for (int x = 0; x < Commands.Count; x++)
            {
                options.Add((x+1), Commands[x]);
            }
            Options = options;
        }

        public void PrintMenuOptions()
        {
            Console.WriteLine("Select the action");
            foreach (var option in Options)
            {
                Console.WriteLine((option.Key) + ") " + option.Value.DisplayName);
            }
            Console.Write("Selection : ");
        }
    }
}
