using System;
﻿using LeagueSharp;
using LeagueSharp.Common;

namespace PRADA_Vayne.MyInitializer
{
    public static partial class PRADALoader
    {
        public static void Init()
        {
            MyUtils.Cache.Load();
            LoadMenu();
            LoadSpells();
            LoadLogic();
            ShowNotifications();
        }
    }
}
