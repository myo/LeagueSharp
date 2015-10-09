using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Poppy.MyUtils;
using Orbwalker = PRADA_Poppy.MyUtils.MyOrbwalker;

namespace PRADA_Poppy.MyLogic.Q
{
    public static partial class Events
    {
        public static void AfterAttack(AttackableUnit sender, AttackableUnit target)
        {
            if (sender.IsMe && Program.Q.IsReady() && Program.ComboMenu.Item("QCombo").GetValue<bool>())
            {
                if (target.IsValid<Obj_AI_Hero>())
                {
                    Program.Q.Cast();
                    Utility.DelayAction.Add(100, MyOrbwalker.ResetAutoAttackTimer);
                }
            }
        }
    }
}
