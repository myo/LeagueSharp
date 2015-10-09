using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace Brain.TheBrain.Activator
{
    public static class Cleanser
    {
        public static void OnUpdate(EventArgs args)
        {
            foreach (var buff in ObjectManager.Player.Buffs)
            {
                var buffMenu = Core.CleanserMenu.SubMenu(buff.Type.ToString());
                if (buff.EndTime - Utils.GameTimeTickCount >= buffMenu.Item("minbuffduration").GetValue<Slider>().Value &&
                    ObjectManager.Player.HealthPercent <= buffMenu.Item("minhealthpercent").GetValue<Slider>().Value &&
                    ObjectManager.Player.CountEnemiesInRange(800) >=
                    buffMenu.Item("minenemies").GetValue<Slider>().Value)
                {
                    if (buffMenu.Item("qss").GetValue<bool>())
                    {
                        if (Items.CanUseItem((int) ItemId.Quicksilver_Sash))
                        {
                            Utility.DelayAction.Add(buffMenu.Item("delay").GetValue<Slider>().Value,
                                () => Items.UseItem((int) ItemId.Quicksilver_Sash));
                            return;
                        }
                        if (Items.CanUseItem((int) ItemId.Mercurial_Scimitar))
                        {
                            Utility.DelayAction.Add(buffMenu.Item("delay").GetValue<Slider>().Value, () => Items.UseItem((int) ItemId.Mercurial_Scimitar));
                            return;
                        }
                    }
                    if (Core.SummonerCleanse.IsReady() && buffMenu.Item("summonercleanse").GetValue<bool>())
                    {
                        Utility.DelayAction.Add(buffMenu.Item("delay").GetValue<Slider>().Value, () => ObjectManager.Player.Spellbook.CastSpell(Core.SummonerCleanse));
                        return;
                    }
                }
            }
        }
    }
}