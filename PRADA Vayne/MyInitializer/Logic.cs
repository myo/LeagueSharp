using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Vayne.MyUtils;

namespace PRADA_Vayne.MyInitializer
{
    public static partial class PRADALoader
    {
        public static void LoadLogic()
        {
            #region Q
            MyOrbwalker.AfterAttack += MyLogic.Q.Events.AfterAttack;
            MyOrbwalker.BeforeAttack += MyLogic.Q.Events.BeforeAttack;
            MyOrbwalker.OnAttack += MyLogic.Q.Events.OnAttack;
            Spellbook.OnCastSpell += MyLogic.Q.Events.OnCastSpell;
            AntiGapcloser.OnEnemyGapcloser += MyLogic.Q.Events.OnGapcloser;
            Obj_AI_Base.OnProcessSpellCast += MyLogic.Q.Events.OnProcessSpellCast;
            Game.OnUpdate += MyLogic.Q.Events.OnUpdate;
            CustomEvents.Game.OnGameLoad += MyLogic.Q.WallTumble.OnLoad;
            #endregion

            #region E

            GameObject.OnCreate += MyLogic.E.AntiAssasins.OnCreateGameObject;
            AntiGapcloser.OnEnemyGapcloser += MyLogic.E.Events.OnGapcloser;
            Game.OnUpdate += MyLogic.E.Events.OnUpdate;
            Interrupter2.OnInterruptableTarget += MyLogic.E.Events.OnPossibleToInterrupt;

            #endregion

            #region R

            Spellbook.OnCastSpell += MyLogic.R.Events.OnCastSpell;

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
