using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace PRADA_Poppy.MyLogic.E
{
    public static partial class Events
    {
        public static void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel == Interrupter2.DangerLevel.High && Program.E.IsReady() && Program.E.IsInRange(sender))
            {
                Program.E.Cast(sender);
            }
        }
    }
}
