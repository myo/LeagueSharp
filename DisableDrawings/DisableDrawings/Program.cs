using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace DisableDrawings
{
    public static class Program
    {
        public static Menu Menu;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                Menu = new Menu("Disable Drawings", "disabledrawingsmeu", true);
                Menu.AddItem(new MenuItem("ddhotkey", "Hotkey").SetValue(new KeyBind('J', KeyBindType.Toggle, false)));
                Game.OnUpdate += OnUpdate;
            };
        }

        private static void OnUpdate(EventArgs args)
        {
            Hacks.DisableDrawings = Menu.Item("ddhotkey").GetValue<KeyBind>().Active;
        }
    }
}
