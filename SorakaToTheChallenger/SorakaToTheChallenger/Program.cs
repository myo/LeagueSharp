using System;
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
            Q = new Spell(SpellSlot.Q, 950, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 900, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.283f, 210, 1100, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);

            Menu = new Menu("Soraka To The Challenger", "sttc", true);
            BlacklistMenu = Menu.AddSubMenu(new Menu("Heal blacklist", "sttc.blacklist"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                var championName = ally.CharData.BaseSkinName;
                BlacklistMenu.AddItem(new MenuItem("dontheal" + championName, championName).SetValue(false));
            }
            var orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "sttc.orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);


            Menu.AddItem(
                new MenuItem("sttc.mode", "Play Mode: ").SetValue(new StringList(new[] { "SMART", "AP-SORAKA" })));
            Menu.AddItem(new MenuItem("sttc.wmyhp", "Don't heal (W) if I'm below HP%").SetValue(new Slider(20, 1)));
            Menu.AddItem(new MenuItem("sttc.dontwtanks", "Don't heal (W) tanks").SetValue(true));
            Menu.AddItem(new MenuItem("sttc.ultmyhp", "ULT if I'm below HP%").SetValue(new Slider(20, 1)));
            Menu.AddItem(new MenuItem("sttc.ultallyhp", "ULT if an ally is below HP%").SetValue(new Slider(15)));
            Menu.AddItem(new MenuItem("sttc.blockaa", "Block AutoAttacks?").SetValue(false));

            Menu.AddToMainMenu();
            Game.OnUpdate += OnUpdate;
            Interrupter2.OnInterruptableTarget += (sender, eventArgs) =>
            {
                if (eventArgs.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    var pos = sender.ServerPosition;
                    if (pos.Distance(ObjectManager.Player.ServerPosition) < 900)
                    {
                        E.Cast(pos);
                    }
                }
            };
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        /// <summary>
        /// The On Process Spell Cast
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="args">The Args</param>
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
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
            RLogic();
            WLogic();
            QLogic();
            ELogic();
            Orbwalker.SetAttack(!Menu.Item("sttc.blockaa").GetValue<bool>());
        }

        /// <summary>
        /// The Q Logic
        /// </summary>
        public static void QLogic()
        {
            if (!Q.IsReady()) return;
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
            }
        }

        /// <summary>
        /// The W Logic
        /// </summary>
        public static void WLogic()
        {
            if (!W.IsReady() || ObjectManager.Player.HealthPercent < Menu.Item("sttc.wmyhp").GetValue<Slider>().Value) return;
            var bestHealingCandidate =
                HeroManager.Allies.Where(
                    a =>
                        !a.IsMe && a.Distance(ObjectManager.Player) < 550 &&
                        !BlacklistMenu.Item("dontheal" + a.CharData.BaseSkinName).GetValue<bool>() &&
                        a.MaxHealth - a.Health > GetWHealingAmount())
                    .OrderByDescending(TargetSelector.GetPriority)
                    .ThenBy(ally => ally.Health).FirstOrDefault();
            if (bestHealingCandidate != null)
            {
                if (Menu.Item("sttc.dontwtanks").GetValue<bool>() &&
                    GetWHealingAmount() < 0.03*bestHealingCandidate.MaxHealth) return;
                W.Cast(bestHealingCandidate);
            }
        }

        /// <summary>
        /// The E Logic
        /// </summary>
        public static void ELogic()
        {
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
            foreach (var enemyMinion in ObjectManager.Get<Obj_AI_Base>().Where(m => m.IsEnemy && m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 900 && m.HasBuff("teleport_target", true) || m.HasBuff("Pantheon_GrandSkyfall_Jump", true)))
            {
                E.Cast(enemyMinion.Position);
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
            return new double[] {120, 150, 180, 210, 240}[ObjectManager.Player.GetSpell(SpellSlot.W).Level -1] +
                   0.6*ObjectManager.Player.FlatMagicDamageMod;
        }
    }
}
