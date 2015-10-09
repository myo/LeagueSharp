using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Brain.TheBrain.Activator
{
    public class Core
    {
        public static Menu ActivatorMenu;
        public static Menu OffensiveMenu;
        public static Menu CleanserMenu;
        public static Menu HealMenu;
        public static Menu ShieldMenu;
        public static SpellSlot SummonerCleanse;
        public Core(Menu menu)
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells)
            {
                if (spell.Name == "summonerboost")
                {
                    SummonerCleanse = spell.Slot;
                }
            }
            ActivatorMenu = menu.AddSubMenu(new Menu("Activator", "activator"));
            OffensiveMenu = menu.AddSubMenu(new Menu("Offensive Items", "offensive"));
            CleanserMenu = menu.AddSubMenu(new Menu("Cleanser", "cleanser"));
            foreach (var item in Enum.GetNames(typeof(BuffType)))
            {
                var buffTypeMenu = CleanserMenu.AddSubMenu(new Menu(item, item));
                buffTypeMenu.AddItem(new MenuItem("Cleanse with QSS", "qss").SetValue(false));
                if (SummonerCleanse != SpellSlot.Unknown)
                {
                    buffTypeMenu.AddItem(new MenuItem("Cleanse with Summoner Cleanse", "summonercleanse").SetValue(false));
                }
                buffTypeMenu.AddItem(new MenuItem("Cleanse with Mikaels", "mikaels").SetValue(false));
                buffTypeMenu.AddItem(new MenuItem("Cleanse if Duration > ms", "minbuffduration").SetValue(new Slider(500, 0, 1000)));
                buffTypeMenu.AddItem(new MenuItem("Cleanse if Health% <=", "minhealthpercent").SetValue(new Slider(100, 1)));
                buffTypeMenu.AddItem(new MenuItem("Cleanse if Enemies near >=", "minenemies").SetValue(new Slider(1, 1, 5)));
                buffTypeMenu.AddItem(new MenuItem("Delay", "delay").SetValue(new Slider(100, 100, 500)));
            }
            HealMenu = menu.AddSubMenu(new Menu("Heals", "heal"));
            ShieldMenu = menu.AddSubMenu(new Menu("Shields", "shield"));
            Game.OnUpdate += Cleanser.OnUpdate;
        }
    }
}
