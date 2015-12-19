using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Varus
    {
        private int _eLastCastTime;
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Varus()
        {
            _q = new Spell(SpellSlot.Q, 1600f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 925f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _r = new Spell(SpellSlot.R, 1200f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};

            _q.SetSkillshot(0.25f, 70f, 1500f, false, SkillshotType.SkillshotLine);
            _e.SetSkillshot(1.0f, 250f, 1750f, false, SkillshotType.SkillshotCircle);
            _r.SetSkillshot(0.25f, 120f, 1200f, false, SkillshotType.SkillshotLine);

            _q.SetCharged("VarusQ", "VarusQ", 250, 1600, 1.2f);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddItem("Q Min Charge", new Slider(800, 0, 1600));
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddItem("Q Min Charge", new Slider(1600, 0, 1600));
            MenuProvider.Champion.Harass.AddUseE(false);
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddUseE(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseE(false);
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Console.WriteLine("Sharpshooter: Varus Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Varus</font> Loaded.");
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                if (args.Slot == SpellSlot.E)
                    _eLastCastTime = Environment.TickCount;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (Orbwalking.CanMove(100))
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (_q.IsReadyPerfectly())
                                {
                                    var killableTarget =
                                        HeroManager.Enemies.FirstOrDefault(
                                            x =>
                                                x.IsValidTarget(_q.ChargedMaxRange) &&
                                                x.IsKillableAndValidTarget(_q.GetDamage(x),
                                                    TargetSelector.DamageType.Physical, _q.Range) &&
                                                _q.GetPrediction(x).Hitchance >= _q.MinHitChance);
                                    if (killableTarget != null)
                                    {
                                        if (_q.IsCharging)
                                        {
                                            if (killableTarget.IsValidTarget(_q.Range))
                                                _q.Cast(killableTarget, false, true);
                                        }
                                        else
                                            _q.StartCharging();
                                    }
                                    else
                                    {
                                        if (_w.Level > 0)
                                        {
                                            var target =
                                                HeroManager.Enemies.FirstOrDefault(
                                                    x =>
                                                        x.IsValidTarget(_q.ChargedMaxRange) &&
                                                        x.GetBuffCount("varuswdebuff") >= 3);
                                            if (target != null)
                                            {
                                                if (_q.IsCharging)
                                                {
                                                    if (_q.Range >=
                                                        MenuProvider.Champion.Combo.GetSliderValue("Q Min Charge").Value)
                                                    {
                                                        if (target.IsValidTarget(_q.Range))
                                                            _q.Cast(target, false, true);
                                                    }
                                                }
                                                else if (MenuProvider.Champion.Combo.UseE ? !_e.IsReadyPerfectly() : true)
                                                    if (_eLastCastTime + 1500 < Environment.TickCount)
                                                        _q.StartCharging();
                                            }
                                            else if (_q.IsCharging)
                                                if (_q.Range >=
                                                    MenuProvider.Champion.Combo.GetSliderValue("Q Min Charge").Value)
                                                {
                                                    var target1 = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                                    if (target1 != null)
                                                        _q.Cast(target1, false, true);
                                                }
                                        }
                                        else
                                        {
                                            if (_q.IsCharging)
                                            {
                                                if (_q.Range >=
                                                    MenuProvider.Champion.Combo.GetSliderValue("Q Min Charge").Value)
                                                {
                                                    var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                                    if (target != null)
                                                        _q.Cast(target, false, true);
                                                }
                                            }
                                            else if (TargetSelector.GetTarget(_q.ChargedMaxRange, _q.DamageType) != null)
                                                _q.StartCharging();
                                        }
                                    }
                                }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var killableTarget =
                                        HeroManager.Enemies.FirstOrDefault(
                                            x =>
                                                x.IsKillableAndValidTarget(_e.GetDamage(x),
                                                    TargetSelector.DamageType.Physical, _e.Range));
                                    if (killableTarget != null)
                                        _e.Cast(killableTarget, false, true);
                                    else
                                    {
                                        if (_w.Level > 0)
                                        {
                                            var target =
                                                HeroManager.Enemies.FirstOrDefault(
                                                    x =>
                                                        x.IsValidTarget(_e.Range) && x.GetBuffCount("varuswdebuff") >= 3);
                                            if (target != null)
                                                _e.Cast(target, false, true);
                                            else
                                                _e.CastIfWillHit(_e.GetTarget(), 3, false);
                                        }
                                        else
                                        {
                                            var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                            if (target != null)
                                                _e.Cast(target, false, true);
                                        }
                                    }
                                }

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_r.Range - 500,
                                        TargetSelector.DamageType.Physical);
                                    if (target != null)
                                        _r.Cast(target, false, true);
                                }

                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                            {
                                if (_q.IsReadyPerfectly())
                                {
                                    if (_q.IsCharging)
                                    {
                                        if (_q.Range >= _q.ChargedMaxRange)
                                        {
                                            var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                            if (target != null)
                                                _q.Cast(target, false, true);
                                        }
                                        else
                                        {
                                            var killableTarget =
                                                HeroManager.Enemies.FirstOrDefault(
                                                    x =>
                                                        x.IsKillableAndValidTarget(_q.GetDamage(x),
                                                            TargetSelector.DamageType.Physical, _q.Range));
                                            if (killableTarget != null)
                                                _q.Cast(killableTarget, false, true);
                                        }
                                    }
                                    else if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                        if (TargetSelector.GetTarget(_q.ChargedMaxRange, _q.DamageType) != null)
                                            _q.StartCharging();
                                }
                            }

                            if (MenuProvider.Champion.Harass.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                        if (target != null)
                                            _e.Cast(target, false, true);
                                    }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Laneclear
                            if (MenuProvider.Champion.Laneclear.UseQ)
                                if (_q.IsReadyPerfectly())
                                {
                                    var farmLocation =
                                        _q.GetLineFarmLocation(MinionManager.GetMinions(_q.ChargedMaxRange));
                                    if (_q.IsCharging)
                                    {
                                        if (_q.Range >= _q.ChargedMaxRange)
                                            _q.Cast(farmLocation.Position);
                                    }
                                    else if (farmLocation.MinionsHit >= 4)
                                        if (
                                            ObjectManager.Player.IsManaPercentOkay(
                                                MenuProvider.Champion.Laneclear.IfMana))
                                            _q.StartCharging();
                                }

                            if (MenuProvider.Champion.Laneclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var farmLocation = _e.GetCircularFarmLocation(MinionManager.GetMinions(_e.Range));
                                        if (farmLocation.MinionsHit >= 4)
                                            _e.Cast(farmLocation.Position);
                                    }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (_q.IsReadyPerfectly())
                                {
                                    var target =
                                        MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                            MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(600));
                                    if (target != null)
                                        if (_q.IsCharging)
                                        {
                                            if (_q.Range >= _q.ChargedMaxRange)
                                                _q.Cast(target);
                                        }
                                        else if (
                                            ObjectManager.Player.IsManaPercentOkay(
                                                MenuProvider.Champion.Jungleclear.IfMana))
                                            _q.StartCharging();
                                }

                            if (MenuProvider.Champion.Jungleclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth)
                                                .FirstOrDefault(
                                                    x => x.IsValidTarget(600) && x.GetBuffCount("varuswdebuff") >= 3);
                                        if (target != null)
                                            _e.Cast(target);
                                    }
                            break;
                        }
                    }
                }
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
                args.Process = !_q.IsCharging;
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                    if (_r.IsReadyPerfectly())
                    {
                        if (gapcloser.Sender.IsValidTarget(_r.Range))
                            _r.Cast(gapcloser.Sender);
                    }
                    else if (gapcloser.Sender.IsValidTarget(_e.Range))
                        _e.Cast(gapcloser.Sender, false, true);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (args.DangerLevel >= Interrupter2.DangerLevel.High)
                    if (sender.IsValidTarget(_r.Range))
                        if (_r.IsReadyPerfectly())
                            _r.CastOnUnit(sender);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.Champion.Drawings.DrawQrange.Active && _q.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range,
                        MenuProvider.Champion.Drawings.DrawQrange.Color);

                if (MenuProvider.Champion.Drawings.DrawErange.Active && _e.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range,
                        MenuProvider.Champion.Drawings.DrawErange.Color);

                if (MenuProvider.Champion.Drawings.DrawRrange.Active && _r.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range,
                        MenuProvider.Champion.Drawings.DrawRrange.Color);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (!ObjectManager.Player.IsWindingUp)
            {
                damage += (float) ObjectManager.Player.GetAutoAttackDamage(enemy, true);
            }

            if (_q.IsReadyPerfectly())
            {
                damage += _q.GetDamage(enemy);
            }

            if (_e.IsReadyPerfectly())
            {
                damage += _e.GetDamage(enemy);
            }

            if (_r.IsReadyPerfectly())
            {
                damage += _r.GetDamage(enemy);
            }

            return damage;
        }
    }
}