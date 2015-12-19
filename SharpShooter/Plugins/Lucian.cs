using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Lucian
    {
        private bool _hasPassive;
        private readonly Spell _q;
        private readonly Spell _qExtended;
        private readonly Spell _w;
        private readonly Spell _wNoCollision;
        private readonly Spell _e;
        private readonly Spell _r;

        public Lucian()
        {
            _q = new Spell(SpellSlot.Q, 675f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W, 1000f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _e = new Spell(SpellSlot.E, 475f);
            _r = new Spell(SpellSlot.R, 1400f);
            _qExtended = new Spell(SpellSlot.Q, 1100f, TargetSelector.DamageType.Physical);
            _wNoCollision = new Spell(SpellSlot.W, 1000f, TargetSelector.DamageType.Physical);

            _qExtended.SetSkillshot(0.5f, 65f, float.MaxValue, false, SkillshotType.SkillshotLine);
            _w.SetSkillshot(0.30f, 55f, 1600f, true, SkillshotType.SkillshotLine);
            _wNoCollision.SetSkillshot(0.30f, 55f, 1600f, false, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW();
            MenuProvider.Champion.Harass.AddAutoHarass();
            MenuProvider.Champion.Harass.AddIfMana();

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddItem("Use Anti-Melee (E)", true);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnBuffRemove += Obj_AI_Base_OnBuffRemove;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;

            Console.WriteLine("Sharpshooter: Lucian Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Lucian</font> Loaded.");
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
                if (Orbwalking.CanMove(100))
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (!ObjectManager.Player.IsDashing())
                                        if (_hasPassive == false)
                                        {
                                            var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                            if (target != null)
                                                _q.CastOnUnit(target);
                                            else
                                            {
                                                var extendedTarget = TargetSelector.GetTarget(_qExtended.Range,
                                                    _q.DamageType);
                                                if (extendedTarget != null)
                                                {
                                                    var minions =
                                                        MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                                                            _q.Range, MinionTypes.All, MinionTeam.NotAlly);
                                                    foreach (var minion in minions)
                                                    {
                                                        var box =
                                                            new Geometry.Polygon.Rectangle(
                                                                ObjectManager.Player.ServerPosition,
                                                                ObjectManager.Player.ServerPosition.Extend(
                                                                    minion.ServerPosition, _qExtended.Range),
                                                                _qExtended.Width);
                                                        var prediction = _qExtended.GetPrediction(extendedTarget);
                                                        if (box.IsInside(prediction.UnitPosition))
                                                            if (prediction.Hitchance >= _q.MinHitChance)
                                                            {
                                                                _q.CastOnUnit(minion);
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var killableTarget =
                                                HeroManager.Enemies.FirstOrDefault(
                                                    x =>
                                                        x.IsKillableAndValidTarget(_q.GetDamage(x),
                                                            TargetSelector.DamageType.Physical, _q.Range));
                                            if (killableTarget != null)
                                                _q.CastOnUnit(killableTarget);
                                        }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                    if (!ObjectManager.Player.IsDashing())
                                        if (_hasPassive == false)
                                        {
                                            if (HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                                            {
                                                var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);
                                                if (target != null)
                                                    _wNoCollision.Cast(target, false, true);
                                            }
                                            else
                                            {
                                                var target = TargetSelector.GetTargetNoCollision(_w);
                                                if (target != null)
                                                    _w.Cast(target);
                                            }
                                        }
                                        else
                                        {
                                            var killableTarget =
                                                HeroManager.Enemies.FirstOrDefault(
                                                    x =>
                                                        x.IsKillableAndValidTarget(_w.GetDamage(x),
                                                            TargetSelector.DamageType.Physical, _w.Range) &&
                                                        _w.GetPrediction(x).Hitchance >= HitChance.High);
                                            if (killableTarget != null)
                                                _w.Cast(killableTarget);
                                        }

                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (_hasPassive == false)
                                    if (_q.IsReadyPerfectly())
                                        if (!ObjectManager.Player.IsDashing())
                                            if (
                                                ObjectManager.Player.IsManaPercentOkay(
                                                    MenuProvider.Champion.Harass.IfMana))
                                            {
                                                var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                                if (target != null)
                                                    _q.CastOnUnit(target);
                                                else
                                                {
                                                    var extendedTarget = TargetSelector.GetTarget(_qExtended.Range,
                                                        _q.DamageType);
                                                    if (extendedTarget != null)
                                                    {
                                                        var minions =
                                                            MinionManager.GetMinions(
                                                                ObjectManager.Player.ServerPosition, _q.Range,
                                                                MinionTypes.All, MinionTeam.NotAlly);
                                                        foreach (var minion in minions)
                                                        {
                                                            var box =
                                                                new Geometry.Polygon.Rectangle(
                                                                    ObjectManager.Player.ServerPosition,
                                                                    ObjectManager.Player.ServerPosition.Extend(
                                                                        minion.ServerPosition, _qExtended.Range),
                                                                    _qExtended.Width);
                                                            var prediction = _qExtended.GetPrediction(extendedTarget);
                                                            if (box.IsInside(prediction.UnitPosition))
                                                                if (prediction.Hitchance >= _q.MinHitChance)
                                                                {
                                                                    _q.CastOnUnit(minion);
                                                                    break;
                                                                }
                                                        }
                                                    }
                                                }
                                            }

                            if (MenuProvider.Champion.Harass.UseW)
                                if (_w.IsReadyPerfectly())
                                    if (!ObjectManager.Player.IsDashing())
                                        if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                            if (_hasPassive == false)
                                            {
                                                if (HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                                                {
                                                    var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);
                                                    if (target != null)
                                                        _wNoCollision.Cast(target, false, true);
                                                }
                                                else
                                                {
                                                    var target = TargetSelector.GetTargetNoCollision(_w);
                                                    if (target != null)
                                                        _w.Cast(target);
                                                }
                                            }
                                            else
                                            {
                                                var killableTarget =
                                                    HeroManager.Enemies.FirstOrDefault(
                                                        x =>
                                                            x.IsKillableAndValidTarget(_w.GetDamage(x),
                                                                TargetSelector.DamageType.Physical, _w.Range) &&
                                                            _w.GetPrediction(x).Hitchance >= HitChance.High);
                                                if (killableTarget != null)
                                                    _w.Cast(killableTarget);
                                            }

                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Laneclear
                            if (MenuProvider.Champion.Laneclear.UseQ)
                                if (_hasPassive == false)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                        if (_q.IsReadyPerfectly())
                                            if (!ObjectManager.Player.IsDashing())
                                            {
                                                var minions =
                                                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                                                        _q.Range);
                                                foreach (var minion in minions)
                                                {
                                                    var box =
                                                        new Geometry.Polygon.Rectangle(
                                                            ObjectManager.Player.ServerPosition,
                                                            ObjectManager.Player.ServerPosition.Extend(
                                                                minion.ServerPosition, _qExtended.Range),
                                                            _qExtended.Width);
                                                    if (minions.Count(x => box.IsInside(x.ServerPosition)) >= 3)
                                                    {
                                                        _q.CastOnUnit(minion);
                                                        break;
                                                    }
                                                }
                                            }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (_hasPassive == false)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                        if (_q.IsReadyPerfectly())
                                            if (!ObjectManager.Player.IsDashing())
                                            {
                                                var target =
                                                    MinionManager.GetMinions(_q.Range, MinionTypes.All,
                                                        MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                                                        .FirstOrDefault(x => x.IsValidTarget(_q.Range));
                                                if (target != null)
                                                    _q.CastOnUnit(target);
                                            }

                            if (MenuProvider.Champion.Jungleclear.UseW)
                                if (_hasPassive == false)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                        if (_w.IsReadyPerfectly())
                                            if (!ObjectManager.Player.IsDashing())
                                            {
                                                var target =
                                                    MinionManager.GetMinions(_w.Range, MinionTypes.All,
                                                        MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                                                        .FirstOrDefault(x => x.IsValidTarget(_w.Range));
                                                if (target != null)
                                                    _w.Cast(target);
                                            }
                            break;
                        }
                    }

                    if (MenuProvider.Champion.Harass.AutoHarass)
                        if (MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo &&
                            MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
                            if (!ObjectManager.Player.IsRecalling())
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (MenuProvider.Champion.Harass.UseQ)
                                    {
                                        var extendedTarget = TargetSelector.GetTarget(_qExtended.Range, _q.DamageType);
                                        if (extendedTarget != null)
                                            if (ObjectManager.Player.UnderTurret(true)
                                                ? !extendedTarget.UnderTurret(true)
                                                : true)
                                            {
                                                var minions =
                                                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                                                        _q.Range, MinionTypes.All, MinionTeam.NotAlly);
                                                foreach (var minion in minions)
                                                {
                                                    var box =
                                                        new Geometry.Polygon.Rectangle(
                                                            ObjectManager.Player.ServerPosition,
                                                            ObjectManager.Player.ServerPosition.Extend(
                                                                minion.ServerPosition, _qExtended.Range),
                                                            _qExtended.Width);
                                                    var prediction = _qExtended.GetPrediction(extendedTarget);
                                                    if (box.IsInside(prediction.UnitPosition))
                                                        if (prediction.Hitchance >= _q.MinHitChance)
                                                        {
                                                            _q.CastOnUnit(minion);
                                                            break;
                                                        }
                                                }
                                            }
                                    }
                }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E ||
                    args.Slot == SpellSlot.R)
                    _hasPassive = true;
            }

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

        private void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
                if (args.Animation == "Spell1" || args.Animation == "Spell2")
                    if (MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }


        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
                if (ObjectManager.Player.HasBuff("LucianR"))
                    args.Process = false;
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                _hasPassive = false;

                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                    {
                        if (MenuProvider.Champion.Combo.UseE)
                            if (_e.IsReadyPerfectly())
                                if (
                                    ObjectManager.Player.Position.Extend(Game.CursorPos, 700).CountEnemiesInRange(700) <=
                                    1)
                                    _e.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, 700));
                        break;
                    }
                }
            }
        }

        private void Obj_AI_Base_OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsMe)
                if (args.Buff.Name == "lucianpassivebuff")
                    _hasPassive = false;
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
                damage += (float) ObjectManager.Player.GetAutoAttackDamage(enemy)*0.5f;
            }

            if (_w.IsReadyPerfectly())
            {
                damage += _w.GetDamage(enemy);
                damage += (float) ObjectManager.Player.GetAutoAttackDamage(enemy)*1.5f;
            }

            if (_e.IsReadyPerfectly())
            {
                damage += (float) ObjectManager.Player.GetAutoAttackDamage(enemy)*1.5f;
            }

            return damage;
        }
    }
}