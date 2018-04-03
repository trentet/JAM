using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobAlertManagerGUI.View;
using JobAlertManagerGUI.Model;
using System.Xml;

namespace JobAlertManagerGUI.Controller
{
    public class MenuController
    {
        public MenuController(MenuModel menu)
        {
            Menu = menu;
        }
        private MenuModel menu;

        public MenuModel Menu { get => menu; set => menu = value; }

        public int LoadMenu()
        {
            Menu.IsActive = true;
            int input = -1;
            bool isValidInput = false;
            while (isValidInput == false)
            {
                Menu.PrintMenuOptions();
                string sInput = Console.ReadLine();
                if (int.TryParse(sInput, out input))
                {
                    if (Menu.Options.ContainsKey(input))
                    {
                        isValidInput = true;
                    }
                }
                if (isValidInput == false) { Console.WriteLine("\nInvalid selection. Please try again...\n"); }
            }
            return input;
        }
        public void ExecuteSelection(int input)
        {
            if (Menu.IsActive == true)
            {
                try
                {
                    Menu.Options[input].Execute();
                    if (Menu.Options[input].DisplayName.Equals("Exit"))
                    {
                        Menu.IsActive = false;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}:{1}", e.Message, e.InnerException);
                    Console.WriteLine("\r");
                }
            }
            else
            {
                Console.WriteLine("Unable to execute command. Menu is not active.");
            }

        }
    }
}
