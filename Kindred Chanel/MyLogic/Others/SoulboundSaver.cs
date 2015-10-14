using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Kindred_Chanel.MyLogic.Others
{
    public static class SoulboundSaver
    {
        private static Dictionary<float, float> _incomingDamage = new Dictionary<float, float>();
        private static Dictionary<float, float> _instantDamage = new Dictionary<float, float>();

        public static float IncomingDamage
        {
            get { return _incomingDamage.Sum(e => e.Value) + _instantDamage.Sum(e => e.Value); }
        }

        //credits to hellsing, and jquery
        public static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (Program.R.IsReady())
                {
                    if ((!(sender is Obj_AI_Hero) || args.SData.IsAutoAttack()) && args.Target != null &&
                        args.Target.NetworkId == ObjectManager.Player.NetworkId)
                    {
                        _incomingDamage.Add(ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)/args.SData.MissileSpeed +
                            Game.Time, (float) sender.GetAutoAttackDamage(ObjectManager.Player));
                    }
                    else if (sender is Obj_AI_Hero)
                    {
                        var attacker = (Obj_AI_Hero) sender;
                        var slot = attacker.GetSpellSlot(args.SData.Name);

                        if (slot != SpellSlot.Unknown)
                        {
                            if (slot == attacker.GetSpellSlot("SummonerDot") && args.Target != null &&
                                args.Target.NetworkId == ObjectManager.Player.NetworkId)
                            {
                                _instantDamage.Add(Game.Time + 2,
                                    (float) attacker.GetSummonerSpellDamage(ObjectManager.Player, Damage.SummonerSpell.Ignite));
                            }
                            else if (slot.HasFlag(SpellSlot.Q | SpellSlot.W | SpellSlot.E | SpellSlot.R) &&
                                     ((args.Target != null && args.Target.NetworkId == ObjectManager.Player.NetworkId) ||
                                      args.End.Distance(ObjectManager.Player.ServerPosition) <
                                      Math.Pow(args.SData.LineWidth, 2)))
                            {
                                _instantDamage.Add(Game.Time + 2, (float) attacker.GetSpellDamage(ObjectManager.Player, slot));
                            }
                        }
                    }
                }
            }

            if (sender.IsMe)
            {
                if (args.SData.Name == "KalistaExpungeWrapper")
                {
                    Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer);
                }
            }
        }

        public static void OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsRecalling() || ObjectManager.Player.InFountain())
                return;

                if (Program.ComboMenu.Item("RComboSelf").GetValue<bool>() && IncomingDamage > ObjectManager.Player.Health && ObjectManager.Player.CountEnemiesInRange(800) > 0)
                {
                    Program.R.Cast(ObjectManager.Player);
                }
                if (Program.ComboMenu.Item("RComboSaver").GetValue<bool>())
                {
                    var salvableAlly =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .FirstOrDefault(
                                h =>
                                    h.IsAlly &&
                                    h.HealthPercent < Program.ComboMenu.Item("RMinHP").GetValue<Slider>().Value &&
                                    h.Distance(ObjectManager.Player) < 500);
                    if (salvableAlly != null)
                    {
                        Program.R.Cast(salvableAlly);
                    }
                }
            }
    }
}
