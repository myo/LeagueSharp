using LeagueSharp;
using LeagueSharp.Common;

namespace PRADA_Poppy.MyInitializer
{
    public static partial class PRADALoader
    {
        public static void LoadSpells()
        {
            Program.Q = new Spell(SpellSlot.Q);
            Program.W = new Spell(SpellSlot.W);
            Program.E = new Spell(SpellSlot.E, 500f);
            Program.R = new Spell(SpellSlot.R, 800f);
        }
    }
}
