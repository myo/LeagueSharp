using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Ashe
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private Spell _e;
        private readonly Spell _r;

        public Ashe()
        {
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W, 1200f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _e = new Spell(SpellSlot.E);
            _r = new Spell(SpellSlot.R, 2500f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.High};

            _w.SetSkillshot(0.25f, 60f, 1500f, true, SkillshotType.SkillshotLine);
            _r.SetSkillshot(0.25f, 130f, 1600f, false, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseW();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseW(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();
            MenuProvider.Champion.Misc.AddItem("Auto R on immobile targets", true);

            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Console.WriteLine("Sharpshooter: Ashe Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Ashe</font> Loaded.");
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
                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTargetNoCollision(_w);
                                    if (target.IsValidTarget(_w.Range))
                                        _w.Cast(target);
                                }

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                    foreach (
                                        var target in
                                            HeroManager.Enemies.Where(
                                                x =>
                                                    x.IsValidTarget(_r.Range) &&
                                                    _r.GetPrediction(x).Hitchance >= HitChance.High))
                                    {
                                        //R Logics
                                        if (
                                            target.IsKillableAndValidTarget(_r.GetDamage(target),
                                                TargetSelector.DamageType.Physical, _r.Range) &&
                                            !Orbwalking.InAutoAttackRange(target))
                                            _r.Cast(target); //killable

                                        if (target.IsValidTarget(600f))
                                            _r.Cast(target); //too close

                                        if (target.IsImmobileUntil() >
                                            target.ServerPosition.Distance(ObjectManager.Player.ServerPosition)/_r.Speed)
                                            _r.Cast(target); //immobile
                                    }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTargetNoCollision(_w);
                                        if (target.IsValidTarget(_w.Range))
                                            _w.Cast(target);
                                    }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Laneclear
                            if (MenuProvider.Champion.Laneclear.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var farmLocation = _w.GetLineFarmLocation(MinionManager.GetMinions(_w.Range));
                                        if (farmLocation.MinionsHit >= 1)
                                            _w.Cast(farmLocation.Position);
                                    }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth)
                                                .FirstOrDefault(
                                                    x =>
                                                        x.IsValidTarget(600) &&
                                                        _w.GetPrediction(x).Hitchance >= HitChance.High);
                                        if (target != null)
                                            _w.Cast(target);
                                    }
                            break;
                        }
                    }
                }

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto R on immobile targets"))
                    if (_r.IsReadyPerfectly())
                    {
                        var rTarget =
                            HeroManager.Enemies.FirstOrDefault(
                                x =>
                                    _r.GetPrediction(x).Hitchance >= HitChance.High && x.IsValidTarget(_r.Range) &&
                                    x.IsImmobileUntil() > x.Distance(ObjectManager.Player.ServerPosition)/_r.Speed);
                        if (rTarget != null)
                            _r.Cast(rTarget);
                    }
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
                                if (ObjectManager.Player.HasBuff("asheqcastready"))
                                    if (_q.IsReadyPerfectly())
                                        _q.Cast();
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (ObjectManager.Player.HasBuff("asheqcastready"))
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
                if (gapcloser.Sender.ChampionName.ToLowerInvariant() != "masteryi")
                    if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                        if (gapcloser.Sender.IsValidTarget(_r.Range))
                            if (_r.IsReadyPerfectly())
                                _r.Cast(gapcloser.Sender);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (args.DangerLevel >= Interrupter2.DangerLevel.High)
                    if (sender.IsValidTarget(_r.Range))
                        if (_r.IsReadyPerfectly())
                            _r.Cast(sender);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.Champion.Drawings.DrawWrange.Active && _w.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range,
                        MenuProvider.Champion.Drawings.DrawWrange.Color);

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

            if (_r.IsReadyPerfectly())
            {
                damage += _r.GetDamage(enemy);
            }

            return damage;
        }
    }
}