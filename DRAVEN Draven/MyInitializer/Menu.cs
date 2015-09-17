using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using DRAVEN_Draven.MyUtils;

namespace DRAVEN_Draven.MyInitializer
{
    public static partial class DRAVENLoader
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
                Program.MainMenu = new Menu("DRAVEN Draven", "pradamenu", true);
                Program.ComboMenu = new Menu("Combo Settings", "combomenu");
                Program.LaneClearMenu = new Menu("Laneclear Settings", "laneclearmenu");
                Program.EscapeMenu = new Menu("Escape Settings", "escapemenu");

                Program.ActivatorMenu = new Menu("CK Activator", "activatormenu");

                Program.DrawingsMenu = new Menu("Drawing Settings", "drawingsmenu");
                Program.DrawingsMenu.AddItem(new MenuItem("streamingmode", "Disable All Drawings").SetValue(false));
                Program.DrawingsMenu.AddItem(new MenuItem("drawenemywaypoints", "Draw Enemy Waypoints").SetValue(true));
                Program.SkinhackMenu = new Menu("Skin Hack", "skinhackmenu");
                Program.OrbwalkerMenu = new Menu("Orbwalker", "orbwalkermenu");
                Program.ComboMenu.AddItem(new MenuItem("QCombo", "Use Q").SetValue(true));
                Program.ComboMenu.AddItem(
                    new MenuItem("QMode", "Q Mode: ").SetValue(
                        new StringList(new[] {"PRADA", "TO MOUSE"})));
                Program.ComboMenu.AddItem(
                    new MenuItem("QMinDist", "Min dist from enemies").SetValue(new Slider(375, 325, 525)));
                Program.ComboMenu.AddItem(new MenuItem("QChecks", "Q Safety Checks").SetValue(true));
                Program.ComboMenu.AddItem(new MenuItem("ChaseW", "Use W while chasing").SetValue(false));
                Program.ComboMenu.AddItem(new MenuItem("ChaseE", "Use E while chasing").SetValue(false));
                Program.ComboMenu.AddItem(new MenuItem("RCombo", "Auto Ult").SetValue(false));
                Program.ComboMenu.AddItem(new MenuItem("AutoBuy", "Auto-Swap Trinkets?").SetValue(true));
                Program.EscapeMenu.AddItem(new MenuItem("EscapeW", "Use W").SetValue(true));
                Program.EscapeMenu.AddItem(new MenuItem("EInterrupt", "Use E to Interrupt").SetValue(true));
                var antigcmenu = Program.EscapeMenu.AddSubMenu(new Menu("Anti-Gapcloser", "antigapcloser"));
                foreach (var hero in Heroes.EnemyHeroes)
                {
                    var championName = hero.CharData.BaseSkinName;
                    antigcmenu.AddItem(new MenuItem("antigc" + championName, championName).SetValue(Lists.CancerChamps.Any(entry => championName == entry)));
                }
                Program.LaneClearMenu.AddItem(new MenuItem("WTowers", "Use W on towers").SetValue(true));
                Program.SkinhackMenu.AddItem(
                new MenuItem("skin", "Skin: ").SetValue(
                    new StringList(new[] { "Classic", "Gladiator", "Primetime", "Pool Party" }))).DontSave().ValueChanged +=
                (sender, args) =>
                {
                    Heroes.Player.SetSkin(Heroes.Player.CharData.BaseSkinName, Program.SkinhackMenu.Item("skin").GetValue<StringList>().SelectedIndex + 1);
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void InitActivator()
        {
            Program.Activator = new MyUtils.Activator(Program.ActivatorMenu);
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
