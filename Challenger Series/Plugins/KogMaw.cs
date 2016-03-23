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
            Obj_AI_Hero.OnDoCast += OnDoCast;
            Rand = new Random();
        }

        private void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name.Contains("BarrageAttack"))
            {
                AttacksLanded++;
            }
            if (IsWActive() && AttacksLanded > HumanizerMaxAttacks.Value && ObjectManager.Player.AttackSpeedMod / 2 > HumanizerAttackSpeed.Value)
            {
                Orbwalker.SetMovementState(false);
            }
        }

        private Random Rand;
        private int AttacksLanded = 0;
        private int HumanizerArmTime = 0;

        private void OnAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
            if (orbwalkingActionArgs.Type == OrbwalkingType.AfterAttack)
            {
                if (orbwalkingActionArgs.Target is Obj_AI_Hero)
                {
                    var target = orbwalkingActionArgs.Target as Obj_AI_Hero;
                    if (IsWActive())
                    {
                        if (!SuperAggressiveHumanizer && base.ValidTargets.Any(
                                enemy => enemy.Health > 1 &&
                                    enemy.IsValidTarget() && enemy.IsMelee &&
                                    enemy.Distance(ObjectManager.Player) < Rand.Next(350, 400)))
                        {
                                Orbwalker.SetMovementState(true);
                        }
                    }
                    var distFromTargetToMe = target.Distance(ObjectManager.Player.ServerPosition);
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
                if (orbwalkingActionArgs.Target is Obj_AI_Minion)
                {
                    if (IsWActive())
                    {
                        if (AttacksLanded > HumanizerMaxAttacks &&
                            ObjectManager.Player.AttackSpeedMod/2 > HumanizerAttackSpeed.Value)
                        {
                            Orbwalker.SetMovementState(false);
                            AttacksLanded++;
                            HumanizerArmTime = Environment.TickCount;
                        }
                    }
                    if (GetJungleCampsOnCurrentMap() != null && Orbwalker.ActiveMode == OrbwalkingMode.LaneClear)
                    {
                        var targetName = (orbwalkingActionArgs.Target as Obj_AI_Minion).CharData.BaseSkinName;

                        if (!targetName.Contains("Mini") && GetJungleCampsOnCurrentMap().Contains(targetName) &&
                            UseWJungleClearMenu[targetName].GetValue<MenuBool>())
                        {
                            W.Cast();
                        }
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
            if (Environment.TickCount - HumanizerArmTime > HumanizerChillTime.Value || !IsWActive())
            {
                AttacksLanded = 0;
                HumanizerArmTime = Environment.TickCount;
                Orbwalker.SetMovementState(true);
            }
            foreach(var targetCloseToMouse in base.ValidTargets.Where(en => en.Distance(ObjectManager.Player.ServerPosition) > ObjectManager.Player.GetRealAutoAttackRange() && en.Distance(ObjectManager.Player.ServerPosition) < 1400 && en.Position.Distance(Game.CursorPos) < 250))
            {
                ELogic(targetCloseToMouse);
            }
            if (!SuperAggressiveHumanizer &&
                !GameObjects.Enemy.Any(
                    en =>
                        en.IsValidTarget() &&
                        en.Distance(ObjectManager.Player) < ObjectManager.Player.GetRealAutoAttackRange()))
            {
                AttacksLanded = 0;
                Orbwalker.SetMovementState(true);
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
        private Menu HumanizerMenu;
        private MenuSlider HumanizerAttackSpeed;
        private MenuSlider HumanizerMaxAttacks;
        private MenuBool SuperAggressiveHumanizer;
        private MenuSlider HumanizerChillTime;
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
            HumanizerMenu = MainMenu.Add(new Menu("koggiehumanizer", "Humanizer Settings: "));
            HumanizerAttackSpeed =
                HumanizerMenu.Add(new MenuSlider("koggiedontmoveifasbiggerthan", "Don't move if AS > %", 10, 2, 10));
            HumanizerMaxAttacks =
                HumanizerMenu.Add(new MenuSlider("koggiehumanizerminattacks", "Min W Attacks before Moving", 3, 0, 13));
            HumanizerChillTime =
                HumanizerMenu.Add(new MenuSlider("koggiehumanizerchilltime", "Chill Time (ms)", 200, 0, 2000));
            SuperAggressiveHumanizer =
                HumanizerMenu.Add(new MenuBool("koggiesuperaggrohumanizer", "Super Aggressive Humanizer", false));
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
            if (target.IsValidTarget() && (int)prediction.Hitchance > (int)HitChance.Medium)
            {
                Q.Cast(prediction.UnitPosition);
            }
        }
        private void WLogic()
        {
            if (W.IsReady() && !IsWActive() &&
                base.ValidTargets.Any(h => h.Health > 1 && h.Distance(ObjectManager.Player.ServerPosition) < GetAttackRangeAfterWIsApplied() && h.IsValidTarget()) && Orbwalker.ActiveMode == OrbwalkingMode.Combo)
            {
                W.Cast();
            }
        }

        private void ELogic(Obj_AI_Hero target)
        {
            if (!UseEBool || !E.IsReady() || Orbwalker.ActiveMode != OrbwalkingMode.Combo) return;
            if (AlwaysSaveManaForWBool && ObjectManager.Player.Mana < GetEMana() + GetQMana()) return;
            var prediction = E.GetPrediction(target);
            if (target.IsValidTarget() && (int)prediction.Hitchance >= (int)HitChance.Medium)
            {
                E.Cast(prediction.UnitPosition);
            }
        }

        private void RLogic()
        {
            if (!UseRBool || !R.IsReady() || ObjectManager.Player.IsRecalling() || Orbwalker.ActiveMode == OrbwalkingMode.None) return;
            if (AlwaysSaveManaForWBool && ObjectManager.Player.Mana < GetRMana() + GetWMana()) return;
            var myPos = ObjectManager.Player.ServerPosition;
            foreach (
                var enemy in
                    base.ValidTargets.Where(h => h.Distance(myPos) < R.Range && (!IsWActive() || h.Distance(myPos) > W.Range) && h.Health < R.GetDamage(h) * 2 && h.IsValidTarget()))
            {
                var dist = enemy.Distance(myPos);
                if (IsWActive() && dist < GetAttackRangeAfterWIsApplied() + 25) break;
                if (Orbwalker.CanAttack() && dist < 550) break;
                var prediction = R.GetPrediction(enemy, true);
                if ((int)prediction.Hitchance >= (int)HitChance.Medium)
                {
                    R.Cast(prediction.UnitPosition);
                }
            }
            if (GetRStacks() >= MaxRStacksSlider.Value) return;
            if (IsWActive() || (Orbwalker.ActiveMode != OrbwalkingMode.Combo && !UseRHarass)) return;

            foreach (var enemy in base.ValidTargets.Where(h => h.Distance(myPos) < R.Range && h.IsValidTarget() && h.HealthPercent < 25))
            {
                var dist = enemy.Distance(ObjectManager.Player.ServerPosition);
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
            "SRU_Baron",
            "SRU_Blue",
            "Sru_Crab",
            "SRU_Dragon",
            "SRU_Gromp",
            "SRU_Krug",
            "SRU_Murkwolf",
            "SRU_Razorbeak",
            "SRU_Red",
        };

        /// <summary>
        /// Twisted Treeline Jungle "Big" Mobs
        /// </summary>
        private List<string> TTMobs = new List<string>
        {
            "TT_NWraith",
            "TT_NGolem",
            "TT_NWolf",
            "TT_Spiderboss"
        };
        #endregion ChampionLogic
    }
}
