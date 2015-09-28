using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using HERMES_Kalista.MyUtils;

namespace HERMES_Kalista.MyInitializer
{
    public static partial class HERMESLoader
    {
        public static void LoadMenu()
        {
            ConstructMenu();
            InitActivator();
            InitOrbwalker();
            FinishMenuInit();
        }

        public static void ConstructMenu()
        {
            try
            {
                Program.MainMenu = new Menu("HERMES Kalista", "pradamenu", true);
                Program.ComboMenu = new Menu("Combo Settings", "combomenu");
                Program.LaneClearMenu = new Menu("Laneclear Settings", "laneclearmenu");
                Program.EscapeMenu = new Menu("Escape Settings", "escapemenu");

                Program.ActivatorMenu = new Menu("MActivator", "activatormenu");

                Program.DrawingsMenu = new Menu("Drawing Settings", "drawingsmenu");
                Program.DrawingsMenu.AddItem(new MenuItem("streamingmode", "Disable All Drawings").SetValue(false));
                Program.DrawingsMenu.AddItem(new MenuItem("drawenemywaypoints", "Draw Enemy Waypoints").SetValue(true));
                Program.SkinhackMenu = new Menu("Skin Hack", "skinhackmenu");
                Program.OrbwalkerMenu = new Menu("Orbwalker", "orbwalkermenu");
                Program.ComboMenu.AddItem(new MenuItem("QCombo", "USE Q").SetValue(true));
                Program.ComboMenu.AddItem(new MenuItem("ECombo", "USE E").SetValue(true));
                Program.ComboMenu.AddItem(
                    new MenuItem("DamageReductionE", "Reduce E dmg by").SetValue(
                        new Slider(0, 0, 300)));;
                Program.ComboMenu.AddItem(new MenuItem("RCombo", "USE R").SetValue(false));
                Program.ComboMenu.AddItem(new MenuItem("AutoBuy", "Auto-Swap Trinkets?").SetValue(true));
                Program.EscapeMenu.AddItem(new MenuItem("SOON", "SOON").SetValue(true));
                var antigcmenu = Program.EscapeMenu.AddSubMenu(new Menu("Anti-Gapcloser", "antigapcloser"));
                foreach (var hero in Heroes.EnemyHeroes)
                {
                    var championName = hero.CharData.BaseSkinName;
                    antigcmenu.AddItem(new MenuItem("antigc" + championName, championName).SetValue(Lists.CancerChamps.Any(entry => championName == entry)));
                }
                Program.SkinhackMenu.AddItem(
                new MenuItem("skin", "Skin: ").SetValue(
                    new StringList(new[]
                    {
                        "Classic", "BloodMoon", "Championship" //, "Lunar"
                    }))).DontSave().ValueChanged +=
                (sender, args) =>
                {
                    switch (Program.SkinhackMenu.Item("skin").GetValue<StringList>().SelectedValue)
                    {
                        case "Classic":
                            Heroes.Player.SetSkin(Heroes.Player.CharData.BaseSkinName, 0);
                            break;
                        case "BloodMoon":
                            Heroes.Player.SetSkin(Heroes.Player.CharData.BaseSkinName, 1);
                            break;
                        case "Championship":
                            Heroes.Player.SetSkin(Heroes.Player.CharData.BaseSkinName, 2);
                            break;
                    }
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void InitActivator()
        {
            Program.Activator = new MActivator();
        }

        public static void InitOrbwalker()
        {
            Program.Orbwalker = new MyOrbwalker.Orbwalker(Program.OrbwalkerMenu);
        }

        public static void FinishMenuInit()
        {
            Program.MainMenu.AddSubMenu(Program.ComboMenu);
            Program.MainMenu.AddSubMenu(Program.LaneClearMenu);
            Program.MainMenu.AddSubMenu(Program.EscapeMenu);
            Program.MainMenu.AddSubMenu(Program.ActivatorMenu);
            Program.MainMenu.AddSubMenu(Program.SkinhackMenu); // XD
            Program.MainMenu.AddSubMenu(Program.DrawingsMenu);
            Program.MainMenu.AddSubMenu(Program.OrbwalkerMenu);
            Program.MainMenu.AddToMainMenu();
        }
    }
}
