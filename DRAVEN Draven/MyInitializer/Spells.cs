using LeagueSharp;
using LeagueSharp.Common;

namespace DRAVEN_Draven.MyInitializer
{
    public static partial class DRAVENLoader
    {
        public static void LoadSpells()
        {
            Program.Q = new Spell(SpellSlot.Q);
            Program.W = new Spell(SpellSlot.W);
            Program.E = new Spell(SpellSlot.E, 1000f);
            Program.R = new Spell(SpellSlot.R, 20000f);
            Program.E.SetSkillshot(0.40f, 50f, 100f, false, SkillshotType.SkillshotLine);
            Program.R.SetSkillshot(0.50f, 150f, 150f, false, SkillshotType.SkillshotLine);
        }
    }
}
