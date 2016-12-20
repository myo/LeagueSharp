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
using SharpDX;
using TreeSharp;
using Random = SharpAI.Utility.Random;

namespace SharpAI.SummonersRift.Logic
{
    public static class Recall
    {
        private static Vector3 _recallSpot;
        static bool ShouldTakeAction()
        {
            return !ObjectManager.Player.InFountain() && ObjectManager.Player.HealthPercent < 30 && !ObjectManager.Player.IsDead;
        }

        static TreeSharp.Action TakeAction()
        {
            return new TreeSharp.Action(a =>
            {
                Logging.Log("SWITCHED MODE TO RECALL");
                _recallSpot =
                    StaticData.GetLanePolygon(ObjectManager.Player.Team, SessionBasedData.CurrentLane)
                        .Points.FirstOrDefault(
                            p =>
                                !ObjectManager.Get<Obj_AI_Hero>()
                                    .Any(h => h.IsEnemy && !h.IsDead && h.Distance(p) < 2200))
                        .ToVector3();
                    SessionBasedData.CurrentLanePolygon.Points.FirstOrDefault(
                        p =>
                            ObjectManager.Get<Obj_AI_Turret>()
                                .Any(
                                    t =>
                                        t.IsAlly && t.Distance(p) < 900 &&
                                        !ObjectManager.Get<Obj_AI_Hero>()
                                            .Any(h => h.IsEnemy && !h.IsDead && h.Distance(t) < 1600))).ToVector3();
                if (ObjectManager.Player.Distance(_recallSpot) < 350)
                {
                    Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                    if (!ObjectManager.Player.IsRecalling())
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall);
                    }
                }
                else
                {
                    if (_recallSpot != Vector3.Zero)
                    {
                        _recallSpot.WalkToPoint(OrbwalkingMode.Combo);
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
                }
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
