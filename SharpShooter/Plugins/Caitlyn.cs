using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;
using Color = System.Drawing.Color;

namespace SharpShooter.Plugins
{
    public class Caitlyn
    {
        private readonly List<GameObject> _myTrapList = new List<GameObject>();
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Caitlyn()
        {
            _q = new Spell(SpellSlot.Q, 1250f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.VeryHigh };
            _w = new Spell(SpellSlot.W, 820f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.VeryHigh };
            _e = new Spell(SpellSlot.E, 800f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };
            _r = new Spell(SpellSlot.R, 2000f);

            _q.SetSkillshot(0.625f, 60f, 2200f, false, SkillshotType.SkillshotLine);
            _w.SetSkillshot(1.00f, 100f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(0.125f, 70f, 1600f, true, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();
            MenuProvider.Champion.Misc.AddItem("Auto R on Killable Target", false);
            MenuProvider.Champion.Misc.AddItem("Auto W on Immobile Target", true);
            MenuProvider.Champion.Misc.AddItem("Dash to Cursor Position (With E)", new KeyBind('G', KeyBindType.Press));
            MenuProvider.Champion.Misc.AddItem("Auto Attack Trapped Target", true);
            MenuProvider.Champion.Misc.AddItem("Use Anti-Melee (E)", true);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddItem("Draw R Killable", new Circle(true, Color.GreenYellow));
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Console.WriteLine("Sharpshooter: Caitlyn Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Caitlyn</font> Loaded.");
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
                                                _e.Cast(sender.Position);
        }

        private void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.IsAlly)
                if (sender.Name == "Cupcake Trap")
                    _myTrapList.RemoveAll(x => x.NetworkId == sender.NetworkId);
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsAlly)
                if (sender.Name == "Cupcake Trap")
                    _myTrapList.Add(sender);
        }

        private void Game_OnUpdate(EventArgs args)
        {
            _r.Range = 1500 + 500 * _r.Level;

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
                                        if (
                                            HeroManager.Enemies.Count(
                                                x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x)) == 0)
                                        {
                                            var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                            if (target != null)
                                                _q.Cast(target, false, true);
                                        }
                                        else
                                        {
                                            var immobileTarget =
                                                HeroManager.Enemies.Where(
                                                    x =>
                                                        x.IsValidTarget(_q.Range) &&
                                                        _q.GetPrediction(x).Hitchance >= HitChance.Immobile)
                                                    .OrderByDescending(x => TargetSelector.GetPriority(x))
                                                    .FirstOrDefault();
                                            if (immobileTarget != null)
                                                _q.Cast(immobileTarget, false, true);
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

                                if (MenuProvider.Champion.Combo.UseR)
                                    if (_r.IsReadyPerfectly())
                                    {
                                        var target =
                                            HeroManager.Enemies.FirstOrDefault(
                                                x =>
                                                    !Orbwalking.InAutoAttackRange(x) &&
                                                    x.IsKillableAndValidTarget(_r.GetDamage(x),
                                                        TargetSelector.DamageType.Physical, _r.Range));
                                        if (target != null)
                                            if (
                                                ObjectManager.Player.GetEnemiesInRange(1500f)
                                                    .Count(x => target.NetworkId != x.NetworkId) <= 0)
                                                if (
                                                    target.GetEnemiesInRange(500f)
                                                        .Count(x => target.NetworkId != x.NetworkId) <= 0)
                                                {
                                                    var collision =
                                                        Collision.GetCollision(new List<Vector3> { target.ServerPosition },
                                                            new PredictionInput
                                                            {
                                                                UseBoundingRadius = true,
                                                                Unit = ObjectManager.Player,
                                                                Delay = 0.5f,
                                                                Speed = 1500f,
                                                                Radius = 500f,
                                                                CollisionObjects = new[] { CollisionableObjects.Heroes }
                                                            })
                                                            .Any(x => x.NetworkId != target.NetworkId);
                                                    if (!collision)
                                                        _r.CastOnUnit(target);
                                                }
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
                                break;
                            }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                            {
                                //Laneclear
                                if (MenuProvider.Champion.Laneclear.UseQ)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                        if (_q.IsReadyPerfectly())
                                        {
                                            var farmLocation = _q.GetLineFarmLocation(MinionManager.GetMinions(_q.Range));
                                            if (farmLocation.MinionsHit >= 4)
                                                _q.Cast(farmLocation.Position);
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
                                                _q.Cast(target);
                                        }
                                break;
                            }
                    }

                    if (MenuProvider.Champion.Misc.GetBoolValue("Auto R on Killable Target"))
                        if (_r.IsReadyPerfectly())
                        {
                            var target =
                                HeroManager.Enemies.FirstOrDefault(
                                    x =>
                                        !Orbwalking.InAutoAttackRange(x) &&
                                        x.IsKillableAndValidTarget(_r.GetDamage(x), TargetSelector.DamageType.Physical,
                                            _r.Range));
                            if (target != null)
                                if (
                                    ObjectManager.Player.GetEnemiesInRange(1500f)
                                        .Count(x => target.NetworkId != x.NetworkId) <= 0)
                                    if (target.GetEnemiesInRange(500f).Count(x => target.NetworkId != x.NetworkId) <= 0)
                                    {
                                        var collision =
                                            Collision.GetCollision(new List<Vector3> { target.ServerPosition },
                                                new PredictionInput
                                                {
                                                    Unit = ObjectManager.Player,
                                                    Delay = 0.5f,
                                                    Speed = 1500f,
                                                    Radius = 500f,
                                                    CollisionObjects = new[] { CollisionableObjects.Heroes }
                                                })
                                                .Any(x => x.NetworkId != target.NetworkId);
                                        if (!collision)
                                            _r.CastOnUnit(target);
                                    }
                        }

                    if (MenuProvider.Champion.Misc.GetBoolValue("Auto W on Immobile Target"))
                        if (_w.IsReadyPerfectly())
                        {
                            var target =
                                HeroManager.Enemies.FirstOrDefault(
                                    x => x.IsValidTarget(_w.Range) && x.IsImmobileUntil() > 0.5f);
                            if (target != null)
                            {
                                if (!_myTrapList.Any(x => x.IsValid && target.Position.Distance(x.Position) <= 100))
                                    _w.Cast(target.Position);
                            }
                        }
                }

                if (MenuProvider.Champion.Misc.GetKeyBindValue("Dash to Cursor Position (With E)").Active)
                    if (_e.IsReadyPerfectly())
                        _e.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, -(_e.Range / 2)));

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto Attack Trapped Target"))
                {
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(1300)))
                    {
                        if (!Orbwalking.CanAttack()) continue;
                        var trapbuff = enemy.GetBuff("caitlynyordletrapinternal");
                        if (trapbuff != null && trapbuff.IsValid)
                            ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, enemy);
                    }
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                    if (gapcloser.Sender.IsValidTarget(_e.Range))
                        if (_e.IsReadyPerfectly())
                            _e.Cast(gapcloser.Sender.Position);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (args.DangerLevel >= Interrupter2.DangerLevel.Medium)
                    if (sender.IsValidTarget(_w.Range))
                        if (_w.IsReadyPerfectly())
                            if (!_myTrapList.Any(x => x.IsValid && sender.Position.Distance(x.Position) <= 100))
                                _w.Cast(sender.Position);
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

                var drawRKillable = MenuProvider.Champion.Drawings.GetCircleValue("Draw R Killable");
                if (drawRKillable.Active && _r.Level > 0)
                    foreach (
                        var target in
                            HeroManager.Enemies.Where(
                                x => x.IsKillableAndValidTarget(_r.GetDamage(x), TargetSelector.DamageType.Physical)))
                    {
                        var targetPos = Drawing.WorldToScreen(target.Position);
                        Render.Circle.DrawCircle(target.Position, target.BoundingRadius, drawRKillable.Color);
                        Drawing.DrawText(targetPos.X, targetPos.Y - 20, drawRKillable.Color, "R Killable");
                    }

                if (MenuProvider.Champion.Misc.GetKeyBindValue("Dash to Cursor Position (With E)").Active)
                {
                    var cursorPos = Drawing.WorldToScreen(Game.CursorPos);

                    if (_e.IsReadyPerfectly())
                    {
                        Drawing.DrawText(cursorPos.X, cursorPos.Y - 40, drawRKillable.Color, "Dash");
                        Render.Circle.DrawCircle(Game.CursorPos, 50, Color.GreenYellow, 3);
                    }
                    else
                    {
                        Drawing.DrawText(cursorPos.X, cursorPos.Y - 40, drawRKillable.Color, "Dash is Not Ready");
                    }
                }
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

            if (_r.IsReadyPerfectly())
            {
                damage += _r.GetDamage(enemy);
            }

            return damage;
        }
    }
}