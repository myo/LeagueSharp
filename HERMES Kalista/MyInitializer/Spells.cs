using LeagueSharp;
using LeagueSharp.Common;

namespace HERMES_Kalista.MyInitializer
{
    public static partial class HERMESLoader
    {
        public static void LoadSpells()
        {
            Program.Q = new Spell(SpellSlot.Q, 1150f);
            Program.W = new Spell(SpellSlot.W, 5000);
            Program.E = new Spell(SpellSlot.E, 1000f);
            Program.R = new Spell(SpellSlot.R, 1500f);
            Program.Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);
        }
    }
}
