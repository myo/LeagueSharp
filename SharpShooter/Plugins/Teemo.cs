using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Teemo
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private Spell _e;
        private readonly Spell _r;

        public Teemo()
        {
            _q = new Spell(SpellSlot.Q, 680f, TargetSelector.DamageType.Magical);
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E);
            _r = new Spell(SpellSlot.R, 300f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};

            _r.SetSkillshot(1.5f, 100f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ();
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseAntiGapcloser();

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;

            Console.WriteLine("Sharpshooter: Teemo Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Teemo</font> Loaded.");
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
                if (target.Type == GameObjectType.obj_AI_Hero)
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.LastHit:
                            break;
                        case Orbwalking.OrbwalkingMode.Mixed:
                            break;
                        case Orbwalking.OrbwalkingMode.LaneClear:
                            break;
                        case Orbwalking.OrbwalkingMode.Combo:
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (target.IsValidTarget(_q.Range))
                                    if (_q.IsReadyPerfectly())
                                        _q.CastOnUnit(target as Obj_AI_Base);
                            break;
                        case Orbwalking.OrbwalkingMode.None:
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (target.IsValidTarget(_q.Range))
                                    if (_q.IsReadyPerfectly())
                                        _q.CastOnUnit(target as Obj_AI_Base);
                            break;
                        default:
                            break;
                    }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            _r.Range = _r.Level*300;

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
                                    if (!HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                                    {
                                        var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                        if (target != null)
                                            _q.CastOnUnit(target);
                                    }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                    if (ObjectManager.Player.CountEnemiesInRange(1000f) >= 1)
                                        _w.Cast();

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                {
                                    var target =
                                        HeroManager.Enemies.FirstOrDefault(
                                            x =>
                                                x.IsValidTarget(_r.Range) && !x.IsFacing(ObjectManager.Player) &&
                                                !x.HasBuff("bantamtraptarget") &&
                                                _r.GetPrediction(x).Hitchance >= _r.MinHitChance);
                                    if (target != null)
                                        _r.Cast(target, false, true);
                                }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (!HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                                    {
                                        var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                        if (target != null)
                                            _q.CastOnUnit(target);
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
                                        var target =
                                            MinionManager.GetMinions(_q.Range)
                                                .FirstOrDefault(
                                                    x =>
                                                        x.IsKillableAndValidTarget(_q.GetDamage(x),
                                                            TargetSelector.DamageType.Magical, _q.Range) &&
                                                        (x.CharData.BaseSkinName.Contains("siege") ||
                                                         x.CharData.BaseSkinName.Contains("super")));
                                        if (target != null)
                                            _q.CastOnUnit(target);
                                    }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_q.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(600));
                                        if (target != null)
                                            _q.CastOnUnit(target);
                                    }
                            break;
                        }
                    }
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
            {
                if (gapcloser.Sender.IsValidTarget(_q.Range))
                    if (_q.IsReadyPerfectly())
                        _q.CastOnUnit(gapcloser.Sender);

                if (_w.IsReadyPerfectly())
                    _w.Cast();

                if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 300)
                    if (_r.IsReadyPerfectly())
                        _r.Cast(ObjectManager.Player.Position);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.Champion.Drawings.DrawQrange.Active && _q.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range,
                        MenuProvider.Champion.Drawings.DrawQrange.Color);

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

            return damage;
        }
    }
}