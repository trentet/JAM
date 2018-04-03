using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobAlertManagerGUI.Model;

namespace JobAlertManagerGUI.Controller
{
    public class JAMController
    {
        public void Run()
        {
            //MenuController m = new MenuController(new MenuModel(IMapCommands.Commands));
            //do
            //{
            //    int select = m.LoadMenu();
            //    m.ExecuteSelection(select);
            //} while (m.Menu.IsActive);
            Console.WriteLine("Program is exiting...");
            Console.ReadKey();
        }       
    }
}
