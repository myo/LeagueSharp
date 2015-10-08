using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Poppy.MyUtils;

namespace PRADA_Poppy.MyInitializer
{
    public static partial class PRADALoader
    {
        public static void LoadLogic()
        {
            #region Q
            MyOrbwalker.BeforeAttack += MyLogic.Q.Events.BeforeAttack;
            #endregion

            #region W
            Obj_AI_Base.OnProcessSpellCast += MyLogic.W.Events.OnProcessSpellcast;
            #endregion

            #region E

            GameObject.OnCreate += MyLogic.E.AntiAssasins.OnCreateGameObject;
            AntiGapcloser.OnEnemyGapcloser += MyLogic.E.Events.OnGapcloser;
            Game.OnUpdate += MyLogic.E.Events.OnUpdate;
            Interrupter2.OnInterruptableTarget += MyLogic.E.Events.OnPossibleToInterrupt;

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
