using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using HERMES_Kalista.Utils;
using SharpDX;
using SpellSlot = LeagueSharp.SpellSlot;

namespace HERMES_Kalista.MyUtils
{
    public class MActivator
    {
        public Menu Config = Program.ActivatorMenu;
        private Obj_AI_Hero _player;
        private Obj_AI_Hero target;
        //private StreamWriter log;
        private int checkCCTick;

        #region Items
        MItem qss = new MItem("Quicksilver Sash", "QSS", "qss", 3140, ItemTypeId.Purifier);
        MItem mercurial = new MItem("ItemMercurial", "Mercurial", "mercurial", 3139, ItemTypeId.Purifier);
        MItem bilgewater = new MItem("BilgewaterCutlass", "Bilgewater", "bilge", 3144, ItemTypeId.Offensive, 450);
        MItem king = new MItem("ItemSwordOfFeastAndFamine", "BoRKing", "king", 3153, ItemTypeId.Offensive, 450);
        MItem youmus = new MItem("YoumusBlade", "Youmuu's", "youmus", 3142, ItemTypeId.Offensive);
        MItem hpPot = new MItem("Health Potion", "HP Pot", "hpPot", 2003, ItemTypeId.HPRegenerator);
        MItem manaPot = new MItem("Mana Potion", "Mana Pot", "manaPot", 2004, ItemTypeId.ManaRegenerator);
        MItem biscuit = new MItem("Total Biscuit of Rejuvenation", "Biscuit", "biscuit", 2010, ItemTypeId.HPRegenerator);
        MItem cFlaskHP = new MItem("Crystalline Flask", "Cryst. Flask HP", "cFlaskHP", 2041, ItemTypeId.HPRegenerator);
        MItem cFlaskMP = new MItem("Crystalline Flask", "Cryst. Flask MP", "cFlaskMP", 2041, ItemTypeId.ManaRegenerator);
        #endregion

        #region SummonerSpells
        // Heal prioritizes the allied champion closest to the cursor at the time the ability is cast.
        // If no allied champions are near the cursor, Heal will target the most wounded allied champion in range.
        MItem heal = new MItem("Heal", "Heal", "SummonerHeal", 0, ItemTypeId.DeffensiveSpell, 700); // 300? www.gamefaqs.com/pc/954437-league-of-legends/wiki/3-1-summoner-spells
        MItem exhaust = new MItem("Exhaust", "Exhaust", "SummonerExhaust", 0, ItemTypeId.OffensiveSpell, 650); //summonerexhaust, low, debuff (buffs)
        MItem barrier = new MItem("Barrier", "Barrier", "SummonerBarrier", 0, ItemTypeId.DeffensiveSpell);
        MItem cleanse = new MItem("Cleanse", "Cleanse", "SummonerBoost", 0, ItemTypeId.PurifierSpell);
        MItem clarity = new MItem("Clarity", "Clarity", "SummonerMana", 0, ItemTypeId.ManaRegeneratorSpell, 600);
        MItem ignite = new MItem("Ignite", "Ignite", "SummonerDot", 0, ItemTypeId.OffensiveSpell, 600);
        MItem smite = new MItem("Smite", "Smite", "SummonerSmite", 0, ItemTypeId.OffensiveSpell, 500);
        MItem smiteAOE = new MItem("SmiteAOE", "smite AOE", "itemsmiteaoe", 0, ItemTypeId.OffensiveSpell, 500);
        MItem smiteDuel = new MItem("SmiteDuel", "smite Duel", "s5_summonersmiteduel", 0, ItemTypeId.OffensiveSpell, 500);
        MItem smiteQuick = new MItem("SmiteQuick", "smite Quick", "s5_summonersmitequick", 0, ItemTypeId.OffensiveSpell, 500);
        MItem smiteGanker = new MItem("SmiteGanker", "smite Ganker", "s5_summonersmiteplayerganker", 0, ItemTypeId.OffensiveSpell, 500);
        #endregion

        #region Jungle Minions
        MMinion blue = new MMinion("SRU_Blue", "Blue", 6, 143);
        MMinion red = new MMinion("SRU_Red", "Red", 6, 143);
        MMinion dragon = new MMinion("SRU_Dragon", "Dragon", 6, 143);
        MMinion baron = new MMinion("SRU_Baron", "Baron", -18, 192);
        MMinion wolf = new MMinion("SRU_Murkwolf", "Murkwolf", 41, 74);
        MMinion razor = new MMinion("SRU_Razorbeak", "Razor", 39, 74); // Ghosts
        MMinion krug = new MMinion("SRU_Krug", "Krug", 38, 80);
        MMinion crab = new MMinion("Sru_Crab", "Crab", 43, 62);
        MMinion gromp = new MMinion("SRU_Gromp", "Gromp", 32, 87); // Ghost
        MMinion tVilemaw = new MMinion("TT_Spiderboss", "Vilemaw", 45, 67);
        MMinion tWraith = new MMinion("TT_NWraith", "Wraith", 45, 67);
        MMinion tGolem = new MMinion("TT_NGolem", "Golem", 45, 67);
        MMinion tWolf = new MMinion("TT_NWolf", "Wolf", 45, 67);
        #endregion

        public MActivator()
        {
            CustomEvents.Game.OnGameLoad += onLoad;
        }

        private void onLoad(EventArgs args)
        {
            try
            {
                _player = ObjectManager.Player;
                checkCCTick = LeagueSharp.Common.Utils.TickCount;
                createMenu();

                LeagueSharp.Drawing.OnDraw += onDraw;
                Game.OnUpdate += onGameUpdate;
                Game.OnEnd += Game_OnGameEnd;
            }
            catch
            {
                Console.WriteLine("MasterActivator error creating menu!");
            }
        }

        private void Game_OnGameEnd(GameEndEventArgs args)
        {
            //log.Close();
        }
        private void onDraw(EventArgs args)
        {
            try
            {
                if (Config.Item("drawStatus").IsActive())
                {
                    Drawing.DrawText(Drawing.Width - 120, 80, Config.Item("enabled").IsActive() ? System.Drawing.Color.Green : System.Drawing.Color.Red, "MActivator");
                }

                if (Config.Item("dSmite").GetValue<bool>())
                {
                    MMinion[] jungleMinions;
                    if (Utility.Map.GetMap().Type.Equals(Utility.Map.MapType.TwistedTreeline))
                    {
                        jungleMinions = new MMinion[] { tVilemaw, tWraith, tWolf, tGolem };
                    }
                    else
                    {
                        jungleMinions = new MMinion[] { blue, red, razor, baron, krug, wolf, dragon, gromp, crab };
                    }

                    var minions = MinionManager.GetMinions(_player.Position, 1500, MinionTypes.All, MinionTeam.Neutral);
                    if (minions.Count() > 0)
                    {
                        foreach (Obj_AI_Base minion in minions)
                        {
                            if (minion.IsHPBarRendered && !minion.IsDead)
                            {
                                foreach (MMinion jMinion in jungleMinions)
                                {
                                    if (minion.Name.StartsWith(jMinion.name) && ((minion.Name.Length - jMinion.name.Length) <= 6) && Config.Item(jMinion.name).GetValue<bool>() && Config.Item("justAS").GetValue<bool>() ||
                                    minion.Name.StartsWith(jMinion.name) && ((minion.Name.Length - jMinion.name.Length) <= 6) && !Config.Item("justAS").GetValue<bool>())
                                    {
                                        Vector2 hpBarPos = minion.HPBarPosition;
                                        hpBarPos.X += jMinion.preX;
                                        hpBarPos.Y += 18;

                                        int smiteDmg = getSmiteDmg();
                                        var damagePercent = smiteDmg / minion.MaxHealth;
                                        float hpXPos = hpBarPos.X + (jMinion.width * damagePercent);
                                        Drawing.DrawLine(hpXPos, hpBarPos.Y, hpXPos, hpBarPos.Y + 5, 2, smiteDmg >= minion.Health ? System.Drawing.Color.Lime : System.Drawing.Color.WhiteSmoke);

                                        // Draw camp
                                        if (Config.Item("dCamp").IsActive())
                                        {
                                            Drawing.DrawCircle(minion.Position, minion.BoundingRadius + smite.range + _player.BoundingRadius, _player.Distance(minion, false) <= (smite.range + minion.BoundingRadius + _player.BoundingRadius) ? System.Drawing.Color.Lime : System.Drawing.Color.WhiteSmoke);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem with MasterActivator(Drawing).");
            }
        }

        private void onGameUpdate(EventArgs args)
        {
            if (Config.Item("enabled").GetValue<KeyBind>().Active)
            {
                try
                {
                    checkAndUse(clarity);
                    if (!_player.InFountain() && !Config.Item("justPredHeal").GetValue<bool>())
                    {
                        teamCheckAndUse(heal, Config.Item("useWithHealDebuff").GetValue<bool>() ? "" : "summonerhealcheck");
                    }

                    checkAndUse(cleanse);
                    checkAndUse(qss);
                    checkAndUse(mercurial);

                    checkAndUse(manaPot, "FlaskOfCrystalWater");
                    checkAndUse(hpPot, "RegenerationPotion");
                    checkAndUse(biscuit, "ItemMiniRegenPotion");
                    checkAndUse(cFlaskHP, "RegenerationPotion");
                    checkAndUse(cFlaskMP, "FlaskOfCrystalWater");

                    if (!Config.Item("justPred").GetValue<bool>() || !Config.Item("predict").GetValue<bool>())
                    {
                        checkAndUse(barrier);
                    }

                    checkAndUse(smite);
                    checkAndUse(smiteAOE);
                    checkAndUse(smiteDuel);
                    checkAndUse(smiteGanker);
                    checkAndUse(smiteQuick);

                    if (Config.Item("comboModeActive").GetValue<KeyBind>().Active)
                    {
                        combo();
                    }
                }
                catch
                {
                    Console.WriteLine("MasterActivator presented a problem, and has been disabled!");
                    Config.Item("enabled").SetValue<KeyBind>(new KeyBind('L', KeyBindType.Toggle, false)); // Check
                }
            }
        }

        private void combo()
        {
            checkAndUse(ignite);
            checkAndUse(youmus);
            checkAndUse(bilgewater);
            checkAndUse(king);
        }

        private void ksDrawDmg(MItem item, Obj_AI_Base minion, MMinion jMinion, Vector2 hpBarPos, float hpXPos)
        {
            if (Config.Item(item.menuVariable) != null)
            {
                if (Config.Item(item.menuVariable).GetValue<bool>() && Config.Item(item.menuVariable + "drawBar").GetValue<bool>())
                {
                    float spellDmg = (float)Damage.GetSpellDamage(_player, minion, item.abilitySlot);
                    var spellDmgPercent = spellDmg / minion.MaxHealth;

                    hpXPos = hpBarPos.X + (jMinion.width * spellDmgPercent);
                    Drawing.DrawLine(hpXPos, hpBarPos.Y, hpXPos, hpBarPos.Y + 5, 2, spellDmg >= minion.Health ? System.Drawing.Color.BlueViolet : System.Drawing.Color.Black);
                }
            }
        }

        // And about ignore HP% check?
        private void justUseAgainstCheck(MItem item, double incDmg, Obj_AI_Base attacker = null, Obj_AI_Base attacked = null, SpellSlot attackerSpellSlot = SpellSlot.Unknown, AttackId attackId = AttackId.Unknown)
        {
            // Se tem o spell
            if (Utility.GetSpellSlot(_player, item.name) != SpellSlot.Unknown)
            {
                if (attacker != null && attacked != null)
                {
                    bool use = false;
                    if (attackId != AttackId.Unknown)
                    {
                        switch (attackId)
                        {
                            case AttackId.Basic:
                                use = Config.Item("basic" + item.menuVariable).GetValue<bool>();
                                break;
                            case AttackId.Ignite:
                                use = Config.Item("king" + item.menuVariable).GetValue<bool>();
                                break;
                            case AttackId.King:
                                use = Config.Item("ignite" + item.menuVariable).GetValue<bool>();
                                break;
                            case AttackId.Tower:
                                use = Config.Item("tower" + item.menuVariable).GetValue<bool>();
                                break;
                            case AttackId.Spell:
                                use = Config.Item(item.menuVariable + attacker.BaseSkinName).GetValue<bool>() && Config.Item(attackerSpellSlot + item.menuVariable + attacker.BaseSkinName).GetValue<bool>();
                                break;
                        }
                    }

                    if (use)
                    {
                        bool ignoreHP = false;
                        if (attackId == AttackId.Spell)
                        {
                            ignoreHP = Config.Item("ignore" + item.menuVariable + attacker.BaseSkinName).GetValue<bool>();
                        }

                        if (item.type == ItemTypeId.Ability && attacked.IsMe)
                        {
                            checkAndUse(item, "", incDmg, ignoreHP);
                        }
                        else if (item.type == ItemTypeId.TeamAbility)
                        {
                            teamCheckAndUse(item, "", incDmg, attacked, attacker, ignoreHP);
                        }
                    }
                }
                // OFF JustPred
                else
                {
                    checkAndUse(item, "", incDmg);
                    teamCheckAndUse(item, "", incDmg, attacked);
                }
            }
        }

        private bool checkBuff(String name)
        {
            var searchedBuff = from buff in _player.Buffs
                               where buff.Name == name
                               select buff;

            return searchedBuff.Count() <= 0 ? false : true;
        }

        private void createMenuItem(MItem item, String parent, int defaultValue = 0, bool mana = false, int minManaPct = 0)
        {
            if (item.type == ItemTypeId.Ability || item.type == ItemTypeId.TeamAbility)
            {
                var abilitySlot = Utility.GetSpellSlot(_player, item.name);
                if (abilitySlot != SpellSlot.Unknown && abilitySlot == item.abilitySlot)
                {
                    var menu = new Menu(item.menuName, "menu" + item.menuVariable);
                    menu.AddItem(new MenuItem(item.menuVariable, "Enable").SetValue(true));
                    menu.AddItem(new MenuItem(item.menuVariable + "UseOnPercent", "Use on HP%")).SetValue(new Slider(defaultValue, 0, 100));
                    if (minManaPct > 0)
                    {
                        menu.AddItem(new MenuItem(item.menuVariable + "UseManaPct", "Min Mana%")).SetValue(new Slider(minManaPct, 0, 100));
                    }
                    var menuUseAgainst = new Menu("Filter", "UseAgainst");
                    menuUseAgainst.AddItem(new MenuItem("tower" + item.menuVariable, "Tower").SetValue(true));
                    menuUseAgainst.AddItem(new MenuItem("ignite" + item.menuVariable, "Ignite").SetValue(true));
                    menuUseAgainst.AddItem(new MenuItem("king" + item.menuVariable, "BoRKing").SetValue(false));
                    menuUseAgainst.AddItem(new MenuItem("basic" + item.menuVariable, "Basic ATK").SetValue(false));

                    var enemyHero = from hero in ObjectManager.Get<Obj_AI_Hero>()
                                    where hero.Team != _player.Team
                                    select hero;

                    if (enemyHero.Count() > 0)
                    {
                        foreach (Obj_AI_Hero hero in enemyHero)
                        {
                            var menuUseAgainstHero = new Menu(hero.BaseSkinName, "useAgainst" + hero.BaseSkinName);
                            menuUseAgainstHero.AddItem(new MenuItem(item.menuVariable + hero.BaseSkinName, "Enabled").SetValue(true));
                            menuUseAgainstHero.AddItem(new MenuItem(SpellSlot.Q + item.menuVariable + hero.BaseSkinName, "Q").SetValue(false));
                            menuUseAgainstHero.AddItem(new MenuItem(SpellSlot.W + item.menuVariable + hero.BaseSkinName, "W").SetValue(false));
                            menuUseAgainstHero.AddItem(new MenuItem(SpellSlot.E + item.menuVariable + hero.BaseSkinName, "E").SetValue(false));
                            menuUseAgainstHero.AddItem(new MenuItem(SpellSlot.R + item.menuVariable + hero.BaseSkinName, "R").SetValue(false));
                            menuUseAgainstHero.AddItem(new MenuItem("ignore" + item.menuVariable + hero.BaseSkinName, "Ignore %HP").SetValue(true));
                            menuUseAgainst.AddSubMenu(menuUseAgainstHero);
                            // Bring all, passives, summoners spells, etc;
                            /*if (hero.Spellbook.Spells.Count() > 0)
                            {
                                var menuUseAgainstHero = new Menu(hero.BaseSkinName, "useAgainst" + hero.BaseSkinName);
                                menuUseAgainstHero.AddItem(new MenuItem(item.menuVariable, "Enable").SetValue(true));
                                foreach(SpellDataInst spell in hero.Spellbook.Spells)
                                {
                                    menuUseAgainstHero.AddItem(new MenuItem("useAgainstSpell" + spell.Name, spell.Name).SetValue(true));
                                }
                                menuUseAgainst.AddSubMenu(menuUseAgainstHero);
                            }
                            else
                            {
                                Console.WriteLine("MasterActivator cant get " + hero.BaseSkinName + " spells!");
                            }*/
                        }
                    }
                    menu.AddSubMenu(menuUseAgainst);
                    Config.SubMenu(parent).AddSubMenu(menu);
                }
            }
            else if (item.type == ItemTypeId.KSAbility)
            {
                var abilitySlot = Utility.GetSpellSlot(_player, item.name);
                if (abilitySlot != SpellSlot.Unknown && abilitySlot == item.abilitySlot)
                {
                    var ksAbMenu = new Menu(item.menuName, "menu" + item.menuVariable);
                    ksAbMenu.AddItem(new MenuItem(item.menuVariable, "Enable").SetValue(true));
                    //choRMenu.AddItem(new MenuItem(choR.menuVariable + "plus", "Plus").SetValue(false));
                    ksAbMenu.AddItem(new MenuItem(item.menuVariable + "drawRange", "Draw Range").SetValue(true));
                    ksAbMenu.AddItem(new MenuItem(item.menuVariable + "drawBar", "Draw Bar").SetValue(true));
                    Config.SubMenu(parent).AddSubMenu(ksAbMenu);
                }
            }
            else
            {
                var menu = new Menu(item.menuName, "menu" + item.menuVariable);
                menu.AddItem(new MenuItem(item.menuVariable, "Enable").SetValue(true));

                if (defaultValue != 0)
                {
                    if (item.type == ItemTypeId.OffensiveAOE)
                    {
                        menu.AddItem(new MenuItem(item.menuVariable + "UseXUnits", "On X Units")).SetValue(new Slider(defaultValue, 1, 5));
                    }
                    else
                    {
                        menu.AddItem(new MenuItem(item.menuVariable + "UseOnPercent", "Use on " + (mana == false ? "%HP" : "%Mana"))).SetValue(new Slider(defaultValue, 0, 100));
                    }
                }
                Config.SubMenu(parent).AddSubMenu(menu);
            }
        }

        private void teamCheckAndUse(MItem item, String buff = "", double incDmg = 0, Obj_AI_Base attacked = null, Obj_AI_Base attacker = null, bool ignoreHP = false)
        {
            if (Config.Item(item.menuVariable) != null)
            {
                // check if is configured to use
                if (Config.Item(item.menuVariable).GetValue<bool>())
                {
                    #region DeffensiveSpell ManaRegeneratorSpell PurifierSpell
                    if (item.type == ItemTypeId.DeffensiveSpell || item.type == ItemTypeId.ManaRegeneratorSpell || item.type == ItemTypeId.PurifierSpell)
                    {
                        //Console.WriteLine("TCandU-> " + item.name);
                        var spellSlot = Utility.GetSpellSlot(_player, item.menuVariable);
                        if (spellSlot != SpellSlot.Unknown)
                        {
                            var activeAllyHeros = getActiveAllyHeros(item);
                            if (activeAllyHeros.Count() > 0)
                            {
                                int usePercent = Config.Item(item.menuVariable + "UseOnPercent").GetValue<Slider>().Value;

                                foreach (Obj_AI_Hero hero in activeAllyHeros)
                                {
                                    //Console.WriteLine("Hero-> " + hero.SkinName);
                                    int enemyInRange = Utility.CountEnemiesInRange(hero, 700);
                                    if (enemyInRange >= 1)
                                    {
                                        int actualHeroHpPercent = (int)(((_player.Health - incDmg) / _player.MaxHealth) * 100); //after dmg not Actual ^^
                                        int actualHeroManaPercent = (int)((_player.Mana / _player.MaxMana) * 100);

                                        //Console.WriteLine("actHp% -> " + actualHeroHpPercent + "   useOn%-> " + usePercent + "  IncDMG-> " + incDmg);

                                        if ((item.type == ItemTypeId.DeffensiveSpell && actualHeroHpPercent <= usePercent) ||
                                            (item.type == ItemTypeId.ManaRegeneratorSpell && actualHeroManaPercent <= usePercent))
                                        {
                                            _player.Spellbook.CastSpell(spellSlot);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    #region TeamAbility TeamAbilityAOE
                    else if (item.type == ItemTypeId.TeamAbility)
                    {
                        try
                        {
                            if (!Config.Item(attacked.SkinName).GetValue<bool>())
                            {
                                return;
                            }

                            if (_player.Distance(attacked, false) <= item.range)
                            {
                                var spellSlot = Utility.GetSpellSlot(_player, item.name);
                                if (spellSlot != SpellSlot.Unknown)
                                {
                                    if (_player.Spellbook.CanUseSpell(spellSlot) == SpellState.Ready)
                                    {
                                        int usePercent = !ignoreHP ? Config.Item(item.menuVariable + "UseOnPercent").GetValue<Slider>().Value : 100;
                                        int manaPercent = Config.Item(item.menuVariable + "UseManaPct") != null ? Config.Item(item.menuVariable + "UseManaPct").GetValue<Slider>().Value : 0;

                                        int actualHeroHpPercent = (int)(((attacked.Health - incDmg) / attacked.MaxHealth) * 100); //after dmg not Actual ^^
                                        int playerManaPercent = (int)((_player.Mana / _player.MaxMana) * 100);
                                        if (playerManaPercent >= manaPercent && actualHeroHpPercent <= usePercent)
                                        {
                                            if (item.type == ItemTypeId.TeamAbility && item.spellType != SpellType.SkillShotCircle && item.spellType != SpellType.SkillShotCone && item.spellType != SpellType.SkillShotLine)
                                            {
                                                _player.Spellbook.CastSpell(item.abilitySlot, attacked);
                                            }
                                            else
                                            {
                                                Vector3 pos = attacked.Position;
                                                // extend 20 to attacker direction THIS 20 COST RANGE
                                                if (attacker != null)
                                                {
                                                    if (_player.Distance(attacked.Position.Extend(attacker.Position, 20), false) <= item.range)
                                                    {
                                                        pos = attacked.Position.Extend(attacker.Position, 20);
                                                    }
                                                }
                                                _player.Spellbook.CastSpell(item.abilitySlot, pos);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Problem with MasterActivator(AutoShieldTeam).");
                            Console.WriteLine(e);
                        }
                    }
                    #endregion
                    #region Others
                    else
                    {
                        if (Items.HasItem(item.id))
                        {
                            if (Items.CanUseItem(item.id))
                            {
                                var activeAllyHeros = getActiveAllyHeros(item);
                                if (activeAllyHeros.Count() > 0)
                                {
                                    foreach (Obj_AI_Hero hero in activeAllyHeros)
                                    {
                                        #region Purifier
                                        if (item.type == ItemTypeId.Purifier)
                                        {
                                            if ((Config.Item("defJustOnCombo").GetValue<bool>() && Config.Item("comboModeActive").GetValue<KeyBind>().Active) ||
                                            (!Config.Item("defJustOnCombo").GetValue<bool>()))
                                            {
                                                if (checkCC(hero))
                                                {
                                                    useItem(item.id, hero);
                                                }
                                            }
                                        }
                                        #endregion
                                        #region Deffensive
                                        else if (item.type == ItemTypeId.Deffensive)
                                        {
                                            int enemyInRange = Utility.CountEnemiesInRange(hero, 700);
                                            if (enemyInRange >= 1)
                                            {
                                                int usePercent = Config.Item(item.menuVariable + "UseOnPercent").GetValue<Slider>().Value;
                                                int actualHeroHpPercent = (int)((hero.Health / hero.MaxHealth) * 100);
                                                if (actualHeroHpPercent <= usePercent)
                                                {
                                                    if (item.spellType == SpellType.Self)
                                                    {
                                                        useItem(item.id);
                                                    }
                                                    else
                                                    {
                                                        useItem(item.id, hero);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        private IEnumerable<Obj_AI_Hero> getActiveAllyHeros(MItem item)
        {
            var activeAllyHeros = from hero in ObjectManager.Get<Obj_AI_Hero>()
                                  where hero.Team == _player.Team &&
                                        Config.Item(hero.SkinName).GetValue<bool>() &&
                                        hero.Distance(_player, false) <= item.range &&
                                        !hero.IsDead
                                  select hero;

            return activeAllyHeros;
        }

        private void checkAndUse(MItem item, String buff = "", double incDamage = 0, bool ignoreHP = false)
        {
            try
            {
                if (Config.Item(item.menuVariable) != null)
                {
                    // check if is configured to use
                    if (Config.Item(item.menuVariable).GetValue<bool>())
                    {
                        int actualHeroHpPercent = (int)(((_player.Health - incDamage) / _player.MaxHealth) * 100); //after dmg not Actual ^^
                        int actualHeroManaPercent = (int)(_player.MaxMana > 0 ? ((_player.Mana / _player.MaxMana) * 100) : 0);

                        #region DeffensiveSpell ManaRegeneratorSpell PurifierSpell OffensiveSpell KSAbility
                        if (item.type == ItemTypeId.DeffensiveSpell || item.type == ItemTypeId.ManaRegeneratorSpell || item.type == ItemTypeId.PurifierSpell || item.type == ItemTypeId.OffensiveSpell || item.type == ItemTypeId.KSAbility)
                        {
                            var spellSlot = Utility.GetSpellSlot(_player, item.menuVariable);
                            if (spellSlot != SpellSlot.Unknown)
                            {
                                if (_player.Spellbook.CanUseSpell(spellSlot) == SpellState.Ready)
                                {
                                    if (item.type == ItemTypeId.DeffensiveSpell)
                                    {
                                        int usePercent = Config.Item(item.menuVariable + "UseOnPercent").GetValue<Slider>().Value;
                                        if (actualHeroHpPercent <= usePercent)
                                        {
                                            _player.Spellbook.CastSpell(spellSlot);
                                        }
                                    }
                                    else if (item.type == ItemTypeId.ManaRegeneratorSpell)
                                    {
                                        int usePercent = Config.Item(item.menuVariable + "UseOnPercent").GetValue<Slider>().Value;
                                        if (actualHeroManaPercent <= usePercent && !_player.InFountain())
                                        {
                                            _player.Spellbook.CastSpell(spellSlot);
                                        }
                                    }
                                    else if (item.type == ItemTypeId.PurifierSpell)
                                    {
                                        if ((Config.Item("defJustOnCombo").GetValue<bool>() && Config.Item("comboModeActive").GetValue<KeyBind>().Active) ||
                                            (!Config.Item("defJustOnCombo").GetValue<bool>()))
                                        {
                                            if (checkCC(_player))
                                            {
                                                _player.Spellbook.CastSpell(spellSlot);
                                                checkCCTick = LeagueSharp.Common.Utils.TickCount + 2500;
                                            }
                                        }
                                    }
                                    else if (item.type == ItemTypeId.OffensiveSpell || item.type == ItemTypeId.KSAbility)
                                    {
                                        #region Ignite
                                        if (item == ignite)
                                        {
                                            // TargetSelector.TargetingMode.LowHP FIX/Check
                                            Obj_AI_Hero target = TargetSelector.GetTarget(item.range); // Check about DamageType
                                            if (target != null)
                                            {

                                                var aaspeed = _player.AttackSpeedMod;
                                                float aadmg = 0;

                                                // attack speed checks
                                                if (aaspeed < 0.8f)
                                                    aadmg = _player.FlatPhysicalDamageMod * 3;
                                                else if (aaspeed > 1f && aaspeed < 1.3f)
                                                    aadmg = _player.FlatPhysicalDamageMod * 5;
                                                else if (aaspeed > 1.3f && aaspeed < 1.5f)
                                                    aadmg = _player.FlatPhysicalDamageMod * 7;
                                                else if (aaspeed > 1.5f && aaspeed < 1.7f)
                                                    aadmg = _player.FlatPhysicalDamageMod * 9;
                                                else if (aaspeed > 2.0f)
                                                    aadmg = _player.FlatPhysicalDamageMod * 11;

                                                // Will calculate for base hp regen, currenthp, etc
                                                float dmg = (_player.Level * 20) + 50;
                                                float regenpersec = (target.FlatHPRegenMod + (target.HPRegenRate * target.Level));
                                                float dmgafter = (dmg - ((regenpersec * 5) / 2));

                                                float aaleft = (dmgafter + target.Health / _player.FlatPhysicalDamageMod);
                                                //var pScreen = Drawing.WorldToScreen(target.Position);

                                                if (target.Health < (dmgafter + aadmg) && _player.Distance(target, false) <= item.range)
                                                {
                                                    bool overIgnite = Config.Item("overIgnite").GetValue<bool>();
                                                    if ((!overIgnite && !target.HasBuff("summonerdot")) || overIgnite)
                                                    {
                                                        _player.Spellbook.CastSpell(spellSlot, target);
                                                        //Drawing.DrawText(pScreen[0], pScreen[1], System.Drawing.Color.Crimson, "Kill in " + aaleft);
                                                    }

                                                }

                                            }
                                        }
                                        #endregion
                                        else
                                        {
                                            try
                                            {
                                                string[] jungleMinions;
                                                if (Utility.Map.GetMap().Type.Equals(Utility.Map.MapType.TwistedTreeline))
                                                {
                                                    jungleMinions = new string[] { tVilemaw.name, tWraith.name, tGolem.name, tWolf.name };
                                                }
                                                else
                                                {
                                                    jungleMinions = new string[] { blue.name, red.name, razor.name, baron.name, krug.name, wolf.name, dragon.name, gromp.name, crab.name };
                                                }

                                                float searchRange = (item.range + 300); // Get minions in 800 range

                                                var minions = MinionManager.GetMinions(_player.Position, searchRange, MinionTypes.All, MinionTeam.Neutral);
                                                if (minions.Count() > 0)
                                                {
                                                    int smiteDmg = getSmiteDmg();

                                                    foreach (Obj_AI_Base minion in minions)
                                                    {
                                                        float range = item.range + minion.BoundingRadius + _player.BoundingRadius;
                                                        if (_player.Distance(minion, false) <= range)
                                                        {
                                                            int dmg = item.type == ItemTypeId.OffensiveSpell ? smiteDmg : (int)Damage.GetSpellDamage(_player, minion, spellSlot);
                                                            if (minion.Health <= dmg && jungleMinions.Any(name => minion.Name.StartsWith(name) && ((minion.Name.Length - name.Length) <= 6) && Config.Item(name).GetValue<bool>()))
                                                            {
                                                                if (item.spellType == SpellType.SkillShotLine || item.spellType == SpellType.SkillShotCone || item.spellType == SpellType.SkillShotCircle)
                                                                {
                                                                    _player.Spellbook.CastSpell(spellSlot, minion.Position);
                                                                }
                                                                else
                                                                {
                                                                    _player.Spellbook.CastSpell(spellSlot, item.spellType == SpellType.Self ? null : minion);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                Console.WriteLine("Problem with MasterActivator(Smite).");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        else if (item.type == ItemTypeId.Ability || item.type == ItemTypeId.TeamAbility)
                        {
                            try
                            {
                                var spellSlot = Utility.GetSpellSlot(_player, item.name);
                                if (spellSlot != SpellSlot.Unknown)
                                {
                                    if (_player.Spellbook.CanUseSpell(spellSlot) == SpellState.Ready)
                                    {
                                        int usePercent = !ignoreHP ? Config.Item(item.menuVariable + "UseOnPercent").GetValue<Slider>().Value : 100;
                                        int manaPercent = Config.Item(item.menuVariable + "UseManaPct") != null ? Config.Item(item.menuVariable + "UseManaPct").GetValue<Slider>().Value : 0;
                                        //Console.WriteLine("ActualMana%-> " + actualHeroManaPercent + "  Mana%->" + manaPercent + "  Acthp%->" + actualHeroHpPercent + "   Use%->" + usePercent);

                                        if (actualHeroManaPercent >= manaPercent && actualHeroHpPercent <= usePercent)
                                        {
                                            if (item.spellType == SpellType.TargetEnemy)
                                            {
                                                if (checkTarget(item.range))
                                                {
                                                    _player.Spellbook.CastSpell(item.abilitySlot, target);
                                                }
                                            }
                                            else
                                            {
                                                _player.Spellbook.CastSpell(item.abilitySlot, _player);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Problem with MasterActivator(AutoShield).");
                                Console.WriteLine(e);
                            }
                        }
                        else
                        {
                            if (Items.HasItem(item.id))
                            {
                                //Console.WriteLine("Tem item->" + item.id + item.name);
                                if (Items.CanUseItem(item.id))
                                {
                                    if (item.type == ItemTypeId.Offensive)
                                    {
                                        if (checkTarget(item.range))
                                        {
                                            int actualTargetHpPercent = (int)((target.Health / target.MaxHealth) * 100);
                                            if (checkUsePercent(item, actualTargetHpPercent))
                                            {
                                                useItem(item.id, (item.range == 0 || item.spellType == SpellType.Self) ? null : target);
                                            }
                                        }
                                    }
                                    else if (item.type == ItemTypeId.OffensiveAOE)
                                    {
                                        if (checkTarget(item.range))
                                        {
                                            // FIX-ME: In frost case, we must check the affected area, not just ppl in range(item).
                                            if (Utility.CountEnemiesInRange(_player, (int)item.range) >= Config.Item(item.menuVariable + "UseXUnits").GetValue<Slider>().Value)
                                            {
                                                useItem(item.id, (item.range == 0 || item.spellType == SpellType.Self) ? null : target);
                                            }
                                        }
                                    }
                                    else if (item.type == ItemTypeId.HPRegenerator)
                                    {
                                        if (checkUsePercent(item, actualHeroHpPercent) && !_player.InFountain() && !Utility.IsRecalling(_player))
                                        {
                                            if ((buff != "" && !checkBuff(buff)) || buff == "")
                                            {
                                                useItem(item.id);
                                            }
                                        }
                                    }
                                    else if (item.type == ItemTypeId.Deffensive)
                                    {
                                        if (checkUsePercent(item, actualHeroHpPercent) && !_player.InFountain() && (Config.Item("useRecalling").GetValue<bool>() || !Utility.IsRecalling(_player)))
                                        {
                                            if ((buff != "" && !checkBuff(buff)) || buff == "")
                                            {
                                                useItem(item.id);
                                            }
                                        }
                                    }
                                    else if (item.type == ItemTypeId.ManaRegenerator)
                                    {
                                        if (checkUsePercent(item, actualHeroManaPercent) && !_player.InFountain() && !Utility.IsRecalling(_player))
                                        {
                                            if ((buff != "" && !checkBuff(buff)) || buff == "")
                                            {
                                                useItem(item.id);
                                            }
                                        }
                                    }
                                    else if (item.type == ItemTypeId.Buff)
                                    {
                                        if (checkTarget(item.range))
                                        {
                                            if (!checkBuff(item.name))
                                            {
                                                useItem(item.id);
                                            }
                                        }
                                        else
                                        {
                                            if (checkBuff(item.name))
                                            {
                                                useItem(item.id);
                                            }
                                        }
                                    }
                                    else if (item.type == ItemTypeId.Purifier)
                                    {
                                        if ((Config.Item("defJustOnCombo").GetValue<bool>() && Config.Item("comboModeActive").GetValue<KeyBind>().Active) ||
                                            (!Config.Item("defJustOnCombo").GetValue<bool>()))
                                        {
                                            if (checkCC(_player))
                                            {
                                                useItem(item.id);
                                                checkCCTick = LeagueSharp.Common.Utils.TickCount + 2500;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void useItem(int id, Obj_AI_Hero target = null)
        {
            try
            {
                if (Items.HasItem(id))
                {
                    if (Items.CanUseItem(id))
                    {
                        Items.UseItem(id, target);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void useItem(int id, Vector3 target)
        {
            try
            {
                if (Items.HasItem(id))
                {
                    if (Items.CanUseItem(id))
                    {
                        Items.UseItem(id, target);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private int getSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }


        private bool checkUsePercent(MItem item, int actualPercent)
        {
            int usePercent = Config.Item(item.menuVariable + "UseOnPercent").GetValue<Slider>().Value;
            return actualPercent <= usePercent ? true : false;
        }

        private bool checkTarget(float range)
        {
            if (range == 0)
            {
                range = _player.AttackRange + 125;
            }

            target = TargetSelector.GetTarget(range);

            return target != null ? true : false;
        }

        private void createMenu()
        {
            Config = Program.ActivatorMenu;
            Config.AddSubMenu(new Menu("Purifiers", "purifiers"));
            createMenuItem(qss, "purifiers");
            createMenuItem(mercurial, "purifiers");
            createMenuItem(cleanse, "purifiers");
            Config.SubMenu("purifiers").AddItem(new MenuItem("defJustOnCombo", "Just on combo")).SetValue(false);

            Config.AddSubMenu(new Menu("Purify", "purify"));
            Config.SubMenu("purify").AddItem(new MenuItem("ccDelay", "Delay(ms)").SetValue(new Slider(0, 0, 2500)));
            Config.SubMenu("purify").AddItem(new MenuItem("blind", "Blind")).SetValue(true);
            Config.SubMenu("purify").AddItem(new MenuItem("charm", "Charm")).SetValue(true);
            Config.SubMenu("purify").AddItem(new MenuItem("fear", "Fear")).SetValue(true);
            Config.SubMenu("purify").AddItem(new MenuItem("flee", "Flee")).SetValue(true);
            Config.SubMenu("purify").AddItem(new MenuItem("snare", "Snare")).SetValue(true);
            Config.SubMenu("purify").AddItem(new MenuItem("taunt", "Taunt")).SetValue(true);
            Config.SubMenu("purify").AddItem(new MenuItem("suppression", "Suppression")).SetValue(true);
            Config.SubMenu("purify").AddItem(new MenuItem("stun", "Stun")).SetValue(true);
            Config.SubMenu("purify").AddItem(new MenuItem("polymorph", "Polymorph")).SetValue(false);
            Config.SubMenu("purify").AddItem(new MenuItem("silence", "Silence")).SetValue(false);
            Config.SubMenu("purify").AddItem(new MenuItem("dehancer", "Dehancer")).SetValue(false);
            Config.SubMenu("purify").AddItem(new MenuItem("zedultexecute", "Zed Ult")).SetValue(true);
            Config.SubMenu("purify").AddItem(new MenuItem("dispellExhaust", "Exhaust")).SetValue(false);
            Config.SubMenu("purify").AddItem(new MenuItem("dispellEsNumeroUno", "Es Numero Uno")).SetValue(false);

            Config.AddSubMenu(new Menu("Smite", "smiteCfg"));
            var menuSmiteSpell = new Menu("Spell", "smiteSpell");
            menuSmiteSpell.AddItem(new MenuItem(smite.menuVariable, smite.menuName).SetValue(true));
            menuSmiteSpell.AddItem(new MenuItem(smiteAOE.menuVariable, smiteAOE.menuName).SetValue(true));
            menuSmiteSpell.AddItem(new MenuItem(smiteDuel.menuVariable, smiteDuel.menuName).SetValue(true));
            menuSmiteSpell.AddItem(new MenuItem(smiteGanker.menuVariable, smiteGanker.menuName).SetValue(true));
            menuSmiteSpell.AddItem(new MenuItem(smiteQuick.menuVariable, smiteQuick.menuName).SetValue(true));
            Config.SubMenu("smiteCfg").AddSubMenu(menuSmiteSpell);

            var menuSmiteMobs = new Menu("Mob", "smiteMobs");
            if (Utility.Map.GetMap().Type.Equals(Utility.Map.MapType.TwistedTreeline))
            {
                menuSmiteMobs.AddItem(new MenuItem("TT_Spiderboss", "Vilemaw")).SetValue(true);
                menuSmiteMobs.AddItem(new MenuItem("TT_NWraith", "Wraith")).SetValue(false);
                menuSmiteMobs.AddItem(new MenuItem("TT_NGolem", "Golem")).SetValue(true);
                menuSmiteMobs.AddItem(new MenuItem("TT_NWolf", "Wolf")).SetValue(true);
            }
            else
            {
                menuSmiteMobs.AddItem(new MenuItem(blue.name, blue.menuName)).SetValue(true);
                menuSmiteMobs.AddItem(new MenuItem(red.name, red.menuName)).SetValue(true);
                menuSmiteMobs.AddItem(new MenuItem(dragon.name, dragon.menuName)).SetValue(true);
                menuSmiteMobs.AddItem(new MenuItem(baron.name, baron.menuName)).SetValue(true);
                menuSmiteMobs.AddItem(new MenuItem(razor.name, razor.menuName)).SetValue(false);
                menuSmiteMobs.AddItem(new MenuItem(krug.name, krug.menuName)).SetValue(false);
                menuSmiteMobs.AddItem(new MenuItem(wolf.name, wolf.menuName)).SetValue(false);
                menuSmiteMobs.AddItem(new MenuItem(gromp.name, gromp.menuName)).SetValue(false);
                menuSmiteMobs.AddItem(new MenuItem(crab.name, crab.menuName)).SetValue(false);

            }
            Config.SubMenu("smiteCfg").AddSubMenu(menuSmiteMobs);

            var menuSmiteDraw = new Menu("Draw", "smiteDraw");
            menuSmiteDraw.AddItem(new MenuItem("dSmite", "Enabled")).SetValue(true);
            menuSmiteDraw.AddItem(new MenuItem("dCamp", "Camp")).SetValue(true);
            menuSmiteDraw.AddItem(new MenuItem("justAS", "Just Selected Mobs")).SetValue(false);
            Config.SubMenu("smiteCfg").AddSubMenu(menuSmiteDraw);

            Config.AddSubMenu(new Menu("Offensive", "offensive"));
            createMenuItem(ignite, "offensive");
            Config.SubMenu("offensive").SubMenu("menu" + ignite.menuVariable).AddItem(new MenuItem("overIgnite", "Over Ignite")).SetValue(false);
            createMenuItem(youmus, "offensive", 100);
            createMenuItem(bilgewater, "offensive", 100);
            createMenuItem(king, "offensive", 100);

            Config.AddSubMenu(new Menu("Deffensive", "deffensive"));
            Config.SubMenu("deffensive").AddItem(new MenuItem("justPred", "Just Predicted")).SetValue(true);
            Config.SubMenu("deffensive").AddItem(new MenuItem("useRecalling", "Use Recalling")).SetValue(false);

            Config.AddSubMenu(new Menu("Regenerators", "regenerators"));
            createMenuItem(heal, "regenerators", 35);
            Config.SubMenu("regenerators").SubMenu("menu" + heal.menuVariable).AddItem(new MenuItem("useWithHealDebuff", "Use with debuff")).SetValue(true);
            Config.SubMenu("regenerators").SubMenu("menu" + heal.menuVariable).AddItem(new MenuItem("justPredHeal", "Just predicted")).SetValue(false);
            createMenuItem(clarity, "regenerators", 25, true);
            createMenuItem(hpPot, "regenerators", 55);
            createMenuItem(manaPot, "regenerators", 55, true);
            createMenuItem(biscuit, "regenerators", 55);
            createMenuItem(cFlaskHP, "regenerators", 40);
            createMenuItem(cFlaskMP, "regenerators", 40, true);

            Config.AddSubMenu(new Menu("Team Use", "teamUseOn"));

            var allyHeros = from hero in ObjectManager.Get<Obj_AI_Hero>()
                            where hero.IsAlly == true
                            select hero.SkinName;

            foreach (String allyHero in allyHeros)
            {
                Config.SubMenu("teamUseOn").AddItem(new MenuItem(allyHero, allyHero)).SetValue(true);
            }

            // Combo mode
            Config.AddSubMenu(new Menu("Combo Mode", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("comboModeActive", "Active")).SetValue(new KeyBind(32, KeyBindType.Press, true));

            // Target selector
            Config.AddSubMenu(new Menu("Target Selector", "targetSelector"));
            TargetSelector.AddToMenu(Config.SubMenu("targetSelector"));

            Config.AddItem(new MenuItem("predict", "Predict DMG")).SetValue(true);

            Config.AddItem(new MenuItem("drawStatus", "Draw Status")).SetValue(true);
            Config.AddItem(new MenuItem("enabled", "Enabled")).SetValue(new KeyBind('L', KeyBindType.Toggle, true));
        }

        private bool checkCC(Obj_AI_Hero hero)
        {
            bool cc = false;

            if (checkCCTick > LeagueSharp.Common.Utils.TickCount)
            {
                Console.WriteLine("tick");
                return cc;
            }

            if (Config.Item("blind").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Blind))
                {
                    cc = true;
                }
            }

            if (Config.Item("charm").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Charm))
                {
                    cc = true;
                }
            }

            if (Config.Item("fear").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Fear))
                {
                    cc = true;
                }
            }

            if (Config.Item("flee").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Flee))
                {
                    cc = true;
                }
            }

            if (Config.Item("snare").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Snare))
                {
                    cc = true;
                }
            }

            if (Config.Item("taunt").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Taunt))
                {
                    cc = true;
                }
            }

            if (Config.Item("suppression").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Suppression))
                {
                    cc = true;
                }
            }

            if (Config.Item("stun").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Stun))
                {
                    cc = true;
                }
            }

            if (Config.Item("polymorph").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Polymorph))
                {
                    cc = true;
                }
            }

            if (Config.Item("silence").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Silence))
                {
                    cc = true;
                }
            }

            if (Config.Item("dehancer").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.CombatDehancer))
                {
                    cc = true;
                }
            }

            if (Config.Item("zedultexecute").GetValue<bool>())
            {
                if (hero.HasBuff("zedultexecute"))
                {
                    cc = true;
                }
            }

            if (Config.Item("dispellExhaust").GetValue<bool>())
            {
                if (hero.HasBuff(exhaust.menuVariable))
                {
                    cc = true;
                }
            }

            if (Config.Item("dispellEsNumeroUno").GetValue<bool>())
            {
                if (hero.HasBuff("MordekaiserCOTGPet"))
                {
                    cc = true;
                }
            }

            checkCCTick = LeagueSharp.Common.Utils.TickCount + Config.Item("ccDelay").GetValue<Slider>().Value;
            return cc;
        }
    }

    class MItem
    {
        public String name { get; set; }
        public String menuName { get; set; }
        public String menuVariable { get; set; }
        public int id { get; set; }
        public float range { get; set; }
        public ItemTypeId type { get; set; }
        public SpellSlot abilitySlot { get; set; }
        public SpellType spellType { get; set; }

        public MItem(String name, String menuName, String menuVariable, int id, ItemTypeId type, float range = 0, SpellSlot abilitySlot = SpellSlot.Unknown, SpellType spellType = SpellType.TargetAll)
        {
            this.name = name;
            this.menuVariable = menuVariable;
            this.menuName = menuName;
            this.id = id;
            this.range = range;
            this.type = type;
            this.abilitySlot = abilitySlot;
            this.spellType = spellType;
        }
    }

    class MMinion
    {
        public String name { get; set; }
        public String menuName { get; set; }
        public float preX { get; set; }
        public float width { get; set; }

        public MMinion(String name, String menuName, float preX, float width)
        {
            this.name = name;
            this.menuName = menuName;
            this.preX = preX;
            this.width = width;
        }
    }

    public enum AttackId
    {
        Unknown = -1,
        Q = 0,
        W = 1,
        E = 2,
        R = 3,
        King = 4,
        Basic = 5,
        Ignite = 6,
        Tower = 7,
        Spell = 8
    }

    public enum ItemTypeId
    {
        Offensive = 0,
        Purifier = 1,
        HPRegenerator = 2,
        ManaRegenerator = 3,
        Deffensive = 4,
        Buff = 5,
        DeffensiveSpell = 6,
        PurifierSpell = 7,
        ManaRegeneratorSpell = 8,
        OffensiveSpell = 9,
        Ability = 10,
        OffensiveAOE = 11,
        TeamAbility = 12,
        Ward = 13,
        VisionWard = 14,
        KSAbility = 15
    }

    public enum SpellType
    {
        SkillShotCircle = 0,
        SkillShotCone = 1,
        SkillShotLine = 2,
        TargetAll = 3,
        TargetEnemy = 4,
        TargetTeam = 5,
        Self = 6
    }
}