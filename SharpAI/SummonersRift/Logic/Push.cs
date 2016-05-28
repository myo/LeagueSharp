using System;
using System.Linq;
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
    public class Push
    {
        static bool ShouldTakeAction()
        {
            return GameObjects.AllyMinions.Count(m=>!m.IsDead && m.Distance(ObjectManager.Player.Position) < 800) == 0 || Minions.GetMinionsInLane(SessionBasedData.MyTeam, SessionBasedData.CurrentLane).Count() <
                   Minions.GetMinionsInLane(SessionBasedData.EnemyTeam, SessionBasedData.CurrentLane).Count() || ObjectManager.Get<Obj_AI_Hero>().Count(e=>e.IsEnemy && !e.IsDead && e.IsVisible && e.Distance(ObjectManager.Player) < 1600) < 1;
        }

        static TreeSharp.Action TakeAction()
        {
            return new Action(a =>
            {
                Logging.Log("SWITCHED MODE TO PUSH");
                Positioning.GetFarmingPosition().WalkToPoint(OrbwalkingMode.LaneClear);
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
