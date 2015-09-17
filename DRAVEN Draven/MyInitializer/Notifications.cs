using LeagueSharp.Common;

namespace DRAVEN_Draven.MyInitializer
{
    public static partial class DRAVENLoader
    {
        public static void ShowNotifications()
        {
            Utility.DelayAction.Add(3000, () =>
            {
                Notifications.AddNotification("DRAVEN Draven baby", 10000);
                Notifications.AddNotification("back in force", 10000);
                Notifications.AddNotification("to carry ur games", 10000);
                Notifications.AddNotification("myo and THE GUCCI,", 10000);
                Notifications.AddNotification("as always,", 10000);
                Notifications.AddNotification("wish u have fun,", 10000);
                Notifications.AddNotification("and remember,", 10000);
                Notifications.AddNotification("u dont need no luck", 10000);
            });
        }
    }
}
