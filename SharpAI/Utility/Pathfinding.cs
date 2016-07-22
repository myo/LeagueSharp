using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using SharpDX;

namespace SharpAI.Utility
{
    public static class Pathfinding
    {
        private static int _humanizer;
        public static void WalkToPoint(this Vector3 point, OrbwalkingMode mode, bool forceIssueOrder = false)
        {
            if (Environment.TickCount - _humanizer > Random.GetRandomInteger(350, 800))
            {
                _humanizer = Environment.TickCount;
                // another lane
                if (point.Distance(ObjectManager.Player.Position) > 800 || forceIssueOrder)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, point, false);
                    Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                    // skip from triggering the orbwalker too
                    return;
                }
                Variables.Orbwalker.ForceOrbwalkPoint = point;
                Variables.Orbwalker.ActiveMode = mode;
            }
        }
    }
}
