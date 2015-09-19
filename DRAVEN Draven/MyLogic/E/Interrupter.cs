using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace DRAVEN_Draven.MyLogic.E
{
    public static partial class Events
    {
        public static void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Program.E.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High && sender.IsValidTarget(700))
            {
                Program.E.Cast(Program.E.GetPrediction(sender).UnitPosition);
            }
        }
    }
}
