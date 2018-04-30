namespace JobAlertManagerGUI.Controller
{
    public static class ThreadHelper
    {
        public static bool LogoutRequested { get; set; } = false;
        public static bool StallLogout { get; set; } = false;
    }
}