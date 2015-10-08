using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Poppy.MyUtils;

namespace PRADA_Poppy.MyLogic.R
{
    public static partial class Events
    {
        public static void OnUpdate(EventArgs args)
        {
            if (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.Combo && Program.R.IsReady())
            {
                switch (Program.ComboMenu.Item("RCombo").GetValue<StringList>().SelectedValue)
                {
                    case "SUPPORTS":
                        Program.R.Cast(
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(h => h.IsEnemy && !h.HasBuffOfType(BuffType.SpellShield) && h.Distance(ObjectManager.Player) < Program.R.Range)
                                .OrderBy(e => e.TotalAttackDamage + e.TotalMagicalDamage)
                                .FirstOrDefault());
                        break;
                    case "CARRIES":
                        Program.R.Cast(ObjectManager.Get<Obj_AI_Hero>().
                            Where(
                                h =>
                                    h.IsEnemy && !h.HasBuffOfType(BuffType.SpellShield) &&
                                    h.Distance(ObjectManager.Player) <
                                    Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
                            .OrderByDescending(e => e.TotalAttackDamage + e.TotalMagicalDamage)
                            .FirstOrDefault());
                        break;
                }
                if (ObjectManager.Player.UnderTurret(true))
                {
                    Program.R.Cast(
                        ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.IsEnemy &&
                                                                             h.Distance(ObjectManager.Player) <
                                                                             Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)));
                }
            }
        }
    }
}
