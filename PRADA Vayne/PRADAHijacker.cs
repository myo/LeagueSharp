using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Vayne.MyInitializer;
using PRADA_Vayne.MyUtils;

namespace PRADA_Vayne
{
    public static class PRADAHijacker
    {
        public static bool IsVHRDetected = false;
        public static Menu HijackedMenu;
        public static int LastUpdateTime = 0;
        public static void AttemptHijack()
        {
            Utility.DelayAction.Add(3000, () =>
            {
                IsVHRDetected = (Menu.GetMenu("VayneHunter Reborn", "dz191.vhr") != null && Menu.GetMenu("VayneHunter Reborn", "dz191.vhr").DisplayName == "VayneHunter Reborn");
                if (!IsVHRDetected)
                {
                    PRADALoader.Init();
                }
                else
                {
                    Game.PrintChat("<font color='#f4ff1c'><b>[PRADA Vayne]</b></font> VHR Detected! Attempting to load PRADA Plugin!");
                    LoadVHRPlugin();
                }
            });
        }

        public static void LoadVHRPlugin()
        {
            MyUtils.Cache.Load();
            PRADALoader.LoadSpells();
            PRADALoader.LoadVHRPluginLogic();
            HijackedMenu =
                Menu.GetMenu("VayneHunter Reborn", "dz191.vhr")
                    .AddSubMenu(new Menu("[PRADA] VHR on Steroids", "pradavhrplugin"));
            var antigcmenu = HijackedMenu.AddSubMenu(new Menu("Anti-Gapcloser", "antigapcloser"));
            foreach (var hero in Heroes.EnemyHeroes)
            {
                var championName = hero.CharData.BaseSkinName;
                antigcmenu.AddItem(new MenuItem("antigc" + championName, championName).SetValue(Lists.CancerChamps.Any(entry => championName == entry)));
            }
            HijackedMenu.AddItem(new MenuItem("usepradaq", "Use PRADA Q").SetValue(false));
            HijackedMenu.AddItem(new MenuItem("usepradae", "Use PRADA E").SetValue(true));
            HijackedMenu.AddItem(new MenuItem("EPushDist", "E Push Distance").SetValue(new Slider(420, 325, 480)));
            HijackedMenu.AddItem(new MenuItem("EHitchance", "E Hitchance").SetValue(new Slider(50, 1, 100)));
            HijackedMenu.AddItem(new MenuItem("AutoBuy", "Auto Swap Trinkets").SetValue(true));
            Game.PrintChat("<font color='#f4ff1c'><b>[PRADA Vayne]</b></font> VHR assembly succesfully hijacked! Enjoy!");
            HijackedMenu.AddSubMenu(MyUtils.EarlyEvade.MenuLocal);
            Game.OnUpdate += UpdateVHRSettings;
        }

        public static void UpdateVHRSettings(EventArgs args)
        {
            if (LeagueSharp.Common.Utils.GameTimeTickCount - LastUpdateTime > 2500)
            {
                LastUpdateTime = LeagueSharp.Common.Utils.GameTimeTickCount;
                DongerSkill(Menu.GetMenu("VayneHunter Reborn", "dz191.vhr").SubMenu("dz191.vhr.combo"), Skills.Q,
                    Orbwalking.OrbwalkingMode.Combo);
                DongerSkill(Menu.GetMenu("VayneHunter Reborn", "dz191.vhr").SubMenu("dz191.vhr.combo"), Skills.E,
                    Orbwalking.OrbwalkingMode.Combo);
            }
        }

        public enum Skills
        {
            Q, E
        }

        public static void DongerSkill(this Menu menu, Skills skill, Orbwalking.OrbwalkingMode mode)
        {
            var vhrname = string.Format("dz191.vhr.{0}.use{1}", mode.ToString().ToLower(), skill.ToString().ToLower());
            var pradaname = string.Format("useprada{0}", skill.ToString().ToLower());
            menu.Item(vhrname).SetValue(!HijackedMenu.Item(pradaname).GetValue<bool>());
        }
    }
}
