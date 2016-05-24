using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;
using SharpDX;
using SharpDX.Win32;

namespace SharpAI.Utility
{
    public static class Misc
    {
        public static bool IsDangerousPosition(this Vector3 pos)
        {
            var possibleTurret = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(t => !t.IsDead && t.IsEnemy && t.Distance(ObjectManager.Player.Position) < 900);
            if (possibleTurret == null)
            {
                return false;
            }
            if (ObjectManager.Get<Obj_AI_Minion>().Count(m => !m.IsDead && m.IsAlly && m.HealthPercent > 50 && m.Distance(possibleTurret) < 850) < 2)

            {
                return true;
            }
            return false;
        }
    }
}
