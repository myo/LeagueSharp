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
using System.Linq;
using Challenger_Series.Utils;
using LeagueSharp;
using LeagueSharp.SDK;
using SharpDX;
using Color = System.Drawing.Color;
using System.Windows.Forms;
using LeagueSharp.Data.Enumerations;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using Menu = LeagueSharp.SDK.UI.Menu;

namespace Challenger_Series.Plugins
{
    public class Humanizer
    {
        public Humanizer(int lifespan)
        {
            ExpireTime = Variables.TickCount + lifespan;
        }
        private int ExpireTime;

        public bool ShouldDestroy
        {
            get
            {
                return !ObjectManager.Player.HasBuff("KogMawBioArcaneBarrage") || Variables.TickCount > ExpireTime;
            }
        }
    }

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
            DelayedOnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnAction += OnAction;
            Obj_AI_Hero.OnDoCast += OnDoCast;
            _rand = new Random();
        }

        private void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name.Contains("BarrageAttack"))
            {
                _attacksSoFar++;
            }
        }

        private Humanizer _humanizer;
        private int _attacksSoFar;
        private Random _rand;

        private void OnAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
            if (orbwalkingActionArgs.Type == OrbwalkingType.AfterAttack)
            {
                if (orbwalkingActionArgs.Target is Obj_AI_Hero)
                {
                    var target = orbwalkingActionArgs.Target as Obj_AI_Hero;
                    var distFromTargetToMe = target.Distance(ObjectManager.Player.ServerPosition);
                    if (Q.IsReady())
                    {
                        QLogic(target);
                    }
                    if (distFromTargetToMe < 350 && target.IsMelee)
                    {
                        ELogic(target);
                    }
                }
                if (orbwalkingActionArgs.Target is Obj_AI_Minion)
                {
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
            if (Q.IsReady() && UseQBool && Orbwalker.ActiveMode == OrbwalkingMode.Combo && ObjectManager.Player.Mana > GetQMana() + GetWMana())
            {
                foreach (
                    var enemy in
                        ValidTargets.Where(t => t.Distance(ObjectManager.Player) < 800)
                            .OrderBy(e => e.Distance(ObjectManager.Player)))
                {
                    var prediction = Q.GetPrediction(enemy);
                    if ((int)prediction.Hitchance >= (int)HitChance.VeryHigh)
                    {
                        Q.Cast(prediction.UnitPosition);
                    }
                }
            }
            var attackrange = GetAttackRangeAfterWIsApplied();
            var target = TargetSelector.GetTarget(attackrange, DamageType.Physical);
            if (IsWActive() && target != null && target.Distance(ObjectManager.Player) > attackrange - 150)
            {
                E.CastIfHitchanceMinimum(target, HitChance.Medium);
            }

            #region Humanizer
            if (HumanizerEnabled)
            {
                if (_humanizer != null)
                {
                    _attacksSoFar = 0;
                }
                else if (_attacksSoFar >= HumanizerMinAttacks.Value)
                {
                    _humanizer = new Humanizer(HumanizerMovementTime.Value);
                }
                if (!IsWActive())
                {
                    _humanizer = null;
                    _attacksSoFar = 0;
                }
                if (_humanizer != null && _humanizer.ShouldDestroy)
                {
                    _humanizer = null;
                }
                Orbwalker.MovementState = CanMove();
                Orbwalker.AttackState = CanAttack();
            }
            else
            {
                _humanizer = null;
                Orbwalker.MovementState = true;
                Orbwalker.AttackState = true;
            }
            #endregion Humanizer
        }

        public override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            base.W.Range = GetAttackRangeAfterWIsApplied();
            base.R.Range = GetRRange();
            if (DrawWRangeBool)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, GetAttackRangeAfterWIsApplied(), W.IsReady() || IsWActive() ? Color.LimeGreen : Color.Red);
            }
            if (DrawRRangeBool)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, GetRRange() + 25, R.IsReady() ? Color.LimeGreen : Color.Red);
            }
        }

        private bool CanAttack()
        {
            if (!HumanizerEnabled) return true;
            if (IsWActive())
            {
                return _humanizer == null;
            }
            return true;
        }
        private bool CanMove()
        {
            if (!HumanizerEnabled) return true;
            if (IsWActive() && ObjectManager.Player.AttackSpeedMod / 2 > _rand.Next(167, 230)/100)
            {
                if ((Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo &&
                        ObjectManager.Player.CountEnemyHeroesInRange(GetAttackRangeAfterWIsApplied() - 25) < 1)
                       ||
                       (Variables.Orbwalker.ActiveMode != OrbwalkingMode.None &&
                        Variables.Orbwalker.ActiveMode != OrbwalkingMode.Combo &&
                        (!GameObjects.EnemyMinions.Any(
                            m => m.IsHPBarRendered && m.Distance(ObjectManager.Player) < GetAttackRangeAfterWIsApplied() - 25) && !GameObjects.Jungle.Any(m => m.IsHPBarRendered && m.Distance(ObjectManager.Player) < GetAttackRangeAfterWIsApplied() - 25))))
                {
                    return true;
                }
                return _humanizer != null;
            }
            return true;
        }

        #endregion Events

        private Menu ComboMenu;
        private Menu HarassMenu;
        private Menu JungleclearMenu;
        private Menu UseWJungleClearMenu;
        private Menu DrawMenu;
        private Menu HumanizerMenu;
        private MenuSlider HumanizerMinAttacks;
        private MenuSlider HumanizerMovementTime;
        private MenuBool HumanizerEnabled;
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
            HumanizerMenu = MainMenu.Add(new Menu("koggiehumanizermenu", "Humanizer Settings: "));
            HumanizerMinAttacks = HumanizerMenu.Add(new MenuSlider("koggieminattacks", "Min attacks before moving", 2, 1, 10));
            HumanizerMovementTime =
                HumanizerMenu.Add(new MenuSlider("koggiehumanizermovetime", "Time for moving (milliseconds)", 200, 0,
                    1000));
            HumanizerEnabled = HumanizerMenu.Add(new MenuBool("koggiehumanizerenabled", "Enable Humanizer? ", true));
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
                ValidTargets.Any(h => h.Health > 1 && h.Distance(ObjectManager.Player.ServerPosition) < GetAttackRangeAfterWIsApplied() && h.IsValidTarget()) && Orbwalker.ActiveMode == OrbwalkingMode.Combo)
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
                    ValidTargets.Where(h => h.Distance(myPos) < R.Range && h.HealthPercent < 25 && h.IsValidTarget()))
            {
                var prediction = R.GetPrediction(enemy, true);
                if ((int)prediction.Hitchance > (int)HitChance.Medium)
                {
                    R.Cast(prediction.UnitPosition);
                }
            }
            if (GetRStacks() >= MaxRStacksSlider.Value) return;
            if ((Orbwalker.ActiveMode != OrbwalkingMode.Combo && !UseRHarass)) return;

            foreach (var enemy in ValidTargets.Where(h => h.Distance(myPos) < R.Range && h.IsValidTarget() && h.HealthPercent < 35))
            {
                var dist = enemy.Distance(ObjectManager.Player.ServerPosition);
                if (Orbwalker.CanAttack && dist < 550) continue;
                var prediction = R.GetPrediction(enemy, true);
                if ((int) prediction.Hitchance > (int) HitChance.Medium)
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
