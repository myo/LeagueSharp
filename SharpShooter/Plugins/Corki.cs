using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Corki
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Corki()
        {
            _q = new Spell(SpellSlot.Q, 825f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Magical);
            _e = new Spell(SpellSlot.E, 600f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.Low};
            _r = new Spell(SpellSlot.R, 1250f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};

            _q.SetSkillshot(0.35f, 250f, 1000f, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(0.0f, 45f, float.MaxValue, false, SkillshotType.SkillshotCone);
            _r.SetSkillshot(0.20f, 40f, 2000f, true, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseR();
            MenuProvider.Champion.Harass.AddItem("Keep R Stacks", new Slider(3, 0, 7));
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseR();
            MenuProvider.Champion.Jungleclear.AddItem("Keep R Stacks", new Slider(5, 0, 7));
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Console.WriteLine("Sharpshooter: Corki Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Corki</font> Loaded.");
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
                                    var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                    if (target != null)
                                        _q.Cast(target, false, true);
                                }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                    if (target != null)
                                        _e.Cast();
                                }

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTargetNoCollision(_r);
                                    if (target != null)
                                        _r.Cast(target);
                                }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_q.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                        if (target != null)
                                            _q.Cast(target, false, true);
                                    }

                            if (MenuProvider.Champion.Harass.UseR)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_r.IsReadyPerfectly())
                                        if (_r.Instance.Ammo >
                                            MenuProvider.Champion.Harass.GetSliderValue("Keep R Stacks").Value)
                                        {
                                            var target = TargetSelector.GetTargetNoCollision(_r);
                                            if (target != null)
                                                _r.Cast(target);
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
                                        var farmLocation = _q.GetCircularFarmLocation(MinionManager.GetMinions(_q.Range));
                                        if (farmLocation.MinionsHit >= 4)
                                            _q.Cast(farmLocation.Position);
                                    }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_q.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(_q.Range, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth)
                                                .FirstOrDefault(x => x.IsValidTarget(_q.Range));
                                        if (target != null)
                                            _q.Cast(target);
                                    }

                            if (MenuProvider.Champion.Jungleclear.UseR)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_r.IsReadyPerfectly())
                                    {
                                        if (_r.Instance.Ammo >
                                            MenuProvider.Champion.Jungleclear.GetSliderValue("Keep R Stacks").Value)
                                        {
                                            var target =
                                                MinionManager.GetMinions(_r.Range, MinionTypes.All, MinionTeam.Neutral,
                                                    MinionOrderTypes.MaxHealth)
                                                    .FirstOrDefault(x => x.IsValidTarget(_r.Range));
                                            if (target != null)
                                                _r.Cast(target);
                                        }
                                    }
                            break;
                        }
                    }
                }
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

            if (_e.IsReadyPerfectly())
            {
                damage += _e.GetDamage(enemy);
            }

            if (_r.IsReadyPerfectly())
            {
                damage += ObjectManager.Player.HasBuff("corkimissilebarragecounterbig")
                    ? _r.GetDamage(enemy, 1)
                    : _r.GetDamage(enemy);
            }

            return damage;
        }
    }
}