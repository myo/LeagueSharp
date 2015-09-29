using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HERMES_Kalista.MyUtils;
using LeagueSharp;
using LeagueSharp.Common;

namespace HERMES_Kalista.MyLogic.Others
{
    public static class Damages
    {
        public static bool IsRendKillable(this Obj_AI_Base target)
        {
            var h = target as Obj_AI_Hero;
            if (h.IsValid<Obj_AI_Hero>())
            {
                if (h.HasSpellShield() || h.HasUndyingBuff()) return false;
            }
            var dmg = Program.E.GetDamage(target);
            if (ObjectManager.Player.HasBuff("SummonerExhaustSlow"))
            {
                dmg *= 0.6f;
            }
            return dmg > target.Health;
        }
        public static BuffInstance GetRendBuff(this Obj_AI_Base target)
        {
            return target.Buffs.Find(b => b.Caster.IsMe && b.IsValidBuff() && b.DisplayName.ToLower() == "kalistaexpungemarker");
        }
    }
}
