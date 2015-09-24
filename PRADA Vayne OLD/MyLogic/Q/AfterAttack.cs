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
                if (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.LaneClear)
                {
                    var meleeMinion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(m => m.IsMelee);
                    if (Program.LaneClearMenu.Item("QWaveClear").GetValue<bool>() && ObjectManager.Player.ManaPercent >= Program.LaneClearMenu.Item("QWaveClearMana").GetValue<Slider>().Value && meleeMinion.IsValidTarget())
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
            }
        }
    }
}
