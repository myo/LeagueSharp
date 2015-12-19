using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Tristana
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Tristana()
        {
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W, 1170f, TargetSelector.DamageType.Magical) { MinHitChance = HitChance.High };
            _e = new Spell(SpellSlot.E);
            _r = new Spell(SpellSlot.R);

            _w.SetSkillshot(0.5f, 270f, 1500f, false, SkillshotType.SkillshotCircle);

            foreach (var enemy in HeroManager.Enemies)
                MenuProvider.ChampionMenuInstance.SubMenu("Combo")
                    .SubMenu("E Targets")
                    .AddItem(new MenuItem("Combo.E Targets." + enemy.ChampionName, "Enemy " + enemy.ChampionName, true))
                    .SetValue(true);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseE();
            MenuProvider.Champion.Harass.AddIfMana();

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseE();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseKillsteal();
            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();
            MenuProvider.Champion.Misc.AddItem("Auto E on Turret", true);

            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddItem("Draw E Killable", new Circle(true, Color.GreenYellow));
            MenuProvider.Champion.Drawings.AddItem("Draw R Killable", new Circle(true, Color.GreenYellow));
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;

            Console.WriteLine("Sharpshooter: Tristana Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Tristana</font> Loaded.");
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
                if (target != null)
                    if (target.Type == GameObjectType.obj_AI_Turret || target.Type == GameObjectType.obj_Turret)
                        if (MenuProvider.Champion.Misc.GetBoolValue("Auto E on Turret"))
                            _e.CastOnUnit(target as Obj_AI_Base);
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;

            _e.Range = Orbwalking.GetRealAutoAttackRange(null) + 65;
            _r.Range = Orbwalking.GetRealAutoAttackRange(null) + 65;

            if (Orbwalking.CanMove(100))
            {
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var target =
                                        HeroManager.Enemies.Where(
                                            x =>
                                                x.IsValidTarget(_e.Range) &&
                                                MenuProvider.ChampionMenuInstance.Item(
                                                    "Combo.E Targets." + x.ChampionName, true).GetValue<bool>())
                                            .OrderByDescending(x => TargetSelector.GetPriority(x))
                                            .FirstOrDefault();
                                    if (target != null)
                                    {
                                        _e.CastOnUnit(target);
                                    }
                                }

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                {
                                    var target =
                                        HeroManager.Enemies.OrderByDescending(x => x.Health)
                                            .FirstOrDefault(
                                                x =>
                                                    x.IsKillableAndValidTarget(_r.GetDamage(x),
                                                        TargetSelector.DamageType.Physical, _r.Range) &&
                                                    !x.IsWillDieByTristanaE());
                                    if (target != null)
                                        _r.CastOnUnit(target);
                                }
                            break;
                        }
                    case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                        if (target != null)
                                            _e.CastOnUnit(target);
                                    }
                            break;
                        }
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(600));
                                        if (target != null)
                                            _e.CastOnUnit(target);
                                    }
                            break;
                        }
                }
            }

            if (MenuProvider.Champion.Misc.UseKillsteal)
            {
                var target =
                    HeroManager.Enemies.OrderByDescending(x => x.Health)
                        .FirstOrDefault(
                            x =>
                                x.IsKillableAndValidTarget(_r.GetDamage(x), TargetSelector.DamageType.Physical,
                                    _r.Range) && !x.IsWillDieByTristanaE());
                if (target != null)
                    _r.CastOnUnit(target);
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
                if (Orbwalking.InAutoAttackRange(args.Target))
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            {
                                if (MenuProvider.Champion.Combo.UseQ)
                                    if (_q.IsReadyPerfectly())
                                        _q.Cast();
                                break;
                            }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                            {
                                if (MenuProvider.Champion.Jungleclear.UseQ)
                                    if (_q.IsReadyPerfectly())
                                        if (
                                            MinionManager.GetMinions(
                                                Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), MinionTypes.All,
                                                MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                                                .Any(x => x.NetworkId == args.Target.NetworkId))
                                            _q.Cast();
                                break;
                            }
                    }
                }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                    if (gapcloser.Sender.IsValidTarget(_r.Range))
                        if (_r.IsReadyPerfectly())
                            _r.CastOnUnit(gapcloser.Sender);
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
                if (MenuProvider.Champion.Drawings.DrawWrange.Active && _w.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range,
                        MenuProvider.Champion.Drawings.DrawWrange.Color);

                if (MenuProvider.Champion.Drawings.DrawErange.Active && _e.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range,
                        MenuProvider.Champion.Drawings.DrawErange.Color);

                if (MenuProvider.Champion.Drawings.DrawRrange.Active && _r.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range,
                        MenuProvider.Champion.Drawings.DrawRrange.Color);

                var drawEKillable = MenuProvider.Champion.Drawings.GetCircleValue("Draw E Killable");
                if (drawEKillable.Active)
                    foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget() && x.IsWillDieByTristanaE())
                        )
                    {
                        var targetPos = Drawing.WorldToScreen(target.Position);
                        Render.Circle.DrawCircle(target.Position, target.BoundingRadius, drawEKillable.Color);
                        Drawing.DrawText(targetPos.X, targetPos.Y - 50, drawEKillable.Color, "will die by E");
                    }

                var drawRKillable = MenuProvider.Champion.Drawings.GetCircleValue("Draw R Killable");
                if (drawRKillable.Active && _r.IsReadyPerfectly())
                    foreach (
                        var target in
                            HeroManager.Enemies.Where(
                                x => x.IsKillableAndValidTarget(_r.GetDamage(x), TargetSelector.DamageType.Magical)))
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
                damage += (float)ObjectManager.Player.GetAutoAttackDamage(enemy, true);
            }

            if (enemy.HasBuff("tristanaecharge"))
            {
                damage += (float)(_e.GetDamage(enemy) * (enemy.GetBuffCount("tristanaecharge") * 0.30)) +
                          _e.GetDamage(enemy);
            }

            if (_r.IsReadyPerfectly())
            {
                damage += _r.GetDamage(enemy);
            }

            return damage;
        }
    }
}