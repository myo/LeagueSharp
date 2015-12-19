using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    internal class QTarget
    {
        internal bool Pix;
        internal Obj_AI_Hero Target;
    }

    public class Lulu
    {
        private readonly string[] _championsihate =
        {
            "ahri", "anivia", "annie", "ashe", "azir", "brand", "caitlyn", "cassiopeia", "corki", "draven",
            "ezreal", "graves", "jinx", "kalista", "karma", "karthus", "katarina", "kennen", "kogmaw", "leblanc",
            "lucian", "lux", "malzahar", "masteryi", "missfortune", "orianna", "quinn", "sivir", "syndra", "talon",
            "teemo", "tristana", "twistedfate", "twitch", "varus", "vayne", "veigar", "velkoz", "viktor", "xerath",
            "zed", "ziggs", "akali", "diana", "ekko", "fiddlesticks", "fiora", "fizz", "heimerdinger", "jayce",
            "kassadin",
            "kayle", "khazix", "lissandra", "mordekaiser", "nidalee", "riven", "shaco", "vladimir", "yasuo",
            "tryndamere"
        };

        private readonly Spell _q;
        private readonly Spell _q2;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;
        private readonly List<QTarget> _qTargets = new List<QTarget>();

        public Lulu()
        {
            _q = new Spell(SpellSlot.Q, 925f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};
            _q2 = new Spell(SpellSlot.Q, 925f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W, 650f, TargetSelector.DamageType.Magical);
            _e = new Spell(SpellSlot.E, 650f, TargetSelector.DamageType.Magical);
            _r = new Spell(SpellSlot.R, 900f, TargetSelector.DamageType.Magical);

            _q.SetSkillshot(0.25f, 70f, 1450f, false, SkillshotType.SkillshotLine);
            _q2.SetSkillshot(0.25f, 70f, 1450f, false, SkillshotType.SkillshotLine);

            foreach (var enemy in HeroManager.Enemies)
                MenuProvider.ChampionMenuInstance.SubMenu("Combo")
                    .SubMenu("W Targets")
                    .AddItem(new MenuItem("Combo.W Targets." + enemy.ChampionName, "Enemy " + enemy.ChampionName, true))
                    .SetValue(_championsihate.Contains(enemy.ChampionName.ToLowerInvariant()));

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddItem("Use R if Will Hit (Airborne) >=", new Slider(2, 1, 6));

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseE();
            MenuProvider.Champion.Harass.AddAutoHarass();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseE();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseKillsteal();
            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();

            MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                .SubMenu("Auto E")
                .AddItem(new MenuItem("Misc.Auto E.Auto E", "Auto E (Shield)", true))
                .SetValue(true);
            MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                .SubMenu("Auto E")
                .AddItem(new MenuItem("Misc.Auto E.ForMe", "For Me", true))
                .SetValue(true);

            MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                .SubMenu("Auto R")
                .AddItem(new MenuItem("Misc.Auto R.Auto R", "Auto R", true))
                .SetValue(true);
            MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                .SubMenu("Auto R")
                .AddItem(new MenuItem("Misc.Auto R.ForMe", "For Me", true))
                .SetValue(true);
            MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                .SubMenu("Auto R")
                .AddItem(new MenuItem("Misc.Auto R.Health", "if Health <=", true))
                .SetValue(new Slider(30, 0, 80));

            foreach (var ally in HeroManager.Allies.Where(x => !x.IsMe))
            {
                MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                    .SubMenu("Auto E")
                    .AddItem(new MenuItem("Misc.Auto E.For" + ally.ChampionName, "For " + "Ally " + ally.ChampionName,
                        true))
                    .SetValue(true);
                MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                    .SubMenu("Auto R")
                    .AddItem(new MenuItem("Misc.Auto R.For" + ally.ChampionName, "For " + "Ally " + ally.ChampionName,
                        true))
                    .SetValue(true);
            }

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Console.WriteLine("Sharpshooter: Lulu Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Lulu</font> Loaded.");
        }

        private GameObject MyPix => ObjectManager.Player.Pet;

        private void Game_OnUpdate(EventArgs args)
        {
            _q2.UpdateSourcePosition(MyPix.Position, MyPix.Position);

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
                                    QLogic(false);
                                }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var target =
                                        HeroManager.Enemies.Where(
                                            x =>
                                                x.IsValidTarget(_w.Range) &&
                                                MenuProvider.ChampionMenuInstance.Item(
                                                    "Combo.W Targets." + x.ChampionName, true).GetValue<bool>())
                                            .OrderByDescending(x => TargetSelector.GetPriority(x))
                                            .FirstOrDefault();

                                    if (target != null)
                                    {
                                        _w.CastOnUnit(target);
                                    }
                                    else
                                    {
                                        if (_w.IsReadyPerfectly())
                                        {
                                            if (ObjectManager.Player.CountEnemiesInRange(1000f) >= 1)
                                                _w.Cast(ObjectManager.Player);
                                        }
                                    }
                                }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);

                                    if (target != null)
                                    {
                                        _e.CastOnUnit(target);
                                    }
                                }

                            if (_r.IsReadyPerfectly())
                            {
                                foreach (var target in HeroManager.Allies.Where(x => x.IsValidTarget(_r.Range, false)))
                                {
                                    if (target.CountEnemiesInRange(350) >=
                                        MenuProvider.Champion.Combo.GetSliderValue("Use R if Will Hit (Airborne) >=")
                                            .Value)
                                        _r.CastOnUnit(target);
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
                                        QLogic(false);
                                    }

                            if (MenuProvider.Champion.Harass.UseE)
                                if (_e.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    {
                                        var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);

                                        if (target != null)
                                        {
                                            _e.CastOnUnit(target);
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

                            if (MenuProvider.Champion.Jungleclear.UseE)
                                if (_e.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).FirstOrDefault();

                                        if (target != null)
                                        {
                                            _e.CastOnUnit(target);
                                        }
                                    }
                            break;
                        }
                    }

                    if (MenuProvider.Champion.Harass.AutoHarass)
                        if (MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo &&
                            MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
                            if (!ObjectManager.Player.IsRecalling())
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                {
                                    if (MenuProvider.Champion.Harass.UseQ)
                                        if (_q.IsReadyPerfectly())
                                            QLogic(false);

                                    if (MenuProvider.Champion.Harass.UseE)
                                        if (_e.IsReadyPerfectly())
                                        {
                                            var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                            if (target != null)
                                                _e.CastOnUnit(target);
                                        }
                                }
                }

                if (MenuProvider.Champion.Misc.UseKillsteal)
                {
                    if (_q.IsReadyPerfectly())
                    {
                        QLogic(true);
                    }

                    foreach (var target in HeroManager.Enemies)
                    {
                        if (_e.IsReadyPerfectly())
                        {
                            if (target.IsKillableAndValidTarget(_e.GetDamage(target), _e.DamageType, _e.Range))
                            {
                                _e.CastOnUnit(target);
                            }
                        }
                    }
                }

                if (MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.Auto R", true).GetValue<bool>())
                {
                    if (_r.IsReadyPerfectly())
                        if (MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.ForMe", true).GetValue<bool>())
                        {
                            if (ObjectManager.Player.HealthPercent <=
                                MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.Health", true)
                                    .GetValue<Slider>()
                                    .Value)
                                if (ObjectManager.Player.CountEnemiesInRange(800f) >= 2)
                                    _r.CastOnUnit(ObjectManager.Player);
                        }

                    if (_r.IsReadyPerfectly())
                    {
                        foreach (
                            var target in
                                HeroManager.Allies.Where(
                                    x =>
                                        x.IsValidTarget(_r.Range, false) && !x.IsZombie && !x.IsMe &&
                                        x.CountEnemiesInRange(800f) >= 2 &&
                                        MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.For" + x.ChampionName, true)
                                            .GetValue<bool>() &&
                                        x.HealthPercent <
                                        MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.Health", true)
                                            .GetValue<Slider>()
                                            .Value).OrderBy(x => x.Health))
                        {
                            if (_r.CastOnUnit(target))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && args != null && args.Target != null)
            {
                if (sender.IsEnemy)
                {
                    if (sender.Type == GameObjectType.obj_AI_Hero || sender.Type == GameObjectType.obj_AI_Turret)
                    {
                        if (args.Target.Type == GameObjectType.obj_AI_Hero)
                        {
                            var target = args.Target as Obj_AI_Hero;

                            if (target.IsMe)
                            {
                                if (_e.IsReadyPerfectly())
                                {
                                    if (
                                        MenuProvider.ChampionMenuInstance.Item("Misc.Auto E.Auto E", true)
                                            .GetValue<bool>())
                                    {
                                        if (
                                            MenuProvider.ChampionMenuInstance.Item("Misc.Auto E.ForMe", true)
                                                .GetValue<bool>())
                                        {
                                            _e.CastOnUnit(ObjectManager.Player);
                                        }
                                    }
                                }

                                if (_r.IsReadyPerfectly())
                                {
                                    if (
                                        MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.Auto R", true)
                                            .GetValue<bool>())
                                    {
                                        if (
                                            MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.ForMe", true)
                                                .GetValue<bool>())
                                        {
                                            if (target.HealthPercent <
                                                MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.Health", true)
                                                    .GetValue<Slider>()
                                                    .Value)
                                            {
                                                _r.CastOnUnit(ObjectManager.Player);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (target.IsAlly)
                            {
                                if (_e.IsReadyPerfectly())
                                {
                                    if (
                                        MenuProvider.ChampionMenuInstance.Item("Misc.Auto E.Auto E", true)
                                            .GetValue<bool>())
                                    {
                                        if (
                                            MenuProvider.ChampionMenuInstance.Item(
                                                "Misc.Auto E.For" + target.ChampionName, true).GetValue<bool>())
                                        {
                                            if (target.IsValidTarget(_e.Range, false))
                                            {
                                                _e.CastOnUnit(target);
                                            }
                                        }
                                    }
                                }

                                if (_r.IsReadyPerfectly())
                                {
                                    if (
                                        MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.Auto R", true)
                                            .GetValue<bool>())
                                    {
                                        if (
                                            MenuProvider.ChampionMenuInstance.Item(
                                                "Misc.Auto R.For" + target.ChampionName, true).GetValue<bool>())
                                        {
                                            if (target.HealthPercent <
                                                MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.Health", true)
                                                    .GetValue<Slider>()
                                                    .Value)
                                            {
                                                if (target.IsValidTarget(_r.Range, false))
                                                {
                                                    _r.CastOnUnit(target);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
            {
                if (_w.IsReadyPerfectly())
                {
                    if (gapcloser.Sender.IsValidTarget(_w.Range))
                    {
                        _w.CastOnUnit(gapcloser.Sender);
                    }
                }
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
            {
                if (_w.IsReadyPerfectly())
                {
                    if (sender.IsValidTarget(_w.Range))
                    {
                        _w.CastOnUnit(sender);
                    }
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
                    Render.Circle.DrawCircle(MyPix.Position, _q.Range, MenuProvider.Champion.Drawings.DrawQrange.Color);
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

            if (_e.IsReadyPerfectly())
            {
                damage += _e.GetDamage(enemy);
            }

            return damage;
        }

        private void QLogic(bool killstealOnly = false)
        {
            _qTargets.Clear();

            foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(_q.Range)))
            {
                _qTargets.Add(new QTarget {Target = target, Pix = false});
            }

            foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(_q.Range, true, MyPix.Position)))
            {
                _qTargets.Add(new QTarget {Target = target, Pix = true});
            }

            var killableTarget =
                _qTargets.FirstOrDefault(x => x.Target.IsKillableAndValidTarget(_q.GetDamage(x.Target), _q.DamageType));

            if (killableTarget != null)
            {
                if (killableTarget.Pix)
                {
                    _q2.Cast(killableTarget.Target, false, true);
                }
                else
                {
                    _q.Cast(killableTarget.Target, false, true);
                }
            }
            else if (!killstealOnly)
            {
                var bestTarget = _qTargets.OrderByDescending(x => TargetSelector.GetPriority(x.Target)).FirstOrDefault();

                if (bestTarget != null)
                {
                    if (bestTarget.Pix)
                    {
                        _q2.Cast(bestTarget.Target, false, true);
                    }
                    else
                    {
                        _q.Cast(bestTarget.Target, false, true);
                    }
                }
            }
        }
    }
}