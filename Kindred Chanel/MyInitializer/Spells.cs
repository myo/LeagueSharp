using LeagueSharp;
using LeagueSharp.Common;

namespace Kindred_Chanel.MyInitializer
{
    public static partial class ChanelLoader
    {
        public static void LoadSpells()
        {
            Program.Q = new Spell(SpellSlot.Q, 340f);
            Program.W = new Spell(SpellSlot.W, 800);
            Program.E = new Spell(SpellSlot.E, 500f);
            Program.R = new Spell(SpellSlot.R, 550f);
            //Program.Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);
        }
    }
}
