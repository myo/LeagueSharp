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
using Action = System.Action;

namespace SharpAI.SummonersRift.Logic
{
    static class Freeze
    {
        static bool ShouldTakeAction()
        {
            return Minions.GetMinionsInLane(SessionBasedData.MyTeam, SessionBasedData.CurrentLane).Count() >=
                   Minions.GetMinionsInLane(SessionBasedData.EnemyTeam, SessionBasedData.CurrentLane).Count() && !ObjectManager.Player.IsUnderAllyTurret() && !ObjectManager.Player.IsUnderEnemyTurret();
        }

        static TreeSharp.Action TakeAction()
        {
            return new TreeSharp.Action(a =>
            {
                Logging.Log("SWITCHED MODE TO FREEZE");
                Positioning.GetFarmingPosition().WalkToPoint(OrbwalkingMode.Hybrid);
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
