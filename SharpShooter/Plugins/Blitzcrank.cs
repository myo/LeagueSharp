using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Blitzcrank
    {
        private bool _dontAutoAttack;
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Blitzcrank()
        {
            _q = new Spell(SpellSlot.Q, 925f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E);
            _r = new Spell(SpellSlot.R, 550f, TargetSelector.DamageType.Magical);

            _q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);

            foreach (var enemy in HeroManager.Enemies)
                MenuProvider.ChampionMenuInstance.SubMenu("Combo")
                    .SubMenu("Q Targets")
                    .AddItem(new MenuItem("Combo.Q Targets." + enemy.ChampionName, enemy.ChampionName, true))
                    .SetValue(true);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseE();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Misc.AddQHitchanceSelector(HitChance.High);
            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddItem("Draw Q Target Mark",
                new Circle(true, Color.FromArgb(100, Color.DeepSkyBlue)));
            MenuProvider.Champion.Drawings.AddItem("Draw Whitelisted Target Mark", new Circle(true, Color.White));
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;

            Console.WriteLine("Sharpshooter: Blitzcrank Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Blitzcrank</font> Loaded.");
        }

        private void Game_OnUpdate(EventArgs args)
        {
            _q.MinHitChance = MenuProvider.Champion.Misc.QSelectedHitchance;

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
                                    var target =
                                        HeroManager.Enemies.Where(
                                            x =>
                                                x.IsValidTarget(_q.Range) &&
                                                _q.GetPrediction(x).Hitchance >= _q.MinHitChance &&
                                                MenuProvider.MenuInstance.Item("Combo.Q Targets." + x.ChampionName, true)
                                                    .GetValue<bool>())
                                            .OrderByDescending(x => TargetSelector.GetPriority(x))
                                            .FirstOrDefault();
                                    if (target != null)
                                        _q.Cast(target, false, true);
                                }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (HeroManager.Enemies.Any(x => x.HasBuff("rocketgrab2")))
                                    _w.Cast();

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                {
                                    foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(_r.Range)))
                                    {
                                        //R Logics
                                        if (target.IsKillableAndValidTarget(_r.GetDamage(target),
                                            TargetSelector.DamageType.Magical, _r.Range))
                                            _r.Cast(target);

                                        if (target.HasBuff("rocketgrab2"))
                                        {
                                            if (MenuProvider.Champion.Combo.UseE)
                                                if (_e.IsReadyPerfectly())
                                                    _e.Cast();

                                            _r.Cast(target);
                                        }

                                        if (target.IsImmobileUntil() > 0f)
                                            if (target.IsImmobileUntil() <= 0.25f)
                                                _r.Cast();
                                    }

                                    if (ObjectManager.Player.CountEnemiesInRange(_r.Range) >= 2)
                                        _r.Cast();
                                }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (_q.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                    if (target != null)
                                        _q.Cast(target, false, true);
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
                if (_r.IsReadyPerfectly())
                    if (gapcloser.Sender.IsValidTarget(_r.Range))
                        _r.Cast();
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (args.DangerLevel >= Interrupter2.DangerLevel.High)
                    if (sender.IsValidTarget(_r.Range))
                        if (_r.IsReadyPerfectly())
                            _r.Cast();
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                if (args.Slot == SpellSlot.E)
                    Orbwalking.ResetAutoAttackTimer();
        }

        private void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Animation == "Spell1" || args.Animation == "Spell4")
                    _dontAutoAttack = true;
                else
                    _dontAutoAttack = false;
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                if (_dontAutoAttack)
                {
                    args.Process = false;
                    return;
                }

                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        if (MenuProvider.Champion.Combo.UseW)
                            if (args.Target.Type == GameObjectType.obj_AI_Hero)
                                if (_w.IsReadyPerfectly())
                                    _w.Cast();
                        break;
                }
            }
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        if (target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                    _e.Cast();
                        }
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        if (target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Harass.UseE)
                                if (_e.IsReadyPerfectly())
                                    _e.Cast();
                        }
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

                if (MenuProvider.Champion.Drawings.DrawRrange.Active && _r.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range,
                        MenuProvider.Champion.Drawings.DrawRrange.Color);

                var drawQTargetMark = MenuProvider.Champion.Drawings.GetCircleValue("Draw Q Target Mark");
                if (drawQTargetMark.Active)
                    if (_q.IsReadyPerfectly())
                    {
                        var target =
                            HeroManager.Enemies.Where(
                                x =>
                                    x.IsValidTarget(_q.Range) && _q.GetPrediction(x).Hitchance >= _q.MinHitChance &&
                                    MenuProvider.MenuInstance.Item("Combo.Q Targets." + x.ChampionName, true)
                                        .GetValue<bool>())
                                .OrderByDescending(x => TargetSelector.GetPriority(x))
                                .FirstOrDefault();
                        if (target != null)
                            Render.Circle.DrawCircle(target.Position, 70, drawQTargetMark.Color, 3, false);
                    }

                var drawWhitelistedTargetMark =
                    MenuProvider.Champion.Drawings.GetCircleValue("Draw Whitelisted Target Mark");
                if (drawWhitelistedTargetMark.Active)
                {
                    if (_q.IsReadyPerfectly())
                    {
                        foreach (
                            var target in
                                HeroManager.Enemies.Where(
                                    x =>
                                        x.IsValidTarget() &&
                                        MenuProvider.MenuInstance.Item("Combo.Q Targets." + x.ChampionName, true)
                                            .GetValue<bool>()))
                        {
                            Render.Circle.DrawCircle(target.Position, 30, drawWhitelistedTargetMark.Color, 5, false);
                        }
                    }
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

            if (_r.IsReadyPerfectly())
            {
                damage += _r.GetDamage(enemy);
            }

            return damage;
        }
    }
}