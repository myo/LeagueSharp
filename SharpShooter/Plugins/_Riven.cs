using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Riven
    {
        private bool _dontAutoAttack;
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;

        public Riven()
        {
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 300f);
            _r = new Spell(SpellSlot.R, 900f) {MinHitChance = HitChance.High};

            _r.SetSkillshot(0.25f, 45f, 1600f, false, SkillshotType.SkillshotCone);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddUseR();

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW();
            MenuProvider.Champion.Harass.AddUseE();

            MenuProvider.Champion.Laneclear.AddUseQ();
            MenuProvider.Champion.Laneclear.AddUseW();

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();

            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();
            MenuProvider.Champion.Misc.AddItem("Q Delay1", new Slider(250, 0, 1000));
            MenuProvider.Champion.Misc.AddItem("Q Delay2", new Slider(250, 0, 1000));
            MenuProvider.Champion.Misc.AddItem("Q Delay3", new Slider(350, 0, 1000));
            MenuProvider.Champion.Misc.AddItem("Use legit cancel", true);

            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;

            Console.WriteLine("Sharpshooter: Riven Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Riven</font> Loaded.");
        }

        private void Game_OnUpdate(EventArgs args)
        {
            _w.Range = ObjectManager.Player.HasBuff("RivenFengShuiEngine") ? 350 : 300;

            if (!ObjectManager.Player.IsDead)
            {
                if (Orbwalking.CanMove(100))
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (MenuProvider.Champion.Combo.UseW)
                                if (!ObjectManager.Player.IsDashing())
                                    if (_w.IsReadyPerfectly())
                                        if (HeroManager.Enemies.Any(x => x.IsValidTarget(_w.Range)))
                                            _w.Cast();

                            if (MenuProvider.Champion.Combo.UseE)
                                if (_e.IsReadyPerfectly())
                                {
                                    var target =
                                        HeroManager.Enemies.OrderByDescending(x => TargetSelector.GetPriority(x))
                                            .FirstOrDefault(
                                                x => x.IsValidTarget(_e.Range + Orbwalking.GetRealAutoAttackRange(x)));
                                    if (target != null)
                                        _e.Cast(target.Position);
                                }

                            if (MenuProvider.Champion.Combo.UseR)
                                if (_r.IsReadyPerfectly())
                                    if (ObjectManager.Player.HasBuff("RivenFengShuiEngine"))
                                    {
                                        var target =
                                            HeroManager.Enemies.FirstOrDefault(
                                                x =>
                                                    x.IsKillableAndValidTarget(_r.GetDamage(x),
                                                        TargetSelector.DamageType.Physical, _r.Range));
                                        if (target != null)
                                            _r.Cast(target);
                                    }
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.Mixed:
                        {
                            if (MenuProvider.Champion.Harass.UseW)
                                if (!ObjectManager.Player.IsDashing())
                                    if (_w.IsReadyPerfectly())
                                        if (HeroManager.Enemies.Any(x => x.IsValidTarget(_w.Range)))
                                            _w.Cast();
                            break;
                        }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            if (MenuProvider.Champion.Laneclear.UseW)
                                if (!ObjectManager.Player.IsDashing())
                                    if (_w.IsReadyPerfectly())
                                        if (MinionManager.GetMinions(_w.Range).Count(x => x.IsValidTarget(_w.Range)) >=
                                            3)
                                            _w.Cast();

                            if (MenuProvider.Champion.Jungleclear.UseW)
                                if (!ObjectManager.Player.IsDashing())
                                    if (_w.IsReadyPerfectly())
                                        if (
                                            MinionManager.GetMinions(_w.Range, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).Any(x => x.IsValidTarget(_w.Range)))
                                            _w.Cast();
                            break;
                        }
                    }
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (_w.IsReadyPerfectly())
                    if (gapcloser.Sender.IsValidTarget(_w.Range))
                        _w.Cast();
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (args.DangerLevel >= Interrupter2.DangerLevel.High)
                    if (sender.IsValidTarget(_w.Range))
                        if (_w.IsReadyPerfectly())
                            _w.Cast();
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
            }
        }

        private void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Animation == "Spell1a")
                {
                    _dontAutoAttack = true;

                    if (MenuProvider.Champion.Misc.GetBoolValue("Use legit cancel"))
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    }
                    else
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        Utility.DelayAction.Add(MenuProvider.Champion.Misc.GetSliderValue("Q Delay1").Value,
                            () => { _dontAutoAttack = false; });
                    }
                }
                else if (args.Animation == "Spell1b")
                {
                    _dontAutoAttack = true;

                    if (MenuProvider.Champion.Misc.GetBoolValue("Use legit cancel"))
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    }
                    else
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        Utility.DelayAction.Add(MenuProvider.Champion.Misc.GetSliderValue("Q Delay2").Value,
                            () => { _dontAutoAttack = false; });
                    }
                }
                else if (args.Animation == "Spell1c")
                {
                    _dontAutoAttack = true;

                    if (MenuProvider.Champion.Misc.GetBoolValue("Use legit cancel"))
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    }
                    else
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        Utility.DelayAction.Add(MenuProvider.Champion.Misc.GetSliderValue("Q Delay3").Value,
                            () => { _dontAutoAttack = false; });
                    }
                }
                else if (args.Animation == "Spell2")
                {
                    _dontAutoAttack = true;

                    Orbwalking.ResetAutoAttackTimer();
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
                else if (args.Animation == "Spell3" || args.Animation == "Spell4")
                {
                    _dontAutoAttack = true;
                }
                else
                    _dontAutoAttack = false;
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                if (_dontAutoAttack)
                {
                    args.Process = false;
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
                                    _q.Cast(target.Position);
                        }
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        if (target.Type == GameObjectType.obj_AI_Hero)
                        {
                            if (MenuProvider.Champion.Harass.UseQ)
                                if (_e.IsReadyPerfectly())
                                    _q.Cast(target.Position);
                        }
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        if (target.Type == GameObjectType.obj_AI_Minion)
                        {
                            if ((target as Obj_AI_Base).IsLaneMob())
                            {
                                if (MenuProvider.Champion.Laneclear.UseQ)
                                    if (_q.IsReadyPerfectly())
                                        _q.Cast(target.Position);
                            }

                            if ((target as Obj_AI_Base).IsJungleMob())
                            {
                                if (MenuProvider.Champion.Jungleclear.UseQ)
                                    if (_q.IsReadyPerfectly())
                                        _q.Cast(target.Position);
                            }
                        }
                        break;
                }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (!ObjectManager.Player.IsWindingUp)
            {
                damage += (float) ObjectManager.Player.GetAutoAttackDamage(enemy, true);
            }

            return damage;
        }
    }
}