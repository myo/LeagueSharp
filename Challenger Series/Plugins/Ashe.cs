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
    public class Ashe : CSPlugin
    {
        public Ashe()
        {
            base.Q = new Spell(SpellSlot.Q);
            base.W = new Spell(SpellSlot.W, 1100);
            base.W.SetSkillshot(250f, 75f, 1500f, true, SkillshotType.SkillshotLine);
            base.E = new Spell(SpellSlot.E, 25000);
            base.R = new Spell(SpellSlot.R, 1400);
            R.SetSkillshot(0.25f, 100f, 1600f, false, SkillshotType.SkillshotLine);
            InitMenu();
            Obj_AI_Hero.OnDoCast += OnDoCast;
            Orbwalker.OnAction += OnAction;
            DelayedOnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Events.OnGapCloser += EventsOnOnGapCloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
        }

        private void OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && args.DangerLevel >= DangerLevel.Medium && args.Sender.Distance(ObjectManager.Player) < 1300)
            {
                R.Cast(args.Sender.ServerPosition);
            }
        }

        private void EventsOnOnGapCloser(object sender, Events.GapCloserEventArgs args)
        {
            if (R.IsReady() && args.IsDirectedToPlayer && args.Sender.Distance(ObjectManager.Player) < 1300)
            {
                R.Cast(args.Sender.ServerPosition);
            }
        }

        private List<Vector2> OrderScoutPositions = new List<Vector2> {new Vector2(7200, 2700), new Vector2(6900, 4700), new Vector2(3200, 6700), new Vector2(2700, 8300)};
        private List<Vector2> ChaosScoutPositions = new List<Vector2> {new Vector2(8200,10000), new Vector2(6800, 12000), new Vector2(11500, 8400), new Vector2(12000, 6700)};
        private Vector2 DragonScoutPosition = new Vector2(10300, 5000);
        private Vector2 BaronScoutPosition = new Vector2(4400, 9600);
        private Vector2 LastELocation = new Vector2(4400, 9600);

        public override void OnDraw(EventArgs args)
        {
            if (DrawWRange)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 1100, Color.Turquoise);
            }
        }

        void WLogic()
        {
            var wTarget = TargetSelector.GetTarget(1100);
            if (wTarget != null && Orbwalker.ActiveMode != OrbwalkingMode.None && UseWHarass && !ValidTargets.Any(e=>e.InAutoAttackRange()))
            {
                var pred = W.GetPrediction(wTarget);
                if (!pred.CollisionObjects.Any() &&
                    pred.UnitPosition.Distance(ObjectManager.Player.ServerPosition) < 1100)
                {
                    if (pred.UnitPosition.CountEnemyHeroesInRange(400) >= 1)
                    W.Cast(pred.UnitPosition);
                }
            }
        }

        void RLogic()
        {
            var rTarget = TargetSelector.GetTarget(1400, DamageType.Physical, false);
            if (rTarget != null &&  R.IsReady() && Orbwalker.ActiveMode == OrbwalkingMode.Combo && UseRCombo)
            {
                var pred = R.GetPrediction(rTarget);
                if (pred.Hitchance >= HitChance.High)
                R.Cast(pred.UnitPosition);
            }
        }

        void ELogic()
        {
            if (E.IsReady() && Orbwalker.ActiveMode != OrbwalkingMode.Combo && Orbwalker.ActiveMode != OrbwalkingMode.None && ValidTargets.Count(e=>e.InAutoAttackRange()) == 0)
            {
                switch (ScoutMode.SelectedValue)
                {
                    case "DragonBaron":
                    {
                        if (LastELocation == BaronScoutPosition)
                        {
                            LastELocation = DragonScoutPosition;
                            E.Cast(DragonScoutPosition.RandomizeToVector3(-150, 150));
                        }
                        else
                        {
                            LastELocation = BaronScoutPosition;
                            E.Cast(BaronScoutPosition.RandomizeToVector3(-150, 150));
                        }
                        break;
                    }
                    case "EnemyJungleClosest":
                    {
                        if (ObjectManager.Player.Team == GameObjectTeam.Order)
                        {
                            var pos =
                                ChaosScoutPositions.Where(v2 => v2.Distance(LastELocation) > 500)
                                    .OrderBy(v2 => v2.Distance(ObjectManager.Player.Position.ToVector2()))
                                    .FirstOrDefault();
                            LastELocation = pos;
                            E.Cast(pos.RandomizeToVector3(-150, 150));
                        }
                        else
                        {
                            var pos =
                                OrderScoutPositions.Where(v2 => v2.Distance(LastELocation) > 500)
                                    .OrderBy(v2 => v2.Distance(ObjectManager.Player.Position.ToVector2()))
                                    .FirstOrDefault();
                            LastELocation = pos;
                            E.Cast(pos.RandomizeToVector3(-150, 150));
                        }
                        break;
                    }
                    case "EnemyJungleFarthest":
                    {
                        if (ObjectManager.Player.Team == GameObjectTeam.Order)
                        {
                            var pos =
                                ChaosScoutPositions.Where(v2 => v2.Distance(LastELocation) > 500)
                                    .OrderByDescending(v2 => v2.Distance(ObjectManager.Player.Position.ToVector2()))
                                    .FirstOrDefault();
                            LastELocation = pos;
                            E.Cast(pos.RandomizeToVector3(-150, 150));
                        }
                        else
                        {
                            var pos =
                                OrderScoutPositions.Where(v2 => v2.Distance(LastELocation) > 500)
                                    .OrderByDescending(v2 => v2.Distance(ObjectManager.Player.Position.ToVector2()))
                                    .FirstOrDefault();
                            LastELocation = pos;
                            E.Cast(pos.RandomizeToVector3(-150,150));
                        }
                        break;
                    }
                    default:
                        break;
                }
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (W.IsReady()) this.WLogic();
            if (R.IsReady()) this.RLogic();
            if (E.IsReady()) this.ELogic();
        }

        private void OnAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
            if (Q.IsReady() && orbwalkingActionArgs.Type == OrbwalkingType.BeforeAttack && orbwalkingActionArgs.Target is Obj_AI_Hero && Orbwalker.ActiveMode == OrbwalkingMode.Combo && UseQCombo)
            {
                Q.Cast();
            }
        }

        private void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (W.IsReady() && sender.IsMe && args.Target is Obj_AI_Hero && !HasQEmpoweredAttack)
            {
                var name = args.SData.Name;
                var target = args.Target as Obj_AI_Hero;
                if (name.Contains("AsheBasicAttack") || name.Contains("AsheCritAttack"))
                {
                    if (Orbwalker.ActiveMode == OrbwalkingMode.Combo && UseWCombo && target.ServerPosition.Distance(ObjectManager.Player.ServerPosition) > 300)
                    {
                        var pred = W.GetPrediction(target);
                        if (pred.UnitPosition.Distance(ObjectManager.Player.ServerPosition) < 1000 &&
                            !pred.CollisionObjects.Any())
                        {
                    if (pred.UnitPosition.CountEnemyHeroesInRange(400) > 0)
                            W.Cast(pred.UnitPosition);
                        }
                    }
                }
            }
        }

        private Menu ComboMenu;
        private MenuBool UseQCombo;
        private MenuBool UseWCombo;
        private MenuBool UseRCombo;
        private MenuBool UseWHarass;
        private MenuBool UseRAntiGapclose;
        private MenuBool UseRInterrupt;
        private MenuBool DrawWRange;
        private MenuList<string> ScoutMode;
        public void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("ashecombomenu", "Combo Settings: "));
            UseQCombo = ComboMenu.Add(new MenuBool("asheqcombo", "Use Q", true));
            UseWCombo = ComboMenu.Add(new MenuBool("ashewcombo", "Use W", true));
            UseRCombo = ComboMenu.Add(new MenuBool("ashercombo", "Use R", true));
            UseWHarass = MainMenu.Add(new MenuBool("ashewharass", "Use W Harass", true));
            UseRAntiGapclose = MainMenu.Add(new MenuBool("asherantigapclose", "Use R AntiGapclose", false));
            UseRInterrupt = MainMenu.Add(new MenuBool("asherinterrupt", "Use R Interrupt", true));
            DrawWRange = MainMenu.Add(new MenuBool("ashedraww", "Draw W Range?", false));
            ScoutMode =
                MainMenu.Add(new MenuList<string>("ashescoutmode", "Scout (E) Mode: ",
                    new[] {"EnemyJungleClosest", "EnemyJungleFarthest", "DragonBaron", "Custom", "Disabled"}));
            MainMenu.Attach();
        }

        private bool HasQEmpoweredAttack => ObjectManager.Player.HasBuff("AsheQAttack");
    }
}
