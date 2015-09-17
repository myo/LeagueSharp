using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRAVEN_Draven.MyUtils;
using LeagueSharp;
using LeagueSharp.Common;

namespace DRAVEN_Draven.MyLogic.W
{
    public static partial class Events
    {
        public static void AfterAttack(AttackableUnit sender, AttackableUnit target)
        {
            if (target.IsValid<Obj_AI_Hero>())
            {
                var tg = target as Obj_AI_Hero;
                if (tg.IsValidTarget())
                {
                    if (Program.ComboMenu.Item("ChaseW").GetValue<bool>() && Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.Combo 
                        && ObjectManager.Player.IsFacing(tg) && !tg.IsFacing(ObjectManager.Player))
                    {
                        Program.W.Cast();
                    }
                }
            }
            if (target is Obj_AI_Turret)
            {
                if (Program.LaneClearMenu.Item("WTowers").GetValue<bool>())
                {
                    Program.W.Cast();
                }
            }
        }
    }
}
