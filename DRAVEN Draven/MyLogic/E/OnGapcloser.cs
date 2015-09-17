using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using DRAVEN_Draven.MyUtils;

namespace DRAVEN_Draven.MyLogic.E
{
    public static partial class Events
    {
        public static void OnGapcloser(ActiveGapcloser gapcloser)
        {
            if (Program.EscapeMenu.SubMenu("antigapcloser")
                .Item("antigc" + gapcloser.Sender.ChampionName)
                .GetValue<bool>())
            {
                if (Heroes.Player.Distance(gapcloser.End) < 1000 && Heroes.Player.Distance(gapcloser.Start) < 1000)
                {
                    Program.E.Cast(gapcloser.Sender);
                }
            }
        }
    }
}
