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
using TreeSharp;

namespace SharpAI.SummonersRift.Logic
{
    public static class FailSafe
    {
        static bool ShouldTakeAction()
        {
            return true;
        }

        static TreeSharp.Action TakeAction()
        {
            return new TreeSharp.Action(a =>
            {
                Logging.Log("SWITCHED MODE TO FAILSAFE");
                StaticData.GetLastTurretInLanePolygon(
                    SessionBasedData.MyTeam, SessionBasedData.CurrentLane).GetRandomPointInPolygon().WalkToPoint(OrbwalkingMode.Hybrid);
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
