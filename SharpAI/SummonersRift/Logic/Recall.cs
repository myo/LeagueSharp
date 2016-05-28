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
using Random = SharpAI.Utility.Random;

namespace SharpAI.SummonersRift.Logic
{
    public static class Recall
    {
        static bool ShouldTakeAction()
        {
            return !ObjectManager.Player.InFountain() && !ObjectManager.Player.IsUnderEnemyTurret() && ObjectManager.Player.HealthPercent < 35 && !ObjectManager.Player.IsDead;
        }

        static TreeSharp.Action TakeAction()
        {
            return new TreeSharp.Action(a =>
            {
                Logging.Log("SWITCHED MODE TO RECALL");
                if (ObjectManager.Get<Obj_AI_Hero>().Count(h=> h.IsEnemy && !h.IsDead && h.IsVisible && h.Distance(ObjectManager.Player) < 1250) == 0 &&
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Count(m => m.IsEnemy && !m.IsDead && m.IsVisible && m.Distance(ObjectManager.Player) < 500) < 1)
                {
                    Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                    if (!ObjectManager.Player.IsRecalling())
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall);
                    }
                }
                else
                {
                    Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                    if (!ObjectManager.Player.IsRecalling() ||
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Any(h => h.IsEnemy && !h.IsDead && h.IsVisible && h.Distance(ObjectManager.Player) < 1250))
                    {
                        ObjectManager.Player.Position.Extend(GameObjects.AllyNexus.Position,
                                Random.GetRandomInteger(400, 600)).WalkToPoint(OrbwalkingMode.None, true);
                        Logging.Log("LOOKING FOR SAFE RECALL SPOT");
                    }
                }
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
