using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Vayne.MyUtils;

namespace PRADA_Vayne.MyLogic.Q
{
    public static partial class Events
    {
        public static void AfterAttack(AttackableUnit sender, AttackableUnit target)
        {
            if (sender.IsMe && target.IsValid<Obj_AI_Hero>())
            {
                var tg = target as Obj_AI_Hero;
                if (tg == null) return;
                var mode = Program.ComboMenu.Item("QMode").GetValue<StringList>().SelectedValue;
                var tumblePosition = Game.CursorPos;
                switch (mode)
                {
                    case "PRADA":
                        tumblePosition = tg.GetTumblePos();
                        break;
                    default:
                        tumblePosition = Game.CursorPos;
                        break;
                }
                Tumble.Cast(tumblePosition);
            }
            if (sender.IsMe && target.IsValid<Obj_AI_Minion>())
            {
                if (Program.LaneClearMenu.Item("QWaveClear").GetValue<bool>() && Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear)
                {
                    var meleeMinion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(m => m.IsMelee);
                    if (ObjectManager.Player.ManaPercent >=
                        Program.LaneClearMenu.Item("QWaveClearMana").GetValue<Slider>().Value &&
                        meleeMinion.IsValidTarget())
                    {
                        if (ObjectManager.Player.Level == 1)
                        {
                            Tumble.Cast(meleeMinion.GetTumblePos());
                        }
                        if (ObjectManager.Player.CountEnemiesInRange(1600) == 0)
                        {
                            Tumble.Cast(meleeMinion.GetTumblePos());
                        }
                    }
                    if (target.Name.Contains("SRU_"))
                    {
                        Tumble.Cast(((Obj_AI_Base) target).GetTumblePos());
                    }
                }
                if (Program.LaneClearMenu.Item("QLastHit").GetValue<bool>() && ObjectManager.Player.ManaPercent >= Program.LaneClearMenu.Item("QLastHitMana").GetValue<Slider>().Value && Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear ||
                    Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LastHit)
                {
                    var MinionList =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                minion =>
                                    minion.IsValidTarget() && MyOrbwalker.InAutoAttackRange(minion) &&
                                    minion.Health <
                                    2*
                                    (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod));

                    foreach (var minion in MinionList)
                    {
                        var t = (int) (ObjectManager.Player.AttackCastDelay*1000) - 100 + Game.Ping/2 +
                                1000*(int) ObjectManager.Player.Distance(minion)/(int) MyOrbwalker.GetMyProjectileSpeed();
                        var predHealth = HealthPrediction.GetHealthPrediction(minion, t, 25);

                        if (minion.Team != GameObjectTeam.Neutral && MinionManager.IsMinion(minion, true))
                        {
                            if (predHealth > 0 && predHealth <= (ObjectManager.Player.GetAutoAttackDamage(minion, true)))
                            {
                                Tumble.Cast(minion.GetTumblePos());
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
