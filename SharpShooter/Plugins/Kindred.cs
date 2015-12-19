using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Kindred
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Kindred()
        {
            _q = new Spell(SpellSlot.Q, 330f);
            _w = new Spell(SpellSlot.W, 800f);
            _e = new Spell(SpellSlot.E, 500f);
            _r = new Spell(SpellSlot.R, 550f);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW();
            MenuProvider.Champion.Harass.AddUseE(false);
            MenuProvider.Champion.Harass.AddIfMana();

            MenuProvider.Champion.Laneclear.AddUseQ();
            MenuProvider.Champion.Laneclear.AddUseW();
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();
            MenuProvider.Champion.Jungleclear.AddUseE(false);
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.ChampionMenuInstance.SubMenu("Misc").AddSubMenu(new Menu("Auto R", "Auto R"));
            MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                .SubMenu("Auto R")
                .AddItem(new MenuItem("Misc.Auto R.ForMe", "For Me", true))
                .SetValue(true);
            MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                .SubMenu("Auto R")
                .AddItem(new MenuItem("Misc.Auto R.ForAlly", "For Ally", true))
                .SetValue(true);
            MenuProvider.ChampionMenuInstance.SubMenu("Misc")
                .SubMenu("Auto R")
                .AddItem(new MenuItem("Misc.Auto R.ifHealth", "if Health Percent <", true))
                .SetValue(new Slider(30, 10, 80));
            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddItem("Use Anti-Melee (Q)", true);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            Console.WriteLine("Sharpshooter: Kindred Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Kindred</font> Loaded.");
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                    if (_e.IsReadyPerfectly())
                        if (gapcloser.Sender.IsValidTarget(_e.Range))
                            _e.CastOnUnit(gapcloser.Sender);
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
                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_e.Range, _e.DamageType);
                                    if (target != null)
                                        _e.CastOnUnit(target);
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
                            if (MenuProvider.Champion.Jungleclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).FirstOrDefault();
                                        if (target != null)
                                            _e.CastOnUnit(target);
                                    }
                            break;
                        }
                    }
                }

                if (MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.ForMe", true).GetValue<bool>())
                    if (_r.IsReadyPerfectly())
                    {
                        if (ObjectManager.Player.HealthPercent <
                            MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.ifHealth", true)
                                .GetValue<Slider>()
                                .Value)
                            if (ObjectManager.Player.CountEnemiesInRange(800f) >= 2)
                                _r.Cast(ObjectManager.Player);
                    }

                if (MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.ForAlly", true).GetValue<bool>())
                    if (_r.IsReadyPerfectly())
                    {
                        foreach (
                            var target in
                                HeroManager.Allies.Where(
                                    x =>
                                        x.IsValidTarget(_r.Range) && !x.IsZombie && !x.IsMe &&
                                        x.CountEnemiesInRange(800f) >= 2 &&
                                        x.HealthPercent <
                                        MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.ifHealth", true)
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

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
                switch (MenuProvider.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        if (target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (
                                        ObjectManager.Player.Position.Extend(Game.CursorPos, 400)
                                            .CountEnemiesInRange(400) <= 1)
                                        if (_q.Cast(Game.CursorPos))
                                            if (MenuProvider.Champion.Combo.UseW)
                                                if (_w.IsReadyPerfectly())
                                                    _w.Cast();

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                    _w.Cast();
                        }
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        if (target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_q.IsReadyPerfectly())
                                        if (
                                            ObjectManager.Player.Position.Extend(Game.CursorPos, 400)
                                                .CountEnemiesInRange(400) <= 1)
                                            if (_q.Cast(Game.CursorPos))
                                                if (MenuProvider.Champion.Harass.UseW)
                                                    if (_w.IsReadyPerfectly())
                                                        _w.Cast();

                            if (MenuProvider.Champion.Harass.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_w.IsReadyPerfectly())
                                        _w.Cast();
                        }
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        if (target.Type == GameObjectType.obj_AI_Minion)
                        {
                            if (MinionManager.GetMinions(float.MaxValue).Any(x => x.NetworkId == target.NetworkId))
                            {
                                if (MenuProvider.Champion.Laneclear.UseQ)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                        if (_q.IsReadyPerfectly())
                                            if (
                                                ObjectManager.Player.Position.Extend(Game.CursorPos, 400)
                                                    .CountEnemiesInRange(400) <= 1)
                                                if (_q.Cast(Game.CursorPos))
                                                    if (MenuProvider.Champion.Laneclear.UseW)
                                                        if (_w.IsReadyPerfectly())
                                                            _w.Cast();

                                if (MenuProvider.Champion.Laneclear.UseW)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                        if (_w.IsReadyPerfectly())
                                            _w.Cast();
                            }


                            if (
                                MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral)
                                    .Any(x => x.NetworkId == target.NetworkId))
                            {
                                if (MenuProvider.Champion.Jungleclear.UseQ)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                        if (_q.IsReadyPerfectly())
                                            if (
                                                ObjectManager.Player.Position.Extend(Game.CursorPos, 400)
                                                    .CountEnemiesInRange(400) <= 1)
                                                if (_q.Cast(Game.CursorPos))
                                                    if (MenuProvider.Champion.Jungleclear.UseW)
                                                        if (_w.IsReadyPerfectly())
                                                            _w.Cast();

                                if (MenuProvider.Champion.Jungleclear.UseW)
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                        if (_w.IsReadyPerfectly())
                                            _w.Cast();
                            }
                        }
                        break;
                }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null)
                if (args.Target != null)
                    if (args.Target.IsValid)
                        if (sender.IsEnemy)
                            if (sender.Type == GameObjectType.obj_AI_Hero || sender.Type == GameObjectType.obj_AI_Turret)
                                if (args.Target.Type == GameObjectType.obj_AI_Hero)
                                {
                                    var target = args.Target as Obj_AI_Base;

                                    if (target.HealthPercent <
                                        MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.ifHealth", true)
                                            .GetValue<Slider>()
                                            .Value)
                                    {
                                        if (target.IsMe)
                                        {
                                            if (
                                                MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.ForMe", true)
                                                    .GetValue<bool>())
                                                if (_r.IsReadyPerfectly())
                                                    if (target.IsValidTarget(_r.Range))
                                                        _r.Cast(target);
                                        }
                                        else if (!target.IsMe && target.IsAlly && !target.IsZombie)
                                        {
                                            if (
                                                MenuProvider.ChampionMenuInstance.Item("Misc.Auto R.ForAlly", true)
                                                    .GetValue<bool>())
                                                if (_r.IsReadyPerfectly())
                                                    if (target.IsValidTarget(_r.Range))
                                                        _r.Cast(target);
                                        }
                                    }
                                }

            if (sender != null)
                if (args.Target != null)
                    if (args.Target.IsMe)
                        if (sender.Type == GameObjectType.obj_AI_Hero)
                            if (sender.IsEnemy)
                                if (sender.IsMelee)
                                    if (args.SData.IsAutoAttack())
                                        if (MenuProvider.Champion.Misc.GetBoolValue("Use Anti-Melee (Q)"))
                                            if (_q.IsReadyPerfectly())
                                                _q.Cast(ObjectManager.Player.Position.Extend(sender.Position, -_q.Range));

            //if (sender.IsMe)
            //    if (args.Slot == SpellSlot.Q)
            //        Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer);
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

            if (_e.IsReadyPerfectly())
                damage += _e.GetDamage(enemy);

            return damage;
        }
    }
}