using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    internal class AxeDropObjectData
    {
        internal int ExpireTime;
        internal GameObject Object;
    }

    public class Draven
    {
        private readonly List<AxeDropObjectData> _axeDropObjectDataList = new List<AxeDropObjectData>();
        private GameObject _bestDropObject;
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Draven()
        {
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 1000f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };
            _r = new Spell(SpellSlot.R, 2500f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };

            _e.SetSkillshot(0.25f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            _r.SetSkillshot(0.40f, 160f, 2000f, true, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW(false);
            MenuProvider.Champion.Harass.AddUseE(false);
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddUseE(false);
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseE();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();
            MenuProvider.Champion.Misc.AddItem("Auto Catch Axe", true);
            MenuProvider.Champion.Misc.AddItem("Axe Catch Range", new Slider(600, 0, 2000));

            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddItem("Draw Catch Axe Range", new Circle(true, Color.FromArgb(100, Color.YellowGreen)));
            MenuProvider.Champion.Drawings.AddItem("Draw Axe Drop Objects", true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;

            Console.WriteLine("Sharpshooter: Draven Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Draven</font> Loaded.");
        }

        private int AxeCount
        {
            get
            {
                var buff = ObjectManager.Player.GetBuff("dravenspinningattack");
                return buff == null ? 0 : buff.Count + _axeDropObjectDataList.Count(x => x.Object.IsValid);
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.Champion.Misc.GetBoolValue("Auto Catch Axe"))
                {
                    var bestObject =
                        _axeDropObjectDataList.Where(x => x.Object.IsValid)
                            .OrderBy(x => x.ExpireTime)
                            .FirstOrDefault(
                                x =>
                                    Game.CursorPos.Distance(x.Object.Position) <=
                                    MenuProvider.Champion.Misc.GetSliderValue("Axe Catch Range").Value);
                    if (bestObject != null)
                    {
                        _bestDropObject = bestObject.Object;
                        if (ObjectManager.Player.GetPath(bestObject.Object.Position).ToList().To2D().PathLength() /
                            ObjectManager.Player.MoveSpeed + Environment.TickCount >= bestObject.ExpireTime)
                        {
                            switch (MenuProvider.Orbwalker.ActiveMode)
                            {
                                case Orbwalking.OrbwalkingMode.Combo:
                                    if (MenuProvider.Champion.Combo.UseW)
                                        _w.Cast();
                                    break;

                                case Orbwalking.OrbwalkingMode.Mixed:
                                    if (MenuProvider.Champion.Harass.UseW)
                                        _w.Cast();
                                    break;
                            }
                        }

                        if (bestObject.Object.Position.Distance(ObjectManager.Player.Position) < 120)
                        {
                            if (MenuProvider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                            {
                                MenuProvider.Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                            }
                        }
                        else
                        {
                            if (MenuProvider.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
                                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, bestObject.Object.Position);
                            else
                                MenuProvider.Orbwalker.SetOrbwalkingPoint(bestObject.Object.Position);
                        }
                    }
                    else
                    {
                        _bestDropObject = null;
                        MenuProvider.Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    }
                }
                else
                    MenuProvider.Orbwalker.SetOrbwalkingPoint(Game.CursorPos);

                if (Orbwalking.CanMove(100))
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            {
                                if (MenuProvider.Champion.Combo.UseW)
                                    if (_w.IsReadyPerfectly())
                                        if (!ObjectManager.Player.HasBuff("dravenfurybuff"))
                                            if (HeroManager.Enemies.Any(x => x.IsValid && Orbwalking.InAutoAttackRange(x)))
                                                _w.Cast();

                                if (MenuProvider.Champion.Combo.UseE)
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                        if (target != null)
                                            _e.Cast(target);
                                    }

                                if (MenuProvider.Champion.Combo.UseR)
                                    if (_r.IsReadyPerfectly())
                                    {
                                        var target =
                                            HeroManager.Enemies.FirstOrDefault(
                                                x =>
                                                    !Orbwalking.InAutoAttackRange(x) &&
                                                    x.IsKillableAndValidTarget(_r.GetDamage(x) * 2,
                                                        TargetSelector.DamageType.Physical, _r.Range));
                                        if (target != null)
                                            _r.Cast(target);
                                    }
                                break;
                            }
                        case Orbwalking.OrbwalkingMode.Mixed:
                            {
                                if (MenuProvider.Champion.Harass.UseW)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                        if (_w.IsReadyPerfectly())
                                            if (!ObjectManager.Player.HasBuff("dravenfurybuff"))
                                                if (
                                                    HeroManager.Enemies.Any(
                                                        x => x.IsValid && Orbwalking.InAutoAttackRange(x)))
                                                    _w.Cast();

                                if (MenuProvider.Champion.Harass.UseE)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                        if (_e.IsReadyPerfectly())
                                        {
                                            var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                            if (target != null)
                                                _e.Cast(target);
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
                                            var farmLocation = _e.GetLineFarmLocation(MinionManager.GetMinions(_e.Range));
                                            if (farmLocation.MinionsHit >= 3)
                                                _e.Cast(farmLocation.Position);
                                        }

                                //Jungleclear
                                if (MenuProvider.Champion.Jungleclear.UseE)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                        if (_e.IsReadyPerfectly())
                                        {
                                            var target =
                                                MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.Neutral,
                                                    MinionOrderTypes.MaxHealth)
                                                    .FirstOrDefault(x => x.IsValidTarget(_e.Range));
                                            if (target != null)
                                                _e.Cast(target);
                                        }
                                break;
                            }
                    }
                }
            }
        }

        private void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (MenuProvider.Champion.Misc.GetBoolValue("Auto Catch Axe"))
                if (sender.IsMe)
                    if (args.Order == GameObjectOrder.MoveTo)
                        if (_bestDropObject != null)
                            if (_bestDropObject.IsValid)
                                if (_bestDropObject.Position.Distance(ObjectManager.Player.Position) < 120)
                                    if (_bestDropObject.Position.Distance(args.TargetPosition) >= 120)
                                        for (var i = _bestDropObject.Position.Distance(args.TargetPosition);
                                            i > 0;
                                            i = i - 1)
                                        {
                                            var position = ObjectManager.Player.Position.Extend(args.TargetPosition, i);
                                            if (_bestDropObject.Position.Distance(position) < 120)
                                            {
                                                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, position);
                                                args.Process = false;
                                                break;
                                            }
                                        }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            _axeDropObjectDataList.RemoveAll(x => !x.Object.IsValid);

            if (args.Unit.IsMe)
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        if (args.Target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (AxeCount < 2)
                                    if (_q.IsReadyPerfectly())
                                        _q.Cast();
                        }
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        if (args.Target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (AxeCount < 2)
                                        if (_q.IsReadyPerfectly())
                                            _q.Cast();
                        }
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        if (MinionManager.GetMinions(float.MaxValue).Any(x => x.NetworkId == args.Target.NetworkId))
                        {
                            if (MenuProvider.Champion.Laneclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    if (AxeCount < 2)
                                        if (_q.IsReadyPerfectly())
                                            _q.Cast();
                        }

                        if (
                            MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral)
                                .Any(x => x.NetworkId == args.Target.NetworkId))
                        {
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (AxeCount < 2)
                                        if (_q.IsReadyPerfectly())
                                            _q.Cast();
                        }
                        break;
                }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (_e.IsReadyPerfectly())
                    if (gapcloser.Sender.IsValidTarget(_e.Range))
                        _e.Cast(gapcloser.Sender);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (_e.IsReadyPerfectly())
                    if (sender.IsValidTarget(_e.Range))
                        _e.Cast(sender);
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Draven_Base_Q_reticle_self.troy")
                _axeDropObjectDataList.Add(new AxeDropObjectData
                {
                    Object = sender,
                    ExpireTime = Environment.TickCount + 1200
                });
        }

        private void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Draven_Base_Q_reticle_self.troy")
                _axeDropObjectDataList.RemoveAll(x => x.Object.NetworkId == sender.NetworkId);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.Champion.Drawings.DrawErange.Active && _e.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range,
                        MenuProvider.Champion.Drawings.DrawErange.Color);

                if (MenuProvider.Champion.Drawings.DrawRrange.Active && _r.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range,
                        MenuProvider.Champion.Drawings.DrawRrange.Color);

                var DrawCatchAxeRange = MenuProvider.Champion.Drawings.GetCircleValue("Draw Catch Axe Range");
                if (DrawCatchAxeRange.Active)
                    Render.Circle.DrawCircle(Game.CursorPos,
                        MenuProvider.Champion.Misc.GetSliderValue("Axe Catch Range").Value,
                        DrawCatchAxeRange.Color, 3);

                if (MenuProvider.Champion.Drawings.GetBoolValue("Draw Axe Drop Objects"))
                {
                    foreach (var data in _axeDropObjectDataList.Where(x => x.Object.IsValid))
                    {
                        var objectPos = Drawing.WorldToScreen(data.Object.Position);
                        Render.Circle.DrawCircle(data.Object.Position, 120,
                            _bestDropObject != null && _bestDropObject.IsValid
                                ? data.Object.NetworkId == _bestDropObject.NetworkId ? Color.YellowGreen : Color.Gray
                                : Color.Gray, 3);
                        Drawing.DrawText(objectPos.X, objectPos.Y,
                            _bestDropObject != null && _bestDropObject.IsValid
                                ? data.Object.NetworkId == _bestDropObject.NetworkId ? Color.YellowGreen : Color.Gray
                                : Color.Gray, ((float)(data.ExpireTime - Environment.TickCount) / 1000).ToString("0.0"));
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

            if (AxeCount > 0)
            {
                _q.GetDamage(enemy);
            }

            if (_e.IsReadyPerfectly())
            {
                damage += _e.GetDamage(enemy);
            }

            if (_r.IsReadyPerfectly())
            {
                damage += _r.GetDamage(enemy);
            }

            return damage;
        }
    }
}