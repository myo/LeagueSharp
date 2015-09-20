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
            if (Program.Q.IsReady() && DravenDecision.QTotalCount <= 2)
            {
                if (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.Combo &&
                    Heroes.Player.CountEnemiesInRange(600) >= 1)
                {
                    Program.Q.Cast();
                    return;
                }
                if (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear &&
                    ObjectManager.Get<Obj_AI_Minion>().Any(m => m.Distance(ObjectManager.Player) < 550))
                {
                    Program.Q.Cast();
                }
            }
            
        }
    }
}
