using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Illaoi
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Illaoi()
        {
            _q = new Spell(SpellSlot.Q, 820f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W, 480f, TargetSelector.DamageType.Physical);
            _e = new Spell(SpellSlot.E, 900f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.VeryHigh};
            _r = new Spell(SpellSlot.R, 450f, TargetSelector.DamageType.Physical);

            _q.SetSkillshot(0.75f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            _e.SetSkillshot(0.066f, 50f, 1900f, true, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddItem("Use R if Will Hit >=", new Slider(2, 2, 6));

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW();
            MenuProvider.Champion.Harass.AddUseE(false);
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddUseW(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Console.WriteLine("Sharpshooter: illaoi Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Illaoi</font> Loaded.");
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
                                    if (MenuProvider.Champion.Combo.UseE ? !_e.IsReadyPerfectly() : true)
                                    {
                                        var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);

                                        if (target != null)
                                        {
                                            _q.Cast(target, false, true);
                                        }
                                    }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);

                                    if (target != null)
                                        if (
                                            !HeroManager.Enemies.Any(
                                                x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x)))
                                        {
                                            _w.Cast();
                                        }
                                }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);

                                    if (target != null)
                                    {
                                        _e.Cast(target, false, true);
                                    }
                                }

                            if (_r.IsReadyPerfectly())
                            {
                                if (
                                    HeroManager.Enemies.Count(
                                        x =>
                                            x.IsValidTarget(_r.Range) &&
                                            ObjectManager.Player.ServerPosition.Distance(
                                                Prediction.GetPrediction(x, 0.5f).UnitPosition) < _r.Range) >=
                                    MenuProvider.Champion.Combo.GetSliderValue("Use R if Will Hit >=").Value)
                                {
                                    _r.Cast();
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
                                        var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);

                                        if (target != null)
                                        {
                                            _q.Cast(target, false, true);
                                        }
                                    }

                            if (MenuProvider.Champion.Harass.UseW)
                                if (_w.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    {
                                        var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);

                                        if (target != null)
                                            if (
                                                !HeroManager.Enemies.Any(
                                                    x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x)))
                                            {
                                                _w.Cast();
                                            }
                                    }

                            if (MenuProvider.Champion.Harass.UseE)
                                if (_e.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    {
                                        var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);

                                        if (target != null)
                                        {
                                            _e.Cast(target, false, true);
                                        }
                                    }

                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Laneclear
                            if (MenuProvider.Champion.Laneclear.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    {
                                        var farmLocation = _q.GetLineFarmLocation(MinionManager.GetMinions(_q.Range));

                                        if (farmLocation.MinionsHit >= 4)
                                        {
                                            _q.Cast(farmLocation.Position);
                                        }
                                    }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    {
                                        var farmLocation =
                                            _q.GetLineFarmLocation(MinionManager.GetMinions(600, MinionTypes.All,
                                                MinionTeam.Neutral, MinionOrderTypes.MaxHealth));

                                        if (farmLocation.MinionsHit >= 1)
                                        {
                                            _q.Cast(farmLocation.Position);
                                        }
                                    }
                            break;
                        }
                    }
                }
            }
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                    {
                        if (target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    if (target.IsValidTarget(_w.Range))
                                    {
                                        _w.Cast();
                                    }
                                }
                        }
                        break;
                    }
                    case Orbwalking.OrbwalkingMode.Mixed:
                    {
                        if (target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Harass.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    if (target.IsValidTarget(_w.Range))
                                    {
                                        _w.Cast();
                                    }
                                }
                        }
                        break;
                    }
                    case Orbwalking.OrbwalkingMode.LaneClear:
                    {
                        if (MinionManager.GetMinions(float.MaxValue).Any(x => x.NetworkId == target.NetworkId))
                        {
                            if (MenuProvider.Champion.Laneclear.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    if (target.IsValidTarget(_w.Range))
                                    {
                                        _w.Cast();
                                    }
                                }
                        }

                        if (
                            MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral)
                                .Any(x => x.NetworkId == target.NetworkId))
                        {
                            if (MenuProvider.Champion.Jungleclear.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    if (target.IsValidTarget(_w.Range))
                                    {
                                        _w.Cast();
                                    }
                                }
                        }
                        break;
                    }
                    default:
                        break;
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null)
                if (sender.IsMe)
                {
                    if (args.Slot == SpellSlot.W)
                    {
                        Orbwalking.ResetAutoAttackTimer();
                    }
                }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.Champion.Drawings.DrawQrange.Active && _q.IsReadyPerfectly())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range,
                        MenuProvider.Champion.Drawings.DrawQrange.Color);
                }

                if (MenuProvider.Champion.Drawings.DrawWrange.Active && _w.IsReadyPerfectly())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range,
                        MenuProvider.Champion.Drawings.DrawWrange.Color);
                }

                if (MenuProvider.Champion.Drawings.DrawErange.Active && _e.IsReadyPerfectly())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range,
                        MenuProvider.Champion.Drawings.DrawErange.Color);
                }

                if (MenuProvider.Champion.Drawings.DrawRrange.Active && _r.IsReadyPerfectly())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range,
                        MenuProvider.Champion.Drawings.DrawRrange.Color);
                }
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

            if (_w.IsReadyPerfectly())
            {
                damage += _w.GetDamage(enemy);
            }

            if (_r.IsReadyPerfectly())
            {
                damage += _r.GetDamage(enemy);
            }

            return damage;
        }
    }
}