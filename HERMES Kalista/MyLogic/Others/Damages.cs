using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace HERMES_Kalista.MyLogic.Others
{
    public static class Damages
    {
        private static Obj_AI_Hero player = ObjectManager.Player;

        private static readonly float[] RawRendDamage = { 20, 30, 40, 50, 60 };
        private static readonly float[] RawRendDamageMultiplier = { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f };
        private static readonly float[] RawRendDamagePerSpear = { 10, 14, 19, 25, 32 };
        private static readonly float[] RawRendDamagePerSpearMultiplier = { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f };

        public static bool IsRendKillable(this Obj_AI_Base target)
        {
            return GetRendDamage(target) - Program.ComboMenu.Item("DamageReductionE").GetValue<Slider>().Value +
                   Program.ComboMenu.Item("DamageAdditionerE").GetValue<Slider>().Value > GetActualHealth(target);
        }

        /// <summary>
        ///     Gets the targets health including the shield amount
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The targets health
        /// </returns>
        public static float GetActualHealth(Obj_AI_Base target)
        {
            return target.Health;
        }

        public static float GetRendDamage(Obj_AI_Hero target)
        {
            return (float)GetRendDamage(target, -1);
        }

        public static float GetRendDamage(Obj_AI_Base target, int customStacks = -1)
        {
            return ((float)player.CalcDamage(target, Damage.DamageType.Physical, GetRawRendDamage(target, customStacks)) - 20 * 0.98f);
        }

        public static bool HasRendBuff(this Obj_AI_Base target)
        {
            return target.GetRendBuff() != null;
        }

        public static BuffInstance GetRendBuff(this Obj_AI_Base target)
        {
            return target.Buffs.Find(b => b.Caster.IsMe && b.IsValidBuff() && b.DisplayName == "kalistaexpungemarker");
        }

        public static float GetRawRendDamage(Obj_AI_Base target, int customStacks = -1)
        {
            if (target.GetBuffCount("kalistaexpungemarker") != 0 || customStacks > -1)
            {
                return (RawRendDamage[Program.E.Level - 1] + RawRendDamageMultiplier[Program.E.Level - 1] * player.TotalAttackDamage()) +
                       ((customStacks < 0 ? target.GetBuffCount("kalistaexpungemarker") : customStacks) - 1) *
                       (RawRendDamagePerSpear[Program.E.Level - 1] + RawRendDamagePerSpearMultiplier[Program.E.Level - 1] * player.TotalAttackDamage());
            }

            return 0;
        }

        public static float GetTotalDamage(Obj_AI_Hero target)
        {
            double damage = player.GetAutoAttackDamage(target);

            if (Program.Q.IsReady())
                damage += player.GetSpellDamage(target, SpellSlot.Q);

            if (Program.E.IsReady())
                damage += GetRendDamage(target);

            return (float)damage;
        }

        public static float TotalAttackDamage(this Obj_AI_Base target)
        {
            return target.BaseAttackDamage + target.FlatPhysicalDamageMod;
        }
    }
}
