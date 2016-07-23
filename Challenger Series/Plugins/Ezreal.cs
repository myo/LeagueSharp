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
    public class Ezreal : CSPlugin
    {
        public Ezreal()
        {
            Q = new Spell(SpellSlot.Q, 1180);
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 950);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);

            E = new Spell(SpellSlot.E, 475);

            R = new Spell(SpellSlot.R, 2500);
            R.SetSkillshot(1.00f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            InitMenu();
            DelayedOnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Events.OnGapCloser += EventsOnOnGapCloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
            Spellbook.OnCastSpell += OnCastSpell;
            Orbwalker.OnAction += OnAction;
            Obj_AI_Base.OnTarget += ObjAiBaseOnOnTarget;
        }

        private Obj_AI_Minion _lastTurretTarget;
        private bool pressedR = false;

        void QLogic()
        {
            var targets = ValidTargets.Where(x => x.IsHPBarRendered && x.Health < Q.GetDamage(x) && x.IsValidTarget(Q.Range) && !x.IsZombie);
                if (targets != null && targets.Any())
                {
                    foreach (var target in targets)
                    {
                        if (target == null) continue;
                        if (target.Health < Q.GetDamage(target) &&
                            (!target.HasBuff("Undying Rage") &&
                             !target.HasBuff("JudicatorIntervention")))
                        {
                            var pred = Q.GetPrediction(target);
                            if (pred.Hitchance >= HitChance.High && !pred.CollisionObjects.Any())
                            {
                                Q.Cast(pred.UnitPosition);
                            }
                        }
                    }
                }
            if (Orbwalker.ActiveMode != OrbwalkingMode.None)
            {
                if (Q.IsReady())
                {
                    var qtarget = TargetSelector.GetTarget(Q);
                    if (qtarget != null && qtarget.IsHPBarRendered)
                    {
                        var pred = Q.GetPrediction(qtarget);
                        if (Q.IsReady() && UseQ && pred.Hitchance >= HitChance.High && !pred.CollisionObjects.Any())
                        {
                            Q.Cast(pred.UnitPosition);
                            return;
                        }
                    }
                }
            }
            
            if (Orbwalker.CanMove && QFarm && ObjectManager.Player.ManaPercent > QMana &&
                (Orbwalker.ActiveMode == OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == OrbwalkingMode.LastHit))
            {
                var minion =
                    GameObjects.EnemyMinions.FirstOrDefault(
                        m =>
                        m.Position.Distance(ObjectManager.Player.Position) < 550
                        && m.Health < ObjectManager.Player.GetAutoAttackDamage(m)
                        && Health.GetPrediction(m, (int)((Game.Ping / 2) + ObjectManager.Player.AttackCastDelay * 1000))
                        < 1 && Health.GetPrediction(m, (int)((Game.Ping / 2) + 250)) > 1);
                if (minion != null)
                {
                    var pred = Q.GetPrediction(minion);
                    if (!pred.CollisionObjects.Any(o => o is Obj_AI_Minion))
                    {
                        Q.Cast(pred.UnitPosition);
                    }
                }
            }
        }

        void WLogic()
        {
            var wMode = UseWMode.SelectedValue;
            if (wMode == "ALWAYS" || (wMode == "COMBO" && Orbwalker.ActiveMode == OrbwalkingMode.Combo))
            {
                var wtarget = TargetSelector.GetTarget(W);
                if (wtarget != null)
                {
                    if (wMode == "COMBO" && wtarget.Distance(ObjectManager.Player) < 500)
                    {
                        if (wtarget.IsHPBarRendered)
                        {
                            var pred = W.GetPrediction(wtarget);
                            if (pred.Hitchance >= HitChance.High)
                            {
                                W.Cast(pred.UnitPosition);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (wtarget.IsHPBarRendered)
                        {
                            var pred = W.GetPrediction(wtarget);
                            if (pred.Hitchance >= HitChance.High)
                            {
                                W.Cast(pred.UnitPosition);
                                return;
                            }
                        }
                    }
                }
            }
        }

        void RLogic()
        {
            var rtarget = TargetSelector.GetTarget(R);
            if (rtarget == null)
            {
                return;
            }
            if (ObjectManager.Player.CountEnemyHeroesInRange(800) < 1)
            {
                R.CastIfWillHit(rtarget, 3);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            base.OnUpdate(args);
                        if (Orbwalker.ActiveMode == OrbwalkingMode.Combo && UseSheenCombo && HasSheenBuff && ObjectManager.Player.CountEnemyHeroesInRange(550) > 0)
            {
                return;
            }
            if (this.SemiAutoRKey.Active)
            {
                if (ObjectManager.Player.CountEnemyHeroesInRange(1300) > 0)
                {
                    var ultTarget = TargetSelector.GetTarget(R);
                    if (ultTarget != null && ultTarget.IsHPBarRendered)
                    {
                        this.pressedR = true;
                        var rPred = R.GetPrediction(ultTarget);
                        if (rPred.Hitchance >= HitChance.High)
                        {
                            R.Cast(rPred.UnitPosition);
                        }
                        return;
                    }
                }
                else
                {
                    R.Cast(Game.CursorPos);
                }
            }
            if (_lastTurretTarget == null || !_lastTurretTarget.IsHPBarRendered)
            {
                _lastTurretTarget = null;
            }
            if (Q.IsReady()) this.QLogic();
            if (W.IsReady()) this.WLogic();
            if (R.IsReady()) this.RLogic();
        }

        private void ObjAiBaseOnOnTarget(Obj_AI_Base sender, Obj_AI_BaseTargetEventArgs args)
        {
            if (sender is Obj_AI_Turret && sender.Distance(ObjectManager.Player) < 850 && args.Target is Obj_AI_Minion && args.Target.IsEnemy)
            {
                _lastTurretTarget = args.Target as Obj_AI_Minion;
            }
        }

        private void OnAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
            if (orbwalkingActionArgs.Type == OrbwalkingType.AfterAttack)
            {
                if (QFarm && Orbwalker.ActiveMode != OrbwalkingMode.Combo && Orbwalker.ActiveMode != OrbwalkingMode.None)
                {
                    if (_lastTurretTarget != null && _lastTurretTarget.IsHPBarRendered &&
                        Q.GetDamage(_lastTurretTarget) > _lastTurretTarget.Health &&
                        _lastTurretTarget.Health > ObjectManager.Player.GetAutoAttackDamage(_lastTurretTarget))
                    {
                        var pred = Q.GetPrediction(_lastTurretTarget);
                        if (!pred.CollisionObjects.Any())
                        {
                            Q.Cast(pred.UnitPosition);
                        }
                    }
                }
            }
        }

        private void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && this.BlockManualR && ObjectManager.Player.CountEnemyHeroesInRange(1450) > 0)
            {
                if (!pressedR && !ObjectManager.Player.IsCastingInterruptableSpell())
                {
                    args.Process = false;
                }
                else
                {
                    args.Process = true;
                    this.pressedR = false;
                }
            }
        }

        private void OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && args.DangerLevel == DangerLevel.High && args.Sender.Distance(ObjectManager.Player) < 400)
            {
                E.Cast(ObjectManager.Player.Position.Extend(args.Sender.Position, -Misc.GiveRandomInt(300, 600)));
            }
        }

        private void EventsOnOnGapCloser(object sender, Events.GapCloserEventArgs args)
        {
            if (E.IsReady() && args.IsDirectedToPlayer && args.Sender.Distance(ObjectManager.Player) < 800)
            {
                E.Cast(ObjectManager.Player.Position.Extend(args.Sender.Position, -Misc.GiveRandomInt(300, 600)));
            }
        }

        public override void OnDraw(EventArgs args)
        {

        }

        private MenuBool UseQ;
        private MenuList<string> UseWMode;
        private MenuBool QFarm;
        private MenuSlider QMana;
        private MenuBool UseSheenCombo;
        private MenuKeyBind SemiAutoRKey;
        private MenuBool BlockManualR;
        private MenuBool ForceR;

        public void InitMenu()
        {
            UseQ = MainMenu.Add(new MenuBool("Ezrealq", "Use Q", true));
            QFarm = MainMenu.Add(new MenuBool("Ezrealqfarm", "Use Q Farm", true));
            QMana = MainMenu.Add(new MenuSlider("Ezrealqfarmmana", "Q Farm Mana", 80, 0, 100));
            UseWMode = MainMenu.Add(new MenuList<string>("Ezrealw", "Use W", new [] {"COMBO", "ALWAYS", "NEVER"}));
            SemiAutoRKey = MainMenu.Add(new MenuKeyBind("Ezrealr", "Use R Key: ", Keys.R, KeyBindType.Press));
            BlockManualR = MainMenu.Add(new MenuBool("Ezrealblockmanualr", "Block manual R", true));
            UseSheenCombo = MainMenu.Add(new MenuBool("Ezrealsheencombo", "Use SHEEN Combo", true));
            MainMenu.Attach();
        }

        public static Vector2 Deviation(Vector2 point1, Vector2 point2, double angle)
        {
            angle *= Math.PI/180.0;
            Vector2 temp = Vector2.Subtract(point2, point1);
            Vector2 result = new Vector2(0);
            result.X = (float) (temp.X*Math.Cos(angle) - temp.Y*Math.Sin(angle))/4;
            result.Y = (float) (temp.X*Math.Sin(angle) + temp.Y*Math.Cos(angle))/4;
            result = Vector2.Add(result, point1);
            return result;
        }

        private bool HasSheenBuff
            => ObjectManager.Player.HasBuff("sheen") || ObjectManager.Player.HasBuff("itemfrozenfist");
    }
}