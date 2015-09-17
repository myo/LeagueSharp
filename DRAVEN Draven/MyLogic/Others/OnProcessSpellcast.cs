using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using DRAVEN_Draven.MyLogic.Q;
using DRAVEN_Draven.MyUtils;
using DRAVEN_Draven.Utils;

namespace DRAVEN_Draven.MyLogic.Others
{
    public static partial class Events
    {
        public static void OnProcessSpellcast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            #region Anti-Stealth
            if (args.SData.Name.ToLower().Contains("talonshadow")) //#TODO get the actual buff name
            {
                if (Items.HasItem((int)ItemId.Oracles_Lens_Trinket) && Items.CanUseItem((int)ItemId.Oracles_Lens_Trinket))
                {
                    Items.UseItem((int)ItemId.Oracles_Lens_Trinket, Heroes.Player.Position);
                }
                else if (Items.HasItem((int)ItemId.Vision_Ward, Heroes.Player))
                {
                    Items.UseItem((int)ItemId.Vision_Ward, Heroes.Player.Position.Randomize(0, 125));
                }
            }
            #endregion

            if (MyWizard.ShouldSaveCondemn()) return;
            if (sender.Distance(Heroes.Player) > 1500 || !args.Target.IsMe || args.SData == null)
                return;
            //how to milk alistar/thresh/everytoplaner
            var spellData = SpellDb.GetByName(args.SData.Name);
            if (spellData != null && !Heroes.Player.UnderTurret(true) && !Lists.UselessChamps.Contains(sender.CharData.BaseSkinName))
            {
                if (spellData.CcType == CcType.Knockup || spellData.CcType == CcType.Stun ||
                    spellData.CcType == CcType.Knockback || spellData.CcType == CcType.Suppression)
                {
                    Program.E.Cast(sender);
                }
            }
        }
    }
}
