/* imsobad 2015
 * just got my PhD in copypaste
 * still not as good as the elohell mates
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kindred_Chanel.MyLogic.Others;
using Kindred_Chanel.MyUtils;
using LeagueSharp;
using LeagueSharp.Common;
using TargetSelector = LeagueSharp.Common.TargetSelector;
using Kindred_Chanel.MyLogic.Others;

namespace Kindred_Chanel.MyLogic
{
    public static class Spells
    {
        //c+p'd from hellsing
        public static void OnLoad(EventArgs args)
        {
            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.Combo)
            {
                var target = Program.Orbwalker.GetTarget() as Obj_AI_Base;
                if (target.IsValidTarget())
                {
                    if (Program.ComboMenu.Item("QCombo").GetValue<bool>() && Program.Q.IsReady())
                    {
                        Program.Q.Cast(target.GetTumblePos());
                    }
                    if (Program.ComboMenu.Item("WCombo").GetValue<bool>() && Program.W.IsReady())
                    {
                        Program.W.Cast();
                    }
                    if (Program.ComboMenu.Item("ECombo").GetValue<bool>() && Program.E.IsReady() && !target.IsFacing(ObjectManager.Player))
                    {
                        Program.E.Cast(target);
                    }
                }
            }
            if (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear)
            {
                var target = Program.Orbwalker.GetTarget() as Obj_AI_Base;
                if (target.IsValidTarget() && target.Name.Contains("SRU_"))
                {
                    
                    if (Program.W.IsReady() && Program.LaneClearMenu.Item("WJungle").GetValue<bool>()){Program.W.Cast();}
                    if (Program.Q.IsReady() && Program.LaneClearMenu.Item("QJungle").GetValue<bool>()){Program.Q.Cast(target.GetTumblePos());}
                    if (Program.E.IsReady() && Program.LaneClearMenu.Item("EJungle").GetValue<bool>())
                    {Program.E.Cast(target);}
                }
            }
        }
    }
}