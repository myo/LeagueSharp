using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            if (target.HasBuff("kindredrnodeathbuff")) return false;
            if (target.Name.Contains("Baron") || target.Name.Contains("Dragon") || target.Health > 5)
            {
                if (target is Obj_AI_Hero)
                {
                    var objaihero_target = target as Obj_AI_Hero;
                    if (objaihero_target.HasSpellShield() || objaihero_target.HasUndyingBuff())
                    {
                        return false;
                    }
                }
                var dmg = Program.E.GetDamage(target);
                if (ObjectManager.Player.HasBuff("SummonerExhaustSlow"))
                {
                    dmg *= 0.55f;
                }
                if (target.Name.Contains("Baron"))
                {
                    dmg -= 20;
                    if (ObjectManager.Player.HasBuff("barontarget"))
                    {
                        dmg *= 0.5f;
                    }
                }
                if (target.Name.Contains("Dragon"))
                {
                    dmg -= 20;
                    if (ObjectManager.Player.HasBuff("s5test_dragonslayerbuff"))
                    {
                        dmg *= (1f - (0.07f*ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff")));
                    }
                }
                return dmg > target.Health;
            }
            return false;
        }
        public static BuffInstance GetRendBuff(this Obj_AI_Base target)
        {
            return target.Buffs.Find(b => b.Caster.IsMe && b.IsValidBuff() && b.DisplayName.ToLower() == "kalistaexpungemarker");
        }
    }
}
