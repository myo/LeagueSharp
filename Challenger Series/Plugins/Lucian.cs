using System;
using System.Collections.Generic;
using System.Linq;
using Challenger_Series.Utils;
using LeagueSharp;
using LeagueSharp.SDK;
using SharpDX;
using Color = System.Drawing.Color;
using Challenger_Series.Utils;
using System.Windows.Forms;
using LeagueSharp.Data.Enumerations;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using Menu = LeagueSharp.SDK.UI.Menu;

namespace Challenger_Series.Plugins
{

    using Menu = LeagueSharp.SDK.UI.Menu;

    public class Lucian : CSPlugin
    {
        public Lucian()
        {
            Q = new Spell(SpellSlot.Q, 675);
            Q2 = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W, 1200f);
            E = new Spell(SpellSlot.E, 475f);
            R = new Spell(SpellSlot.R, 1400);

            Q.SetTargetted(0.25f, 1400f);
            Q2.SetSkillshot(0.5f, 50, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.30f, 70f, 1600f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.2f, 110f, 2500, true, SkillshotType.SkillshotLine);
            InitMenu();
            DelayedOnUpdate += OnUpdate;
            Events.OnGapCloser += EventsOnOnGapCloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
            Orbwalker.OnAction += OnAction;
            Spellbook.OnCastSpell += OnCastSpell;
        }

        private bool pressedR = false;

        private int ECastTime = 0;

        bool QLogic(AttackableUnit target)
        {
            var hero = (Obj_AI_Hero)target;
            if (hero != null && Orbwalker.ActiveMode == OrbwalkingMode.Combo && UseQCombo)
            {
                Q.Cast(hero);
                return true;
            }
            return false;
        }

        bool ELogic(Obj_AI_Base target)
        {
            switch (UseEMode.SelectedValue)
            {
                case "Side":
                    {
                        E.Cast(
                            Deviation(ObjectManager.Player.Position.ToVector2(), target.Position.ToVector2(), 65)
                                .ToVector3());
                        return true;
                    }
                case "Cursor":
                    {
                        if (!IsDangerousPosition(Game.CursorPos))
                        {
                            E.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, Misc.GiveRandomInt(50, 100)));
                        }
                        return true;
                    }
                case "Enemy":
                    {
                        E.Cast(ObjectManager.Player.Position.Extend(target.Position, Misc.GiveRandomInt(50, 100)));
                        return true;
                    }
            }
            return false;
        }

        bool WLogic(Obj_AI_Base target)
        {
            if (this.UseWCombo)
            {
                if (IgnoreWCollision && target.Distance(ObjectManager.Player) < 600)
                {
                    W.Cast(target.ServerPosition);
                    return true;
                }
                var pred = W.GetPrediction(target);
                if (target.Health < ObjectManager.Player.GetAutoAttackDamage(target) * 3)
                {
                    W.Cast(pred.UnitPosition);
                    return true;
                }
                if (pred.Hitchance >= HitChance.High)
                {
                    W.Cast(pred.UnitPosition);
                    return true;
                }
            }
            return false;
        }

        void JungleClear(AttackableUnit target)
        {
            var tg = target as Obj_AI_Minion;
            if (tg != null && !HasPassive && Orbwalker.CanMove())
            {
                if (tg.IsHPBarRendered && tg.CharData.BaseSkinName.Contains("SRU")
                    && !tg.CharData.BaseSkinName.Contains("Mini"))
                {
                    if (EJg && E.IsReady())
                    {

                        E.Cast(
                            Deviation(ObjectManager.Player.Position.ToVector2(), tg.Position.ToVector2(), 60)
                                .ToVector3());
                        return;
                    }
                    if (QJg && Q.IsReady())
                    {
                        Q.Cast(tg);
                        return;
                    }
                    if (WJg && W.IsReady())
                    {
                        var pred = W.GetPrediction(tg);
                        W.Cast(pred.UnitPosition);
                        return;
                    }
                }
            }
        }

        void QExHarass()
        {
            var q2tg = TargetSelector.GetTarget(Q2.Range);
            if (q2tg != null && q2tg.IsHPBarRendered)
            {
                if (q2tg.Distance(ObjectManager.Player) > 600)
                {
                    if (Orbwalker.ActiveMode != OrbwalkingMode.None && Orbwalker.ActiveMode != OrbwalkingMode.Combo)
                    {
                        var menuItem = QExtendedBlacklist["qexbl" + q2tg.CharData.BaseSkinName];
                        if (UseQExtended && ObjectManager.Player.ManaPercent > QExManaPercent && menuItem != null
                            && !menuItem.GetValue<MenuBool>())
                        {
                            var QPred = Q2.GetPrediction(q2tg);
                            if (QPred.Hitchance >= HitChance.Medium)
                            {
                                var minions =
                                    GameObjects.EnemyMinions.Where(
                                        m => m.IsHPBarRendered && m.Distance(ObjectManager.Player) < Q.Range);
                                var objAiMinions = minions as IList<Obj_AI_Minion> ?? minions.ToList();
                                if (objAiMinions.Any())
                                {
                                    foreach (var minion in objAiMinions)
                                    {
                                        var QHit = new Utils.Geometry.Rectangle(
                                            ObjectManager.Player.Position,
                                            ObjectManager.Player.Position.Extend(minion.Position, Q2.Range),
                                            Q2.Width);
                                        if (!QPred.UnitPosition.IsOutside(QHit))
                                        {
                                            Q.Cast(minion);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (!HasPassive)
                {
                    Q.Cast(q2tg);
                }
            }
        }

        private void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if (args.Slot == SpellSlot.R && this.BlockManualR)
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
                if (args.Slot == SpellSlot.E)
                {
                    this.ECastTime = Variables.TickCount;
                }
            }
        }

        private void OnAction(object sender, OrbwalkingActionArgs args)
        {
            if (args.Type == OrbwalkingType.BeforeAttack)
            {
                //Anti Melee
                var possibleNearbyMeleeChampion =
                    ValidTargets.FirstOrDefault(
                        e => e.IsMelee && e.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 350);

                if (possibleNearbyMeleeChampion.IsValidTarget())
                {
                    if (E.IsReady() && UseEAntiMelee)
                    {
                        var pos = ObjectManager.Player.ServerPosition.Extend(possibleNearbyMeleeChampion.ServerPosition,
                            -Misc.GiveRandomInt(250, 475));
                        if (!IsDangerousPosition(pos))
                        {
                            E.Cast(pos);
                        }
                    }
                }
            }
            if (args.Type == OrbwalkingType.AfterAttack)
            {
                //JungleClear
                if (args.Target is Obj_AI_Minion)
                {
                    JungleClear(args.Target);
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
            if (E.IsReady() && UseEGapclose && args.Sender.IsMelee && args.IsDirectedToPlayer && args.Sender.Distance(ObjectManager.Player) < 800)
            {
                E.Cast(ObjectManager.Player.Position.Extend(args.Sender.Position, -Misc.GiveRandomInt(250, 600)));
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            var ultTarget = TargetSelector.GetTarget(R);
            if (this.SemiAutoRKey.Active && ultTarget != null && ultTarget.IsHPBarRendered)
            {
                this.pressedR = true;
                R.Cast(R.GetPrediction(ultTarget).UnitPosition);
                return;
            }
            if (Variables.TickCount - this.ECastTime > 250)
            {
                if (!HasPassive && Orbwalker.CanMove() && !ObjectManager.Player.IsCastingInterruptableSpell())
                {
                    if (Orbwalker.ActiveMode == OrbwalkingMode.Combo)
                    {
                        if (E.IsReady())
                        {
                            var target = TargetSelector.GetTarget(875, DamageType.Physical);
                            if (target != null && target.IsHPBarRendered)
                            {
                                var dist = target.Distance(ObjectManager.Player);
                                if (dist > 500)
                                {
                                    var pos = ObjectManager.Player.ServerPosition.Extend(
                                        target.ServerPosition,
                                        Math.Abs(dist - 500));
                                    if (!IsDangerousPosition(pos))
                                    {
                                        E.Cast(pos);
                                        return;
                                    }
                                }
                                else
                                {
                                    if (ELogic(target))
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                        var qtar = TargetSelector.GetTarget(Q);
                        if (qtar != null && qtar.IsHPBarRendered)
                        {
                            if (Q.IsReady())
                            {
                                if (QLogic(qtar)) return;
                            }
                            if (W.IsReady())
                            {
                                if (WLogic(qtar)) return;
                            }
                        }
                        else
                        {
                            this.QExHarass();
                        }
                    }
                    if (Orbwalker.ActiveMode != OrbwalkingMode.None && Orbwalker.ActiveMode != OrbwalkingMode.Combo)
                    {
                        this.QExHarass();
                    }
                }
            }
            if (UsePassiveOnEnemy && HasPassive)
            {
                var tg = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, DamageType.Physical);
                if (tg != null && tg.IsHPBarRendered)
                {
                    Orbwalker.ForceTarget = tg;
                    return;
                }
            }
            else
            {
                Orbwalker.ForceTarget = null;
            }
        }

        private Menu ComboMenu;
        private MenuBool UseQCombo;
        private MenuBool UseWCombo;

        private MenuBool IgnoreWCollision;
        private MenuList<string> UseEMode;
        private MenuBool UseEGapclose;
        private MenuBool UseEAntiMelee;
        private MenuKeyBind SemiAutoRKey;
        private MenuBool BlockManualR;
        private MenuBool ForceR;
        private Menu HarassMenu;
        private MenuBool UseQExtended;
        private MenuSlider QExManaPercent;
        private Menu QExtendedBlacklist;
        private MenuBool UseQHarass;
        private MenuBool UsePassiveOnEnemy;
        private Menu JungleMenu;
        private MenuBool QJg;
        private MenuBool WJg;
        private MenuBool EJg;
        private MenuBool QKS;

        public void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("Luciancombomenu", "Combo Settings: "));
            UseQCombo = ComboMenu.Add(new MenuBool("Lucianqcombo", "Use Q", true));
            UseWCombo = ComboMenu.Add(new MenuBool("Lucianwcombo", "Use W", true));
            IgnoreWCollision = ComboMenu.Add(new MenuBool("Lucianignorewcollision", "Ignore W collision (for passive)", false));
            UseEMode =
                ComboMenu.Add(new MenuList<string>("Lucianecombo", "E Mode", new[] {"Side", "Cursor", "Enemy", "Never"}));
            UseEAntiMelee = ComboMenu.Add(new MenuBool("Lucianecockblocker", "Use E to get away from melees", true));
            UseEGapclose = ComboMenu.Add(new MenuBool("Lucianegoham", "Use E to go HAM", false));
            SemiAutoRKey = ComboMenu.Add(
                new MenuKeyBind("Luciansemiauto", "Semi-Auto R Key", Keys.R, KeyBindType.Press));
            BlockManualR = this.ComboMenu.Add(new MenuBool("Lucianblockmanualr", "Block manual R", true));
            ForceR = ComboMenu.Add(new MenuBool("Lucianrcombo", "Auto R", true));
            HarassMenu = MainMenu.Add(new Menu("Lucianharassmenu", "Harass Settings: "));
            UseQExtended = HarassMenu.Add(new MenuBool("Lucianqextended", "Use Extended Q", true));
            QExManaPercent =
                HarassMenu.Add(new MenuSlider("Lucianqexmanapercent", "Only use extended Q if mana > %", 75, 0, 100));
            QExtendedBlacklist = HarassMenu.Add(new Menu("Lucianqexblacklist", "Extended Q Blacklist: "));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
            {
                var championName = ally.CharData.BaseSkinName;
                QExtendedBlacklist.Add(new MenuBool("qexbl" + championName, championName, false));
            }
            UseQHarass = HarassMenu.Add(new MenuBool("Lucianqharass", "Use Q Harass", true));
            UsePassiveOnEnemy = HarassMenu.Add(new MenuBool("Lucianpassivefocus", "Use Passive On Champions", true));
            JungleMenu = MainMenu.Add(new Menu("Lucianjunglemenu", "Jungle Settings: "));
            QJg = JungleMenu.Add(new MenuBool("Lucianqjungle", "Use Q", true));
            WJg = JungleMenu.Add(new MenuBool("Lucianwjungle", "Use W", true));
            EJg = JungleMenu.Add(new MenuBool("Lucianejungle", "Use E", true));
            QKS = new MenuBool("Lucianqks", "Use Q for KS", true);
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

        private bool IsDangerousPosition(Vector3 pos)
        {
            return GameObjects.EnemyHeroes.Any(
                e => e.IsValidTarget() &&
                     (e.Distance(pos) < 375) && (E.GetPrediction(e).UnitPosition.Distance(pos) > 500)) ||
                   (pos.UnderTurret(true) && !ObjectManager.Player.UnderTurret(true));
        }

        public bool HasPassive => ObjectManager.Player.HasBuff("LucianPassiveBuff");
    }
}
