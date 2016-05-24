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
    public static class PrivillegeCheck
    {
        static bool ShouldTakeAction()
        {
            return ObjectManager.Player.Position.IsDangerousPosition();
        }

        static TreeSharp.Action TakeAction()
        {
            return new TreeSharp.Action(a =>
            {
                Logging.Log("SWITCHED MODE TO PRIVILLEGE CHECK");
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                        ObjectManager.Player.Position.Extend(GameObjects.AllyNexus.Position, Utility.Random.GetRandomInteger(400, 600)), true);
                Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
