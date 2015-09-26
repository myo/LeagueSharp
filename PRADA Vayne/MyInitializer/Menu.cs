using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Vayne.MyUtils;

namespace PRADA_Vayne.MyInitializer
{
    public static partial class PRADALoader
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
                Program.MainMenu = new Menu("PRADA Vayne", "pradamenu", true);
                Program.ComboMenu = new Menu("Combo Settings", "combomenu");
                Program.LaneClearMenu = new Menu("Laneclear Settings", "laneclearmenu");
                Program.EscapeMenu = new Menu("Escape Settings", "escapemenu");

                Program.ActivatorMenu = new Menu("MActivator", "activatormenu");

                Program.DrawingsMenu = new Menu("Drawing Settings", "drawingsmenu");
                Program.DrawingsMenu.AddItem(new MenuItem("streamingmode", "Disable All Drawings").SetValue(false));
                Program.DrawingsMenu.AddItem(new MenuItem("drawenemywaypoints", "Draw Enemy Waypoints").SetValue(true));
                Program.SkinhackMenu = new Menu("Skin Hack", "skinhackmenu");
                Program.OrbwalkerMenu = new Menu("Orbwalker", "orbwalkermenu");
                Program.ComboMenu.AddItem(new MenuItem("QCombo", "Auto Tumble").SetValue(true));
                Program.ComboMenu.AddItem(
                    new MenuItem("QMode", "Q Mode: ").SetValue(
                        new StringList(new[] {"PRADA", "TO MOUSE"})));
                Program.ComboMenu.AddItem(
                    new MenuItem("QMinDist", "Min dist from enemies").SetValue(new Slider(375, 325, 525)));
                Program.ComboMenu.AddItem(new MenuItem("QChecks", "Q Safety Checks").SetValue(true));
                Program.ComboMenu.AddItem(new MenuItem("EQ", "Q After E").SetValue(false));
                Program.ComboMenu.AddItem(new MenuItem("QWall", "Enable Wall Tumble?").SetValue(true));
                Program.ComboMenu.AddItem(new MenuItem("QR", "Q after Ult").SetValue(true));
                //Program.ComboMenu.AddItem(new MenuItem("FocusTwoW", "Focus 2 W Stacks").SetValue(true)); #TODO ?
                Program.ComboMenu.AddItem(new MenuItem("ECombo", "Auto Condemn").SetValue(true));
                Program.ComboMenu.AddItem(
                    new MenuItem("ManualE", "Semi-Manual Condemn").SetValue(new KeyBind('E', KeyBindType.Press)));
                Program.ComboMenu.AddItem(
                    new MenuItem("EMode", "E Mode").SetValue(
                        new StringList(new[] {"PRADASMART", "PRADAPERFECT", "MARKSMAN", "SHARPSHOOTER", "GOSU", "VHR", "PRADALEGACY", "FASTEST", "OLDPRADA"})));
                Program.ComboMenu.AddItem(
                    new MenuItem("EPushDist", "E Push Distance").SetValue(new Slider(450, 300, 475)));
                Program.ComboMenu.AddItem(new MenuItem("EHitchance", "E % Hitchance").SetValue(new Slider(50)));
                Program.ComboMenu.AddItem(new MenuItem("RCombo", "Auto Ult").SetValue(false));
                Program.ComboMenu.AddItem(new MenuItem("AutoBuy", "Auto-Swap Trinkets?").SetValue(true));
                Program.EscapeMenu.AddItem(new MenuItem("QUlt", "Smart Q-Ult").SetValue(true));
                Program.EscapeMenu.AddItem(new MenuItem("EInterrupt", "Use E to Interrupt").SetValue(true));
                var antigcmenu = Program.EscapeMenu.AddSubMenu(new Menu("Anti-Gapcloser", "antigapcloser"));
                foreach (var hero in Heroes.EnemyHeroes)
                {
                    var championName = hero.CharData.BaseSkinName;
                    antigcmenu.AddItem(new MenuItem("antigc" + championName, championName).SetValue(Lists.CancerChamps.Any(entry => championName == entry)));
                }
                Program.LaneClearMenu.AddItem(new MenuItem("QLastHit", "Use Q to Lasthit").SetValue(true));
                Program.LaneClearMenu.AddItem(
                    new MenuItem("QLastHitMana", "Min Mana% for Q Lasthit").SetValue(new Slider(45))); 
                Program.LaneClearMenu.AddItem(new MenuItem("QWaveClear", "Use Q to clear the wave").SetValue(false));
                Program.LaneClearMenu.AddItem(
                    new MenuItem("QWaveClearMana", "Min Mana for Q Wave clear").SetValue(new Slider(75)));
                Program.SkinhackMenu.AddItem(
                new MenuItem("skin", "Skin: ").SetValue(
                    new StringList(new[] { "Classic", "Vindicator", "Aristocrat", "Dragonslayer", "Heartseeker", "SKT T1", "Arclight", "Dragonslayer Green", "Dragonslayer Red", "Dragonslayer Azure" }))).DontSave().ValueChanged +=
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
