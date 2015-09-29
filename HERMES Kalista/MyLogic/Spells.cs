/* imsobad 2015
 * just got my PhD in copypaste
 * still not as good as the elohell mates
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HERMES_Kalista.MyLogic.Others;
using HERMES_Kalista.MyUtils;
using LeagueSharp;
using LeagueSharp.Common;
using TargetSelector = LeagueSharp.Common.TargetSelector;
using HERMES_Kalista.MyLogic.Others;

namespace HERMES_Kalista.MyLogic
{
    public static class Spells
    {
        //c+p'd from hellsing
        public static void OnLoad(EventArgs args)
        {
            Game.OnUpdate += OnUpdate;
            MyOrbwalker.OnNonKillableMinion += OnNonKillableMinion;
        }

        private static void OnNonKillableMinion(AttackableUnit minion)
        {
            var objaiminion = (Obj_AI_Base) minion;
            if (objaiminion.IsRendKillable())
            {
                Program.E.Cast();
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Program.E.IsReady())
            {
                //KS
                if (HeroManager.Enemies.Any(en => Program.E.IsInRange(en) && en.IsRendKillable()))
                {
                    Program.E.Cast();
                }
                //Jungle Clear
                if (ObjectManager.Player.Level > 1 &&
                    MinionManager.GetMinions(Program.Q.Range, MinionTypes.All, MinionTeam.Neutral)
                        .Any(m => m.IsRendKillable())) //TODO: check for jungler
                {
                    Program.E.Cast();
                    return;
                }
                //Minion Resets
                if (Program.ComboMenu.Item("EComboMinionReset").GetValue<bool>() &&
                    MinionManager.GetMinions(Program.E.Range).Any(m => m.IsRendKillable()))
                {
                    if (
                        HeroManager.Enemies.Where(e => !e.HasUndyingBuff() && !e.HasSpellShield())
                            .Select(en => en.GetRendBuff())
                            .Any(buf => buf != null &&
                                        buf.Count >=
                                        Program.ComboMenu.Item("EComboMinionResetStacks").GetValue<Slider>().Value))
                    {
                        Program.E.Cast();
                        return;
                    }
                }
                //E poke, slow
                if ((from enemy in HeroManager.Enemies.Where(e => Program.E.IsInRange(e))
                    let buff = enemy.GetRendBuff()
                    where Program.E.IsReady() && buff != null && Program.E.IsInRange(enemy)
                    where buff.Count >= Program.ComboMenu.Item("EComboMinStacks").GetValue<Slider>().Value
                    where (enemy.Distance(ObjectManager.Player, true) > Math.Pow(Program.E.Range*0.80, 2) ||
                           buff.EndTime - Game.Time < 0.3)
                    select enemy).Any())
                {
                    Program.E.Cast();
                    return;
                }
                //E Laneclear
                if (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear &&
                    Program.LaneClearMenu.Item("LaneclearE").GetValue<bool>() && ObjectManager.Player.ManaPercent <
                    Program.LaneClearMenu.Item("LaneclearEMinMana").GetValue<Slider>().Value &&
                    MinionManager.GetMinions(Program.E.Range).Count(m => m.IsRendKillable()) >
                    Program.LaneClearMenu.Item("LaneclearEMinions").GetValue<Slider>().Value)
                {
                    Program.E.Cast();
                }
            }
            if (Program.ComboMenu.Item("QCombo").GetValue<bool>() && Program.Q.IsReady() &&
                Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.Combo)
            {
                var target = TargetSelector.GetTarget(Program.Q.Range, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget())
                {
                    Program.Q.Cast(target);
                    return;
                }
            }
        }
    }
}