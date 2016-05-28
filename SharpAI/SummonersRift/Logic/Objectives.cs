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
using LeagueSharp.SDK.Utils;
using TreeSharp;
using Action = TreeSharp.Action;

namespace SharpAI.SummonersRift.Logic
{
    public static class Objectives
    {
        static bool ShouldTakeAction()
        {
            return !ObjectManager.Player.Position.IsDangerousPosition() && ObjectManager.Get<Obj_AI_Turret>().Any(t=>t.IsEnemy && !t.IsDead && t.Distance(ObjectManager.Player) < ObjectManager.Player.AttackRange) || ObjectManager.Get<Obj_BarracksDampener>().Any(b=>b.IsEnemy && !b.IsDead && b.Distance(ObjectManager.Player) < ObjectManager.Player.AttackRange) || GameObjects.EnemyNexus.Distance(ObjectManager.Player) < ObjectManager.Player.GetRealAutoAttackRange();
        }

        static TreeSharp.Action TakeAction()
        {
            return new Action(a =>
            {
                var turret =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(t => t.IsEnemy && !t.IsDead && t.IsVisible && t.Distance(ObjectManager.Player) < 1250);
                if (turret != null)
                {
                    if (!turret.Position.IsDangerousPosition())
                    {
                        turret.Position.Extend(ObjectManager.Player.Position, ObjectManager.Player.AttackRange - 225).WalkToPoint(OrbwalkingMode.LaneClear);
                    }
                }
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
