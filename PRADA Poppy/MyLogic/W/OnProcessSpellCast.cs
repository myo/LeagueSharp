using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Poppy.Utils;

namespace PRADA_Poppy.MyLogic.W
{
    public static partial class Events
    {
        public static void OnProcessSpellcast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Program.ComboMenu.Item("WCombo").GetValue<bool>() && sender.IsEnemy && args.Target.IsMe)
            {
                if (SpellDb.GetByName(args.SData.Name).Dangerous)
                {
                    Program.W.Cast();
                }
            }
        }
    }
}
