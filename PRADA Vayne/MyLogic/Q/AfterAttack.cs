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
            if (!Program.Q.IsReady()) return;
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
                if (Program.LaneClearMenu.Item("QWaveClear").GetValue<bool>() &&
                       Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear)
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
                        Tumble.Cast(((Obj_AI_Base)target).GetTumblePos());
                    }
                }
                if (Program.LaneClearMenu.Item("QLastHit").GetValue<bool>() &&
                    ObjectManager.Player.ManaPercent >=
                    Program.LaneClearMenu.Item("QLastHitMana").GetValue<Slider>().Value &&
                    Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LastHit ||
                    Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                m =>
                                    MyOrbwalker.InAutoAttackRange(m)).Count(m =>
                                    m.Health <= ObjectManager.Player.GetAutoAttackDamage(m)) > 2)
                    {
                        var cursorPos = Game.CursorPos;
                        if (!cursorPos.IsDangerousPosition())
                        {
                            Program.Q.Cast(ObjectManager.Player.GetTumblePos());
                            return;
                        }
                    }
                }
            }
        }
    }
}
