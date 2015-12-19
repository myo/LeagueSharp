using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class MissFortune
    {
        private const string RBuffName = "missfortunebulletsound";
        private bool _iWantToCancelR;
        private int _loveTapTargetNetworkId = -1;
        private bool _pressed;
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;


        public MissFortune()
        {
            _q = new Spell(SpellSlot.Q, 650f, TargetSelector.DamageType.Physical);
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 1000f);
            _r = new Spell(SpellSlot.R, 1400f);

            _q.SetTargetted(0.25f, 1400f);
            _e.SetSkillshot(0.5f, 100f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddItem("Use Q2", true);
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();

            MenuProvider.Champion.Harass.AddUseQ(false);
            MenuProvider.Champion.Harass.AddItem("Use Q2", true);
            MenuProvider.Champion.Harass.AddUseE(false);
            MenuProvider.Champion.Harass.AddAutoHarass();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseE(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseE(false);
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddItem("Harass Q2 Only if Kills Unit", false);
            MenuProvider.Champion.Misc.AddItem("Making new AutoAttack Target for Passive (LoveTap)", true);
            MenuProvider.Champion.Misc.AddItem("Block Movement order While Using R", true);
            MenuProvider.Champion.Misc.AddItem("Cancel R", new KeyBind('R', KeyBindType.Press));

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddItem("Draw R Killable", new Circle(true, Color.GreenYellow));
            MenuProvider.Champion.Drawings.AddItem("Draw Q Cone",
                new Circle(true, Color.FromArgb(150, Color.GreenYellow)));
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;

            Console.WriteLine("Sharpshooter: MissFortune Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">MissFortune</font> Loaded.");
        }

        private bool UsingR => ObjectManager.Player.HasBuff(RBuffName);

        private void Game_OnUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (Orbwalking.CanMove(100) && !UsingR)
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (_q.IsReadyPerfectly())
                            {
                                if (MenuProvider.Champion.Combo.GetBoolValue("Use Q2"))
                                {
                                    Q2Logic();
                                }

                                if (MenuProvider.Champion.Combo.UseQ)
                                {
                                    var shortRangeTarget = TargetSelector.GetTarget(_q.Range, _q.DamageType);

                                    if (shortRangeTarget != null)
                                        _q.CastOnUnit(shortRangeTarget);
                                }
                            }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                    if (target != null)
                                        _e.Cast(target, false, true);
                                }

                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                if (_q.IsReadyPerfectly())
                                {
                                    if (MenuProvider.Champion.Harass.GetBoolValue("Use Q2"))
                                    {
                                        Q2Logic();
                                    }

                                    if (MenuProvider.Champion.Harass.UseQ)
                                    {
                                        var shortRangeTarget = TargetSelector.GetTarget(_q.Range, _q.DamageType);

                                        if (shortRangeTarget != null)
                                            _q.CastOnUnit(shortRangeTarget);
                                    }
                                }

                            if (MenuProvider.Champion.Harass.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                        if (target != null)
                                            _e.Cast(target, false, true);
                                    }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Laneclear
                            if (MenuProvider.Champion.Laneclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var farmLocation = _e.GetCircularFarmLocation(MinionManager.GetMinions(_e.Range));
                                        if (farmLocation.MinionsHit >= 4)
                                            _e.Cast(farmLocation.Position);
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

                            if (MenuProvider.Champion.Jungleclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(600));
                                        if (target != null)
                                            _e.Cast(target);
                                    }
                            break;
                        }
                    }

                    if (MenuProvider.Champion.Harass.AutoHarass)
                        if (MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo &&
                            MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
                            if (!ObjectManager.Player.IsRecalling())
                            {
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_q.IsReadyPerfectly())
                                    {
                                        if (MenuProvider.Champion.Harass.GetBoolValue("Use Q2"))
                                        {
                                            Q2Logic();
                                        }

                                        if (MenuProvider.Champion.Harass.UseQ)
                                        {
                                            var shortRangeTarget = TargetSelector.GetTarget(_q.Range, _q.DamageType);

                                            if (shortRangeTarget != null)
                                                _q.CastOnUnit(shortRangeTarget);
                                        }
                                    }
                            }
                }

                if (MenuProvider.Champion.Misc.GetKeyBindValue("Cancel R").Active)
                {
                    if (!_pressed)
                    {
                        if (UsingR)
                        {
                            _iWantToCancelR = true;
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                    }

                    _pressed = true;
                }
                else
                {
                    _pressed = false;
                }
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        if (_w.IsReadyPerfectly())
                            if (MenuProvider.Champion.Combo.UseW)
                                if (Orbwalking.InAutoAttackRange(args.Target))
                                    _w.Cast();
                        break;
                }

                if (MenuProvider.Champion.Misc.GetBoolValue("Making new AutoAttack Target for Passive (LoveTap)"))
                {
                    if (args.Target.Type == GameObjectType.obj_AI_Hero)
                    {
                        if (args.Target.Health + args.Target.PhysicalShield >
                            ObjectManager.Player.GetAutoAttackDamage(args.Target as Obj_AI_Base, true)*2)
                        {
                            if (args.Target.NetworkId == _loveTapTargetNetworkId)
                            {
                                var newTarget =
                                    HeroManager.Enemies.Where(
                                        x =>
                                            x.IsValidTarget() && Orbwalking.InAutoAttackRange(x) &&
                                            x.NetworkId != _loveTapTargetNetworkId)
                                        .OrderByDescending(x => TargetSelector.GetPriority(x))
                                        .FirstOrDefault();

                                if (newTarget != null)
                                {
                                    args.Process = false;
                                    MenuProvider.Orbwalker.ForceTarget(newTarget);
                                }
                                else
                                {
                                    MenuProvider.Orbwalker.ForceTarget(null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MenuProvider.Orbwalker.ForceTarget(null);
                }
            }
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                if (target.IsValidTarget())
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            if (target.Type == GameObjectType.obj_AI_Hero)
                            {
                                if (MenuProvider.Champion.Combo.UseQ)
                                    if (_q.IsReadyPerfectly())
                                        _q.CastOnUnit(target as Obj_AI_Base);
                            }
                            break;
                        case Orbwalking.OrbwalkingMode.Mixed:
                            if (target.Type == GameObjectType.obj_AI_Hero)
                            {
                                if (MenuProvider.Champion.Harass.UseQ)
                                    if (_q.IsReadyPerfectly())
                                        _q.CastOnUnit(target as Obj_AI_Base);
                            }
                            break;
                    }
                }

                _loveTapTargetNetworkId = target.NetworkId;
            }
        }

        private void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Buff.Name == RBuffName)
                {
                    _iWantToCancelR = false;
                }
            }
        }

        private void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe)
            {
                if (MenuProvider.Champion.Misc.GetBoolValue("Block Movement order While Using R"))
                {
                    if (UsingR)
                    {
                        if (!_iWantToCancelR)
                        {
                            args.Process = false;
                        }
                    }
                }
            }
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

                if (MenuProvider.Champion.Drawings.DrawRrange.Active && _r.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range,
                        MenuProvider.Champion.Drawings.DrawRrange.Color);

                var drawRKillable = MenuProvider.Champion.Drawings.GetCircleValue("Draw R Killable");
                if (drawRKillable.Active && _r.IsReadyPerfectly())
                    foreach (
                        var target in
                            HeroManager.Enemies.Where(
                                x => x.IsKillableAndValidTarget(float.MaxValue, TargetSelector.DamageType.Physical)))
                    {
                        var targetPos = Drawing.WorldToScreen(target.Position);
                        var rDamage = _r.GetDamage(target);
                        var rWave = 10 + 2*_r.Level;

                        for (var i = 1; i < rWave; i++)
                        {
                            if (target.Health + target.PhysicalShield + target.HPRegenRate < rDamage*i)
                            {
                                Render.Circle.DrawCircle(target.Position, target.BoundingRadius, drawRKillable.Color);
                                Drawing.DrawText(targetPos.X, targetPos.Y - 20, drawRKillable.Color,
                                    "R Killable" + " (with " + i + " waves)");
                                break;
                            }
                        }
                    }

                var drawQCone = MenuProvider.Champion.Drawings.GetCircleValue("Draw Q Cone");
                if (drawQCone.Active)
                {
                    var targets = new List<Obj_AI_Base>();
                    targets.AddRange(
                        MinionManager.GetMinions(_q.Range, MinionTypes.All, MinionTeam.NotAlly)
                            .Where(x => x.IsValidTarget()));
                    targets.AddRange(HeroManager.Enemies.Where(x => x.IsValidTarget(_q.Range)));
                    targets.OrderBy(x => x.Health);

                    foreach (var target in targets)
                    {
                        var direction = target.ServerPosition.Extend(ObjectManager.Player.ServerPosition, -500);
                        var radian = (float) Math.PI/180f;
                        var targetPosition = target.Position;

                        new Geometry.Polygon.Sector(targetPosition, direction, 40f*radian, 450f).Draw(drawQCone.Color);
                    }
                }

                if (UsingR)
                {
                    var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                    Drawing.DrawText(playerPos.X, playerPos.Y - 20, drawRKillable.Color,
                        "Press " + Convert.ToChar(MenuProvider.Champion.Misc.GetKeyBindValue("Cancel R").Key) +
                        " if you want to cancel R");
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

            if (_r.IsReadyPerfectly())
            {
                damage += _r.GetDamage(enemy)*(10 + 2*_r.Level);
            }

            return damage;
        }

        private bool Q2Logic()
        {
            /*
            --Not Trusted-- http://leagueoflegends.wikia.com/wiki/Miss_Fortune
            The second shot follows a priority order on targets within 500 units of the primary target:
            Enemy champions in a 40° cone marked by Love Tap.
            Minions and neutral monsters within a 40° cone.
            Enemy champions within a 40° cone.
            Enemy or neutral units within a 110° cone.
            Enemy or neutral units within a 150-range 160° cone.
            Double Up's range is not listed as spell range, but instead matches her basic attack range.
            Double Up can bounce to units in brush or fog of war if they are in range of the target the spell is initially cast on.
            Double Up must kill the first target for the second hit to deal enhanced damage. If the first target dies before the first hit lands, the second target receives the normal second target bonus damage.
            */
            if (_q.IsReadyPerfectly())
            {
                var q2Range = _q.Range + 450;
                var longRangeTarget = TargetSelector.GetTarget(q2Range, _q.DamageType);

                if (longRangeTarget != null)
                {
                    Obj_AI_Base bestTarget = null;

                    var targets = new List<Obj_AI_Base>();
                    targets.AddRange(
                        MinionManager.GetMinions(_q.Range, MinionTypes.All, MinionTeam.NotAlly)
                            .Where(x => x.IsValidTarget()));
                    targets.AddRange(HeroManager.Enemies.Where(x => x.IsValidTarget(_q.Range)));
                    targets.OrderBy(x => x.Health);

                    foreach (
                        var target in
                            MenuProvider.Champion.Misc.GetBoolValue("Harass Q2 Only if Kills Unit")
                                ? targets.Where(
                                    x => x.IsKillableAndValidTarget(_q.GetDamage(x), _q.DamageType, _q.Range))
                                : targets)
                    {
                        var direction = target.ServerPosition.Extend(ObjectManager.Player.ServerPosition, -500);
                        var radian = (float) Math.PI/180f;
                        var targetServerPosition = target.ServerPosition;
                        var time = ObjectManager.Player.ServerPosition.Distance(target.ServerPosition)/_q.Speed +
                                   _q.Delay;
                        var predic = Prediction.GetPrediction(longRangeTarget, time);

                        var cone40 = new Geometry.Polygon.Sector(targetServerPosition, direction, 40f*radian, 450f);

                        if (cone40.IsInside(longRangeTarget.ServerPosition))
                        {
                            if (cone40.IsInside(predic.UnitPosition))
                            {
                                if (longRangeTarget.NetworkId == _loveTapTargetNetworkId)
                                {
                                    bestTarget = target;
                                    break;
                                }
                                if (
                                    !MinionManager.GetMinions(q2Range)
                                        .Where(
                                            x =>
                                                x.IsValidTarget() && x.NetworkId != target.NetworkId &&
                                                cone40.IsInside(x))
                                        .Any(
                                            x =>
                                                target.ServerPosition.Distance(longRangeTarget.ServerPosition) >=
                                                target.ServerPosition.Distance(x.ServerPosition)))
                                {
                                    bestTarget = target;
                                    break;
                                }
                            }
                        }
                    }

                    if (bestTarget != null)
                    {
                        return _q.CastOnUnit(bestTarget);
                    }
                    return false;
                }
                return false;
            }
            return false;
        }
    }
}