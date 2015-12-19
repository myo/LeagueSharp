using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Ezreal
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Ezreal()
        {
            _q = new Spell(SpellSlot.Q, 1150f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W, 1000f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};
            _e = new Spell(SpellSlot.E, 475f, TargetSelector.DamageType.Magical);
            _r = new Spell(SpellSlot.R, 3000f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};

            _q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);
            _w.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);
            _r.SetSkillshot(1.0f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseR();
            MenuProvider.Champion.Combo.AddItem("Cast R if Will Hit >=", new Slider(3, 2, 5));

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW(false);
            MenuProvider.Champion.Harass.AddAutoHarass();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ();
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddItem("Use Anti-Melee (E)", true);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            Console.WriteLine("Sharpshooter: Ezreal Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Ezreal</font> Loaded.");
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
                                    if (target.IsValidTarget(_q.Range))
                                        _q.Cast(target);
                                }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);
                                    if (target != null)
                                        _w.Cast(target, false, true);
                                }

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                    if (ObjectManager.Player.CountEnemiesInRange(700) == 0)
                                    {
                                        var killableTarget =
                                            HeroManager.Enemies.FirstOrDefault(
                                                x =>
                                                    !Orbwalking.InAutoAttackRange(x) &&
                                                    x.IsKillableAndValidTarget(_r.GetDamage(x),
                                                        TargetSelector.DamageType.Magical, _r.Range) &&
                                                    _r.GetPrediction(x).Hitchance >= _r.MinHitChance);
                                        if (killableTarget != null)
                                            _r.Cast(killableTarget, false, true);
                                        _r.CastIfWillHit(TargetSelector.GetTarget(_r.Range, _r.DamageType), 3);
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
                                        if (target.IsValidTarget(_q.Range))
                                            _q.Cast(target);
                                    }

                            if (MenuProvider.Champion.Harass.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);
                                        if (target != null)
                                            _w.Cast(target, false, true);
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
                                                .OrderBy(x => x.Health)
                                                .FirstOrDefault(
                                                    x =>
                                                        x.IsKillableAndValidTarget(_q.GetDamage(x),
                                                            TargetSelector.DamageType.Physical, _q.Range) &&
                                                        _q.GetPrediction(x).Hitchance >= _q.MinHitChance);
                                        if (target != null)
                                            _q.Cast(target);
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
                            break;
                        }
                    }

                    if (MenuProvider.Champion.Harass.AutoHarass)
                        if (MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                            if (!ObjectManager.Player.IsRecalling())
                            {
                                if (_q.IsReadyPerfectly())
                                    if (MenuProvider.Champion.Harass.UseQ)
                                        if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                        {
                                            var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                            if (target != null)
                                                if (ObjectManager.Player.UnderTurret(true)
                                                    ? !target.UnderTurret(true)
                                                    : true)
                                                    _q.Cast(target);
                                        }

                                if (_w.IsReadyPerfectly())
                                    if (MenuProvider.Champion.Harass.UseW)
                                        if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                        {
                                            var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);
                                            if (target != null)
                                                if (ObjectManager.Player.UnderTurret(true)
                                                    ? !target.UnderTurret(true)
                                                    : true)
                                                    _w.Cast(target);
                                        }
                            }
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null)
                if (args.Target != null)
                    if (args.Target.IsMe)
                        if (sender.Type == GameObjectType.obj_AI_Hero)
                            if (sender.IsEnemy)
                                if (sender.IsMelee)
                                    if (args.SData.IsAutoAttack())
                                        if (MenuProvider.Champion.Misc.GetBoolValue("Use Anti-Melee (E)"))
                                            if (_e.IsReadyPerfectly())
                                                _e.Cast(ObjectManager.Player.Position.Extend(sender.Position, -_e.Range));
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
                if (target.Type == GameObjectType.obj_AI_Hero)
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            if (_q.IsReadyPerfectly())
                            {
                                if (MenuProvider.Champion.Combo.UseQ)
                                    if (target.IsValidTarget(_q.Range))
                                        _q.Cast(target as Obj_AI_Base);
                            }
                            else if (_w.IsReadyPerfectly())
                                if (MenuProvider.Champion.Combo.UseW)
                                    if (target.IsValidTarget(_w.Range))
                                        _w.Cast(target as Obj_AI_Base);
                            break;
                        case Orbwalking.OrbwalkingMode.Mixed:
                            if (_q.IsReadyPerfectly())
                            {
                                if (MenuProvider.Champion.Harass.UseQ)
                                    if (target.IsValidTarget(_q.Range))
                                        _q.Cast(target as Obj_AI_Base);
                            }
                            else if (_w.IsReadyPerfectly())
                                if (MenuProvider.Champion.Harass.UseW)
                                    if (target.IsValidTarget(_w.Range))
                                        _w.Cast(target as Obj_AI_Base);
                            break;
                    }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                    if (gapcloser.Sender.ChampionName.ToLowerInvariant() != "masteryi")
                        if (_e.IsReadyPerfectly())
                            _e.Cast(ObjectManager.Player.Position.Extend(gapcloser.Sender.Position, -_e.Range));
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
                damage += _r.GetDamage(enemy);
            }

            return damage;
        }
    }
}