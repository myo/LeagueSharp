#region License
/* Copyright (c) LeagueSharp 2016
 * No reproduction is allowed in any way unless given written consent
 * from the LeagueSharp staff.
 * 
 * Author: imsosharp
 * Date: 2/21/2016
 * File: Soraka.cs
 */
#endregion License

using System;
using System.Linq;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Utils;
using Color = System.Drawing.Color;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace Challenger_Series
{
    public class Soraka : CSPlugin
    {
        public Soraka()
        {
            this.Q = new Spell(SpellSlot.Q, 800);
            this.W = new Spell(SpellSlot.W, 550);
            this.E = new Spell(SpellSlot.E, 900);
            this.R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.26f, 125, 1600, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1600, false, SkillshotType.SkillshotCircle);

            InitializeMenu();

            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        #region Events

        public override void OnUpdate(EventArgs args)
        {
            base.OnUpdate(args);
            if (ObjectManager.Player.IsRecalling()) return;
            WLogic();
            RLogic();
            if (!NoNeedForSpacebarBool && Orbwalker.ActiveMode != OrbwalkingMode.Combo &&
                Orbwalker.ActiveMode != OrbwalkingMode.Hybrid) return;
            QLogic();
            ELogic();
            Orbwalker.SetAttackState(!BlockAutoAttacksBool);
        }

        public override void OnProcessSpellCast(GameObject sender, GameObjectProcessSpellCastEventArgs args)
        {
            base.OnProcessSpellCast(sender, args);
            if (sender is Obj_AI_Hero && sender.IsEnemy)
            {
                var sdata = SpellDatabase.GetByName(args.SData.Name);
                if (sdata != null && args.End.Distance(ObjectManager.Player.ServerPosition) < E.Range && sdata.SpellTags != null &&
                    sdata.SpellTags.Any(st => st == SpellTags.Dash || st == SpellTags.Blink))
                {
                    E.Cast(args.Start.Extend(args.End, sdata.Range));
                }
            }
        }

        public override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            if (DrawW)
                Drawing.DrawCircle(ObjectManager.Player.Position, 550, W.IsReady() ? Color.Turquoise : Color.Red);
            if (DrawQ)
                Drawing.DrawCircle(ObjectManager.Player.Position, 800, Q.IsReady() ? Color.DarkMagenta : Color.Red);
            if (DrawDebugBool)
            {
                foreach (var healingCandidate in GameObjects.AllyHeroes.Where(
                    a =>
                        !a.IsMe && a.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 550 &&
                        !HealBlacklistMenu["dontheal" + a.CharData.BaseSkinName]))
                {
                    if (healingCandidate != null)
                    {
                        var wtsPos = Drawing.WorldToScreen(healingCandidate.Position);
                        Drawing.DrawText(wtsPos.X, wtsPos.Y, Color.White,
                            "1W Heals " + Math.Round(GetWHealingAmount()) + "HP");
                    }
                }
            }
            if (DrawEnemyWaypoints)
            {
                foreach (
                    var e in
                        GameObjects.EnemyHeroes.Where(
                            en => en.IsValidTarget() && en.Distance(ObjectManager.Player) < 2500))
                {
                    var ip = Drawing.WorldToScreen(e.Position); //start pos

                    var wp = MathUtils.GetWaypoints(e);
                    var c = wp.Count - 1;
                    if (wp.Count() <= 1) break;

                    var w = Drawing.WorldToScreen(wp[c].ToVector3()); //endpos

                    Drawing.DrawLine(ip.X, ip.Y, w.X, w.Y, 2, Color.Red);
                }
            }
        }

        #endregion Events

        #region Menu

        private Menu PriorityMenu;
        private Menu HealBlacklistMenu;
        private Menu UltBlacklistMenu;
        private MenuList<string> PlayModeStringList;
        private MenuSlider OnlyQIfMyHPLessThanSlider;
        private MenuBool NoNeedForSpacebarBool;
        private MenuBool DontWTanksBool;
        private MenuSlider ATankTakesXHealsToHealSlider;
        private MenuSlider UseUltForMeIfMyHpIsLessThanSlider;
        private MenuSlider UltIfAnAllyHpIsLessThanSlider;
        private MenuBool CheckIfAllyCanSurviveBool;
        private MenuBool TryToUltAfterIgniteBool;
        private MenuBool BlockAutoAttacksBool;
        private MenuKeyBind DisableQKey;
        private MenuSlider DontHealIfImBelowHpSlider;
        private MenuBool DrawW;
        private MenuBool DrawQ;
        private MenuBool DrawDebugBool;
        private MenuBool DrawEnemyWaypoints;

        public override void InitializeMenu()
        {
            HealBlacklistMenu = MainMenu.Add(new Menu("healblacklist", "Do NOT Heal (W): ", false, "Soraka"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                var championName = ally.CharData.BaseSkinName;
                HealBlacklistMenu.Add(new MenuBool("dontheal" + championName, championName, false));
            }

            UltBlacklistMenu = MainMenu.Add(new Menu("ultblacklist", "Do NOT Ult (R): ", false, "Soraka"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                var championName = ally.CharData.BaseSkinName;
                UltBlacklistMenu.Add(new MenuBool("dontult" + championName, championName, false));
            }

            PriorityMenu = MainMenu.Add(new Menu("sttcselector", "Heal Priority", false, "Soraka"));

            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                PriorityMenu.Add(
                    new MenuSlider("STTCSelector" + ally.ChampionName + "Priority", ally.ChampionName,
                        GetPriorityFromDb(ally.ChampionName), 1, 5));
            }

            PlayModeStringList =
                MainMenu.Add(new MenuList<string>("playmode", "Play Mode: ", new[] {"CHALLENGER", "BRONZE"}));

            OnlyQIfMyHPLessThanSlider =
                MainMenu.Add(new MenuSlider("rakaqonlyifmyhp", "Only Q if my HP < %", 100, 0, 100));

            NoNeedForSpacebarBool = MainMenu.Add(new MenuBool("noneed4spacebar", "PLAY ONLY WITH MOUSE! NO SPACEBAR", true));

            DisableQKey = MainMenu.Add(new MenuKeyBind("UseQ", "DISABLE AUTO Q HOTKEY: ", Keys.L, KeyBindType.Toggle));

            DontHealIfImBelowHpSlider = MainMenu.Add(new MenuSlider("wmyhp", "Don't Heal (W) if Below HP%: ", 20, 1));

            DontWTanksBool = MainMenu.Add(new MenuBool("dontwtanks", "Don't Heal (W) Tanks", true));

            ATankTakesXHealsToHealSlider =
                MainMenu.Add(new MenuSlider("atanktakesxheals", "A TANK takes X Heals (W) to  FULLHP", 15, 5, 30));

            UseUltForMeIfMyHpIsLessThanSlider = MainMenu.Add(new MenuSlider("ultmyhp", "Ult if MY HP% < ", 15, 1, 25));

            UltIfAnAllyHpIsLessThanSlider = MainMenu.Add(new MenuSlider("ultallyhp", "Ult If Ally HP% < ", 15, 5, 35));

            CheckIfAllyCanSurviveBool =
                MainMenu.Add(new MenuBool("checkallysurvivability", "Check if ult will save ally", true));

            TryToUltAfterIgniteBool = MainMenu.Add(new MenuBool("ultafterignite", "ULT (R) after IGNITE", false));

            BlockAutoAttacksBool = MainMenu.Add(new MenuBool("blockaas", "Block AutoAttacks?", true));

            DrawW = MainMenu.Add(new MenuBool("draww", "Draw W?", true));

            DrawQ = MainMenu.Add(new MenuBool("drawq", "Draw Q?", true));

            DrawDebugBool = MainMenu.Add(new MenuBool("drawdebug", "Draw Heal Info", false));

            DrawEnemyWaypoints = MainMenu.Add(new MenuBool("drawenemywaypoints", "Draw Enemy Waypoints"));

            MainMenu.Attach();
        }

        #endregion Menu

        #region ChampionData

        public double GetQHealingAmount()
        {
            var spellLevel = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level;
            if (spellLevel < 1) return 0;
            return Math.Min(
                new double[] {25, 35, 45, 55, 65}[spellLevel - 1] +
                0.4*ObjectManager.Player.FlatMagicDamageMod +
                (0.1*(ObjectManager.Player.MaxHealth - ObjectManager.Player.Health)),
                new double[] {50, 70, 90, 110, 130}[spellLevel - 1] +
                0.8*ObjectManager.Player.FlatMagicDamageMod);
        }

        public double GetWHealingAmount()
        {
            var spellLevel = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level;
            if (spellLevel < 1) return 0;
            return new double[] {120, 150, 180, 210, 240}[spellLevel - 1] +
                   0.6*ObjectManager.Player.FlatMagicDamageMod;
        }

        public double GetRHealingAmount()
        {
            var spellLevel = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level;
            if (spellLevel < 1) return 0;
            return new double[] {120, 150, 180, 210, 240}[spellLevel - 1] +
                   0.6*ObjectManager.Player.FlatMagicDamageMod;
        }

        public int GetWManaCost()
        {
            var spellLevel = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level;
            if (spellLevel < 1) return 0;
            return new[] {40, 45, 50, 55, 60}[spellLevel - 1];
        }

        public double GetWHealthCost()
        {
            return 0.10*ObjectManager.Player.MaxHealth;
        }

        #endregion ChampionData

        #region ChampionLogic

        public bool CanW()
        {
            return !ObjectManager.Player.InFountain() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level >= 1 &&
                   ObjectManager.Player.Health - GetWHealthCost() >
                   DontHealIfImBelowHpSlider.Value/100f*ObjectManager.Player.MaxHealth;
        }

        public void QLogic()
        {
            if (!Q.IsReady() || DisableQKey.Active || (ObjectManager.Player.Mana < 3*GetWManaCost() && CanW())) return;
            var shouldntKS =
                GameObjects.AllyHeroes.Any(
                    h => h.Position.Distance(ObjectManager.Player.Position) < 900 && !h.IsDead && !h.IsMe);
            switch (PlayModeStringList.SelectedValue)
            {
                case "CHALLENGER":
                    if (ObjectManager.Player.CountAllyHeroesInRange(550) >= 1 && ObjectManager.Player.Health > 200 &&
                        ObjectManager.Player.Mana < 100)
                    {
                        return;
                    }
                    if (ObjectManager.Player.HealthPercent <= OnlyQIfMyHPLessThanSlider && ObjectManager.Player.MaxHealth - ObjectManager.Player.Health > GetQHealingAmount())
                    {
                        foreach (var hero in GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(925)))
                        {
                            if (shouldntKS && Q.GetDamage(hero) > hero.Health)
                            {
                                return;
                            }
                            var pred = Q.GetPrediction(hero);
                            if ((int)pred.Hitchance > (int)HitChance.Medium && pred.UnitPosition.Distance(ObjectManager.Player.ServerPosition) < Q.Range)
                            {
                                Q.Cast(pred.UnitPosition);
                            }
                        }
                    }
                    break;
                case "BRONZE":
                    foreach (var hero in GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(925)))
                    {
                        if (shouldntKS && Q.GetDamage(hero) > hero.Health)
                        {
                            return;
                        }
                        var pred = Q.GetPrediction(hero);
                        if ((int)pred.Hitchance > (int)HitChance.Medium && pred.UnitPosition.Distance(ObjectManager.Player.ServerPosition) < Q.Range)
                        {
                            Q.Cast(pred.UnitPosition);
                        }
                    }
                    break;
            }
        }

        public void WLogic()
        {
            if (!W.IsReady() || !CanW()) return;
            var bestHealingCandidate =
                GameObjects.AllyHeroes.Where(
                    a =>
                        !a.IsMe && a.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 550 &&
                        a.MaxHealth - a.Health > GetWHealingAmount() && !a.IsRecalling())
                    .OrderByDescending(GetPriority)
                    .ThenBy(ally => ally.Health).FirstOrDefault();
            if (bestHealingCandidate != null)
            {
                if (HealBlacklistMenu["dontheal" + bestHealingCandidate.CharData.BaseSkinName] != null &&
                    HealBlacklistMenu["dontheal" + bestHealingCandidate.CharData.BaseSkinName].GetValue<MenuBool>())
                {
                    Console.WriteLine("STTC: Skipped healing " + bestHealingCandidate.CharData.BaseSkinName +
                                      " because he is blacklisted.");
                    return;
                }
                if (DontWTanksBool != null && DontWTanksBool.GetValue<MenuBool>() && bestHealingCandidate.Health > 300 &&
                    ATankTakesXHealsToHealSlider.Value*GetWHealingAmount() <
                    bestHealingCandidate.MaxHealth - bestHealingCandidate.Health)
                {
                    Console.WriteLine("STTC: Skipped healing " + bestHealingCandidate.CharData.BaseSkinName +
                                      " because he is a tank.");
                    return;
                }
                W.Cast(bestHealingCandidate);
            }
        }

        public void ELogic()
        {
            if (!E.IsReady()) return;
            var goodTarget =
                GameObjects.EnemyHeroes.FirstOrDefault(
                    e =>
                        e.IsValidTarget(900) && e.HasBuffOfType(BuffType.Knockup) || e.HasBuffOfType(BuffType.Snare) ||
                        e.HasBuffOfType(BuffType.Stun) || e.HasBuffOfType(BuffType.Suppression) || e.IsCharmed ||
                        e.IsCastingInterruptableSpell());
            if (goodTarget != null)
            {
                var pos = goodTarget.ServerPosition;
                if (pos.Distance(ObjectManager.Player.ServerPosition) < 900)
                {
                    E.Cast(goodTarget.ServerPosition);
                }
            }
            foreach (
                var enemyMinion in
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(
                            m =>
                                m.IsEnemy && m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < E.Range &&
                                m.HasBuff("teleport_target")))
            {
                DelayAction.Add(3250, () =>
                {
                    if (enemyMinion.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 900)
                    {
                        E.Cast(enemyMinion.ServerPosition);
                    }
                });
            }
        }

        public void RLogic()
        {
            if (!R.IsReady()) return;
            if (ObjectManager.Player.CountEnemyHeroesInRange(800) >= 1 &&
                ObjectManager.Player.HealthPercent <= UseUltForMeIfMyHpIsLessThanSlider.Value)
            {
                R.Cast();
            }
            var minAllyHealth = UltIfAnAllyHpIsLessThanSlider.Value;
            if (minAllyHealth <= 1) return;
            foreach (var ally in GameObjects.AllyHeroes)
            {
                if (TryToUltAfterIgniteBool && ally.HasBuff("summonerdot") && ally.Health > 400) return;
                if (CheckIfAllyCanSurviveBool && ally.CountAllyHeroesInRange(800) == 0 &&
                    ally.CountEnemyHeroesInRange(800) > 2) return;
                if (ally.CountEnemyHeroesInRange(800) >= 1 && ally.HealthPercent > 2 &&
                    ally.HealthPercent <= minAllyHealth && !ally.IsZombie && !ally.IsDead)
                {
                    R.Cast();
                }
            }
        }

        #endregion ChampionLogic

        #region STTCSelector        

        public float GetPriority(Obj_AI_Hero hero)
        {
            var p = 1;
            if (PriorityMenu["STTCSelector" + hero.ChampionName + "Priority"] != null)
            {
                p = PriorityMenu["STTCSelector" + hero.ChampionName + "Priority"].GetValue<MenuSlider>().Value;
            }
            else
            {
                p = GetPriorityFromDb(hero.ChampionName);
            }

            switch (p)
            {
                case 2:
                    return 1.5f;
                case 3:
                    return 1.75f;
                case 4:
                    return 2f;
                case 5:
                    return 2.5f;
                default:
                    return 1f;
            }
        }

        private static int GetPriorityFromDb(string championName)
        {
            string[] p1 =
            {
                "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
                "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
                "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona",
                "Taric", "TahmKench", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra"
            };

            string[] p2 =
            {
                "Aatrox", "Darius", "Elise", "Evelynn", "Galio", "Gangplank", "Gragas", "Irelia", "Jax",
                "Lee Sin", "Maokai", "Morgana", "Nocturne", "Pantheon", "Poppy", "Rengar", "Rumble", "Ryze", "Swain",
                "Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao", "RekSai"
            };

            string[] p3 =
            {
                "Akali", "Diana", "Ekko", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce", "Kassadin",
                "Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir", "Yasuo",
                "Zilean"
            };

            string[] p4 =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Kindred",
                "Leblanc", "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra",
                "Talon", "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "Velkoz", "Viktor",
                "Xerath", "Zed", "Ziggs", "Jhin", "Soraka"
            };

            if (p1.Contains(championName))
            {
                return 1;
            }
            if (p2.Contains(championName))
            {
                return 2;
            }
            if (p3.Contains(championName))
            {
                return 3;
            }
            return p4.Contains(championName) ? 4 : 1;
        }

        #endregion STTCSelector

    }
}