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
    public class Kalista
    {
        private readonly int[] _qManaCost = {0, 50, 55, 60, 65, 70};
        private readonly Vector3 _baronLocation;
        private readonly Vector3 _dragonLocation;
        private int _eLastCastTime;
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Kalista()
        {
            _q = new Spell(SpellSlot.Q, 1150f, TargetSelector.DamageType.Physical) {MinHitChance = HitChance.High};
            _w = new Spell(SpellSlot.W, 5000f);
            _e = new Spell(SpellSlot.E, 950f);
            _r = new Spell(SpellSlot.R, 1500f);

            _q.SetSkillshot(0.35f, 40f, 2400f, true, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddItem("Attack Minion For Chasing", false);

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddIfMana();

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddItem("Cast Q if Killable Minion Number >=", new Slider(3, 1, 7));
            MenuProvider.Champion.Laneclear.AddUseE();
            MenuProvider.Champion.Laneclear.AddItem("Cast E if Killable Minion Number >=", new Slider(2, 1, 5));
            MenuProvider.Champion.Laneclear.AddIfMana(20);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseE();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddItem("Use Killsteal (With E)", true);
            MenuProvider.Champion.Misc.AddItem("Use Mobsteal (With E)", true);
            MenuProvider.Champion.Misc.AddItem("Use Lasthit Assist (With E)", true);
            MenuProvider.Champion.Misc.AddItem("Use Soulbound Saver (With R)", true);
            MenuProvider.Champion.Misc.AddItem("Auto Balista Combo (With R)", true);
            MenuProvider.Champion.Misc.AddItem("Auto Steal Siege minion & Super minion (With E)", true);
            MenuProvider.Champion.Misc.AddItem("Auto E Harass (With E)", true);
            MenuProvider.Champion.Misc.AddItem("^ Don't do this in ComboMode", false);
            MenuProvider.Champion.Misc.AddItem("Auto E Before Die", true);
            MenuProvider.Champion.Misc.AddItem("Auto W on Dragon or Baron (With W)", true);
            MenuProvider.Champion.Misc.AddItem("Cast W on Dragon", new KeyBind('J', KeyBindType.Press));
            MenuProvider.Champion.Misc.AddItem("Cast W on Baron", new KeyBind('K', KeyBindType.Press));

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddItem("Draw E Damage Percent", true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);
            MenuProvider.Champion.Drawings.AddDamageIndicatorForJungle(GetJungleDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.OnNonKillableMinion += Orbwalking_OnNonKillableMinion;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            _baronLocation = new Vector3(5064f, 10568f, -71f);
            _dragonLocation = new Vector3(9796f, 4432f, -71f);

            Console.WriteLine("Sharpshooter: Kalista Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Kalista</font> Loaded.");
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
                            if (MenuProvider.Champion.Combo.GetBoolValue("Attack Minion For Chasing"))
                                if (MenuProvider.Orbwalker.GetTarget() == null)
                                    if (
                                        !HeroManager.Enemies.Any(
                                            x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x)))
                                    {
                                        var minion =
                                            MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(null) + 65,
                                                MinionTypes.All, MinionTeam.NotAlly)
                                                .OrderBy(x => x.Distance(ObjectManager.Player))
                                                .FirstOrDefault();
                                        if (minion != null)
                                            Orbwalking.Orbwalk(minion, Game.CursorPos, 0f);
                                    }

                            if (MenuProvider.Champion.Combo.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (!ObjectManager.Player.IsDashing())
                                        if (!ObjectManager.Player.IsWindingUp)
                                        {
                                            var target = TargetSelector.GetTargetNoCollision(_q);
                                            if (target != null)
                                                if (ObjectManager.Player.Mana - _qManaCost[_q.Level] >= 40)
                                                    _q.Cast(target);
                                                else
                                                {
                                                    var killableTarget =
                                                        HeroManager.Enemies.FirstOrDefault(
                                                            x =>
                                                                !Orbwalking.InAutoAttackRange(x) &&
                                                                x.IsKillableAndValidTarget(_q.GetDamage(x),
                                                                    TargetSelector.DamageType.Physical, _q.Range) &&
                                                                _q.GetPrediction(x).Hitchance >= _q.MinHitChance);
                                                    if (killableTarget != null)
                                                        _q.Cast(killableTarget);
                                                }
                                        }

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                    if (
                                        HeroManager.Enemies.Any(
                                            x =>
                                                HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                                x.IsKillableAndValidTarget(_e.GetDamage(x),
                                                    TargetSelector.DamageType.Physical, _e.Range)))
                                        _e.Cast();

                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (!ObjectManager.Player.IsDashing())
                                        if (!ObjectManager.Player.IsWindingUp)
                                            if (_q.IsReadyPerfectly())
                                            {
                                                var target = TargetSelector.GetTargetNoCollision(_q);
                                                if (target != null)
                                                    _q.Cast(target);
                                            }

                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            //Lane
                            if (MenuProvider.Champion.Laneclear.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (!ObjectManager.Player.IsDashing())
                                        if (!ObjectManager.Player.IsWindingUp)
                                            if (
                                                ObjectManager.Player.IsManaPercentOkay(
                                                    MenuProvider.Champion.Laneclear.IfMana))
                                            {
                                                foreach (
                                                    var killableMinion in
                                                        MinionManager.GetMinions(_q.Range)
                                                            .Where(
                                                                x =>
                                                                    _q.GetPrediction(x).Hitchance >= _q.MinHitChance &&
                                                                    x.IsKillableAndValidTarget(_q.GetDamage(x),
                                                                        TargetSelector.DamageType.Physical, _q.Range)))
                                                {
                                                    var killableNumber = 0;

                                                    var collisionMinions =
                                                        Collision.GetCollision(
                                                            new List<Vector3>
                                                            {
                                                                ObjectManager.Player.ServerPosition.Extend(
                                                                    killableMinion.ServerPosition, _q.Range)
                                                            },
                                                            new PredictionInput
                                                            {
                                                                Unit = ObjectManager.Player,
                                                                Delay = _q.Delay,
                                                                Speed = _q.Speed,
                                                                Radius = _q.Width,
                                                                Range = _q.Range,
                                                                CollisionObjects = new[] {CollisionableObjects.Minions},
                                                                UseBoundingRadius = false
                                                            }
                                                            ).OrderBy(x => x.Distance(ObjectManager.Player));

                                                    foreach (Obj_AI_Minion collisionMinion in collisionMinions)
                                                    {
                                                        if (
                                                            collisionMinion.IsKillableAndValidTarget(
                                                                ObjectManager.Player.GetSpellDamage(collisionMinion,
                                                                    SpellSlot.Q), TargetSelector.DamageType.Physical,
                                                                _q.Range))
                                                            killableNumber++;
                                                        else
                                                            break;
                                                    }

                                                    if (killableNumber >=
                                                        MenuProvider.Champion.Laneclear.GetSliderValue(
                                                            "Cast Q if Killable Minion Number >=").Value)
                                                    {
                                                        if (!ObjectManager.Player.IsWindingUp)
                                                        {
                                                            _q.Cast(killableMinion.ServerPosition);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }

                            if (MenuProvider.Champion.Laneclear.UseE)
                                if (_e.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                        if (
                                            MinionManager.GetMinions(_e.Range)
                                                .Count(
                                                    x =>
                                                        HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                                        x.IsKillableAndValidTarget(_e.GetDamage(x),
                                                            TargetSelector.DamageType.Physical)) >=
                                            MenuProvider.Champion.Laneclear.GetSliderValue(
                                                "Cast E if Killable Minion Number >=").Value)
                                            _e.Cast();

                            //Jugnle
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (_q.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    {
                                        var qTarget =
                                            MinionManager.GetMinions(_q.Range, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth)
                                                .FirstOrDefault(
                                                    x =>
                                                        x.IsValidTarget(_q.Range) &&
                                                        _q.GetPrediction(x).Hitchance >= HitChance.High);

                                        if (qTarget != null)
                                            _q.Cast(qTarget);
                                    }

                            if (MenuProvider.Champion.Jungleclear.UseE)
                                if (_e.IsReadyPerfectly())
                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                        if (
                                            MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth)
                                                .Any(
                                                    x =>
                                                        HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                                        x.IsKillableAndValidTarget(_e.GetDamage(x),
                                                            TargetSelector.DamageType.Physical)))
                                            _e.Cast();

                            break;
                        }
                    }
                }

                if (MenuProvider.Champion.Misc.GetBoolValue("Use Killsteal (With E)"))
                    if (_e.IsReadyPerfectly())
                        if (
                            HeroManager.Enemies.Any(
                                x =>
                                    HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                    x.IsKillableAndValidTarget(_e.GetDamage(x), TargetSelector.DamageType.Physical,
                                        _e.Range)))
                            _e.Cast();

                if (MenuProvider.Champion.Misc.GetBoolValue("Use Mobsteal (With E)"))
                {
                    if (_e.IsReadyPerfectly())
                        if (
                            MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.Neutral,
                                MinionOrderTypes.MaxHealth)
                                .Any(
                                    x =>
                                        HealthPrediction.GetHealthPrediction(x, 500) > 0 &&
                                        x.IsKillableAndValidTarget(_e.GetDamage(x), TargetSelector.DamageType.Physical)))
                            _e.Cast();
                }

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto Steal Siege minion & Super minion (With E)"))
                {
                    if (_e.IsReadyPerfectly())
                        if (
                            MinionManager.GetMinions(_e.Range)
                                .Any(
                                    x =>
                                        HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                        x.IsKillableAndValidTarget(_e.GetDamage(x), TargetSelector.DamageType.Physical) &&
                                        (x.CharData.BaseSkinName.ToLower().Contains("siege") ||
                                         x.CharData.BaseSkinName.ToLower().Contains("super"))))
                            _e.Cast();
                }

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto Balista Combo (With R)"))
                    if (_r.IsReadyPerfectly())
                    {
                        var myBlitzcrank =
                            HeroManager.Allies.FirstOrDefault(
                                x => !x.IsDead && x.HasBuff("kalistacoopstrikeally") && x.ChampionName == "Blitzcrank");
                        if (myBlitzcrank != null)
                        {
                            var grabTarget =
                                HeroManager.Enemies.FirstOrDefault(x => !x.IsDead && x.HasBuff("rocketgrab2"));
                            if (grabTarget != null)
                                if (ObjectManager.Player.Distance(grabTarget) > myBlitzcrank.Distance(grabTarget))
                                    _r.Cast();
                        }
                    }

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto E Harass (With E)"))
                    if (_e.IsReadyPerfectly())
                        if (
                            !(MenuProvider.Champion.Misc.GetBoolValue("^ Don't do this in ComboMode") &&
                              MenuProvider.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
                            if (ObjectManager.Player.Mana - _e.ManaCost >= _e.ManaCost)
                                if (HeroManager.Enemies.Any(x => x.IsValidTarget(_e.Range) && _e.GetDamage(x) > 10))
                                    if (
                                        MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.NotAlly)
                                            .Any(
                                                x =>
                                                    HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                                    x.IsKillableAndValidTarget(_e.GetDamage(x),
                                                        TargetSelector.DamageType.Physical, _e.Range)))
                                        _e.Cast();

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto W on Dragon or Baron (With W)"))
                    if (ObjectManager.Player.IsManaPercentOkay(50))
                        if (!ObjectManager.Player.IsRecalling())
                            if (ObjectManager.Player.Position.CountEnemiesInRange(1500f) <= 0)
                                if (MenuProvider.Orbwalker.GetTarget() == null)
                                {
                                    if (_w.IsReadyPerfectly())
                                        if (ObjectManager.Player.Distance(_baronLocation) <= _w.Range)
                                            _w.Cast(_baronLocation);

                                    if (_w.IsReadyPerfectly())
                                        if (ObjectManager.Player.Distance(_dragonLocation) <= _w.Range)
                                            _w.Cast(_dragonLocation);
                                }

                if (MenuProvider.Champion.Misc.GetKeyBindValue("Cast W on Dragon").Active)
                    if (_w.IsReadyPerfectly())
                        if (ObjectManager.Player.Distance(_dragonLocation) <= _w.Range)
                            _w.Cast(_dragonLocation);

                if (MenuProvider.Champion.Misc.GetKeyBindValue("Cast W on Baron").Active)
                    if (_w.IsReadyPerfectly())
                        if (ObjectManager.Player.Distance(_baronLocation) <= _w.Range)
                            _w.Cast(_baronLocation);
            }
        }

        private void Orbwalking_OnNonKillableMinion(AttackableUnit minion)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var Minion = minion as Obj_AI_Minion;
                if (MenuProvider.Champion.Misc.GetBoolValue("Use Lasthit Assist (With E)"))
                    if (_e.IsReadyPerfectly())
                        if (Minion.IsKillableAndValidTarget(_e.GetDamage(Minion), TargetSelector.DamageType.Physical))
                            if (HealthPrediction.GetHealthPrediction(Minion, 250) > 0)
                                if (!HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                                    _e.Cast();
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
                if (sender.Owner.IsMe)
                    if (args.Slot == SpellSlot.E)
                        if (_eLastCastTime > Utils.TickCount - 700)
                            args.Process = false;
                        else
                            _eLastCastTime = Utils.TickCount;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && args.Target != null)
                if (sender.IsEnemy)
                    if (sender.Type == GameObjectType.obj_AI_Hero)
                    {
                        if (MenuProvider.Champion.Misc.GetBoolValue("Use Soulbound Saver (With R)"))
                            if (_r.IsReadyPerfectly())
                            {
                                var soulbound =
                                    HeroManager.Allies.FirstOrDefault(
                                        x => !x.IsDead && x.HasBuff("kalistacoopstrikeally"));
                                if (soulbound != null)
                                    if (args.Target.NetworkId == soulbound.NetworkId ||
                                        args.End.Distance(soulbound.Position) <= 200)
                                        if (soulbound.HealthPercent < 20)
                                            _r.Cast();
                            }

                        if (MenuProvider.Champion.Misc.GetBoolValue("Auto E Before Die"))
                            if (args.Target.IsMe)
                                if (ObjectManager.Player.HealthPercent <= 10)
                                    if (_e.IsReadyPerfectly())
                                        if (
                                            HeroManager.Enemies.Any(
                                                x => x.IsValidTarget(_e.Range) && _e.GetDamage(x) > 0))
                                            _e.Cast();
                    }
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

                if (MenuProvider.Champion.Drawings.GetBoolValue("Draw E Damage Percent"))
                {
                    foreach (var target in HeroManager.Enemies.Where(x => !x.IsDead && x.IsVisible))
                    {
                        if (_e.GetDamage(target) > 2)
                        {
                            var targetPos = Drawing.WorldToScreen(target.Position);
                            var damagePercent = (_e.GetDamage(target)/target.Health + target.PhysicalShield)*100;

                            if (damagePercent > 0)
                                Drawing.DrawText(targetPos.X, targetPos.Y - 100,
                                    damagePercent >= 100 ? Color.Red : Color.GreenYellow, damagePercent.ToString("0.0"));
                        }
                    }

                    foreach (
                        var target in
                            MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral)
                                .Where(x => !x.IsDead && x.IsVisible))
                    {
                        if (_e.GetDamage(target) > 2)
                        {
                            var targetPos = Drawing.WorldToScreen(target.Position);
                            var damagePercent = (_e.GetDamage(target)/target.Health + target.PhysicalShield)*100;

                            if (damagePercent > 0)
                                Drawing.DrawText(targetPos.X, targetPos.Y - 100,
                                    damagePercent >= 100 ? Color.Red : Color.GreenYellow, damagePercent.ToString("0.0"));
                        }
                    }
                }
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = _e.GetDamage(enemy);

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                damage *= 0.6f;

            if (enemy.HasBuff("FerociousHowl"))
                damage *= 0.3f;

            return _e.IsReadyPerfectly() ? damage : 0;
        }

        private float GetJungleDamage(Obj_AI_Minion enemy)
        {
            var damage = _e.GetDamage(enemy);
            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                damage *= 0.6f;

            var dragonSlayerBuff = ObjectManager.Player.GetBuff("s5test_dragonslayerbuff");
            if (dragonSlayerBuff != null)
            {
                if (dragonSlayerBuff.Count >= 4)
                    damage += dragonSlayerBuff.Count == 5 ? damage*0.30f : damage*0.15f;

                if (enemy.CharData.BaseSkinName.ToLowerInvariant().Contains("dragon"))
                    damage *= 1 - dragonSlayerBuff.Count*0.07f;
            }

            if (enemy.CharData.BaseSkinName.ToLowerInvariant().Contains("baron") &&
                ObjectManager.Player.HasBuff("barontarget"))
                damage *= 0.5f;

            return _e.IsReadyPerfectly() ? damage : 0;
        }
    }
}