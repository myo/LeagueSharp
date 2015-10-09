using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Brain.TheBrain
{
    public class Core
    {
        public static Menu BrainMenu;
        public static Activator.Core Activator;
        public Core(Menu menu)
        {
            BrainMenu = menu.AddSubMenu(new Menu("Brain", "brain"));
            Activator = new Activator.Core(BrainMenu);
        }
    }
}
