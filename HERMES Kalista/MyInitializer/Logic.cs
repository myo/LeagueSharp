using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using HERMES_Kalista.MyUtils;

namespace HERMES_Kalista.MyInitializer
{
    public static partial class HERMESLoader
    {
        public static void LoadLogic()
        {
            CustomEvents.Game.OnGameLoad += MyLogic.Spells.OnLoad;
            Game.OnUpdate += MyLogic.Others.SoulboundSaver.OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += MyLogic.Others.SoulboundSaver.OnProcessSpellCast;

            #region Others

            Game.OnUpdate += MyLogic.Others.Events.OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += MyLogic.Others.Events.OnProcessSpellcast;
            Drawing.OnDraw += MyLogic.Others.Events.OnDraw;
            Game.OnUpdate += MyLogic.Others.SkinHack.OnUpdate;

            #endregion
        }
    }
}
