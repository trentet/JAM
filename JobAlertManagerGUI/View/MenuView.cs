using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobAlertManagerGUI.Model;

namespace JobAlertManagerGUI.View
{
    public class MenuView
    {
        private MenuModel menu;

        public MenuModel Menu { get => menu; set => menu = value; }

        public MenuView(MenuModel menu)
        {
            Menu = menu;
        }

        public void PrintMenuOptions()
        {
            Console.WriteLine("Select the action");
            foreach (var option in Menu.Options)
            {
                Console.WriteLine(option);
            }
            Console.Write("Input  :[" + menu.Options.Count + "]");
        }
    }
}
