using System;
﻿using LeagueSharp;
using LeagueSharp.Common;

namespace DRAVEN_Draven.MyInitializer
{
    public static partial class DRAVENLoader
    {
        public static void Init()
        {
            CustomEvents.Game.OnGameLoad += args =>
            {
                Notifications.AddNotification("Draven draven disabled; use MoonDraven soon tm", 10000);
                if (ObjectManager.Player.CharData.BaseSkinName == "ProjectFioraMistress")
                {
                    MyUtils.Cache.Load();
                    LoadMenu();
                    LoadSpells();
                    LoadLogic();
                    ShowNotifications();
                }
            };
        }
    }
}
