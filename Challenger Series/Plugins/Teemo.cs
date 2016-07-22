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
    public class Teemo : CSPlugin
    {
        public Teemo()
        {
            Q = new Spell(SpellSlot.Q, 680);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 300);

            Q.SetTargetted(0.5f, 1500f);
            R.SetSkillshot(0.5f, 120f, 1000f, false, SkillshotType.SkillshotCircle);
            InitMenu();
            Orbwalker.OnAction += OnAction;
            DelayedOnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Events.OnGapCloser += OnGapCloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
        }

        public override void OnUpdate(EventArgs args)
        {
            if (Q.IsReady()) this.QLogic();
            if (W.IsReady()) this.WLogic();
            if (R.IsReady()) this.RLogic();
        }

        private void OnGapCloser(object oSender, Events.GapCloserEventArgs args)
        {
            /*var sender = args.Sender;
            if (UseEAntiGapclose)
            {
                if (args.IsDirectedToPlayer && args.Sender.Distance(ObjectManager.Player) < 750)
                {
                    if (E.IsReady())
                    {
                        E.Cast(sender.ServerPosition);
                    }
                }
            }*/
        }

        private void OnInterruptableTarget(object oSender, Events.InterruptableTargetEventArgs args)
        {
            /*var sender = args.Sender;
            if (!GameObjects.AllyMinions.Any(m => !m.IsDead && m.CharData.BaseSkinName.Contains("trap") && m.Distance(sender.ServerPosition) < 100) && ObjectManager.Player.Distance(sender) < 550)
            {
                W.Cast(sender.ServerPosition);
            }*/
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            base.OnProcessSpellCast(sender, args);
            /*if (sender is Obj_AI_Hero && sender.IsEnemy)
            {
                if (args.SData.Name == "summonerflash" && args.End.Distance(ObjectManager.Player.ServerPosition) < 650)
                {
                    var pred = Prediction.GetPrediction((Obj_AI_Hero)args.Target, E);
                    if (!pred.Item3.Any(o => o.IsMinion && !o.IsDead && !o.IsAlly))
                    {
                        E.Cast(args.End);
                    }
                }
            }*/
        }

        public override void OnDraw(EventArgs args)
        {
            var drawRange = DrawRange.Value;
            if (drawRange > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, drawRange, Color.Gold);
            }
        }

        private void OnAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
            if (orbwalkingActionArgs.Type == OrbwalkingType.BeforeAttack)
            {
                /*if (orbwalkingActionArgs.Target is Obj_AI_Minion && HasPassive && FocusOnHeadShotting &&
                    Orbwalker.ActiveMode == OrbwalkingMode.LaneClear)
                {
                    var target = orbwalkingActionArgs.Target as Obj_AI_Minion;
                    if (target != null && !target.CharData.BaseSkinName.Contains("MinionSiege") && target.Health > 60)
                    {
                        var tg = (Obj_AI_Hero)TargetSelector.GetTarget(715, DamageType.Physical);
                        if (tg != null && tg.IsHPBarRendered)
                        {
                            Orbwalker.ForceTarget = tg;
                            orbwalkingActionArgs.Process = false;
                        }
                    }
                }*/
            }
            if (orbwalkingActionArgs.Type == OrbwalkingType.AfterAttack)
            {
                Orbwalker.ForceTarget = null;
                if (E.IsReady() && this.UseECombo)
                {
                    if (!OnlyUseEOnMelees)
                    {
                        var eTarget = TargetSelector.GetTarget(UseEOnEnemiesCloserThanSlider.Value, DamageType.Physical);
                        if (eTarget != null)
                        {
                            var pred = Prediction.GetPrediction(eTarget, E);
                            if (pred.Item3.Count == 0 && (int)pred.Item1 >= (int)HitChance.High)
                            {
                                E.Cast(pred.Item2);
                            }
                        }
                    }
                    else
                    {
                        var eTarget =
                            ValidTargets.FirstOrDefault(
                                e =>
                                e.IsMelee && e.Distance(ObjectManager.Player) < UseEOnEnemiesCloserThanSlider.Value
                                && !e.IsZombie);
                        var pred = Prediction.GetPrediction(eTarget, E);
                        if (pred.Item3.Count == 0 && (int)pred.Item1 > (int)HitChance.Medium)
                        {
                            E.Cast(pred.Item2);
                        }
                    }
                }
            }
        }

        private Menu ComboMenu;

        private MenuBool UseQCombo;

        private MenuBool UseWChase;

        private MenuBool UseECombo;


        private MenuKeyBind UseRCombo;

        private MenuBool AlwaysQAfterE;

        private MenuBool FocusOnHeadShotting;

        private MenuList<string> QHarassMode;

        private MenuBool UseWInterrupt;

        private Menu AutoRConfig;

        private MenuSlider UseEOnEnemiesCloserThanSlider;

        private MenuBool OnlyUseEOnMelees;

        private MenuBool UseEAntiGapclose;

        private MenuSlider DrawRange;

        public void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("teemocombomenu", "Combo Settings: "));
            UseQCombo = ComboMenu.Add(new MenuBool("teemoqcombo", "Use Q", true));
            UseWChase = ComboMenu.Add(new MenuBool("usewchase", "Use W when chasing"));
            UseECombo = ComboMenu.Add(new MenuBool("teemorcombo", "Use R", true));
            AutoRConfig = MainMenu.Add(new Menu("teemoautor", "R Settings: "));
            new Utils.Logic.PositionSaver(AutoRConfig, R);
            MainMenu.Attach();
        }

        #region Logic

        void QLogic()
        {
            if (Orbwalker.ActiveMode == OrbwalkingMode.Combo)
            {
                if (UseQCombo && Q.IsReady() && ObjectManager.Player.CountEnemyHeroesInRange(800) == 0
                    && ObjectManager.Player.CountEnemyHeroesInRange(1100) > 0)
                {
                    Q.CastIfWillHit(TargetSelector.GetTarget(900, DamageType.Physical), 2);
                    var goodQTarget =
                        ValidTargets.FirstOrDefault(
                            t =>
                            t.Distance(ObjectManager.Player) < 950 && t.Health < Q.GetDamage(t)
                            || SquishyTargets.Contains(t.CharData.BaseSkinName));
                    if (goodQTarget != null)
                    {
                        var pred = Prediction.GetPrediction(goodQTarget, Q);
                        if ((int)pred.Item1 > (int)HitChance.Medium)
                        {
                            Q.Cast(pred.Item2);
                        }
                    }
                }
            }
            if (Orbwalker.ActiveMode != OrbwalkingMode.None && Orbwalker.ActiveMode != OrbwalkingMode.Combo
                && ObjectManager.Player.CountEnemyHeroesInRange(850) == 0)
            {
                var qHarassMode = QHarassMode.SelectedValue;
                if (qHarassMode != "DISABLED")
                {
                    var qTarget = TargetSelector.GetTarget(1100, DamageType.Physical);
                    if (qTarget != null)
                    {
                        var pred = Prediction.GetPrediction(qTarget, Q);
                        if ((int)pred.Item1 > (int)HitChance.Medium)
                        {
                            if (qHarassMode == "ALLOWMINIONS")
                            {
                                Q.Cast(pred.Item2);
                            }
                            else if (pred.Item3.Count == 0)
                            {
                                Q.Cast(pred.Item2);
                            }
                        }
                    }
                }
            }
        }

        void WLogic()
        {
            var goodTarget =
                ValidTargets.FirstOrDefault(
                    e =>
                    !e.IsDead && e.HasBuffOfType(BuffType.Knockup) || e.HasBuffOfType(BuffType.Snare)
                    || e.HasBuffOfType(BuffType.Stun) || e.HasBuffOfType(BuffType.Suppression) || e.IsCharmed
                    || e.IsCastingInterruptableSpell() || !e.CanMove);
            if (goodTarget != null)
            {
                var pos = goodTarget.ServerPosition;
                if (!GameObjects.AllyMinions.Any(m => !m.IsDead && m.CharData.BaseSkinName.Contains("trap") && m.Distance(goodTarget.ServerPosition) < 100) && pos.Distance(ObjectManager.Player.ServerPosition) < 820)
                {
                    W.Cast(goodTarget.ServerPosition);
                }
            }
            foreach (var enemyMinion in
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        m =>
                        m.IsEnemy && m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < W.Range
                        && m.HasBuff("teleport_target")))
            {

                W.Cast(enemyMinion.ServerPosition);
            }
            foreach (var hero in GameObjects.EnemyHeroes.Where(h => h.Distance(ObjectManager.Player) < W.Range))
            {
                var pred = Prediction.GetPrediction(hero, W);
                if (!GameObjects.AllyMinions.Any(m => !m.IsDead && m.CharData.BaseSkinName.Contains("trap") && m.Distance(pred.Item2) < 100) && (int)pred.Item1 > (int)HitChance.Medium)
                {
                    W.Cast(pred.Item2);
                }
            }
        }

        void RLogic()
        {
            if (UseRCombo.Active && R.IsReady() && ObjectManager.Player.CountEnemyHeroesInRange(900) == 0)
            {
                foreach (var rTarget in
                    ValidTargets.Where(
                        e =>
                        SquishyTargets.Contains(e.CharData.BaseSkinName) && R.GetDamage(e) > 0.1 * e.MaxHealth
                        || R.GetDamage(e) > e.Health))
                {
                    if (rTarget.Distance(ObjectManager.Player) > 1400)
                    {
                        var pred = Prediction.GetPrediction(rTarget, R);
                        if (!pred.Item3.Any(obj => obj is Obj_AI_Hero))
                        {
                            R.CastOnUnit(rTarget);
                        }
                        break;
                    }
                    R.CastOnUnit(rTarget);
                }
            }
        }

        #endregion

        private bool HasPassive => ObjectManager.Player.HasBuff("caitlynheadshot");

        private string[] SquishyTargets =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia",
                "Corki", "Draven", "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus",
                "Katarina", "Kennen", "KogMaw", "Kindred", "Leblanc", "Lucian", "Lux",
                "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon", "Teemo",
                "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "Velkoz",
                "Viktor", "Xerath", "Zed", "Ziggs", "Jhin", "Soraka"
            };


    }
}