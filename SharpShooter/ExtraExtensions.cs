using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter
{
    internal static class ExtraExtensions
    {
        internal static bool IsReadyPerfectly(this Spell spell)
        {
            return spell != null && spell.Slot != SpellSlot.Unknown && spell.Instance.State != SpellState.Cooldown &&
                   spell.Instance.State != SpellState.Disabled && spell.Instance.State != SpellState.NoMana &&
                   spell.Instance.State != SpellState.NotLearned && spell.Instance.State != SpellState.Surpressed &&
                   spell.Instance.State != SpellState.Unknown && spell.Instance.State == SpellState.Ready;
        }

        internal static bool IsKillableAndValidTarget(this Obj_AI_Hero target, double calculatedDamage,
            TargetSelector.DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.CharData.BaseSkinName == "gangplankbarrel")
                return false;

            if (target.HasBuff("kindredrnodeathbuff"))
            {
                return false;
            }

            // Tryndamere's Undying Rage (R)
            if (target.HasBuff("Undying Rage"))
            {
                return false;
            }

            // Kayle's Intervention (R)
            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            // Poppy's Diplomatic Immunity (R)
            if (target.HasBuff("DiplomaticImmunity") && !ObjectManager.Player.HasBuff("poppyulttargetmark"))
            {
                //TODO: Get the actual target mark buff name
                return false;
            }

            // Banshee's Veil (PASSIVE)
            if (target.HasBuff("BansheesVeil"))
            {
                // TODO: Get exact Banshee's Veil buff name.
                return false;
            }

            // Sivir's Spell Shield (E)
            if (target.HasBuff("SivirShield"))
            {
                // TODO: Get exact Sivir's Spell Shield buff name
                return false;
            }

            // Nocturne's Shroud of Darkness (W)
            if (target.HasBuff("ShroudofDarkness"))
            {
                // TODO: Get exact Nocturne's Shourd of Darkness buff name
                return false;
            }

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            if (target.ChampionName == "Blitzcrank")
                if (!target.HasBuff("manabarriercooldown"))
                    if (target.Health + target.HPRegenRate +
                        (damageType == TargetSelector.DamageType.Physical ? target.PhysicalShield : target.MagicalShield) +
                        target.Mana * 0.6 + target.PARRegenRate < calculatedDamage)
                        return true;

            if (target.ChampionName == "Garen")
                if (target.HasBuff("GarenW"))
                    calculatedDamage *= 0.7;

            if (target.HasBuff("FerociousHowl"))
                calculatedDamage *= 0.3;

            return target.Health + target.HPRegenRate +
                   (damageType == TargetSelector.DamageType.Physical ? target.PhysicalShield : target.MagicalShield) <
                   calculatedDamage - 2;
        }

        internal static bool IsKillableAndValidTarget(this Obj_AI_Minion target, double calculatedDamage,
            TargetSelector.DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.Health <= 0 ||
                target.HasBuffOfType(BuffType.SpellImmunity) || target.HasBuffOfType(BuffType.SpellShield) ||
                target.CharData.BaseSkinName == "gangplankbarrel")
                return false;

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            var dragonSlayerBuff = ObjectManager.Player.GetBuff("s5test_dragonslayerbuff");
            if (dragonSlayerBuff != null)
            {
                if (dragonSlayerBuff.Count >= 4)
                    calculatedDamage += dragonSlayerBuff.Count == 5 ? calculatedDamage * 0.30 : calculatedDamage * 0.15;

                if (target.CharData.BaseSkinName.ToLowerInvariant().Contains("dragon"))
                    calculatedDamage *= 1 - dragonSlayerBuff.Count * 0.07;
            }

            if (target.CharData.BaseSkinName.ToLowerInvariant().Contains("baron") &&
                ObjectManager.Player.HasBuff("barontarget"))
                calculatedDamage *= 0.5;

            return target.Health + target.HPRegenRate +
                   (damageType == TargetSelector.DamageType.Physical ? target.PhysicalShield : target.MagicalShield) <
                   calculatedDamage - 2;
        }

        internal static bool IsKillableAndValidTarget(this Obj_AI_Base target, double calculatedDamage,
            TargetSelector.DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.CharData.BaseSkinName == "gangplankbarrel")
                return false;

            if (target.HasBuff("kindredrnodeathbuff"))
            {
                return false;
            }

            // Tryndamere's Undying Rage (R)
            if (target.HasBuff("Undying Rage"))
            {
                return false;
            }

            // Kayle's Intervention (R)
            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            // Poppy's Diplomatic Immunity (R)
            if (target.HasBuff("DiplomaticImmunity") && !ObjectManager.Player.HasBuff("poppyulttargetmark"))
            {
                //TODO: Get the actual target mark buff name
                return false;
            }

            // Banshee's Veil (PASSIVE)
            if (target.HasBuff("BansheesVeil"))
            {
                // TODO: Get exact Banshee's Veil buff name.
                return false;
            }

            // Sivir's Spell Shield (E)
            if (target.HasBuff("SivirShield"))
            {
                // TODO: Get exact Sivir's Spell Shield buff name
                return false;
            }

            // Nocturne's Shroud of Darkness (W)
            if (target.HasBuff("ShroudofDarkness"))
            {
                // TODO: Get exact Nocturne's Shourd of Darkness buff name
                return false;
            }

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            if (target.CharData.BaseSkinName == "Blitzcrank")
                if (!target.HasBuff("manabarriercooldown"))
                    if (target.Health + target.HPRegenRate +
                        (damageType == TargetSelector.DamageType.Physical ? target.PhysicalShield : target.MagicalShield) +
                        target.Mana * 0.6 + target.PARRegenRate < calculatedDamage)
                        return true;

            if (target.CharData.BaseSkinName == "Garen")
                if (target.HasBuff("GarenW"))
                    calculatedDamage *= 0.7;


            if (target.HasBuff("FerociousHowl"))
                calculatedDamage *= 0.3;

            var dragonSlayerBuff = ObjectManager.Player.GetBuff("s5test_dragonslayerbuff");
            if (dragonSlayerBuff != null)
                if (target.IsMinion)
                {
                    if (dragonSlayerBuff.Count >= 4)
                        calculatedDamage += dragonSlayerBuff.Count == 5 ? calculatedDamage * 0.30 : calculatedDamage * 0.15;

                    if (target.CharData.BaseSkinName.ToLowerInvariant().Contains("dragon"))
                        calculatedDamage *= 1 - dragonSlayerBuff.Count * 0.07;
                }

            if (target.CharData.BaseSkinName.ToLowerInvariant().Contains("baron") &&
                ObjectManager.Player.HasBuff("barontarget"))
                calculatedDamage *= 0.5;

            return target.Health + target.HPRegenRate +
                   (damageType == TargetSelector.DamageType.Physical ? target.PhysicalShield : target.MagicalShield) <
                   calculatedDamage - 2;
        }

        internal static bool IsManaPercentOkay(this Obj_AI_Hero hero, int manaPercent)
        {
            return hero.ManaPercent > manaPercent;
        }

        internal static double IsImmobileUntil(this Obj_AI_Hero unit)
        {
            var result =
                unit.Buffs.Where(
                    buff =>
                        buff.IsActive && Game.Time <= buff.EndTime &&
                        (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                         buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare))
                    .Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return result - Game.Time;
        }

        internal static bool IsWillDieByTristanaE(this Obj_AI_Base target)
        {
            if (ObjectManager.Player.ChampionName == "Tristana")
                if (target.HasBuff("tristanaecharge"))
                    if (
                        target.IsKillableAndValidTarget(
                            (float)
                                (ObjectManager.Player.GetSpellDamage(target, SpellSlot.E) *
                                 (target.GetBuffCount("tristanaecharge") * 0.30) +
                                 ObjectManager.Player.GetSpellDamage(target, SpellSlot.E)),
                            TargetSelector.DamageType.Physical))
                        return true;
            return false;
        }

        internal static Spell.CastStates CastWithExtraTrapLogic(this Spell spell)
        {
            if (spell.IsReadyPerfectly())
            {
                var teleport = MinionManager.GetMinions(spell.Range).FirstOrDefault(x => x.HasBuff("teleport_target"));
                var zhonya =
                    HeroManager.Enemies.FirstOrDefault(
                        x => ObjectManager.Player.Distance(x) <= spell.Range && x.HasBuff("zhonyasringshield"));

                if (teleport != null)
                    return spell.Cast(teleport);

                if (zhonya != null)
                    return spell.Cast(zhonya);
            }
            return Spell.CastStates.NotCasted;
        }

        internal static float GetRealAutoAttackRange(this AttackableUnit unit, AttackableUnit target,
            int autoAttackRange)
        {
            var result = autoAttackRange + unit.BoundingRadius;
            if (target.IsValidTarget())
                return result + target.BoundingRadius;
            return result;
        }

        internal static bool IsJungleMob(this Obj_AI_Base mob)
        {
            return
                MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                    .Any(x => x.NetworkId == mob.NetworkId);
        }

        internal static bool IsLaneMob(this Obj_AI_Base mob)
        {
            return MinionManager.GetMinions(float.MaxValue).Any(x => x.NetworkId == mob.NetworkId);
        }
    }
}