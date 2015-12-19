using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Sivir
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Sivir()
        {
            _q = new Spell(SpellSlot.Q, 1250f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E);
            _r = new Spell(SpellSlot.R, 1000f);

            _q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ();
            MenuProvider.Champion.Laneclear.AddUseW();
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddItem("Auto Q on immobile Target", true);
            MenuProvider.Champion.Misc.AddItem("Auto E against targeted spells", true);
            MenuProvider.Champion.Misc.AddItem("Use E Humanizer", false);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Console.WriteLine("Sharpshooter: Sivir Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Sivir</font> Loaded.");
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;
            if (!Orbwalking.CanMove(100)) return;

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
                                var farmLocation = _q.GetLineFarmLocation(MinionManager.GetMinions(_q.Range));
                                if (farmLocation.MinionsHit >= 4)
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
            }

            if (MenuProvider.Champion.Misc.GetBoolValue("Auto Q on immobile Target"))
                if (_q.IsReadyPerfectly())
                {
                    var target =
                        HeroManager.Enemies.FirstOrDefault(
                            x => x.IsValidTarget(_q.Range) && _q.GetPrediction(x).Hitchance >= HitChance.Immobile);
                    if (target != null)
                        _q.Cast(target);
                }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null)
                if (args.Target != null)
                    if (sender.Type == GameObjectType.obj_AI_Hero)
                        if (args.Target.IsMe)
                            if (sender.IsEnemy)
                                if (MenuProvider.Champion.Misc.GetBoolValue("Auto E against targeted spells"))
                                    if (_e.IsReadyPerfectly())
                                        if (!args.SData.IsAutoAttack())
                                        {
                                            if (!args.SData.Name.Contains("summoner"))
                                                if (!args.SData.Name.Contains("TormentedSoil"))
                                                    Utility.DelayAction.Add(
                                                        MenuProvider.Champion.Misc.GetBoolValue("Use E Humanizer")
                                                            ? new Random().Next(100, 150)
                                                            : 20, () => _e.Cast());
                                        }
                                        else if (args.SData.Name == "BlueCardAttack" ||
                                                 args.SData.Name == "RedCardAttack" ||
                                                 args.SData.Name == "GoldCardAttack")
                                        {
                                            Utility.DelayAction.Add(
                                                MenuProvider.Champion.Misc.GetBoolValue("Use E Humanizer")
                                                    ? new Random().Next(100, 150)
                                                    : 20, () => _e.Cast());
                                        }
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
                if (Orbwalking.InAutoAttackRange(target))
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                    _w.Cast();
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            if (MenuProvider.Champion.Laneclear.UseW)
                                if (_w.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                        if (
                                            MinionManager.GetMinions(
                                                Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
                                                .Any(x => x.NetworkId == target.NetworkId))
                                            _w.Cast();

                            if (MenuProvider.Champion.Jungleclear.UseW)
                                if (_w.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                        if (
                                            MinionManager.GetMinions(
                                                Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), MinionTypes.All,
                                                MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                                                .Any(x => x.NetworkId == target.NetworkId))
                                            _w.Cast();
                            break;
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
                damage += _q.GetDamage(enemy)*1.4f;
            }

            if (_w.IsReadyPerfectly())
            {
                damage += (float) ObjectManager.Player.GetAutoAttackDamage(enemy);
            }

            return damage;
        }
    }
}