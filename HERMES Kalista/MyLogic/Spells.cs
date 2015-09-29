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
            if (objaiminion.IsValidTarget() && objaiminion.IsRendKillable() && Program.E.Cast())
            {
                return;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Program.E.IsReady())
            {
                if (HeroManager.Enemies.Any(e => e.IsRendKillable()) &&
                    Program.E.Cast())
                {
                    return;
                }
                if (Program.ComboMenu.Item("EComboMinionReset").GetValue<bool>() &&
                    MinionManager.GetMinions(Program.E.Range).Any(m => m.IsRendKillable()))
                {
                    foreach (var en in HeroManager.Enemies)
                    {
                        var buf = Extensions.GetRendBuff(en);
                        if (buf != null && buf.IsValidBuff() &&
                            buf.Count >=
                            Program.ComboMenu.Item("EComboMinionResetStacks").GetValue<Slider>().Value)
                        {
                            Program.E.Cast();
                            return;
                        }
                    }
                }
                if (ObjectManager.Player.Level > 1 &&
                    MinionManager.GetMinions(Program.Q.Range, MinionTypes.All, MinionTeam.Neutral)
                        .Any(m => m.IsRendKillable()) && Program.E.Cast()) //TODO: check for jungler
                {
                    return;
                }
            }
            if (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.Combo)
            {
                var target = TargetSelector.GetTarget((Program.Q.IsReady()) ? Program.Q.Range : (Program.E.Range*1.2f),
                    TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    // Q usage
                    if (Program.Q.IsReady())
                    {
                        Program.Q.Cast(target);
                        return;
                    }

                    // E usage
                    var buff = Extensions.GetRendBuff(target);
                    if (Program.E.IsReady() && buff != null && Program.E.IsInRange(target))
                    {
                        // Check if the target would die from E
                        if (target.IsRendKillable() && Program.E.Cast())
                        {
                            return;
                        }

                        // Check if target has the desired amount of E stacks on
                        if (buff.Count >= 5)
                        {
                            // Check if target is about to leave our E range or the buff is about to run out
                            if ((target.Distance(ObjectManager.Player, true) > Math.Pow(Program.E.Range*0.80, 2) ||
                                 buff.EndTime - Game.Time < 0.3) && Program.E.Cast())
                            {
                                return;
                            }
                        }

                        // E to slow
                        if (ObjectManager.Get<Obj_AI_Base>().Any(o => Program.E.IsInRange(o) && o.IsRendKillable()) &&
                            Program.E.Cast())
                        {
                            return;
                        }
                    }
                }
            }
            else if (Program.LaneClearMenu.Item("LaneclearE").GetValue<bool>()
                     && Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear)
            {
                if (ObjectManager.Player.ManaPercent < 50)
                {
                    return;
                }
                if (!Program.Q.IsReady() && !Program.E.IsReady())
                {
                    return;
                }

                // Minions around
                var minions = MinionManager.GetMinions(Program.Q.Range);
                if (minions.Count == 0)
                {
                    return;
                }

                // TODO: C+P his Q logic XD

                #region E usage

                if (Program.E.IsReady())
                {
                    // Get minions in E range
                    var minionsInRange = minions.Where(m => Program.E.IsInRange(m)).ToArray();

                    // Validate available minions
                    if (minionsInRange.Length >= 2)
                    {
                        // Check if enough minions die with E
                        var killableNum = 0;
                        foreach (var minion in minionsInRange)
                        {
                            if (minion.IsRendKillable())
                            {
                                // Increase kill number
                                killableNum++;

                                // Cast on condition met
                                if (killableNum >= 2)
                                {
                                    Program.E.Cast();
                                    break;
                                }
                            }
                        }
                    }
                }

                #endregion
            }
        }
    }
}