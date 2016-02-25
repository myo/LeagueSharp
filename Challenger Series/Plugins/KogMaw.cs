#region License
/* Copyright (c) LeagueSharp 2016
 * No reproduction is allowed in any way unless given written consent
 * from the LeagueSharp staff.
 * 
 * Author: imsosharp
 * Date: 2/24/2016
 * File: KogMaw.cs
 */
#endregion License

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Utils;

namespace Challenger_Series.Plugins
{
    public class KogMaw : CSPlugin
    {
        public KogMaw()
        {
            base.Q = new Spell(SpellSlot.Q, 1175);
            base.Q.SetSkillshot(0.25f, 70f, 1650f, true, SkillshotType.SkillshotLine);
            base.W = new Spell(SpellSlot.W, 630);
            base.E = new Spell(SpellSlot.E, 1250);
            base.E.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            base.R = new Spell(SpellSlot.R, 1200);
            base.R.SetSkillshot(1.2f, 75f, 12000f, false, SkillshotType.SkillshotCircle);
            InitializeMenu();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnAction += OnAction;
        }

        private void OnAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
            if (orbwalkingActionArgs.Type == OrbwalkingType.AfterAttack)
            {
                if (orbwalkingActionArgs.Target is Obj_AI_Hero)
                {
                    var target = orbwalkingActionArgs.Target as Obj_AI_Hero;
                    var distFromTargetToMe = target.DistanceToPlayer();
                    if (Q.IsReady())
                    {
                        QLogic(target);
                    }
                    if (distFromTargetToMe < 350 && target.IsMelee)
                    {
                        ELogic(target);
                    }
                    if (IsWActive() && distFromTargetToMe > GetAttackRangeAfterWIsApplied() - 100)
                    {
                        ELogic(target);
                    }
                }
                if (orbwalkingActionArgs.Target is Obj_AI_Minion && GetJungleCampsOnCurrentMap() != null &&
                     GetJungleCampsOnCurrentMap().Contains(orbwalkingActionArgs.Target.Name))
                {
                    if (UseWJungleClearMenu[orbwalkingActionArgs.Target.Name].GetValue<MenuBool>())
                    {
                        W.Cast();
                    }
                }
            }
        }

        #region Events

        public override void OnUpdate(EventArgs args)
        {
            base.OnUpdate(args);
            if (UseWBool)
            {
                WLogic();
            }
            if (UseRBool)
            {
                RLogic();
            }
        }

        public override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            base.W.Range = GetAttackRangeAfterWIsApplied();
            base.R.Range = GetRRange();
            if (DrawWRangeBool)
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, GetAttackRangeAfterWIsApplied(), W.IsReady() || IsWActive() ? Color.LimeGreen : Color.Red);
            }
            if (DrawRRangeBool)
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, GetRRange() + 25, R.IsReady() ? Color.LimeGreen : Color.Red);
            }
            foreach(var targetCloseToMouse in GameObjects.EnemyHeroes.Where(en => en.DistanceToPlayer() > ObjectManager.Player.GetRealAutoAttackRange() && en.Position.Distance(Game.CursorPos) < 250))
            {
                ELogic(targetCloseToMouse);
            }
        }

        #endregion Events

        private Menu ComboMenu;
        private Menu HarassMenu;
        private Menu JungleclearMenu;
        private Menu UseWJungleClearMenu;
        private Menu DrawMenu;
        private MenuBool UseQBool;
        private MenuBool UseWBool;
        private MenuBool UseEBool;
        private MenuBool UseRBool;
        private MenuSlider MaxRStacksSlider;
        private MenuBool AlwaysSaveManaForWBool;
        private MenuBool UseRHarass;
        private MenuBool GetInPositionForWBeforeActivatingBool;
        private MenuBool DrawWRangeBool;
        private MenuBool DrawRRangeBool;

        public override void InitializeMenu()
        {
            base.InitializeMenu();
            ComboMenu = MainMenu.Add(new Menu("koggiecombomenu", "Combo Settings: "));
            UseQBool = ComboMenu.Add(new MenuBool("koggieuseq", "Use Q", true));
            UseWBool = ComboMenu.Add(new MenuBool("koggieusew", "Use W", true));
            UseEBool = ComboMenu.Add(new MenuBool("koggieusee", "Use E", true));
            UseRBool = ComboMenu.Add(new MenuBool("koggieuser", "Use R", true));
            GetInPositionForWBeforeActivatingBool =
                ComboMenu.Add(new MenuBool("koggiewintime", "Dont Activate W if In Danger!", false));
            HarassMenu = MainMenu.Add(new Menu("koggieharassmenu", "Harass Settings"));
            UseRHarass = HarassMenu.Add(new MenuBool("koggieuserharass", "Use R", true));
            JungleclearMenu = MainMenu.Add(new Menu("koggiejgclearmenu", "Jungleclear Settings: "));
            UseWJungleClearMenu = JungleclearMenu.Add(new Menu("koggiewjgcleartargets", "W if TARGET is: "));

            if (GetJungleCampsOnCurrentMap() != null)
            {
                foreach (var mob in GetJungleCampsOnCurrentMap())
                {
                    UseWJungleClearMenu.Add(new MenuBool(mob, mob, true));
                }
            }
            DrawMenu = MainMenu.Add(new Menu("koggiedrawmenu", "Drawing Settings"));
            DrawWRangeBool = DrawMenu.Add(new MenuBool("koggiedraww", "Draw W Range", true));
            DrawRRangeBool = DrawMenu.Add(new MenuBool("koggiedrawr", "Draw R Range", true));
            MaxRStacksSlider = MainMenu.Add(new MenuSlider("koggiermaxstacks", "R Max Stacks: ", 2, 0, 11));
            AlwaysSaveManaForWBool = MainMenu.Add(new MenuBool("koggiesavewmana", "Always Save Mana For W!", true));
            MainMenu.Attach();

        }

        #region ChampionLogic

        private void QLogic(Obj_AI_Hero target)
        {
            if (!UseQBool || !Q.IsReady() || Orbwalker.ActiveMode != OrbwalkingMode.Combo) return;
            if (AlwaysSaveManaForWBool && ObjectManager.Player.Mana < GetQMana() + GetWMana()) return;
            var prediction = Q.GetPrediction(target);
            if (target.Health > 1 && (int)prediction.Hitchance > (int)HitChance.Medium)
            {
                Q.Cast(prediction.UnitPosition);
            }
        }
        private void WLogic()
        {
            if (W.IsReady() && !IsWActive() &&
                GameObjects.EnemyHeroes.Any(h => h.DistanceToPlayer() < GetAttackRangeAfterWIsApplied() && h.Health > 1 && h.IsVisible) && Orbwalker.ActiveMode == OrbwalkingMode.Combo)
            {
                W.Cast();
            }
        }

        private void ELogic(Obj_AI_Hero target)
        {
            if (!UseEBool || !E.IsReady() || Orbwalker.ActiveMode != OrbwalkingMode.Combo) return;
            if (AlwaysSaveManaForWBool && ObjectManager.Player.Mana < GetEMana() + GetQMana()) return;
            var prediction = E.GetPrediction(target);
            if (target.Health > 1 && (int)prediction.Hitchance >= (int)HitChance.Medium)
            {
                E.Cast(prediction.UnitPosition);
            }
        }

        private void RLogic()
        {
            if (!UseRBool || !R.IsReady() || ObjectManager.Player.IsRecalling()) return;
            if (AlwaysSaveManaForWBool && ObjectManager.Player.Mana < GetRMana() + GetWMana()) return;
            if (GetRStacks() >= MaxRStacksSlider.Value) return;
            foreach (
                var enemy in
                    GameObjects.EnemyHeroes.Where(h => h.DistanceToPlayer() < R.Range && h.Health < R.GetDamage(h) && h.Health > 1))
            {
                var dist = enemy.DistanceToPlayer();
                if (IsWActive() && dist < GetAttackRangeAfterWIsApplied() + 25) break;
                if (Orbwalker.CanAttack() && dist < 550) break;
                var prediction = R.GetPrediction(enemy, true);
                if ((int) prediction.Hitchance >= (int) HitChance.Medium)
                {
                    R.Cast(prediction.UnitPosition);
                }
            }
            if (IsWActive() || (Orbwalker.ActiveMode != OrbwalkingMode.Combo && !UseRHarass)) return;

            foreach (var enemy in GameObjects.EnemyHeroes.Where(h => h.DistanceToPlayer() < R.Range && h.Health > 1))
            {
                var dist = enemy.DistanceToPlayer();
                if (Orbwalker.CanAttack() && dist < 550) break;
                var prediction = R.GetPrediction(enemy, true);
                if ((int) prediction.Hitchance >= (int) HitChance.Medium)
                {
                    R.Cast(prediction.UnitPosition);
                }
            }
        }

        private float GetAttackRangeAfterWIsApplied()
        {
            return W.Level > 0 ? new[] {630,660,690,720,750}[W.Level - 1] : 540;
        }

        private float GetRRange()
        {
            return R.Level > 0 ? new[] {1200,1500,1800}[R.Level - 1] : 1200;
        }

        private float GetQMana()
        {
            return 60;
        }

        private float GetWMana()
        {
            return 40;
        }

        private float GetEMana()
        {
            return E.Level > 0 ? new[] {80, 90, 100, 110, 120}[E.Level - 1] : 80;
        }

        private float GetRMana()
        {
            return new[] {50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 500}[GetRStacks()];
        }

        private int GetRStacks()
        {
            return ObjectManager.Player.HasBuff("kogmawlivingartillerycost") ? ObjectManager.Player.GetBuff("kogmawlivingartillerycost").Count : 0;
        }

        private bool IsWActive()
        {
            return ObjectManager.Player.HasBuff("KogMawBioArcaneBarrage");
        }
        private List<string> GetJungleCampsOnCurrentMap()
        {
            switch ((int)Game.MapId)
            {
                //Summoner's Rift
                case 11:
                    {
                        return SRMobs;
                    }
                //Twisted Treeline
                case 10:
                    {
                        return TTMobs;
                    }
            }
            return null;
        }

        /// <summary>
        /// Summoner's Rift Jungle "Big" Mobs
        /// </summary>
        private List<string> SRMobs = new List<string>
        {
            "SRU_Baron12.1.1",
            "SRU_Blue1.1.1",
            "SRU_Blue7.1.1",
            "Sru_Crab15.1.1",
            "Sru_Crab16.1.1",
            "SRU_Dragon6.1.1",
            "SRU_Gromp13.1.1",
            "SRU_Gromp14.1.1",
            "SRU_Krug5.1.2",
            "SRU_Krug11.1.2",
            "SRU_Murkwolf2.1.1",
            "SRU_Murkwolf8.1.1",
            "SRU_Razorbeak3.1.1",
            "SRU_Razorbeak9.1.1",
            "SRU_Red4.1.1",
            "SRU_Red10.1.1"
        };

        /// <summary>
        /// Twisted Treeline Jungle "Big" Mobs
        /// </summary>
        private List<string> TTMobs = new List<string>
        {
            "TT_NWraith1.1.1",
            "TT_NGolem2.1.1",
            "TT_NWolf3.1.1",
            "TT_NWraith4.1.1",
            "TT_NGolem5.1.1",
            "TT_NWolf6.1.1",
            "TT_Spiderboss8.1.1"
        };
        #endregion ChampionLogic
    }
}
