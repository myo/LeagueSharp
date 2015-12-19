using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Twitch
    {
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _recall;

        public Twitch()
        {
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W, 950f, TargetSelector.DamageType.True) {MinHitChance = HitChance.High};
            _e = new Spell(SpellSlot.E, 1200f);

            _recall = new Spell(SpellSlot.Recall);

            _w.SetSkillshot(0.25f, 100f, 1400f, false, SkillshotType.SkillshotCircle);

            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();

            MenuProvider.Champion.Jungleclear.AddUseW(false);
            MenuProvider.Champion.Jungleclear.AddUseE();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseKillsteal();
            MenuProvider.Champion.Misc.AddItem("Stealth Recall", new KeyBind('T', KeyBindType.Press));

            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddItem("R Pierce Line", true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Console.WriteLine("Sharpshooter: Twitch Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Twitch</font> Loaded.");
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
                                    var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);
                                    if (target != null)
                                        _w.Cast(target, false, true);
                                }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                    if (
                                        HeroManager.Enemies.Any(
                                            x =>
                                                x.IsValidTarget(_e.Range) &&
                                                (x.GetBuffCount("twitchdeadlyvenom") >= 6 ||
                                                 x.IsKillableAndValidTarget(_e.GetDamage(x),
                                                     TargetSelector.DamageType.Physical))))
                                        _e.Cast();
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var farmLocation =
                                            _w.GetCircularFarmLocation(MinionManager.GetMinions(_e.Range,
                                                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth));
                                        if (farmLocation.MinionsHit >= 1)
                                            _w.Cast(farmLocation.Position);
                                    }

                            if (MenuProvider.Champion.Jungleclear.UseE)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_e.IsReadyPerfectly())
                                        if (
                                            MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth)
                                                .Any(
                                                    x =>
                                                        x.IsValidTarget(_e.Range) &&
                                                        (x.GetBuffCount("twitchdeadlyvenom") >= 6 ||
                                                         x.IsKillableAndValidTarget(_e.GetDamage(x),
                                                             TargetSelector.DamageType.Physical))))
                                            _e.Cast();
                            break;
                        }
                    }
                }

                if (MenuProvider.Champion.Misc.UseKillsteal)
                    if (
                        HeroManager.Enemies.Any(
                            x =>
                                x.IsKillableAndValidTarget(_e.GetDamage(x), TargetSelector.DamageType.Physical, _e.Range)))
                        _e.Cast();

                if (MenuProvider.Champion.Misc.GetKeyBindValue("Stealth Recall").Active)
                    if (_q.IsReadyPerfectly())
                        if (_recall.IsReadyPerfectly())
                        {
                            _q.Cast();
                            _recall.Cast();
                        }
            }
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

                if (MenuProvider.Champion.Drawings.GetBoolValue("R Pierce Line"))
                    if (ObjectManager.Player.HasBuff("TwitchFullAutomatic"))
                    {
                        var target = MenuProvider.Orbwalker.GetTarget() as Obj_AI_Base;
                        if (target.IsValidTarget())
                        {
                            var from = Drawing.WorldToScreen(ObjectManager.Player.Position);
                            var dis = Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 300 -
                                      ObjectManager.Player.Distance(target, false);
                            var to =
                                Drawing.WorldToScreen(dis > 0
                                    ? target.ServerPosition.Extend(ObjectManager.Player.Position, -dis)
                                    : target.ServerPosition);
                            Drawing.DrawLine(from[0], from[1], to[0], to[1], 10, Color.FromArgb(200, Color.GreenYellow));
                        }
                    }
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (_e.IsReadyPerfectly())
            {
                damage += _e.GetDamage(enemy);
            }

            return damage;
        }
    }
}