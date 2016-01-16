using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Vayne.Utils;
using SharpDX;
using SpellSlot = LeagueSharp.SpellSlot;

namespace PRADA_Vayne.MyUtils
{
    public class MActivator
    {
        public Menu Config = Program.ActivatorMenu;
        private Obj_AI_Hero _player;
        private Obj_AI_Hero target;
        private int checkCCTick;

        #region Items
        MItem qss = new MItem("Quicksilver Sash", "QSS", "qss", 3140, ItemTypeId.Purifier);
        MItem mercurial = new MItem("ItemMercurial", "Mercurial", "mercurial", 3139, ItemTypeId.Purifier);
        MItem bilgewater = new MItem("BilgewaterCutlass", "Bilgewater", "bilge", 3144, ItemTypeId.Offensive, 450);
        MItem king = new MItem("ItemSwordOfFeastAndFamine", "BoRKing", "king", 3153, ItemTypeId.Offensive, 450);
        MItem youmus = new MItem("YoumusBlade", "Youmuu's", "youmus", 3142, ItemTypeId.Offensive);
        MItem hpPot = new MItem("Health Potion", "HP Pot", "hpPot", 2003, ItemTypeId.HPRegenerator);
        MItem biscuit = new MItem("Total Biscuit of Rejuvenation", "Biscuit", "biscuit", 2010, ItemTypeId.HPRegenerator);
        #endregion

        #region SummonerSpells
        // Heal prioritizes the allied champion closest to the cursor at the time the ability is cast.
        // If no allied champions are near the cursor, Heal will target the most wounded allied champion in range.
        MItem heal = new MItem("Heal", "Heal", "SummonerHeal", 0, ItemTypeId.DeffensiveSpell, 700); // 300? www.gamefaqs.com/pc/954437-league-of-legends/wiki/3-1-summoner-spells
        MItem exhaust = new MItem("Exhaust", "Exhaust", "SummonerExhaust", 0, ItemTypeId.OffensiveSpell, 650); //summonerexhaust, low, debuff (buffs)
        MItem cleanse = new MItem("Cleanse", "Cleanse", "SummonerBoost", 0, ItemTypeId.PurifierSpell);
        MItem ignite = new MItem("Ignite", "Ignite", "SummonerDot", 0, ItemTypeId.OffensiveSpell, 600);
        #endregion

        public MActivator()
        {
            CustomEvents.Game.OnGameLoad += onLoad;
        }

        private void onLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            checkCCTick = LeagueSharp.Common.Utils.TickCount;
            createMenu();

            Game.OnUpdate += onGameUpdate;
        }

        private void onGameUpdate(EventArgs args)
        {
            if (Config.Item("enabled").GetValue<KeyBind>().Active)
            {
                if (ObjectManager.Player.InFountain() || ObjectManager.Player.CountEnemiesInRange(1400) < 1) return;
                checkAndUse(cleanse);
                checkAndUse(qss);
                checkAndUse(mercurial);
                checkAndUse(hpPot, "RegenerationPotion");
                checkAndUse(biscuit, "ItemMiniRegenPotion");

                if (Config.Item("comboModeActive").GetValue<KeyBind>().Active)
                {
                    combo();
                }
            }
        }

        private void combo()
        {
            if (ObjectManager.Player.CountEnemiesInRange(600) >= 1)
            {
                checkAndUse(ignite);
                checkAndUse(youmus);
                checkAndUse(bilgewater);
                checkAndUse(king);
                checkAndUse(heal);
            }
        }

        private bool checkBuff(string name)
        {
            return _player.Buffs.Any(buf => buf.Name == name);
        }

        private void createMenuItem(MItem item, String parent, int defaultValue = 0, bool mana = false,
            int minManaPct = 0)
        {
            var menu = new Menu(item.menuName, "menu" + item.menuVariable);
            menu.AddItem(new MenuItem(item.menuVariable, "Enable").SetValue(true));

            if (defaultValue != 0)
            {
                menu.AddItem(new MenuItem(item.menuVariable + "UseOnPercent",
                    "Use on " + (mana == false ? "%HP" : "%Mana"))).SetValue(new Slider(defaultValue, 0, 100));
            }
            Config.SubMenu(parent).AddSubMenu(menu);
        }

        private void checkAndUse(MItem item, String buff = "", double incDamage = 0, bool ignoreHP = false)
        {
            if (Config.Item(item.menuVariable) != null)
            {
                // check if is configured to use
                if (Config.Item(item.menuVariable).GetValue<bool>())
                {
                    int actualHeroHpPercent = (int) (((_player.Health - incDamage)/_player.MaxHealth)*100);
                        //after dmg not Actual ^^
                    int actualHeroManaPercent = (int) (_player.MaxMana > 0 ? ((_player.Mana/_player.MaxMana)*100) : 0);

                    #region DeffensiveSpell ManaRegeneratorSpell PurifierSpell OffensiveSpell KSAbility

                    if (item.type == ItemTypeId.DeffensiveSpell ||
                        item.type == ItemTypeId.PurifierSpell || item.type == ItemTypeId.OffensiveSpell)
                    {
                        var spellSlot = Utility.GetSpellSlot(_player, item.menuVariable);
                        if (spellSlot != SpellSlot.Unknown)
                        {
                            if (_player.Spellbook.CanUseSpell(spellSlot) == SpellState.Ready)
                            {
                                if (item.type == ItemTypeId.PurifierSpell)
                                {
                                    if ((Config.Item("defJustOnCombo").GetValue<bool>() &&
                                         Config.Item("comboModeActive").GetValue<KeyBind>().Active) ||
                                        (!Config.Item("defJustOnCombo").GetValue<bool>()))
                                    {
                                        if (checkCC(_player))
                                        {
                                            _player.Spellbook.CastSpell(spellSlot);
                                            checkCCTick = LeagueSharp.Common.Utils.TickCount + 2500;
                                        }
                                    }
                                }
                            }
                        }
                    }
                        #endregion

                    else
                    {
                        if (Items.HasItem(item.id))
                        {
                            if (Items.CanUseItem(item.id))
                            {
                                if (item.type == ItemTypeId.Offensive)
                                {
                                    if (checkTarget(item.range))
                                    {
                                        int actualTargetHpPercent = (int) ((target.Health/target.MaxHealth)*100);
                                        if (checkUsePercent(item, actualTargetHpPercent))
                                        {
                                            Items.UseItem(item.id,
                                                (item.range == 0 || item.spellType == SpellType.Self) ? null : target);
                                        }
                                    }
                                }
                                else if (item.type == ItemTypeId.HPRegenerator)
                                {
                                    if (checkUsePercent(item, actualHeroHpPercent) && !_player.InFountain() &&
                                        !Utility.IsRecalling(_player))
                                    {
                                        if ((buff != "" && !checkBuff(buff)) || buff == "")
                                        {
                                            Items.UseItem(item.id);
                                        }
                                    }
                                }
                                else if (item.type == ItemTypeId.Purifier)
                                {
                                    if ((Config.Item("defJustOnCombo").GetValue<bool>() &&
                                         Config.Item("comboModeActive").GetValue<KeyBind>().Active) ||
                                        (!Config.Item("defJustOnCombo").GetValue<bool>()))
                                    {
                                        if (checkCC(_player))
                                        {
                                            Items.UseItem(item.id);
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

            Config.AddSubMenu(new Menu("Offensive", "offensive"));
            createMenuItem(ignite, "offensive");
            Config.SubMenu("offensive").SubMenu("menu" + ignite.menuVariable).AddItem(new MenuItem("overIgnite", "Over Ignite")).SetValue(false);
            createMenuItem(youmus, "offensive", 100);
            createMenuItem(bilgewater, "offensive", 100);
            createMenuItem(king, "offensive", 100);

            Config.AddSubMenu(new Menu("Regenerators", "regenerators"));
            createMenuItem(heal, "regenerators", 35);
            Config.SubMenu("regenerators").SubMenu("menu" + heal.menuVariable).AddItem(new MenuItem("useWithHealDebuff", "Use with debuff")).SetValue(true);
            createMenuItem(hpPot, "regenerators", 55);
            createMenuItem(biscuit, "regenerators", 55);

            // Combo mode
            Config.AddSubMenu(new Menu("Combo Mode", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("comboModeActive", "Active")).SetValue(new KeyBind(32, KeyBindType.Press, true));

            // Target selector
            Config.AddSubMenu(new Menu("Target Selector", "targetSelector"));
            TargetSelector.AddToMenu(Config.SubMenu("targetSelector"));

            Config.AddItem(new MenuItem("enabled", "Enabled")).SetValue(false);
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

        public MItem(String name, String menuName, String menuVariable, int id, ItemTypeId type, float range = 0)
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

    public enum ItemTypeId
    {
        Offensive = 0,
        Purifier = 1,
        HPRegenerator = 2,
        Deffensive = 3,
        DeffensiveSpell = 4,
        PurifierSpell = 5,
        OffensiveSpell = 6,
    }
}