using System;
﻿using LeagueSharp;
using LeagueSharp.Common;

namespace Kindred_Chanel.MyInitializer
{
    public static partial class ChanelLoader
    {
        public static void Init()
        {
            CustomEvents.Game.OnGameLoad += args =>
            {
                if (ObjectManager.Player.CharData.BaseSkinName == "Kindred")
                {
                    MyUtils.Cache.Load();
                    LoadMenu();
                    LoadSpells();
                    LoadLogic();
                    ShowNotifications();
                    Draw();
                }
            };
        }
    }
}
