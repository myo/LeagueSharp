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
                if (Program.LaneClearMenu.Item("QLastHit").GetValue<bool>() &&
                    ObjectManager.Player.ManaPercent >=
                    Program.LaneClearMenu.Item("QLastHitMana").GetValue<Slider>().Value &&
                    Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear ||
                    Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LastHit)
                {
                    var minion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .FirstOrDefault(
                                m =>
                                    MyOrbwalker.InAutoAttackRange(m) &&
                                    m.Health <= (Program.Q.GetDamage(m) + ObjectManager.Player.GetAutoAttackDamage(m)));
                    if (minion != null && minion.IsValidTarget())
                    {
                        Program.Q.Cast(minion.GetTumblePos());
                        return;
                    }
                }
            }
        }
    }
}
