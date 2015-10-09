using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Poppy.MyUtils;

namespace PRADA_Poppy.MyInitializer
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
                Program.MainMenu = new Menu("PRADA Poppy", "pradamenu", true);
                Program.ComboMenu = new Menu("Combo Settings", "combomenu");
                Program.LaneClearMenu = new Menu("Laneclear Settings", "laneclearmenu");
                Program.EscapeMenu = new Menu("Escape Settings", "escapemenu");

                Program.ActivatorMenu = new Menu("MActivator", "activatormenu");

                Program.DrawingsMenu = new Menu("Drawing Settings", "drawingsmenu");
                Program.DrawingsMenu.AddItem(new MenuItem("streamingmode", "Disable All Drawings").SetValue(false));
                Program.DrawingsMenu.AddItem(new MenuItem("drawenemywaypoints", "Draw Enemy Waypoints").SetValue(true));
                Program.SkinhackMenu = new Menu("Skin Hack", "skinhackmenu");
                Program.OrbwalkerMenu = new Menu("Orbwalker", "orbwalkermenu");
                Program.ComboMenu.AddItem(new MenuItem("QCombo", "Auto Q before attack?").SetValue(true));
                Program.ComboMenu.AddItem(new MenuItem("WCombo", "W before danger spell?").SetValue(true));
                Program.ComboMenu.AddItem(new MenuItem("ECombo", "E to auto stun").SetValue(true));
                Program.ComboMenu.AddItem(new MenuItem("ManualE", "E Stun Trigger").SetValue(new KeyBind('E', KeyBindType.Press)));
                Program.ComboMenu.AddItem(
                    new MenuItem("EMode", "E Mode").SetValue(
                        new StringList(new[] {"PRADASMART", "PRADAPERFECT", "MARKSMAN", "SHARPSHOOTER", "GOSU", "VHR", "PRADALEGACY", "FASTEST", "OLDPRADA"})));
                Program.ComboMenu.AddItem(
                    new MenuItem("EPushDist", "E Push Distance").SetValue(new Slider(450, 300, 475)));
                Program.ComboMenu.AddItem(new MenuItem("RCombo", "Auto Ult Mode").SetValue(new StringList(new []{ "SUPPORTS", "CARRIES", "NONE" })));
                var dontultmenu = Program.ComboMenu.AddSubMenu(new Menu("Do not ult: ", "dontult"));
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h=>h.IsEnemy))
                {
                    var championName = hero.CharData.BaseSkinName;
                    dontultmenu.AddItem(new MenuItem("dontult" + championName, championName).SetValue(false));
                }
                Program.ComboMenu.AddItem(new MenuItem("AutoBuy", "Auto-Swap Trinkets?").SetValue(true));
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
            Program.MainMenu.AddSubMenu(Program.ActivatorMenu);
            Program.MainMenu.AddSubMenu(Program.DrawingsMenu);
            Program.MainMenu.AddSubMenu(Program.OrbwalkerMenu);
            Program.MainMenu.AddToMainMenu();
        }
    }
}
