using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            InitMenu();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Events.OnGapCloser += EventsOnOnGapCloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
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
            if (Q.IsReady())
            {
                var targets = ValidTargets.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Q.GetDamage(target) &&
                        (!target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") &&
                         !target.HasBuff("JudicatorIntervention")))
                    {
                        Q.CastIfHitchanceMinimum(target, HitChance.High);
                    }
                }
            }
            if (Orbwalker.ActiveMode != OrbwalkingMode.None)
            {
                if (Q.IsReady())
                {
                    var qtarget = TargetSelector.GetTarget(Q);
                    if (qtarget.IsHPBarRendered)
                    {
                        var pred = Q.GetPrediction(qtarget);
                        if (Q.IsReady() && UseQ && pred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(pred.UnitPosition);
                            return;
                        }
                    }
                }
                if (W.IsReady())
                {
                    var wMode = UseWMode.SelectedValue;
                    if (wMode == "ALWAYS" || (wMode == "COMBO" && Orbwalker.ActiveMode == OrbwalkingMode.Combo))
                    {
                        var wtarget = TargetSelector.GetTarget(W);
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
            if (R.IsReady())
            {
                var rtarget = TargetSelector.GetTarget(R);
                if (UseRKey.Active)
                {
                    var pred = R.GetPrediction(rtarget);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        Q.Cast(pred.UnitPosition);
                        return;
                    }
                }
                if (ObjectManager.Player.CountEnemyHeroesInRange(800) < 1)
                {
                    R.CastIfWillHit(rtarget, 3);
                }
            }
        }

        private MenuBool UseQ;
        private MenuList<string> UseWMode;
        private MenuKeyBind UseRKey;

        public void InitMenu()
        {
            UseQ = MainMenu.Add(new MenuBool("Ezrealq", "Use Q", true));
            UseWMode = MainMenu.Add(new MenuList<string>("Ezrealw", "Use W", new [] {"COMBO", "ALWAYS", "NEVER"}));
            UseRKey = MainMenu.Add(new MenuKeyBind("Ezrealr", "Use R Key: ", Keys.R, KeyBindType.Press));
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
    }
}