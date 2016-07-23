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
    public class Xerath : CSPlugin
    {
        public Xerath()
        {
            Q = new Spell(SpellSlot.Q, 1550);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 1150);
            R = new Spell(SpellSlot.R, 675);

            Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            InitMenu();
            Obj_AI_Hero.OnDoCast += OnDoCast;
            Orbwalker.OnAction += OnAction;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Events.OnGapCloser += EventsOnOnGapCloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
            Obj_AI_Base.OnIssueOrder += OnIssueOrder;
        }

        private void OnAction(object sender, OrbwalkingActionArgs args)
        {
            if (args.Type == OrbwalkingType.BeforeAttack)
            {
                args.Process = AttacksEnabled;
            }
        }

        private void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe && IsCastingR)
            {
                args.Process = false;
            }
        }

        private void OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs args)
        {
            if (EInterrupt && args.Sender.Distance(ObjectManager.Player) < 750)
            {
                var pred = E.GetPrediction(args.Sender);
                if (!pred.CollisionObjects.Any())
                {
                    E.Cast(args.Sender);
                }
            }
        }

        private void EventsOnOnGapCloser(object sender, Events.GapCloserEventArgs args)
        {
            if (EAntiGapcloser && args.IsDirectedToPlayer && args.Sender.Distance(ObjectManager.Player) < 800)
            {
                var pred = E.GetPrediction(args.Sender);
                if (pred.Hitchance >= HitChance.High)
                {
                    E.Cast(pred.UnitPosition);
                }
            }
        }

        public override void OnDraw(EventArgs args)
        {
            if (!Q.IsCharging && ObjectManager.Player.CountEnemyHeroesInRange(1200) > 0)
            {
                Q.StartCharging();
            }
        }

        public override void OnUpdate(EventArgs args)
        {
        }

        private void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
        }

        private Menu ComboMenu;
        private MenuBool UseQCombo;
        private MenuList<string> QMode;
        private MenuBool UseWCombo;
        private MenuBool UseECombo;
        private MenuBool UseRCombo;
        private MenuBool QHarass;
        private MenuBool WHarass;
        private MenuBool EAntiGapcloser;
        private MenuBool EInterrupt;
        public void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("Xerathcombomenu", "Combo Settings: "));
            UseQCombo = ComboMenu.Add(new MenuBool("Xerathqcombo", "Use Q", true));
            QMode =
                ComboMenu.Add(new MenuList<string>("Xerathqmode", "Q Mode: ", new[] {"PREDICTION", "TARGETPOSITION"}));
            UseWCombo = ComboMenu.Add(new MenuBool("Xerathwcombo", "Use W", true));
            UseECombo = ComboMenu.Add(new MenuBool("Xerathecombo", "Use E", true));
            UseRCombo = ComboMenu.Add(new MenuBool("Xerathrcombo", "Use R", true));
            QHarass = MainMenu.Add(new MenuBool("Xerathqharass", "Use Q Harass", true));
            WHarass = MainMenu.Add(new MenuBool("Xerathwharass", "Use W Harass", false));
            EInterrupt = MainMenu.Add(new MenuBool("Xeratheinterrupt", "Use E Interrupt", true));
            EAntiGapcloser = MainMenu.Add(new MenuBool("Xerathegc", "Use E Anti-Gapcloser", true));
            MainMenu.Attach();
        }
        private bool AttacksEnabled
        {
            get
            {
                if (IsCastingR)
                    return false;

                if (Q.IsCharging)
                    return false;

                if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo)
                    return IsPassiveUp || (!Q.IsReady() && !W.IsReady() && !E.IsReady());

                return true;
            }
        }

        public bool IsPassiveUp
        {
            get { return ObjectManager.Player.HasBuff("xerathascended2onhit"); }
        }

        public bool IsCastingR
        {
            get
            {
                return ObjectManager.Player.HasBuff("XerathLocusOfPower2");
            }
        }
    }
}
