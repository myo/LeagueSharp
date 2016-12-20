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
            return !ObjectManager.Player.IsUnderEnemyTurret() && ObjectManager.Get<Obj_AI_Hero>().Where(h =>h.IsEnemy && !h.IsDead && h.IsVisible && h.Distance(ObjectManager.Player) < 1100).All(h=>h.Level <= ObjectManager.Player.Level) && ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsEnemy && !h.IsDead && h.Distance(ObjectManager.Player) < 1100 && h.IsVisible && h.Health < ObjectManager.Player.GetAutoAttackDamage(h) * 2 && !h.IsUnderEnemyTurret()) || ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsEnemy && !h.IsDead && h.Distance(ObjectManager.Player) < 1100 && h.IsVisible) &&  (ObjectManager.Get<Obj_AI_Hero>().Count(h=>h.IsAlly && !h.IsMe && !h.IsDead && h.Distance(ObjectManager.Player) < 1100) + 1 > ObjectManager.Get<Obj_AI_Hero>().Count(h => h.IsEnemy && !h.IsDead && h.Distance(ObjectManager.Player) < 1100)
                || (ObjectManager.Get<Obj_AI_Hero>().Count(h => h.IsAlly && !h.IsDead && h.Distance(ObjectManager.Player) < 1100) == ObjectManager.Get<Obj_AI_Hero>().Count(h => h.IsEnemy && !h.IsDead && h.Distance(ObjectManager.Player) < 1100) && ObjectManager.Get<Obj_AI_Hero>().All(h => h.IsEnemy && !h.IsDead && h.Distance(ObjectManager.Player) < 1100 && ObjectManager.Player.Level - 1 > h.Level)));
        }

        static TreeSharp.Action TakeAction()
        {
            return new TreeSharp.Action(a =>
            {
                Logging.Log("SWITCHED MODE TO FIGHT");
                var target =
                Variables.TargetSelector.GetTarget(1100);
                if (target != null && !target.IsUnderEnemyTurret())
                {
                    if (
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Any(h => h.IsAlly && !h.IsMe && !h.IsDead && h.Distance(ObjectManager.Player) < 1100))
                    {
                        Positioning.GetTeamfightPosition().WalkToPoint(OrbwalkingMode.Hybrid);
                    }
                    else
                    {
                        target.Position.Extend(ObjectManager.Player.Position,
                            ObjectManager.Player.GetRealAutoAttackRange() - 250)
                            .WalkToPoint(OrbwalkingMode.Hybrid);
                    }
                }
                else
                {
                    Push.BehaviorComposite.Tick(null);
                }
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
