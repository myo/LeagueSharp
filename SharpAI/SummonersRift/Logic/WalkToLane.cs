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
    public static class WalkToLane
    {
        static bool ShouldTakeAction()
        {
            return !GameObjects.Minions.Any(m=>m.Position.IsInside(StaticData.GetWholeLane(SessionBasedData.CurrentLane)));
        }

        static Action TakeAction()
        {
            return new Action(a =>
            {
                Logging.Log("SWITCHED MODE TO WALKTOLANE");
                StaticData.GetLastTurretInLanePolygon(
                    SessionBasedData.MyTeam, SessionBasedData.CurrentLane).GetRandomPointInPolygon().WalkToPoint(OrbwalkingMode.Combo);
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
