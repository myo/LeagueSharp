using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAI.Enums;
using SharpAI.SummonersRift.Data;
using SharpAI.Utility;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using TreeSharp;
using Action = TreeSharp.Action;

namespace SharpAI.SummonersRift.Logic
{
    public static class Objectives
    {
        static bool ShouldTakeAction()
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(t=>t.IsEnemy && !t.IsDead && t.Distance(ObjectManager.Player) < ObjectManager.Player.AttackRange) || ObjectManager.Get<Obj_BarracksDampener>().Any(b=>b.IsEnemy && !b.IsDead && b.Distance(ObjectManager.Player) < ObjectManager.Player.AttackRange);
        }

        static TreeSharp.Action TakeAction()
        {
            return new Action(a =>
            {
                Logging.Log("SWITCHED MODE TO OBJECTIVES");
                Variables.Orbwalker.ForceOrbwalkingPoint = Positioning.GetFarmingPosition();
                Variables.Orbwalker.Enabled = true;
                Variables.Orbwalker.ActiveMode = OrbwalkingMode.LaneClear;
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
