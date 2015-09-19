using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using DRAVEN_Draven.MyUtils;

namespace DRAVEN_Draven.MyLogic.E
{
    public static partial class Events
    {
        public static void OnUpdate(EventArgs args)
        {
            if (Program.E.IsReady())
            {
                var chasee = Heroes.EnemyHeroes.FirstOrDefault(e => e.Distance(ObjectManager.Player) < 700 && e.HealthPercent < 50 && !e.IsFacing(Heroes.Player));
                if (ObjectManager.Player.CountEnemiesInRange(1200) <= 2 && chasee != null)
                {
                    Program.E.Cast(Program.E.GetPrediction(chasee).UnitPosition);
                }
                var treeSmoker = Heroes.EnemyHeroes.FirstOrDefault(enemy => enemy.Distance(ObjectManager.Player) < 350);
                if (treeSmoker != null && treeSmoker.IsValidTarget())
                {
                    Program.E.Cast(Program.E.GetPrediction(treeSmoker).UnitPosition);
                }
            }
        }
    }
}
