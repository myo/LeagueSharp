using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Karthus
    {
        private int _lastPingTime;
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Karthus()
        {
            _q = new Spell(SpellSlot.Q, 875f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W, 1000f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.VeryHigh};
            _e = new Spell(SpellSlot.E, 520f);
            _r = new Spell(SpellSlot.R);

            _q.SetSkillshot(1.0f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _w.SetSkillshot(0.25f, 1f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddItem("Disable AutoAttack", true);

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW(false);
            MenuProvider.Champion.Harass.AddUseE(false);
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Lasthit.AddUseQ();
            MenuProvider.Champion.Lasthit.AddIfMana();

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddQHitchanceSelector(HitChance.VeryHigh);
            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddItem("Ping Notify on R Killable Targets", true);
            MenuProvider.Champion.Misc.AddItem("Use E Humanizer", true);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddItem("Draw R Killable Mark", new Circle(true, Color.GreenYellow));
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Console.WriteLine("Sharpshooter: Karthus Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Karthus</font> Loaded.");
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
                                    var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                    if (target != null)
                                        _q.Cast(target, false, true);
                                }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);
                                    if (target != null)
                                        _w.Cast(target, false, true);
                                }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                    ESwitch(ObjectManager.Player.CountEnemiesInRange(_e.Range) > 0);
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (_q.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                    if (target != null)
                                        _q.Cast(target, false, true);
                                }

                            if (MenuProvider.Champion.Harass.UseW)
                                if (_w.IsReadyPerfectly())
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);
                                        if (target != null)
                                            _w.Cast(target, false, true);
                                    }

                            if (MenuProvider.Champion.Harass.UseE)
                                if (_e.IsReadyPerfectly())
                                    ESwitch(ObjectManager.Player.CountEnemiesInRange(_e.Range) > 0);
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
                                        if (farmLocation.MinionsHit >= 1)
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
                        case Orbwalking.OrbwalkingMode.LastHit:
                        {
                            if (MenuProvider.Champion.Lasthit.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_q.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(_q.Range)
                                                .FirstOrDefault(
                                                    x =>
                                                        x.IsKillableAndValidTarget(_q.GetDamage(x, 1), _q.DamageType,
                                                            _q.Range));
                                        if (target != null)
                                            _q.Cast(target);
                                    }
                        }
                            break;
                    }
                }

                if (MenuProvider.Champion.Misc.GetBoolValue("Ping Notify on R Killable Targets"))
                    if (_r.IsReadyPerfectly())
                    {
                        if (Environment.TickCount - _lastPingTime >= 333)
                        {
                            foreach (
                                var target in
                                    HeroManager.Enemies.Where(
                                        x => x.IsKillableAndValidTarget(_r.GetDamage(x), _r.DamageType)))
                                Game.ShowPing(PingCategory.Normal, target.Position, true);

                            _lastPingTime = Environment.TickCount;
                        }
                    }
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                    {
                        if (MenuProvider.Champion.Combo.GetBoolValue("Disable AutoAttack"))
                            args.Process = false;
                        else if (MenuProvider.Champion.Combo.UseQ)
                            if (_q.IsReadyPerfectly())
                                args.Process = false;
                        break;
                    }
                }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                    if (gapcloser.Sender.IsValidTarget())
                        if (_w.IsReadyPerfectly())
                            _w.Cast(gapcloser.Sender);
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

                var drawRKillableMark = MenuProvider.Champion.Drawings.GetCircleValue("Draw R Killable Mark");
                if (drawRKillableMark.Active)
                    if (_r.IsReadyPerfectly())
                    {
                        foreach (
                            var target in
                                HeroManager.Enemies.Where(
                                    x => x.IsKillableAndValidTarget(_r.GetDamage(x), _r.DamageType)))
                        {
                            var targetPos = Drawing.WorldToScreen(target.Position);
                            Render.Circle.DrawCircle(target.Position, target.BoundingRadius, drawRKillableMark.Color);
                            Drawing.DrawText(targetPos.X, targetPos.Y - 20, drawRKillableMark.Color, "R Killable");
                        }
                    }
            }
        }

        private void ESwitch(bool activate)
        {
            if (activate)
            {
                if (_e.Instance.ToggleState == 1)
                    _e.Cast();
            }
            else
            {
                Utility.DelayAction.Add(
                    MenuProvider.Champion.Misc.GetBoolValue("Use E Humanizer") ? new Random().Next(300, 1000) : 20,
                    () =>
                    {
                        if (_e.Instance.ToggleState != 1)
                            _e.Cast();
                    });
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