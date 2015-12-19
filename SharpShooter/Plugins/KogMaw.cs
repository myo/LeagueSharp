using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class KogMaw
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;
        private bool IsZombie => ObjectManager.Player.HasBuff("kogmawicathiansurprise");

        public KogMaw()
        {
            _q = new Spell(SpellSlot.Q, 950f) { MinHitChance = HitChance.High };
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 1260f) { MinHitChance = HitChance.High };
            _r = new Spell(SpellSlot.R) { MinHitChance = HitChance.High };

            _q.SetSkillshot(0.25f, 70f, 1650f, true, SkillshotType.SkillshotLine);
            _e.SetSkillshot(0.50f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            _r.SetSkillshot(1.5f, 225f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddUseR();
            MenuProvider.Champion.Combo.AddItem("R Stacks Limit", new Slider(3, 1, 6));
            MenuProvider.Champion.Combo.AddItem("Keep Mana For W", true);

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseE();
            MenuProvider.Champion.Harass.AddUseR();
            MenuProvider.Champion.Harass.AddItem("R Stacks Limit", new Slider(1, 1, 6));
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseE(false);
            MenuProvider.Champion.Laneclear.AddUseR(false);
            MenuProvider.Champion.Laneclear.AddItem("R Stacks Limit", new Slider(1, 1, 6));
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();
            MenuProvider.Champion.Jungleclear.AddUseE();
            MenuProvider.Champion.Jungleclear.AddUseR();
            MenuProvider.Champion.Jungleclear.AddItem("R Stacks Limit", new Slider(1, 1, 6));
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Console.WriteLine("Sharpshooter: KogMaw Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">KogMaw</font> Loaded.");
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;

            _w.Range = 565 + 60 + _w.Level * 30 + 65;
            _r.Range = 900 + _r.Level * 300;

            if (Orbwalking.CanMove(100))
            {
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (MenuProvider.Champion.Combo.GetBoolValue("Keep Mana For W") && _w.Level > 0
                                        ? ObjectManager.Player.Mana - _q.ManaCost >= _w.ManaCost
                                        : true)
                                    {
                                        var target = TargetSelector.GetTargetNoCollision(_q);
                                        if (target != null)
                                            _q.Cast(target);
                                    }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                    if (HeroManager.Enemies.Any(x => x.IsValidTarget(_w.Range)))
                                        _w.Cast();

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                    if (MenuProvider.Champion.Combo.GetBoolValue("Keep Mana For W") && _w.Level > 0
                                        ? ObjectManager.Player.Mana - _e.ManaCost >= _w.ManaCost
                                        : true)
                                    {
                                        var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                        if (target != null)
                                            _e.Cast(target, false, true);
                                    }

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                    if (MenuProvider.Champion.Combo.GetBoolValue("Keep Mana For W") && _w.Level > 0
                                        ? ObjectManager.Player.Mana - _r.ManaCost >= _w.ManaCost
                                        : true)
                                    {
                                        if (ObjectManager.Player.GetBuffCount("kogmawlivingartillerycost") <
                                            MenuProvider.Champion.Combo.GetSliderValue("R Stacks Limit").Value)
                                        {
                                            var target = TargetSelector.GetTarget(_r.Range, _r.DamageType);
                                            if (target != null)
                                                _r.Cast(target, false, true);
                                        }
                                        else
                                        {
                                            var killableTarget =
                                                HeroManager.Enemies.FirstOrDefault(
                                                    x =>
                                                        x.IsKillableAndValidTarget(_r.GetDamage(x),
                                                            TargetSelector.DamageType.Magical, _r.Range) &&
                                                        _r.GetPrediction(x).Hitchance >= _r.MinHitChance);
                                            if (killableTarget != null)
                                                _r.Cast(killableTarget, false, true);
                                        }
                                    }

                            break;
                        }
                    case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    {
                                        var target = TargetSelector.GetTargetNoCollision(_q);
                                        if (target != null)
                                            _q.Cast(target);
                                    }

                            if (MenuProvider.Champion.Harass.UseE)
                                if (_e.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    {
                                        var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                        if (target != null)
                                            _e.Cast(target, false, true);
                                    }

                            if (MenuProvider.Champion.Harass.UseR)
                                if (_r.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                        if (ObjectManager.Player.GetBuffCount("kogmawlivingartillerycost") <
                                            MenuProvider.Champion.Harass.GetSliderValue("R Stacks Limit").Value)
                                        {
                                            var target = TargetSelector.GetTarget(_r.Range, _r.DamageType);
                                            if (target != null)
                                                _r.Cast(target, false, true);
                                        }
                            break;
                        }
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Laneclear
                            if (MenuProvider.Champion.Laneclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var farmLocation = _e.GetLineFarmLocation(MinionManager.GetMinions(_e.Range));
                                        if (farmLocation.MinionsHit >= 4)
                                            _e.Cast(farmLocation.Position);
                                    }

                            if (MenuProvider.Champion.Laneclear.UseR)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (_r.IsReadyPerfectly())
                                        if (ObjectManager.Player.GetBuffCount("kogmawlivingartillerycost") <
                                            MenuProvider.Champion.Laneclear.GetSliderValue("R Stacks Limit").Value)
                                        {
                                            var farmLocation =
                                                _r.GetCircularFarmLocation(MinionManager.GetMinions(_r.Range));
                                            if (farmLocation.MinionsHit >= 4)
                                                _r.Cast(farmLocation.Position);
                                        }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(600));
                                        if (target != null)
                                            _e.Cast(target);
                                    }

                            if (MenuProvider.Champion.Jungleclear.UseR)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_r.IsReadyPerfectly())
                                        if (ObjectManager.Player.GetBuffCount("kogmawlivingartillerycost") <
                                            MenuProvider.Champion.Jungleclear.GetSliderValue("R Stacks Limit").Value)
                                        {
                                            var target =
                                                MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                    MinionOrderTypes.MaxHealth)
                                                    .FirstOrDefault(x => x.IsValidTarget(600));
                                            if (target != null)
                                                _r.Cast(target);
                                        }

                            break;
                        }
                }
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!args.Unit.IsMe) return;

            if (IsZombie)
                args.Process = false;

            switch (MenuProvider.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (_w.IsReadyPerfectly())
                        if (MenuProvider.Champion.Combo.UseW)
                            if (args.Target.IsValidTarget(_w.Range))
                                _w.Cast();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (_w.IsReadyPerfectly())
                        if (MenuProvider.Champion.Jungleclear.UseW)
                            if (
                                MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player),
                                    MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                                    .Any(x => x.NetworkId == args.Target.NetworkId))
                                _w.Cast();
                    break;
            }
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
                damage += (float)ObjectManager.Player.GetAutoAttackDamage(enemy, true);
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