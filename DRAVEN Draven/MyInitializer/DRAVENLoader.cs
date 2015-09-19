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
                if (ObjectManager.Player.CharData.BaseSkinName == "Draven")
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
