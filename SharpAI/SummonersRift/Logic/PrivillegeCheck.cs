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
            return Hotfixes.AttackedByTurretFlag || (Hotfixes.AttackedByMinionsFlag && Variables.Orbwalker.ActiveMode != OrbwalkingMode.Combo) || ObjectManager.Player.Position.IsDangerousPosition() || ObjectManager.Get<Obj_AI_Hero>().Any(h=>h.IsEnemy && !h.IsDead && h.IsVisible && h.Level > ObjectManager.Player.Level + 1 && h.Distance(ObjectManager.Player) < h.AttackRange + 50*h.Level-ObjectManager.Player.Level) ||  ObjectManager.Get<Obj_AI_Hero>().Count(h => h.IsEnemy && !h.IsDead && h.Distance(ObjectManager.Player) < 850) > ObjectManager.Get<Obj_AI_Hero>().Count(h=>h.IsAlly && !h.IsDead && h.Distance(ObjectManager.Player) < 850);
        }

        static TreeSharp.Action TakeAction()
        {
            return new TreeSharp.Action(a =>
            {
                if (Variables.Orbwalker.CanMove || Hotfixes.AttackedByTurretFlag)
                {
                    Logging.Log("SWITCHED MODE TO PRIVILLEGE CHECK");
                    ObjectManager.Player.Position.Extend(GameObjects.AllyNexus.Position,
                            Utility.Random.GetRandomInteger(400, 600)).WalkToPoint(OrbwalkingMode.None, true);
                }
            });
        }

        public static Composite BehaviorComposite => new Decorator(t => ShouldTakeAction(), TakeAction());
    }
}
