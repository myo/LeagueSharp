using System;
﻿using LeagueSharp;
using LeagueSharp.Common;

namespace PRADA_Vayne.MyInitializer
{
    public static partial class PRADALoader
    {
        public static void Init()
        {
            CustomEvents.Game.OnGameLoad += args =>
            {
                if (ObjectManager.Player.CharData.BaseSkinName == "Vayne")
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
