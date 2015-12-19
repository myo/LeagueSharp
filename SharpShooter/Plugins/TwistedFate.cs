using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class TwistedFate
    {
        private Cards _cardiNeed = Cards.None;
        private readonly SpellSlot _flash;
        private readonly Spell _q;
        private readonly Spell _w;
        private Spell _e;
        private readonly Spell _r;

        public TwistedFate()
        {
            _flash = ObjectManager.Player.GetSpellSlot("SummonerFlash");

            _q = new Spell(SpellSlot.Q, 1450f, TargetSelector.DamageType.Magical) {MinHitChance = HitChance.VeryHigh};
            _w = new Spell(SpellSlot.W, 1200f, TargetSelector.DamageType.Magical);
            _e = new Spell(SpellSlot.E);
            _r = new Spell(SpellSlot.R, 5500f);

            _q.SetSkillshot(0.25f, 40f, 1000f, false, SkillshotType.SkillshotLine);

            MenuProvider.ChampionMenuInstance.SubMenu("Pick A Card")
                .AddItem(new MenuItem("Pick A Card.Blue", "Blue", true))
                .SetValue(new KeyBind('E', KeyBindType.Press));
            MenuProvider.ChampionMenuInstance.SubMenu("Pick A Card")
                .AddItem(new MenuItem("Pick A Card.Red", "Red", true))
                .SetValue(new KeyBind('T', KeyBindType.Press));
            MenuProvider.ChampionMenuInstance.SubMenu("Pick A Card")
                .AddItem(new MenuItem("Pick A Card.Gold", "Gold", true))
                .SetValue(new KeyBind('G', KeyBindType.Press));

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddItem("Cast Q On Immobile Target Only", false);
            MenuProvider.Champion.Combo.AddUseW();
            MenuProvider.Champion.Combo.AddItem("Use Blue Card if Mana is Low", true);

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddUseW();
            MenuProvider.Champion.Harass.AddIfMana(60);

            MenuProvider.Champion.Laneclear.AddUseQ();
            MenuProvider.Champion.Laneclear.AddUseW();
            MenuProvider.Champion.Laneclear.AddIfMana(60);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseW();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddUseKillsteal();
            MenuProvider.Champion.Misc.AddUseAntiGapcloser();
            MenuProvider.Champion.Misc.AddUseInterrupter();
            MenuProvider.Champion.Misc.AddItem("Select Gold Card When Using Ultimate (gate)", true);

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), true);
            MenuProvider.Champion.Drawings.AddItem("Draw Flash+AA Range",
                new Circle(true, Color.FromArgb(100, Color.Gold)));
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Console.WriteLine("Sharpshooter: Twisted Fate Loaded.");
            Game.PrintChat(
                "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">Twisted Fate</font> Loaded.");
        }

        private bool Picking => _w.IsReadyPerfectly() && _w.Instance.Name != "PickACard";

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
                            if (MenuProvider.Champion.Combo.UseQ)
                                if (_q.IsReadyPerfectly())
                                {
                                    if (ObjectManager.Player.Mana - _q.ManaCost > _w.ManaCost)
                                    {
                                        if (!MenuProvider.Champion.Combo.GetBoolValue("Cast Q On Immobile Target Only"))
                                        {
                                            var target = TargetSelector.GetTarget(_q.Range, _q.DamageType);
                                            if (target != null)
                                            {
                                                _q.Cast(target, false, true);
                                            }
                                        }
                                        else
                                        {
                                            var target =
                                                HeroManager.Enemies.Where(
                                                    x =>
                                                        x.IsValidTarget(_q.Range) &&
                                                        _q.GetPrediction(x).Hitchance >= HitChance.Immobile)
                                                    .OrderByDescending(x => TargetSelector.GetPriority(x))
                                                    .FirstOrDefault();
                                            if (target != null)
                                            {
                                                _q.Cast(target, false, true);
                                            }
                                        }
                                    }
                                }

                            if (MenuProvider.Champion.Combo.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);

                                    if (target != null)
                                    {
                                        if (MenuProvider.Champion.Combo.GetBoolValue("Use Blue Card if Mana is Low")
                                            ? ObjectManager.Player.Mana - _w.ManaCost < _q.ManaCost + _w.ManaCost
                                            : false)
                                        {
                                            PickACard(Cards.Blue);
                                        }
                                        else
                                        {
                                            PickACard(Cards.Gold);
                                        }
                                    }
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
                                        {
                                            _q.Cast(target, false, true);
                                        }
                                    }

                            if (MenuProvider.Champion.Harass.UseW)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                    if (_w.IsReadyPerfectly())
                                    {
                                        var target = TargetSelector.GetTarget(_w.Range, _w.DamageType);

                                        if (target != null)
                                        {
                                            PickACard(Cards.Blue);
                                        }
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
                                        var farm = _q.GetLineFarmLocation(MinionManager.GetMinions(_q.Range));

                                        if (farm.MinionsHit >= 5)
                                        {
                                            _q.Cast(farm.Position);
                                        }
                                    }

                            if (MenuProvider.Champion.Laneclear.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var minioncount =
                                        MinionManager.GetMinions(float.MaxValue)
                                            .Where(x => Orbwalking.InAutoAttackRange(x))
                                            .Count();

                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                    {
                                        if (minioncount >= 4)
                                        {
                                            PickACard(Cards.Red);
                                        }
                                        else if (minioncount >= 1)
                                        {
                                            PickACard(Cards.Blue);
                                        }
                                    }
                                    else
                                    {
                                        if (minioncount >= 1)
                                        {
                                            PickACard(Cards.Blue);
                                        }
                                    }
                                }

                            //Jungleclear
                            if (MenuProvider.Champion.Jungleclear.UseQ)
                                if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    if (_q.IsReadyPerfectly())
                                    {
                                        var target =
                                            MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                                MinionOrderTypes.MaxHealth).FirstOrDefault();
                                        if (target != null)
                                            _q.Cast(target);
                                    }

                            if (MenuProvider.Champion.Jungleclear.UseW)
                                if (_w.IsReadyPerfectly())
                                {
                                    var minionCount =
                                        MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.Neutral,
                                            MinionOrderTypes.MaxHealth).Count();

                                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                    {
                                        if (minionCount >= 3)
                                        {
                                            PickACard(Cards.Red);
                                        }
                                        else if (minionCount >= 1)
                                        {
                                            PickACard(Cards.Blue);
                                        }
                                    }
                                    else if (minionCount >= 1)
                                    {
                                        PickACard(Cards.Blue);
                                    }
                                }
                            break;
                        }
                    }
                }

                if (MenuProvider.Champion.Misc.UseKillsteal)
                {
                    foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget()))
                    {
                        if (target.IsKillableAndValidTarget(_q.GetDamage(target), TargetSelector.DamageType.Physical,
                            _q.Range))
                        {
                            _q.Cast(target, false, true);
                        }
                    }
                }

                if (MenuProvider.ChampionMenuInstance.Item("Pick A Card.Blue", true).GetValue<KeyBind>().Active)
                {
                    PickACard(Cards.Blue);
                }

                if (MenuProvider.ChampionMenuInstance.Item("Pick A Card.Red", true).GetValue<KeyBind>().Active)
                {
                    PickACard(Cards.Red);
                }

                if (MenuProvider.ChampionMenuInstance.Item("Pick A Card.Gold", true).GetValue<KeyBind>().Active)
                {
                    PickACard(Cards.Gold);
                }
            }
            else
            {
                _cardiNeed = Cards.None;
            }

            if (Picking)
            {
                if (_cardiNeed != Cards.None)
                {
                    if (_w.Instance.Name.ToLowerInvariant() == _cardiNeed.ToString().ToLowerInvariant() + "cardlock")
                    {
                        _cardiNeed = Cards.None;
                        Utility.DelayAction.Add(100, () => _w.Cast());
                    }
                }
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                if (Picking)
                {
                    if (MenuProvider.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    {
                        if (_cardiNeed == Cards.Gold)
                        {
                            args.Process = false;
                        }
                    }
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null)
            {
                if (sender.IsMe)
                {
                    if (args.Slot == SpellSlot.W)
                    {
                        if (args.SData.Name != "PickACard")
                        {
                            _cardiNeed = Cards.None;
                        }
                    }

                    if (args.Slot == SpellSlot.R)
                    {
                        if (args.SData.Name == "gate")
                        {
                            if (MenuProvider.Champion.Misc.GetBoolValue("Select Gold Card When Using Ultimate (gate)"))
                            {
                                PickACard(Cards.Gold);
                            }
                        }
                    }
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
            {
                if (gapcloser.Sender.IsValidTarget(_w.Range))
                {
                    PickACard(Cards.Gold);

                    if (ObjectManager.Player.HasBuff("goldcardpreattack"))
                    {
                        if (Orbwalking.InAutoAttackRange(gapcloser.Sender))
                        {
                            if (Orbwalking.CanAttack())
                            {
                                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                            }
                        }
                    }
                }
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
            {
                if (sender.IsValidTarget(_w.Range))
                {
                    PickACard(Cards.Gold);

                    if (ObjectManager.Player.HasBuff("goldcardpreattack"))
                    {
                        if (Orbwalking.InAutoAttackRange(sender))
                        {
                            if (Orbwalking.CanAttack())
                            {
                                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                            }
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

                if (MenuProvider.Champion.Drawings.DrawWrange.Active && _w.IsReadyPerfectly())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range,
                        MenuProvider.Champion.Drawings.DrawWrange.Color);

                if (MenuProvider.Champion.Drawings.DrawRrange.Active && _r.IsReadyPerfectly())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range,
                        MenuProvider.Champion.Drawings.DrawRrange.Color);
                    Utility.DrawCircle(ObjectManager.Player.Position, _r.Range,
                        MenuProvider.Champion.Drawings.DrawRrange.Color, 2, 30, true);
                }

                if (MenuProvider.Champion.Drawings.GetCircleValue("Draw Flash+AA Range").Active)
                {
                    var target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(null) + 65 + 400,
                        TargetSelector.DamageType.Magical);

                    if (target != null && _flash != SpellSlot.Unknown && _flash.IsReady())
                    {
                        Render.Circle.DrawCircle(ObjectManager.Player.Position,
                            Orbwalking.GetRealAutoAttackRange(null) + 65 + 400, Color.Gold, 5);

                        if (!target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65))
                        {
                            Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Gold);

                            var targetpos = Drawing.WorldToScreen(target.Position);

                            Drawing.DrawText(targetpos[0] - 70, targetpos[1] + 20, Color.Gold, "Flash+AA possible");
                        }
                    }
                    else
                    {
                        Render.Circle.DrawCircle(ObjectManager.Player.Position,
                            Orbwalking.GetRealAutoAttackRange(null) + 65 + 400, Color.Gray, 5);
                    }
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

            if (_w.IsReadyPerfectly())
            {
                damage += _w.GetDamage(enemy, 3);
            }

            return damage;
        }

        /// <summary>
        ///     카드 뽑기
        /// </summary>
        /// <param name="card">뽑을 카드</param>
        private void PickACard(Cards card)
        {
            _cardiNeed = card;

            if (!Picking)
            {
                if (_w.IsReadyPerfectly())
                {
                    if (_w.Instance.Name == "PickACard")
                    {
                        _w.Cast();
                    }
                }
            }
        }

        private enum Cards
        {
            Red,
            Blue,
            Gold,
            None
        }
    }
}