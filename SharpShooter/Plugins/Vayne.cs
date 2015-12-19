using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Vayne
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private Spell _r;

        public Vayne()
        {
            _q = new Spell(SpellSlot.Q, 915f);
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 700f) {Width = 1f, MinHitChance = HitChance.VeryHigh};
            _r = new Spell(SpellSlot.R);

            _e.SetTargetted(0.375f, float.MaxValue);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseE();

            MenuProvider.Champion.Harass.AddUseQ(false);
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseAntiGapcloser(false);
            MenuProvider.Champion.Misc.AddUseInterrupter();
            MenuProvider.Champion.Misc.AddItem("Auto Q when using R", true);
            MenuProvider.Champion.Misc.AddItem("Q Stealth duration (ms)", new Slider(1000, 0, 1000));
            MenuProvider.Champion.Misc.AddItem("Use Anti-Melee (Q)", true);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddItem("Draw E Crash Prediction", new Circle(true, Color.YellowGreen));
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Console.WriteLine("Sharpshooter: Vayne Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Vayne</font> Loaded.");
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                    {
                        if (MenuProvider.Champion.Combo.UseE)
                            if (_e.IsReadyPerfectly())
                                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(_e.Range)))
                                {
                                    var prediction = _e.GetPrediction(enemy);
                                    if (prediction.Hitchance >= _e.MinHitChance)
                                    {
                                        var finalPosition = prediction.UnitPosition.Extend(
                                            ObjectManager.Player.Position, -400);
                                        if (finalPosition.IsWall())
                                            _e.CastOnUnit(enemy);
                                        else
                                            for (var i = 1; i < 400; i += 50)
                                            {
                                                var loc3 = prediction.UnitPosition.Extend(
                                                    ObjectManager.Player.Position, -i);
                                                if (loc3.IsWall())
                                                {
                                                    _e.CastOnUnit(enemy);
                                                    break;
                                                }
                                            }
                                    }
                                }
                        break;
                    }
                }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Slot == SpellSlot.R)
                    if (MenuProvider.Champion.Misc.GetBoolValue("Auto Q when using R"))
                        if (_q.IsReadyPerfectly())
                            _q.Cast(Game.CursorPos);
            }

            if (sender != null)
                if (args.Target != null)
                    if (args.Target.IsMe)
                        if (sender.Type == GameObjectType.obj_AI_Hero)
                            if (sender.IsEnemy)
                                if (sender.IsMelee)
                                    if (args.SData.IsAutoAttack())
                                        if (MenuProvider.Champion.Misc.GetBoolValue("Use Anti-Melee (Q)"))
                                            if (_q.IsReadyPerfectly())
                                                _q.Cast(ObjectManager.Player.Position.Extend(sender.Position, -_q.Range));
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                    {
                        if (target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (
                                        ObjectManager.Player.Position.Extend(Game.CursorPos, 700)
                                            .CountEnemiesInRange(700) <= 1)
                                        _q.Cast(Game.CursorPos);
                        }
                        break;
                    }
                    case Orbwalking.OrbwalkingMode.Mixed:
                    {
                        if (target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                        if (
                                            ObjectManager.Player.Position.Extend(Game.CursorPos, 700)
                                                .CountEnemiesInRange(700) <= 1)
                                            if (
                                                !ObjectManager.Player.Position.Extend(Game.CursorPos, 300)
                                                    .UnderTurret(true))
                                                _q.Cast(Game.CursorPos);
                        }
                        break;
                    }
                    case Orbwalking.OrbwalkingMode.LaneClear:
                    {
                        //Lane
                        if (MinionManager.GetMinions(float.MaxValue).Any(x => x.NetworkId == target.NetworkId))
                            if (MenuProvider.Champion.Laneclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (_q.IsReadyPerfectly())
                                        if (
                                            ObjectManager.Player.Position.Extend(Game.CursorPos, 700)
                                                .CountEnemiesInRange(700) <= 1)
                                            if (
                                                !ObjectManager.Player.Position.Extend(Game.CursorPos, 300)
                                                    .UnderTurret(true))
                                                if (
                                                    MinionManager.GetMinions(
                                                        ObjectManager.Player.Position.Extend(Game.CursorPos, 300), 615,
                                                        MinionTypes.All, MinionTeam.Enemy)
                                                        .Any(
                                                            x =>
                                                                x.NetworkId != target.NetworkId &&
                                                                x.IsKillableAndValidTarget(
                                                                    ObjectManager.Player.GetAutoAttackDamage(x) +
                                                                    _q.GetDamage(x), TargetSelector.DamageType.Physical)))
                                                    _q.Cast(Game.CursorPos);

                        //Jungle
                        if (
                            MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral,
                                MinionOrderTypes.MaxHealth).Any(x => x.NetworkId == target.NetworkId))
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_q.IsReadyPerfectly())
                                        _q.Cast(Game.CursorPos);

                        break;
                    }
                }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                var buff = ObjectManager.Player.GetBuff("vaynetumblefade");
                if (buff != null)
                    if (buff.IsValidBuff())
                        if (buff.EndTime - Game.Time >
                            buff.EndTime - buff.StartTime -
                            MenuProvider.Champion.Misc.GetSliderValue("Q Stealth duration (ms)").Value/1000)
                            if (!ObjectManager.Player.Position.UnderTurret(true))
                                args.Process = false;
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                    if (_e.IsReadyPerfectly())
                        if (gapcloser.Sender.IsValidTarget(_e.Range))
                            _e.CastOnUnit(gapcloser.Sender);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (args.DangerLevel >= Interrupter2.DangerLevel.High)
                    if (sender.IsValidTarget(_e.Range))
                        if (_e.IsReadyPerfectly())
                            _e.CastOnUnit(sender);
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

                var drawECrashPrediction = MenuProvider.Champion.Drawings.GetCircleValue("Draw E Crash Prediction");
                if (drawECrashPrediction.Active)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(_e.Range)))
                    {
                        var prediction = _e.GetPrediction(enemy);
                        for (var i = 1; i < 400; i += 50)
                        {
                            var loc3 = prediction.UnitPosition.Extend(ObjectManager.Player.Position, -i);
                            if (loc3.IsWall())
                                Render.Circle.DrawCircle(loc3, 30, drawECrashPrediction.Color, 5, false);
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

            var buff = enemy.GetBuff("vaynesilvereddebuff");

            if (buff != null)
                if (buff.Caster.IsMe)
                    if (buff.Count == 2)
                        damage += _w.GetDamage(enemy) + (float) ObjectManager.Player.GetAutoAttackDamage(enemy);

            if (_q.IsReadyPerfectly())
            {
                damage += _q.GetDamage(enemy) + (float) ObjectManager.Player.GetAutoAttackDamage(enemy);
            }

            if (ObjectManager.Player.HasBuff("vaynetumblebonus"))
            {
                damage += _q.GetDamage(enemy) + (float) ObjectManager.Player.GetAutoAttackDamage(enemy);
            }

            return 0;
        }
    }
}