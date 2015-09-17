using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using DRAVEN_Draven.MyUtils;

namespace DRAVEN_Draven.MyInitializer
{
    public static partial class DRAVENLoader
    {
        public static void LoadLogic()
        {
            #region Q
            Game.OnUpdate += MyLogic.Q.Events.OnUpdate;
            #endregion

            #region E

            GameObject.OnCreate += MyLogic.E.AntiAssasins.OnCreateGameObject;
            AntiGapcloser.OnEnemyGapcloser += MyLogic.E.Events.OnGapcloser;
            Game.OnUpdate += MyLogic.E.Events.OnUpdate;
            Interrupter2.OnInterruptableTarget += MyLogic.E.Events.OnPossibleToInterrupt;

            #endregion

            #region W

            Game.OnUpdate += MyLogic.W.Events.OnUpdate;
            Orbwalking.AfterAttack += MyLogic.W.Events.AfterAttack;
            #endregion

            #region R

            Game.OnUpdate += MyLogic.R.Events.OnUpdate;

            #endregion

            #region Others

            Game.OnUpdate += MyLogic.Others.Events.OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += MyLogic.Others.Events.OnProcessSpellcast;
            Drawing.OnDraw += MyLogic.Others.Events.OnDraw;
            Game.OnUpdate += MyLogic.Others.SkinHack.OnUpdate;

            #endregion
        }
    }
}
