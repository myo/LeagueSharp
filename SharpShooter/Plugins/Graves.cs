using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Graves
    {
        private const int EManaCost = 40;
        private const int RManaCost = 100;
        private readonly int[] _qManaCost = {0, 50, 55, 60, 65, 70};
        private readonly int[] _wManaCost = {0, 70, 75, 80, 85, 90};
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Graves()
        {
            _q = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W, 850f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};
            _e = new Spell(SpellSlot.E, 425f);
            _r = new Spell(SpellSlot.R, 1100f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};

            _q.SetSkillshot(0.25f, 45f, 2000f, false, SkillshotType.SkillshotLine);
            _w.SetSkillshot(0.25f, 250f, 1650f, false, SkillshotType.SkillshotCircle);
            _r.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddItem("Use R on Killable Target", true);
            MenuProvider.Champion.Combo.AddItem("Use R if Will Hit >=", new Slider(3, 2, 6));
            MenuProvider.Champion.Combo.AddItem("^ And Main Target HealthPercent <=", new Slider(80, 10, 100));
            MenuProvider.Champion.Combo.AddItem("Keep Mana for R", true);

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseKillsteal();
            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
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
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Console.WriteLine("Sharpshooter: Graves Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Graves</font> Loaded.");
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
                                if (MenuProvider.Champion.Combo.GetBoolValue("Keep Mana for R")
                                    ? ObjectManager.Player.Mana - _qManaCost[_q.Level] >= RManaCost
                                    : true)
                                    if (_q.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                        if (target != null)
                                            _q.Cast(target, false, true);
                                    }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (MenuProvider.Champion.Combo.GetBoolValue("Keep Mana for R")
                                    ? ObjectManager.Player.Mana - _wManaCost[_w.Level] >= RManaCost
                                    : true)
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var target =
                                            HeroManager.Enemies.Where(
                                                x => x.IsValidTarget(_w.Range) && !Orbwalking.InAutoAttackRange(x))
                                                .OrderByDescending(x => TargetSelector.GetPriority(x))
                                                .FirstOrDefault();
                                        if (target != null)
                                            _w.Cast(target);
                                    }

                            if (_r.IsReadyPerfectly())
                            {
                                if (MenuProvider.Champion.Combo.GetBoolValue("Use R on Killable Target"))
                                {
                                    var rKillableTarget =
                                        HeroManager.Enemies.FirstOrDefault(
                                            x =>
                                                x.IsKillableAndValidTarget(_r.GetDamage(x),
                                                    TargetSelector.DamageType.Physical, _r.Range));

                                    if (rKillableTarget != null)
                                        _r.Cast(rKillableTarget, false, true);
                                }

                                var target = TargetSelector.GetTarget(_r.Range, _r.DamageType);

                                if (target != null)
                                    if (target.HealthPercent <=
                                        MenuProvider.Champion.Combo.GetSliderValue("^ And Main Target HealthPercent <=")
                                            .Value)
                                        _r.CastIfWillHit(target,
                                            MenuProvider.Champion.Combo.GetSliderValue("Use R if Will Hit >=").Value);
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
                                        var farmlocation = _q.GetLineFarmLocation(MinionManager.GetMinions(_q.Range));

                                        if (farmlocation.MinionsHit >= 3)
                                            _q.Cast(farmlocation.Position);
                                    }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_q.IsReadyPerfectly())
                                    {
                                        var qTarget =
                                            MinionManager.GetMinions(_q.Range, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth)
                                                .FirstOrDefault(x => x.IsValidTarget(_q.Range));

                                        if (qTarget != null)
                                            _q.Cast(qTarget);
                                    }
                            break;
                        }
                    }
                }

                if (MenuProvider.Champion.Misc.UseKillsteal)
                {
                    foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                    {
                        if (target.IsKillableAndValidTarget(_q.GetDamage(target), TargetSelector.DamageType.Physical,
                            _q.Range))
                            _q.Cast(target, false, true);

                        if (target.IsKillableAndValidTarget(_w.GetDamage(target), TargetSelector.DamageType.Physical,
                            _w.Range))
                            _w.Cast(target, false, true);

                        if (target.IsKillableAndValidTarget(_r.GetDamage(target), TargetSelector.DamageType.Physical,
                            _r.Range))
                            _r.Cast(target, false, true);
                    }
                }
            }
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
                            if (MenuProvider.Champion.Combo.UseW)
                                if (MenuProvider.Champion.Combo.GetBoolValue("Keep Mana for R")
                                    ? ObjectManager.Player.Mana - _wManaCost[_w.Level] >= RManaCost
                                    : true)
                                    if (_w.IsReadyPerfectly())
                                        _w.Cast(target as Obj_AI_Base, false, true);

                            if (MenuProvider.Champion.Combo.UseE)
                                if (MenuProvider.Champion.Combo.GetBoolValue("Keep Mana for R")
                                    ? ObjectManager.Player.Mana - EManaCost >= RManaCost
                                    : true)
                                    if (_e.IsReadyPerfectly())
                                        if (
                                            ObjectManager.Player.Position.Extend(Game.CursorPos, 700)
                                                .CountEnemiesInRange(700) <= 1)
                                            if (!_q.IsReadyPerfectly())
                                                _e.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, 450));
                                            else if (ObjectManager.Player.Mana - _e.ManaCost >= _q.ManaCost)
                                                _e.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, 450));
                        }
                        break;
                    }
                }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                    if (gapcloser.Sender.IsValidTarget())
                        if (gapcloser.Sender.ChampionName.ToLowerInvariant() != "masteryi")
                            if (_e.IsReadyPerfectly())
                                _e.Cast(ObjectManager.Player.Position.Extend(gapcloser.Sender.Position, -_e.Range));
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
                if (drawRKillable.Active && _r.IsReadyPerfectly())
                    foreach (
                        var target in
                            HeroManager.Enemies.Where(
                                x => x.IsKillableAndValidTarget(_r.GetDamage(x), TargetSelector.DamageType.Physical)))
                    {
                        var targetPos = Drawing.WorldToScreen(target.Position);
                        Render.Circle.DrawCircle(target.Position, target.BoundingRadius, drawRKillable.Color);
                        Drawing.DrawText(targetPos.X, targetPos.Y - 20, drawRKillable.Color, "R Killable");
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