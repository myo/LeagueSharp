using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using SharpAI.Utility;
using TreeSharp;
using Random = SharpAI.Utility.Random;

namespace SharpAI.SummonersRift.Logic
{
    class StayInFountainToHeal
    {
        static bool ShouldTakeAction()
        {
            return ObjectManager.Player.IsDead || (ObjectManager.Player.InFountain() && ObjectManager.Player.HealthPercent < 100) || ObjectManager.Player.IsRecalling();
        }

        static TreeSharp.Action TakeAction()
        {
            return new TreeSharp.Action(a =>
            {
                Logging.Log("SWITCHED MODE TO HEAL IN FOUNTAIN");
                Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                //do nothing
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}