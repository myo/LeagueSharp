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
    public class Kayle : CSPlugin
    {
        public Kayle()
        {
            base.Q = new Spell(SpellSlot.Q);
            base.W = new Spell(SpellSlot.W, 1100);
            base.W.SetSkillshot(250f, 75f, 1500f, true, SkillshotType.SkillshotLine);
            base.E = new Spell(SpellSlot.E, 25000);
            base.R = new Spell(SpellSlot.R, 1400);
            base.R.SetSkillshot(250f, 80f, 1500f, false, SkillshotType.SkillshotLine);
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
        }

        private void EventsOnOnGapCloser(object sender, Events.GapCloserEventArgs args)
        {
        }

        public override void OnDraw(EventArgs args)
        {
        }

        public override void OnUpdate(EventArgs args)
        {
        }

        private void OnAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
        }

        private void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
        }

        private Menu ComboMenu;
        private MenuBool UseQCombo;
        private MenuBool UseWCombo;
        private MenuBool UseECombo;
        private MenuBool UseRCombo;
        public void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("Kaylecombomenu", "Combo Settings: "));
            UseQCombo = ComboMenu.Add(new MenuBool("Kayleqcombo", "Use Q", true));
            UseWCombo = ComboMenu.Add(new MenuBool("Kaylewcombo", "Use W", true));
            UseECombo = ComboMenu.Add(new MenuBool("Kayleecombo", "Use E", true));
            UseRCombo = ComboMenu.Add(new MenuBool("Kaylercombo", "Use R", true));
            MainMenu.Attach();
        }

    }
}
