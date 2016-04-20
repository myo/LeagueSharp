using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Challenger_Series.Utils;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Utils;
using SharpDX;
using Color = System.Drawing.Color;
using Challenger_Series.Utils;

namespace Challenger_Series.Plugins
{
    using LeagueSharp.SDK.Core.Wrappers.Damages;

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
            Obj_AI_Hero.OnDoCast += OnDoCast;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Events.OnGapCloser += EventsOnOnGapCloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
            Orbwalker.OnAction += OnAction;
        }

        private void OnAction(object sender, OrbwalkingActionArgs args)
        {
            if (args.Type == OrbwalkingType.BeforeAttack)
            {
                var possibleNearbyMeleeChampion =
                    ValidTargets.FirstOrDefault(
                        e => e.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 350);

                if (possibleNearbyMeleeChampion.IsValidTarget())
                {
                    if (E.IsReady() && UseEAntiMelee)
                    {
                        var pos = ObjectManager.Player.ServerPosition.Extend(possibleNearbyMeleeChampion.ServerPosition,
                            -Misc.GiveRandomInt(300, 475));
                        if (!IsDangerousPosition(pos))
                        {
                            E.Cast(pos);
                        }
                    }
                }
            }
            if (args.Type == OrbwalkingType.AfterAttack)
            {

                if (!HasPassive)
                {
                    var minion = args.Target as Obj_AI_Minion;
                    if (minion != null)
                    {
                        var tg = minion;
                        if (tg.CharData.BaseSkinName.Contains("SRU") && !tg.CharData.BaseSkinName.Contains("Mini"))
                        {
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
                            if (EJg && E.IsReady())
                            {

                                E.Cast(
                                    Deviation(ObjectManager.Player.Position.ToVector2(), tg.Position.ToVector2(),
                                        60).ToVector3());
                                return;
                            }
                        }
                    }
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
            if (E.IsReady() && UseEGapclose && args.IsDirectedToPlayer && args.Sender.Distance(ObjectManager.Player) < 800)
            {
                E.Cast(ObjectManager.Player.Position.Extend(args.Sender.Position, -Misc.GiveRandomInt(300, 600)));
            }
        }

        public override void OnDraw(EventArgs args)
        {
            #region Logic

            if (!HasPassive)

            {
                var target = TargetSelector.GetTarget(Q);
                
                if (target != null && Orbwalker.ActiveMode == OrbwalkingMode.Combo &&
                    target.Distance(ObjectManager.Player) < Q.Range)
                {
                    if (UseQCombo && Q.IsReady())
                    {
                        Q.Cast(target);
                        return;
                    }
                }
            var q2tg = TargetSelector.GetTarget(Q2.Range);
            if (q2tg != null && Q.IsReady() && q2tg.IsHPBarRendered)
            {
                if (q2tg.Distance(ObjectManager.Player) > 600)
                {
                    if (Orbwalker.ActiveMode != OrbwalkingMode.None && Orbwalker.ActiveMode != OrbwalkingMode.Combo)
                    {
                        var menuItem = QExtendedBlacklist["qexbl" + q2tg.CharData.BaseSkinName];
                        if (UseQExtended &&
                            ObjectManager.Player.ManaPercent > QExManaPercent && menuItem != null && !menuItem.GetValue<MenuBool>())
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
                else if (Q.IsReady() && UseQCombo)
                {
                    Q.Cast(q2tg);
                }
            }
            }

            #endregion

            if (QKS && Q.IsReady())
            {
                var targets = ValidTargets.Where(x => x.IsHPBarRendered && x.Health < Q.GetDamage(x) && x.IsValidTarget(Q.Range) && !x.IsZombie);
                var objAiHeroes = targets as IList<Obj_AI_Hero> ?? targets.ToList();
                if (targets != null && objAiHeroes.Any())
                {
                    foreach (var target in objAiHeroes)
                    {
                        if (target.Health < Q.GetDamage(target) &&
                            (!target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") &&
                             !target.HasBuff("JudicatorIntervention")))
                        {
                            Q.Cast(target);
                            return;
                        }
                    }
                }
            }
            if (R.IsReady() && ForceR)
            {
                var target = TargetSelector.GetTarget(900);
                if (target != null && target.IsHPBarRendered && target.Health < R.GetDamage(target)*0.8 &&
                    target.Distance(ObjectManager.Player) > 300)
                {
                    var pred = R.GetPrediction(target);
                    if (!pred.CollisionObjects.Any() && pred.Hitchance >= HitChance.High)
                    {
                        R.Cast(pred.UnitPosition);
                    }
                }
            }
            var tg = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, DamageType.Physical);
            if (tg != null && HasPassive)
            {
                if (UsePassiveOnEnemy && tg.IsValidTarget())
                {
                    Orbwalker.ForceTarget = tg;
                    return;
                }
            }
            Orbwalker.ForceTarget = null;
        }

        private void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name == "LucianPassiveShot" || args.SData.Name.Contains("LucianBasicAttack"))
            {
                if (!HasPassive)
                {
                    var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                    if (target != null && Orbwalker.ActiveMode == OrbwalkingMode.Combo &&
                        target.Distance(ObjectManager.Player) < ObjectManager.Player.AttackRange && target.IsHPBarRendered)
                    {
                        if (E.IsReady())
                        {
                            switch (UseEMode.SelectedValue)
                            {
                                case "Side":
                                    E.Cast(
                                        Deviation(ObjectManager.Player.Position.ToVector2(), target.Position.ToVector2(),
                                            65).ToVector3());
                                    break;
                                case "Cursor":
                                {
                                    if (!IsDangerousPosition(Game.CursorPos))
                                    E.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos,
                                        Misc.GiveRandomInt(50, 100)));
                                    break;
                                }
                                case "Enemy":
                                    E.Cast(ObjectManager.Player.Position.Extend(target.Position,
                                        Misc.GiveRandomInt(50, 100)));
                                    break;
                            }
                        }
                        if (UseQCombo && Q.IsReady())
                        {
                            Q.Cast(target);
                            return;
                        }
                        if (UseWCombo && W.IsReady())
                        {
                            var pred = W.GetPrediction(target);
                            if (target.Health < ObjectManager.Player.GetAutoAttackDamage(target) * 2)
                            {
                                W.Cast(pred.UnitPosition);
                                return;
                            }
                            if (pred.Hitchance >= HitChance.High)
                            {
                                W.Cast(pred.UnitPosition);
                                return;
                            }
                        }
                    }
                    if (args.Target != null && args.Target is Obj_AI_Minion)
                    {
                        var tg = args.Target as Obj_AI_Minion;
                        if (tg.IsHPBarRendered && tg.CharData.BaseSkinName.Contains("SRU") && !tg.CharData.BaseSkinName.Contains("Mini"))
                        {
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
                            if (EJg && E.IsReady())
                            {

                                E.Cast(
                                    Deviation(ObjectManager.Player.Position.ToVector2(), tg.Position.ToVector2(),
                                        60).ToVector3());
                                return;
                            }
                        }
                    }
                }
            }
        }

        private Menu ComboMenu;
        private MenuBool UseQCombo;
        private MenuBool UseWCombo;
        private MenuList<string> UseEMode;
        private MenuBool UseEGapclose;
        private MenuBool UseEAntiMelee;
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
            UseEMode =
                ComboMenu.Add(new MenuList<string>("Lucianecombo", "E Mode", new[] {"Side", "Cursor", "Enemy", "Never"}));
            UseEAntiMelee = ComboMenu.Add(new MenuBool("Lucianecockblocker", "Use E to get away from melees", true));
            UseEGapclose = ComboMenu.Add(new MenuBool("Lucianegoham", "Use E to go HAM", false));
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
