using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SorakaToTheChallenger
{
    public static class Program
    {
        /// <summary>
        /// The Q Spell
        /// </summary>
        public static Spell Q;
        /// <summary>
        /// The W Spell
        /// </summary>
        public static Spell W;
        /// <summary>
        /// The E Spell
        /// </summary>
        public static Spell E;
        /// <summary>
        /// The R Spell
        /// </summary>
        public static Spell R;
        /// <summary>
        /// The Menu
        /// </summary>
        public static Menu Menu;
        /// <summary>
        /// The Blacklist Menu
        /// </summary>
        public static Menu BlacklistMenu;
        /// <summary>
        /// The Orbwalker
        /// </summary>
        public static Orbwalking.Orbwalker Orbwalker;
        /// <summary>
        /// The Priority Menu
        /// </summary>
        public static Menu PriorityMenu;

        /// <summary>
        /// The Frankfurt
        /// </summary>
        /// <param name="args">The args</param>
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Load;
        }

        /// <summary>
        /// The Load
        /// </summary>
        /// <param name="args">The args</param>
        public static void Load(EventArgs args)
        {
            if (ObjectManager.Player.CharData.BaseSkinName != "Soraka") return;
            Q = new Spell(SpellSlot.Q, 800, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 900, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.3f, 160, 1600, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1600, false, SkillshotType.SkillshotCircle);

            Menu = new Menu("Soraka To The Challenger", "sttc", true);
            PriorityMenu = Menu.AddSubMenu(new Menu("Heal Priority", "sttc.priority"));
            BlacklistMenu = Menu.AddSubMenu(new Menu("Heal Blacklist", "sttc.blacklist"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                var championName = ally.CharData.BaseSkinName;
                BlacklistMenu.AddItem(new MenuItem("dontheal" + championName, championName).SetValue(false));
            }
            var orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "sttc.orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);


            Menu.AddItem(
                new MenuItem("sttc.mode", "Play Mode: ").SetValue(new StringList(new[] { "SMART", "AP-SORAKA", "ONLYHEAL" })));
            Menu.AddItem(new MenuItem("sttc.wmyhp", "Don't heal (W) if I'm below HP%").SetValue(new Slider(20, 1)));
            Menu.AddItem(new MenuItem("sttc.dontwtanks", "Don't heal (W) tanks").SetValue(true));
            Menu.AddItem(new MenuItem("sttc.ultmyhp", "ULT if I'm below HP%").SetValue(new Slider(20, 1)));
            Menu.AddItem(new MenuItem("sttc.ultallyhp", "ULT if an ally is below HP%").SetValue(new Slider(15)));
            Menu.AddItem(new MenuItem("sttc.blockaa", "Block AutoAttacks?").SetValue(false));
            Menu.AddItem(new MenuItem("sttc.drawq", "Draw Q?")
                .SetValue(new Circle(true, Color.DarkMagenta)));
            Menu.AddItem(new MenuItem("sttc.draww", "Draw W?")
                 .SetValue(new Circle(true, Color.Turquoise)));
            Menu.AddItem(new MenuItem("sttc.drawdebug", "Draw Heal Info?")
                 .SetValue(new Circle(false, Color.White)));

            Menu.AddToMainMenu();
            Game.OnUpdate += OnUpdate;
            Interrupter2.OnInterruptableTarget += (sender, eventArgs) =>
            {
                if (Menu.Item("sttc.mode").GetValue<StringList>().SelectedValue == "ONLYHEAL") return;
                if (eventArgs.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    var pos = sender.ServerPosition;
                    if (pos.Distance(ObjectManager.Player.ServerPosition) < 900)
                    {
                        E.Cast(pos);
                    }
                }
            };
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            GameObject.OnCreate += (sender, eventArgs) =>
            {
                if (Menu.Item("sttc.mode").GetValue<StringList>().SelectedValue == "ONLYHEAL") return;
                if (sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
                {
                    E.Cast(sender.Position);
                }
            };
            Drawing.OnDraw += eventArgs =>
            {
                if (Menu.Item("sttc.drawq").GetValue<Circle>().Active)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 950,
                        Menu.Item("sttc.drawq").GetValue<Circle>().Color,
                        7);
                } 
                if (Menu.Item("sttc.draww").GetValue<Circle>().Active)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 550, W.IsReady() ?
                        Menu.Item("sttc.draww").GetValue<Circle>().Color : Color.Red,
                        7);
                }
                if (Menu.Item("sttc.drawdebug").GetValue<Circle>().Active)
                {
                    if (ObjectManager.Player.GetSpell(SpellSlot.W).Level > 0)
                    {
                        foreach (var healingCandidate in HeroManager.Allies.Where(
                        a =>
                            !a.IsMe && a.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 550 &&
                            !BlacklistMenu.Item("dontheal" + a.CharData.BaseSkinName).GetValue<bool>()))
                        {
                            if (healingCandidate != null)
                            {
                                var wtsPos = Drawing.WorldToScreen(healingCandidate.Position);
                                Drawing.DrawText(wtsPos.X, wtsPos.Y, Menu.Item("sttc.drawdebug").GetValue<Circle>().Color, "1W Heals " + Math.Round(GetWHealingAmount()).ToString() + "HP");
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// The On Enemy Gapcloser
        /// </summary>
        /// <param name="gapcloser">The Gapcloser</param>
        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Menu.Item("sttc.mode").GetValue<StringList>().SelectedValue == "ONLYHEAL") return;
            if (gapcloser.Sender.IsMelee && HeroManager.Allies.Any(a => a.Distance(gapcloser.End) < 200) && ObjectManager.Player.ServerPosition.Distance(gapcloser.Sender.ServerPosition) < 900)
            {
                E.Cast(gapcloser.End);
            }
        }

        /// <summary>
        /// The On Process Spell Cast
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="args">The Args</param>
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Menu.Item("sttc.mode").GetValue<StringList>().SelectedValue == "ONLYHEAL") return;
            if (sender.Type != GameObjectType.obj_AI_Hero) return;
            var target = sender as Obj_AI_Hero;
            var pos = sender.ServerPosition;
            if (sender.CharData.BaseSkinName == "Yasuo")
            {
                if (target.GetSpellSlot(args.SData.Name) == SpellSlot.R &&
                    ObjectManager.Player.ServerPosition.Distance(pos) < 900)
                {
                    E.Cast(target.ServerPosition);
                }
            }
            if (sender.CharData.BaseSkinName == "Vi")
            {
                if (target.GetSpellSlot(args.SData.Name) == SpellSlot.R &&
                    ObjectManager.Player.ServerPosition.Distance(pos) < 900)
                {
                    E.Cast(target.ServerPosition);
                }
            }
        }

        /// <summary>
        /// The OnUpdate
        /// </summary>
        /// <param name="args">The Args</param>
        public static void OnUpdate(EventArgs args)
        {
            WLogic();
            RLogic();
            QLogic();
            ELogic();
            Orbwalker.SetAttack(!Menu.Item("sttc.blockaa").GetValue<bool>());
        }

        /// <summary>
        /// The Q Logic
        /// </summary>
        public static void QLogic()
        {
            if (!Q.IsReady() || (ObjectManager.Player.Mana < 3*GetWManaCost() && CanW())) return;
            switch (Menu.Item("sttc.mode").GetValue<StringList>().SelectedValue)
            {
                case "SMART":
                    if (ObjectManager.Player.MaxHealth - ObjectManager.Player.Health > GetQHealingAmount())
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(925)))
                        {
                            Q.CastIfHitchanceEquals(hero, HitChance.VeryHigh);
                        }
                    }
                    break;
                case "AP-SORAKA":
                    foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(925)))
                    {
                        Q.CastIfHitchanceEquals(hero, HitChance.VeryHigh);
                    }
                    break;
                case "ONLYHEAL":
                    break;
            }
        }

        /// <summary>
        /// The W Logic
        /// </summary>
        public static void WLogic()
        {
            if (!W.IsReady() || !CanW()) return;
            var bestHealingCandidate =
                HeroManager.Allies.Where(
                    a =>
                        !a.IsMe && a.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 550 &&
                        a.MaxHealth - a.Health > GetWHealingAmount())
                    .OrderByDescending(TargetSelector.GetPriority)
                    .ThenBy(ally => ally.Health).FirstOrDefault();
            if (bestHealingCandidate != null)
            {
                if (Menu.SubMenu("sttc.blacklist").Item("dontheal" + bestHealingCandidate.CharData.BaseSkinName) != null &&
                    Menu.SubMenu("sttc.blacklist").Item("dontheal" + bestHealingCandidate.CharData.BaseSkinName).GetValue<bool>())
                {
                    Console.WriteLine("STTC: Skipped healing " + bestHealingCandidate.CharData.BaseSkinName + " because he is blacklisted.");
                    return;
                }
                if (Menu.Item("sttc.dontwtanks").GetValue<bool>() &&
                    10 * GetWHealingAmount() > bestHealingCandidate.Health)
                {
                    Console.WriteLine("STTC: Skipped healing " + bestHealingCandidate.CharData.BaseSkinName + " because he is a tank.");
                    return;
                }
                W.Cast(bestHealingCandidate);
            }
        }

        /// <summary>
        /// The E Logic
        /// </summary>
        public static void ELogic()
        {
            if (Menu.Item("sttc.mode").GetValue<StringList>().SelectedValue == "ONLYHEAL") return;
            if (!E.IsReady()) return;
            var goodTarget =
                HeroManager.Enemies.FirstOrDefault(e => e.IsValidTarget(900) && e.HasBuffOfType(BuffType.Knockup) || e.HasBuffOfType(BuffType.Snare) || e.HasBuffOfType(BuffType.Stun) || e.HasBuffOfType(BuffType.Suppression));
            if (goodTarget != null)
            {
                var pos = goodTarget.ServerPosition;
                if (pos.Distance(ObjectManager.Player.ServerPosition) < 900)
                {
                    E.Cast(goodTarget.ServerPosition);
                }
            } 
            foreach (var enemyMinion in ObjectManager.Get<Obj_AI_Base>().Where(m => m.IsEnemy && m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 900 && m.HasBuff("teleport_target") || m.HasBuff("Pantheon_GrandSkyfall_Jump")))
            {
                Utility.DelayAction.Add(2000, () =>
                {
                    if (enemyMinion.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 900)
                    {
                        E.Cast(enemyMinion.ServerPosition);
                    }
                });
            }
        }

        /// <summary>
        /// The R Logic
        /// </summary>
        public static void RLogic()
        {
            if (!R.IsReady()) return;
            if (ObjectManager.Player.CountEnemiesInRange(800) >= 1 &&
                ObjectManager.Player.HealthPercent <= Menu.Item("sttc.ultmyhp").GetValue<Slider>().Value)
            {
                R.Cast();
            }
            var minAllyHealth = Menu.Item("sttc.ultallyhp").GetValue<Slider>().Value;
            if (minAllyHealth < 1) return;
            foreach (var ally in HeroManager.Allies)
            {
                if (ally.CountEnemiesInRange(800) >= 1 && ally.HealthPercent <= minAllyHealth && !ally.IsZombie && !ally.IsDead)
                {
                    R.Cast();
                }
            }
        }

        /// <summary>
        /// The Get Q Healing Amount
        /// </summary>
        /// <returns>The Q Healing Amount</returns>
        public static double GetQHealingAmount()
        {
            return Math.Min(
                new double[] {25, 35, 45, 55, 65}[ObjectManager.Player.GetSpell(SpellSlot.W).Level -1] +
                0.4*ObjectManager.Player.FlatMagicDamageMod +
                (0.1*(ObjectManager.Player.MaxHealth - ObjectManager.Player.Health)),
                new double[] {50, 70, 90, 110, 130}[ObjectManager.Player.GetSpell(SpellSlot.W).Level -1] +
                0.8*ObjectManager.Player.FlatMagicDamageMod);
        }

        /// <summary>
        /// The Get W Healing Amount
        /// </summary>
        /// <returns>The W Healing Amount</returns>
        public static double GetWHealingAmount()
        {
            return new double[] {120, 150, 180, 210, 240}[ObjectManager.Player.GetSpell(SpellSlot.W).Level -1] +
                   0.6*ObjectManager.Player.FlatMagicDamageMod;
        }

        /// <summary>
        /// The Get R Healing Amount
        /// </summary>
        /// <returns>The R Healing Amount</returns>
        public static double GetRHealingAmount()
        {
            return new double[] {120, 150, 180, 210, 240}[ObjectManager.Player.GetSpell(SpellSlot.R).Level -1] +
                   0.6*ObjectManager.Player.FlatMagicDamageMod;
        }

        public static int GetWManaCost()
        {
            return new[] {40,45,50,55,60}[ObjectManager.Player.GetSpell(SpellSlot.W).Level - 1];
        }

        public static double GetWHealthCost()
        {
            return 0.10*ObjectManager.Player.MaxHealth;
        }

        public static bool CanW()
        {
            return !ObjectManager.Player.InFountain() && ObjectManager.Player.Health - GetWHealthCost() >
            Menu.Item("sttc.wmyhp").GetValue<Slider>().Value/100f*ObjectManager.Player.MaxHealth;
        }
    }
}
