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
                    foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(MyOrbwalker.InAutoAttackRange))
                    {
                        var healthPred = MyUtils.HealthPrediction.GetHealthPrediction(
                            minion, (int) (250));
                        if (healthPred > 0 && healthPred < ObjectManager.Player.BaseAttackDamage - 15)
                        {
                            Tumble.Cast(minion.GetTumblePos());
                            Program.Orbwalker.ForceTarget(minion);
                            return;
                        }
                    }
                }
            }
        }
    }
}
