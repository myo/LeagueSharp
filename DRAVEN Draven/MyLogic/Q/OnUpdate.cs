using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRAVEN_Draven.MyUtils;
using LeagueSharp;
using LeagueSharp.Common;

namespace DRAVEN_Draven.MyLogic.Q
{
    public static partial class Events
    {
        public static void OnUpdate(EventArgs args)
        {
            if (Program.Q.IsReady() && Heroes.EnemyHeroes.Any(h => h.Distance(Heroes.Player) < 600) ||
                ObjectManager.Get<Obj_AI_Minion>().Any(m => m.Distance(ObjectManager.Player) < 600))
            {
                Program.Q.Cast();
            }
        }
    }
}
