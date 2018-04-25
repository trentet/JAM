using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlertManagerGUI.Controller
{
    public static class ThreadHelper
    {
        private static bool logoutRequested = false;
        private static bool stallLogout = false;

        public static bool LogoutRequested { get => logoutRequested; set => logoutRequested = value; }
        public static bool StallLogout { get => stallLogout; set => stallLogout = value; }
    }
}
