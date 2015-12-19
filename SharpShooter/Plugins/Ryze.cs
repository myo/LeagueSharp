using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Ryze
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;
        private readonly Spell _qNoCollision;

        public Ryze()
        {
            _q = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};
            _qNoCollision = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Magical)
            {
                MinHitChance = HitChance.Low
            };
            _w = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Magical);
            _e = new Spell(SpellSlot.E, 600f, TargetSelector.DamageType.Magical);
            _r = new Spell(SpellSlot.R);

            _q.SetSkillshot(0.25f, 50f, 1400f, true, SkillshotType.SkillshotLine);
            _qNoCollision.SetSkillshot(0.25f, 50f, 1400f, false, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddUseR();
            MenuProvider.Champion.Combo.AddItem("Ignore Collision", true);

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW();
            MenuProvider.Champion.Harass.AddUseE();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddUseW(false);
            MenuProvider.Champion.Laneclear.AddUseE(false);
            MenuProvider.Champion.Laneclear.AddItem("Use Burst Laneclear if Passive is Activated", true);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();
            MenuProvider.Champion.Jungleclear.AddUseE();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseKillsteal();
            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();
            MenuProvider.Champion.Misc.AddItem("Auto Keep Passive Stacks", new KeyBind('T', KeyBindType.Toggle, true));
            MenuProvider.Champion.Misc.AddItem("^ Min Mana", new Slider(70, 0, 100));

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddItem("Draw The Number of Passive Stacks", true);
            MenuProvider.Champion.Drawings.AddItem("Draw Remaining Time of Charged Passive", true);
            MenuProvider.Champion.Drawings.AddItem("Draw Auto Keep Passive Status", true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Console.WriteLine("Sharpshooter: Ryze Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Ryze</font> Loaded.");
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (ObjectManager.Player.HasBuff("ryzepassivecharged") ? true : Orbwalking.CanMove(100))
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (_q.IsReadyPerfectly())
                                {
                                    if (!(MenuProvider.Champion.Combo.UseW && _w.IsReadyPerfectly()))
                                    {
                                        if (MenuProvider.Champion.Combo.GetBoolValue("Ignore Collision"))
                                        {
                                            var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);

                                            if (target != null)
                                                _qNoCollision.Cast(target);
                                        }
                                        else
                                        {
                                            var target = TargetSelector.GetTargetNoCollision(_q);

                                            if (target != null)
                                                _q.Cast(target);
                                        }
                                    }
                                }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);

                                    if (target != null)
                                        _w.CastOnUnit(target);
                                }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    if (!(MenuProvider.Champion.Combo.UseQ && _q.IsReadyPerfectly()))
                                    {
                                        if (!(MenuProvider.Champion.Combo.UseW && _w.IsReadyPerfectly()))
                                        {
                                            var target = TargetSelector.GetTarget(_e.Range, _w.DamageType);

                                            if (target != null)
                                                _e.CastOnUnit(target);
                                        }
                                    }
                                }

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                {
                                    if (!(MenuProvider.Champion.Combo.UseW && _w.IsReadyPerfectly()))
                                    {
                                        if (!(MenuProvider.Champion.Combo.UseE && _e.IsReadyPerfectly()))
                                        {
                                            if (ObjectManager.Player.CountEnemiesInRange(1000) >= 1)
                                            {
                                                _r.Cast();
                                            }
                                        }
                                    }
                                }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (_q.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);

                                    if (target != null)
                                        _q.Cast(target);
                                }

                            if (MenuProvider.Champion.Harass.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);

                                    if (target != null)
                                        _w.CastOnUnit(target);
                                }

                            if (MenuProvider.Champion.Harass.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);

                                    if (target != null)
                                        _e.CastOnUnit(target);
                                }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Laneclear
                            if (MenuProvider.Champion.Laneclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (_q.IsReadyPerfectly())
                                    {
                                        if (
                                            MenuProvider.Champion.Laneclear.GetBoolValue(
                                                "Use Burst Laneclear if Passive is Activated") &&
                                            ObjectManager.Player.HasBuff("ryzepassivecharged"))
                                        {
                                            var target =
                                                MinionManager.GetMinions(_q.Range)
                                                    .FirstOrDefault(
                                                        x =>
                                                            x.IsValidTarget(_q.Range) &&
                                                            _q.GetPrediction(x).Hitchance >= _q.MinHitChance);

                                            if (target != null)
                                                _q.Cast(target);
                                        }
                                        else
                                        {
                                            var target =
                                                MinionManager.GetMinions(_q.Range)
                                                    .FirstOrDefault(
                                                        x =>
                                                            x.IsKillableAndValidTarget(_q.GetDamage(x),
                                                                TargetSelector.DamageType.Magical, _q.Range) &&
                                                            _q.GetPrediction(x).Hitchance >= _q.MinHitChance);

                                            if (target != null)
                                                _q.Cast(target);
                                        }
                                    }

                            if (MenuProvider.Champion.Laneclear.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (_w.IsReadyPerfectly())
                                    {
                                        if (
                                            MenuProvider.Champion.Laneclear.GetBoolValue(
                                                "Use Burst Laneclear if Passive is Activated") &&
                                            ObjectManager.Player.HasBuff("ryzepassivecharged"))
                                        {
                                            var target =
                                                MinionManager.GetMinions(_w.Range)
                                                    .FirstOrDefault(x => x.IsValidTarget(_w.Range));

                                            if (target != null)
                                                _w.CastOnUnit(target);
                                        }
                                        else
                                        {
                                            var target =
                                                MinionManager.GetMinions(_w.Range)
                                                    .FirstOrDefault(
                                                        x =>
                                                            x.IsKillableAndValidTarget(_w.GetDamage(x),
                                                                TargetSelector.DamageType.Magical, _w.Range));

                                            if (target != null)
                                                _w.CastOnUnit(target);
                                        }
                                    }

                            if (MenuProvider.Champion.Laneclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        if (
                                            MenuProvider.Champion.Laneclear.GetBoolValue(
                                                "Use Burst Laneclear if Passive is Activated") &&
                                            ObjectManager.Player.HasBuff("ryzepassivecharged"))
                                        {
                                            var target =
                                                MinionManager.GetMinions(_e.Range)
                                                    .FirstOrDefault(x => x.IsValidTarget(_e.Range));

                                            if (target != null)
                                                _e.CastOnUnit(target);
                                        }
                                        else
                                        {
                                            var target =
                                                MinionManager.GetMinions(_e.Range)
                                                    .FirstOrDefault(
                                                        x =>
                                                            x.IsKillableAndValidTarget(_e.GetDamage(x),
                                                                TargetSelector.DamageType.Magical, _e.Range));

                                            if (target != null)
                                                _e.CastOnUnit(target);
                                        }
                                    }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_q.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth)
                                                .FirstOrDefault(
                                                    x =>
                                                        x.IsValidTarget(600) &&
                                                        _q.GetPrediction(x).Hitchance >= _q.MinHitChance);

                                        if (target != null)
                                            _q.Cast(target);
                                    }

                            if (MenuProvider.Champion.Jungleclear.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(600));

                                        if (target != null)
                                            _w.CastOnUnit(target);
                                    }

                            if (MenuProvider.Champion.Jungleclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(600));

                                        if (target != null)
                                            _e.CastOnUnit(target);
                                    }
                            break;
                        }
                    }
                }

                if (MenuProvider.Champion.Misc.UseKillsteal)
                {
                    foreach (var target in HeroManager.Enemies.Where(x => x.IsValid))
                    {
                        if (_q.IsReadyPerfectly())
                        {
                            if (target.IsKillableAndValidTarget(_q.GetDamage(target), TargetSelector.DamageType.Magical,
                                _q.Range))
                            {
                                _q.Cast(target);
                            }
                        }

                        if (_w.IsReadyPerfectly())
                        {
                            if (target.IsKillableAndValidTarget(_w.GetDamage(target), TargetSelector.DamageType.Magical,
                                _w.Range))
                            {
                                _w.CastOnUnit(target);
                            }
                        }

                        if (_e.IsReadyPerfectly())
                        {
                            if (target.IsKillableAndValidTarget(_e.GetDamage(target), TargetSelector.DamageType.Magical,
                                _e.Range))
                            {
                                _e.CastOnUnit(target);
                            }
                        }
                    }
                }

                if (MenuProvider.Champion.Misc.GetKeyBindValue("Auto Keep Passive Stacks").Active)
                    if (
                        ObjectManager.Player.IsManaPercentOkay(
                            MenuProvider.Champion.Misc.GetSliderValue("^ Min Mana").Value))
                        if (!ObjectManager.Player.IsRecalling())
                            if (_q.Level > 0)
                                if (_w.Level > 0)
                                    if (_e.Level > 0)
                                        if (_q.IsReadyPerfectly())
                                            if (!ObjectManager.Player.HasBuff("ryzepassivecharged"))
                                            {
                                                var passive = ObjectManager.Player.GetBuff("ryzepassivestack");
                                                var passiveCount = passive != null ? passive.Count : 0;

                                                if (passiveCount < 4)
                                                    if (passive == null ? true : passive.EndTime - Game.ClockTime <= 0.6)
                                                    {
                                                        var target1 = TargetSelector.GetTargetNoCollision(_q);
                                                        if (target1 != null)
                                                        {
                                                            if (_q.Cast(target1) != Spell.CastStates.SuccessfullyCasted)
                                                            {
                                                                var target2 =
                                                                    MinionManager.GetMinions(_q.Range)
                                                                        .FirstOrDefault(
                                                                            x =>
                                                                                x.IsValidTarget(_q.Range) &&
                                                                                _q.GetPrediction(x).Hitchance >=
                                                                                _q.MinHitChance);

                                                                if (target2 != null)
                                                                {
                                                                    if (_q.Cast(target2) !=
                                                                        Spell.CastStates.SuccessfullyCasted)
                                                                    {
                                                                        _q.Cast(Game.CursorPos);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    _q.Cast(Game.CursorPos);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var target2 =
                                                                MinionManager.GetMinions(_q.Range)
                                                                    .FirstOrDefault(
                                                                        x =>
                                                                            x.IsValidTarget(_q.Range) &&
                                                                            _q.GetPrediction(x).Hitchance >=
                                                                            _q.MinHitChance);

                                                            if (target2 != null)
                                                            {
                                                                if (_q.Cast(target2) !=
                                                                    Spell.CastStates.SuccessfullyCasted)
                                                                {
                                                                    _q.Cast(Game.CursorPos);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _q.Cast(Game.CursorPos);
                                                            }
                                                        }
                                                    }
                                            }
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                    {
                        if (_w.IsReadyPerfectly())
                            args.Process = false;
                        break;
                    }
                }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (_w.IsReadyPerfectly())
                    if (gapcloser.Sender.IsValidTarget(_w.Range))
                        _w.CastOnUnit(gapcloser.Sender);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (_w.IsReadyPerfectly())
                    if (sender.IsValidTarget(_w.Range))
                        _w.CastOnUnit(sender);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.Champion.Drawings.DrawQrange.Active && _q.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range,
                        MenuProvider.Champion.Drawings.DrawQrange.Color);

                if (MenuProvider.Champion.Drawings.DrawWrange.Active && _w.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range,
                        MenuProvider.Champion.Drawings.DrawWrange.Color);

                if (MenuProvider.Champion.Drawings.DrawErange.Active && _e.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range,
                        MenuProvider.Champion.Drawings.DrawErange.Color);

                var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);

                if (MenuProvider.Champion.Drawings.GetBoolValue("Draw The Number of Passive Stacks"))
                {
                    var buff = ObjectManager.Player.GetBuff("ryzepassivestack");
                    if (buff != null)
                        Drawing.DrawText(playerPos.X, playerPos.Y - 120, Color.GreenYellow, buff.Count.ToString());
                }

                if (MenuProvider.Champion.Drawings.GetBoolValue("Draw Remaining Time of Charged Passive"))
                {
                    var buff = ObjectManager.Player.GetBuff("ryzepassivecharged");
                    if (buff != null)
                        Drawing.DrawText(playerPos.X, playerPos.Y - 120, Color.GreenYellow,
                            (buff.EndTime - Game.Time).ToString("0.0"));
                }

                if (MenuProvider.Champion.Drawings.GetBoolValue("Draw Auto Keep Passive Status"))
                {
                    Drawing.DrawText(playerPos.X, playerPos.Y + 20, Color.GreenYellow,
                        "Auto Stack:" +
                        (MenuProvider.Champion.Misc.GetKeyBindValue("Auto Keep Passive Stacks").Active ? "ON" : "OFF"));
                }
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (!ObjectManager.Player.IsWindingUp)
                damage += (float) ObjectManager.Player.GetAutoAttackDamage(enemy, true);

            if (_q.IsReadyPerfectly())
                damage += _q.GetDamage(enemy);

            if (_w.IsReadyPerfectly())
                damage += _w.GetDamage(enemy);

            if (_e.IsReadyPerfectly())
                damage += _e.GetDamage(enemy);

            return damage;
        }
    }
}