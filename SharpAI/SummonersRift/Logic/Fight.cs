using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAI.SummonersRift.Data;
using SharpAI.Utility;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.Utils;
using TreeSharp;

namespace SharpAI.SummonersRift.Logic
{
    public static class Fight
    {
        static bool ShouldTakeAction()
        {
            if (ObjectManager.Player.CountEnemyHeroesInRange(1400) >= 2)
            {
                return false;
            }
            var target = (Obj_AI_Hero)Variables.Orbwalker.GetTarget();
            if (target == null)
            {
                return false;
            }
            return ObjectManager.Player.HealthPercent > target.HealthPercent && target.HealthPercent < 45 && !target.IsUnderEnemyTurret();
        }

        static TreeSharp.Action TakeAction()
        {
            return new TreeSharp.Action(a =>
            {
                Logging.Log("SWITCHED MODE TO FIGHT");
                Variables.Orbwalker.ForceOrbwalkingPoint = Variables.Orbwalker.GetTarget().Position.Extend(ObjectManager.Player.Position, ObjectManager.Player.GetRealAutoAttackRange() - 200);
                Variables.Orbwalker.Enabled = true;
                Variables.Orbwalker.ActiveMode = OrbwalkingMode.Combo;
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
