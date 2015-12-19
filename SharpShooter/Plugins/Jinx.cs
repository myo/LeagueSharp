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
    public class Jinx
    {
        private const int DefaultRange = 525;
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;
        private int _wCastTime;

        public Jinx()
        {
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W, 1450f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.VeryHigh};
            _e = new Spell(SpellSlot.E, 900f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _r = new Spell(SpellSlot.R, 2500f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};

            _w.SetSkillshot(0.6f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            _e.SetSkillshot(1.1f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            _r.SetSkillshot(0.6f, 140f, 1700f, false, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddItem("Switch to Rocket If will hit enemy Number >=", new Slider(3, 2, 6));
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW();
            MenuProvider.Champion.Harass.AddAutoHarass();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Lasthit.AddUseQ();
            MenuProvider.Champion.Lasthit.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ();
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddWHitchanceSelector(HitChance.VeryHigh);
            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();
            MenuProvider.Champion.Misc.AddItem("Auto E on Immobile Target", true);
            MenuProvider.Champion.Misc.AddItem("Auto R on Killable Target", true);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddItem("Draw Rocket explosion range on AutoAttack Target", true);
            MenuProvider.Champion.Drawings.AddItem("Draw R Killable", new Circle(true, Color.GreenYellow));
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Console.WriteLine("Sharpshooter: Jinx Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Jinx</font> Loaded.");
        }

        private int GetQRange
        {
            get { return DefaultRange + 25*_q.Level; }
        }

        private bool IsQActive
        {
            get { return ObjectManager.Player.HasBuff("JinxQ"); }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            _w.MinHitChance = MenuProvider.Champion.Misc.WSelectedHitchance;

            if (!ObjectManager.Player.IsDead)
            {
                if (Orbwalking.CanMove(100))
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (MenuProvider.Champion.Combo.UseQ)
                            {
                                if (_q.IsReadyPerfectly())
                                    if (ObjectManager.Player.CountEnemiesInRange(2000f) > 0)
                                    {
                                        var target =
                                            HeroManager.Enemies.Where(
                                                x =>
                                                    x.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange(x,
                                                        GetQRange)))
                                                .OrderByDescending(a => TargetSelector.GetPriority(a))
                                                .FirstOrDefault();
                                        if (target != null)
                                        {
                                            if (target.CountEnemiesInRange(200) >=
                                                MenuProvider.Champion.Combo.GetSliderValue(
                                                    "Switch to Rocket If will hit enemy Number >=").Value)
                                                QSwitch(true);
                                            else
                                                QSwitch(
                                                    !target.IsValidTarget(
                                                        ObjectManager.Player.GetRealAutoAttackRange(target, DefaultRange)));
                                        }
                                        else
                                            QSwitch(true);
                                    }
                                    else
                                        QSwitch(false);
                            }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (ObjectManager.Player.CountEnemiesInRange(400f) == 0)
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTargetNoCollision(_w);
                                        if (target.IsValidTarget(_w.Range))
                                            _w.Cast(target);
                                    }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var target =
                                        HeroManager.Enemies.Where(
                                            x =>
                                                x.IsValidTarget(600) && _e.GetPrediction(x).Hitchance >= _e.MinHitChance &&
                                                x.IsMoving)
                                            .OrderBy(x => x.Distance(ObjectManager.Player))
                                            .FirstOrDefault();
                                    if (target != null)
                                        _e.Cast(target, false, true);
                                    else
                                        _e.CastWithExtraTrapLogic();
                                }

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                    if (_wCastTime + 1060 <= Environment.TickCount)
                                    {
                                        var target =
                                            HeroManager.Enemies.FirstOrDefault(
                                                x =>
                                                    !x.IsZombie && x.CountAlliesInRange(500) < 2 &&
                                                    HealthPrediction.GetHealthPrediction(x, 5000) > 0 &&
                                                    ObjectManager.Player.Distance(x) >= GetQRange &&
                                                    x.IsKillableAndValidTarget(GetRDamage(x),
                                                        TargetSelector.DamageType.Physical, _r.Range) &&
                                                    _r.GetPrediction(x).Hitchance >= HitChance.High);
                                        if (target != null)
                                        {
                                            var prediction = _r.GetPrediction(target);
                                            var collision =
                                                Collision.GetCollision(new List<Vector3> {prediction.UnitPosition},
                                                    new PredictionInput
                                                    {
                                                        UseBoundingRadius = true,
                                                        Unit = ObjectManager.Player,
                                                        Delay = _r.Delay,
                                                        Speed = _r.Speed,
                                                        Radius = 200,
                                                        CollisionObjects = new[] {CollisionableObjects.Heroes}
                                                    })
                                                    .Any(x => x.NetworkId != target.NetworkId);
                                            if (!collision)
                                                _r.Cast(target);
                                        }
                                    }

                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                            {
                                if (_q.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                        if (ObjectManager.Player.CountEnemiesInRange(2000f) > 0)
                                        {
                                            var target =
                                                HeroManager.Enemies.Where(
                                                    x =>
                                                        x.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange(x,
                                                            GetQRange)))
                                                    .OrderByDescending(a => TargetSelector.GetPriority(a))
                                                    .FirstOrDefault();
                                            QSwitch(
                                                !target.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange(
                                                    target, DefaultRange)));
                                        }
                                        else
                                            QSwitch(false);
                                    else
                                        QSwitch(false);
                            }
                            else
                                QSwitch(false);

                            if (MenuProvider.Champion.Harass.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (ObjectManager.Player.CountEnemiesInRange(400f) == 0)
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
                            if (MenuProvider.Champion.Laneclear.UseQ)
                            {
                                if (_q.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    {
                                    }
                                    else
                                        QSwitch(false);
                            }
                            else
                                QSwitch(false);

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
                                                        _w.GetPrediction(x).Hitchance >= _w.MinHitChance);
                                        if (target != null)
                                            _w.Cast(target);
                                    }

                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LastHit:
                        {
                            if (MenuProvider.Champion.Lasthit.UseQ)
                            {
                                if (_q.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Lasthit.IfMana))
                                    {
                                        var target =
                                            MinionManager.GetMinions(float.MaxValue)
                                                .FirstOrDefault(
                                                    x =>
                                                        x.IsKillableAndValidTarget(
                                                            ObjectManager.Player.GetAutoAttackDamage(x, false) +
                                                            _q.GetDamage(x), TargetSelector.DamageType.Physical) &&
                                                        x.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange(x,
                                                            GetQRange)) &&
                                                        !x.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange(x,
                                                            DefaultRange)));
                                        if (target != null)
                                        {
                                            QSwitch(true);

                                            if (Orbwalking.InAutoAttackRange(target))
                                                MenuProvider.Orbwalker.ForceTarget(target);
                                        }
                                        else
                                            QSwitch(false);
                                    }
                                    else
                                        QSwitch(false);
                            }
                            else
                                QSwitch(false);

                            break;
                        }
                    }

                    if (MenuProvider.Champion.Misc.GetBoolValue("Auto R on Killable Target"))
                    {
                        if (_r.IsReadyPerfectly())
                            if (_wCastTime + 1060 <= Environment.TickCount)
                            {
                                var target =
                                    HeroManager.Enemies.FirstOrDefault(
                                        x =>
                                            !x.IsZombie && x.CountAlliesInRange(500) < 2 &&
                                            HealthPrediction.GetHealthPrediction(x, 5000) > 0 &&
                                            ObjectManager.Player.Distance(x) >= GetQRange &&
                                            x.IsKillableAndValidTarget(GetRDamage(x), TargetSelector.DamageType.Physical,
                                                _r.Range) && _r.GetPrediction(x).Hitchance >= HitChance.High);
                                if (target != null)
                                {
                                    var prediction = _r.GetPrediction(target);
                                    var collision =
                                        Collision.GetCollision(new List<Vector3> {prediction.UnitPosition},
                                            new PredictionInput
                                            {
                                                Unit = ObjectManager.Player,
                                                Delay = _r.Delay,
                                                Speed = _r.Speed,
                                                Radius = _r.Width,
                                                CollisionObjects = new[] {CollisionableObjects.Heroes}
                                            })
                                            .Any(x => x.NetworkId != target.NetworkId);
                                    if (!collision)
                                        _r.Cast(target);
                                }
                            }
                    }

                    if (MenuProvider.Champion.Misc.GetBoolValue("Auto E on Immobile Target"))
                        if (_e.IsReadyPerfectly())
                        {
                            var target =
                                HeroManager.Enemies.FirstOrDefault(
                                    x => x.IsValidTarget(_e.Range) && x.IsImmobileUntil() > 0.5f);
                            if (target != null)
                                _e.Cast(target, false, true);
                        }

                    if (MenuProvider.Champion.Harass.AutoHarass)
                        if (MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                            if (!ObjectManager.Player.IsRecalling())
                                if (MenuProvider.Champion.Harass.UseW)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    {
                                        var target = TargetSelector.GetTargetNoCollision(_w);
                                        if (target != null)
                                            if (ObjectManager.Player.UnderTurret(true)
                                                ? !target.UnderTurret(true)
                                                : true)
                                                _w.Cast(target);
                                    }
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (_e.IsReadyPerfectly())
                    if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                        _e.Cast(gapcloser.End);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (_e.IsReadyPerfectly())
                    if (args.DangerLevel >= Interrupter2.DangerLevel.Medium)
                        _e.Cast(sender);
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                if (args.Slot == SpellSlot.W)
                    _wCastTime = Environment.TickCount;
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Mixed:
                        if (MenuProvider.Champion.Harass.UseQ)
                            if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                            {
                                if (
                                    args.Target.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange(args.Target,
                                        DefaultRange)))
                                    if (IsQActive)
                                    {
                                        QSwitch(false);
                                        args.Process = false;
                                    }
                            }
                            else
                                QSwitch(false);
                        else
                            QSwitch(false);

                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        if (MinionManager.GetMinions(float.MaxValue).Any(x => x.NetworkId == args.Target.NetworkId))
                        {
                            if (MenuProvider.Champion.Laneclear.UseQ)
                            {
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                {
                                    if (
                                        MinionManager.GetMinions(float.MaxValue)
                                            .Count(
                                                x =>
                                                    x.IsValidTarget(200, true, args.Target.Position) &&
                                                    (x.Health > ObjectManager.Player.GetAutoAttackDamage(x)*2 ||
                                                     x.Health <=
                                                     ObjectManager.Player.GetAutoAttackDamage(x) + _q.GetDamage(x))) >=
                                        2)
                                        QSwitch(true);
                                    else
                                        QSwitch(false);
                                }
                                else
                                    QSwitch(false);
                            }
                            else
                                QSwitch(false);
                        }
                        else if (
                            MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral,
                                MinionOrderTypes.MaxHealth).Any(x => x.NetworkId == args.Target.NetworkId))
                        {
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                            {
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                {
                                    if (
                                        MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral,
                                            MinionOrderTypes.MaxHealth)
                                            .Count(x => x.IsValidTarget(200, true, args.Target.Position)) >= 2)
                                        QSwitch(true);
                                    else
                                        QSwitch(false);
                                }
                                else
                                    QSwitch(false);
                            }
                            else
                                QSwitch(false);
                        }
                        else
                        {
                            QSwitch(false);
                        }
                        break;
                }
            }
        }


        private void Drawing_OnDraw(EventArgs args)
        {
            if (MenuProvider.Champion.Drawings.DrawQrange.Active && _q.IsReadyPerfectly() && !IsQActive)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, GetQRange + 185,
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

            if (MenuProvider.Champion.Drawings.GetBoolValue("Draw Rocket explosion range on AutoAttack Target"))
                if (IsQActive)
                {
                    var aaTarget = MenuProvider.Orbwalker.GetTarget();
                    if (aaTarget != null)
                        Render.Circle.DrawCircle(aaTarget.Position, 200, Color.Red, 4, true);
                }

            var drawRKillable = MenuProvider.Champion.Drawings.GetCircleValue("Draw R Killable");
            if (drawRKillable.Active && _r.IsReadyPerfectly())
                foreach (
                    var target in
                        HeroManager.Enemies.Where(
                            x => x.IsKillableAndValidTarget(GetRDamage(x), TargetSelector.DamageType.Physical)))
                {
                    var targetPos = Drawing.WorldToScreen(target.Position);
                    Render.Circle.DrawCircle(target.Position, target.BoundingRadius, drawRKillable.Color);
                    Drawing.DrawText(targetPos.X, targetPos.Y - 20, drawRKillable.Color, "R Killable");
                }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (!ObjectManager.Player.IsWindingUp)
            {
                damage += (float) ObjectManager.Player.GetAutoAttackDamage(enemy, true);
            }

            if (_w.IsReadyPerfectly())
            {
                damage += _w.GetDamage(enemy);
            }

            if (_r.IsReadyPerfectly())
            {
                damage += (float) GetRDamage(enemy);
            }

            return damage;
        }

        private void QSwitch(bool activate)
        {
            if (_q.IsReadyPerfectly())
                if (!ObjectManager.Player.IsWindingUp)
                    switch (activate)
                    {
                        case true:
                            if (!ObjectManager.Player.HasBuff("JinxQ"))
                                _q.Cast();
                            break;
                        case false:
                            if (ObjectManager.Player.HasBuff("JinxQ"))
                                _q.Cast();
                            break;
                    }
        }

        private double GetRDamage(Obj_AI_Base target)
        {
            return ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical,
                new double[] {0, 25, 30, 35}[_r.Level]/100*(target.MaxHealth - target.Health) +
                (new double[] {0, 25, 35, 45}[_r.Level] + 0.1*ObjectManager.Player.FlatPhysicalDamageMod)*
                Math.Min(1 + ObjectManager.Player.Distance(target.ServerPosition)/15*0.09d, 10));
        }
    }
}