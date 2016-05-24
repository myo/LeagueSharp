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
            return Minions.GetMinionsInLane(SessionBasedData.MyTeam, SessionBasedData.CurrentLane).Count() <
                   Minions.GetMinionsInLane(SessionBasedData.EnemyTeam, SessionBasedData.CurrentLane).Count() || ObjectManager.Get<Obj_AI_Hero>().Count(e=>e.IsEnemy && !e.IsDead && e.IsVisible && e.Distance(ObjectManager.Player) < 1600) < 1;
        }

        static TreeSharp.Action TakeAction()
        {
            return new Action(a =>
            {
                Logging.Log("SWITCHED MODE TO PUSH");
                Variables.Orbwalker.ForceOrbwalkingPoint = Positioning.GetFarmingPosition();
                Variables.Orbwalker.Enabled = true;
                Variables.Orbwalker.ActiveMode = OrbwalkingMode.LaneClear;
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
