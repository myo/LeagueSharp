using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Vayne.MyUtils;

namespace PRADA_Vayne.MyLogic.Others
{
    public static partial class Events
    {
        public static void OnUpdate(EventArgs args)
        {
            if (Heroes.Player.HasBuff("rengarralertsound"))
            {
                if (Items.HasItem((int)ItemId.Oracles_Lens_Trinket, Heroes.Player) && Items.CanUseItem((int)ItemId.Oracles_Lens_Trinket))
                {
                    Items.UseItem((int)ItemId.Oracles_Lens_Trinket, Heroes.Player.Position);
                }
                else if (Items.HasItem((int)ItemId.Vision_Ward, Heroes.Player))
                {
                    Items.UseItem((int)ItemId.Vision_Ward, Heroes.Player.Position.Randomize(0, 125));
                }
            }

            var enemyVayne = Heroes.EnemyHeroes.FirstOrDefault(e => e.CharData.BaseSkinName == "Vayne");
            if (enemyVayne != null && enemyVayne.Distance(Heroes.Player) < 700 && enemyVayne.HasBuff("VayneInquisition"))
            {
                if (Items.HasItem((int)ItemId.Oracles_Lens_Trinket, Heroes.Player) && Items.CanUseItem((int)ItemId.Oracles_Lens_Trinket))
                {
                    Items.UseItem((int)ItemId.Oracles_Lens_Trinket, Heroes.Player.Position);
                }
                else if (Items.HasItem((int)ItemId.Vision_Ward, Heroes.Player))
                {
                    Items.UseItem((int)ItemId.Vision_Ward, Heroes.Player.Position.Randomize(0, 125));
                }
            }

            if (Heroes.Player.InFountain() && Program.ComboMenu.Item("AutoBuy").GetValue<bool>() && Heroes.Player.Level > 6 && Items.HasItem((int)ItemId.Warding_Totem_Trinket))
            {
                Heroes.Player.BuyItem(ItemId.Scrying_Orb_Trinket);
            }
            if (Heroes.Player.InFountain() && Program.ComboMenu.Item("AutoBuy").GetValue<bool>() && !Items.HasItem((int)ItemId.Oracles_Lens_Trinket, Heroes.Player) && Heroes.Player.Level > 6 && HeroManager.Enemies.Any(h => h.CharData.BaseSkinName == "Rengar" || h.CharData.BaseSkinName == "Talon" || h.CharData.BaseSkinName == "Vayne"))
            {
                Heroes.Player.BuyItem(ItemId.Sweeping_Lens_Trinket);
            }
            if (Heroes.Player.InFountain() && Program.ComboMenu.Item("AutoBuy").GetValue<bool>() && Heroes.Player.Level >= 9 && Items.HasItem((int)ItemId.Sweeping_Lens_Trinket))
            {
                Heroes.Player.BuyItem(ItemId.Oracles_Lens_Trinket);
            }
        }
    }
}
