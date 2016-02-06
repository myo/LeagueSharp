using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mastery_Badge_Spammer
{
    public static class Program
    {
        public static Menu Menu;
        public static int LastEmoteSpam = 0;
        public static int MyKills = 0;
        public static int MyAssits = 0;
        public static int MyDeaths = 0;
        public static Random Random;
        public static SpellSlot FlashSlot = SpellSlot.Unknown;
        public static SpellSlot IgniteSlot = SpellSlot.Unknown;
        public static string[] KnownDisrespectStarts = new[] {"", "gj ", "nice ", "wp ", "lol gj ", "nice 1 ", "gg ", "very wp ", "ggwp ", "sweet ", "ty ", "thx ", "wow nice ", "lol ", "wow ", "so good ", "heh ", "hah ", "haha ", "hahaha ", "hahahaha ", "u did well ", "you did well ", "loved it ", "loved that ", "love u ", "love you ", "ahaha ", "ahahaha "};
        public static string[] KnownDisrespectEndings = new[] {"", " XD", " XDD", " XDDD", " XDDD", "XDDDD", " haha", " hahaha", " hahahaha", " ahaha"," ahahaha"," lol"," rofl", " roflmao"};
        public static int LastDeathNetworkId = 0;
        public static int LastChat = 0;
        public static Dictionary<int, int> DeathsHistory = new Dictionary<int, int>(); 
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static void OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Mastery Emote Spammer", "masteryemotespammermenu", true);
            Menu.AddItem(new MenuItem("mode", "Mode").SetValue(new StringList(new[] { "MASTERY", "LAUGH" })));
            Menu.AddItem(
                new MenuItem("chatdisrespectmode", "Chat Disrespect Mode").SetValue(
                    new StringList(new[] { "DISABLED", "CHAMPION NAME", "SUMMONER NAME" })));
            Menu.AddItem(new MenuItem("onkill", "After Kill").SetValue(true));
            Menu.AddItem(new MenuItem("onassist", "After Assist").SetValue(true));
            Menu.AddItem(new MenuItem("ondeath", "After Death").SetValue(false));
            Menu.AddItem(new MenuItem("neardead", "Near Dead Bodies").SetValue(true));
            Menu.AddItem(new MenuItem("ondodgedskillshot", "After you dodge a skillshot").SetValue(true));
            Menu.AddItem(new MenuItem("afterignite", "Dubstep Ignite").SetValue(true));
            Menu.AddItem(new MenuItem("afterflash", "Challenger Flash").SetValue(false));
            Menu.AddItem(new MenuItem("afterq", "After Q").SetValue(false));
            Menu.AddItem(new MenuItem("afterw", "After W").SetValue(false));
            Menu.AddItem(new MenuItem("aftere", "After E").SetValue(false));
            Menu.AddItem(new MenuItem("afterr", "After R").SetValue(false));
            Menu.AddItem(new MenuItem("usepackets", "PACKET CASTING? (GARENA FIX)").SetValue(false));
            Menu.AddItem(new MenuItem("guccimode", "Are you THE GUCCI?").SetValue(false));
            Menu.AddToMainMenu();
            Random = new Random();
            FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            
            //init chat disrespekter
            foreach (var en in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
            {
                DeathsHistory.Add(en.NetworkId, en.Deaths);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var sData = SpellDatabase.GetByName(args.SData.Name);
            if (Menu.Item("ondodgedskillshot").GetValue<bool>() && sender.IsEnemy && sData != null &&
                ObjectManager.Player.Distance(sender) < sData.Range)
            {
                Utility.DelayAction.Add((int)Math.Round(sData.Delay + sender.Distance(ObjectManager.Player)/sData.MissileSpeed), DoEmote);
            }
            if (sender.IsMe)
            {
                if (args.Slot == SpellSlot.Q && Menu.Item("afterq").GetValue<bool>())
                {
                    Utility.DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (args.Slot == SpellSlot.W && Menu.Item("afterw").GetValue<bool>())
                {
                    Utility.DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (args.Slot == SpellSlot.E && Menu.Item("aftere").GetValue<bool>())
                {
                    Utility.DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (args.Slot == SpellSlot.R && Menu.Item("afterr").GetValue<bool>())
                {
                    Utility.DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (IgniteSlot != SpellSlot.Unknown && args.Slot == IgniteSlot && Menu.Item("afterignite").GetValue<bool>())
                {
                    Utility.DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (FlashSlot != SpellSlot.Unknown && args.Slot == FlashSlot && Menu.Item("afterflash").GetValue<bool>())
                {
                    Utility.DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.ChampionsKilled > MyKills && Menu.Item("onkill").GetValue<bool>())
            {
                MyKills = ObjectManager.Player.ChampionsKilled;
                DoEmote();
            }
            if (ObjectManager.Player.Assists > MyAssits && Menu.Item("onassist").GetValue<bool>())
            {
                MyAssits = ObjectManager.Player.Assists;
                DoEmote();
            }
            if (ObjectManager.Player.Deaths > MyDeaths && Menu.Item("ondeath").GetValue<bool>())
            {
                MyDeaths = ObjectManager.Player.Deaths;
                DoEmote();
            }
            if (Menu.Item("neardead").GetValue<bool>() &&
                ObjectManager.Get<Obj_AI_Hero>()
                    .Any(h => h.IsEnemy && h.IsVisible && h.IsDead && ObjectManager.Player.Distance(h) < 300))
            {
                DoEmote();
            }

            switch (Menu.Item("chatdisrespectmode").GetValue<StringList>().SelectedValue)
            {
                case "DISABLED":
                    break;
                case "CHAMPION NAME":
                    foreach (var en in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
                    {
                        if (DeathsHistory.FirstOrDefault(record => record.Key == en.NetworkId).Value < en.Deaths)
                        {
                            var championName = en.ChampionName.ToLower();
                            DeathsHistory.Remove(en.NetworkId);
                            DeathsHistory.Add(en.NetworkId, en.Deaths);
                            if (en.Distance(ObjectManager.Player) < 1000)
                            {
                                Utility.DelayAction.Add(Random.Next(1000, 5000), () => DoChatDisrespect(championName));
                            }
                        }
                    }
                    break;
                case "SUMMONER NAME":
                    foreach (var en in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
                    {
                        if (DeathsHistory.FirstOrDefault(record => record.Key == en.NetworkId).Value < en.Deaths)
                        {
                            var name = en.Name.ToLower();
                            DeathsHistory.Remove(en.NetworkId);
                            DeathsHistory.Add(en.NetworkId, en.Deaths);
                            if (en.Distance(ObjectManager.Player) < 1000)
                            {
                                Utility.DelayAction.Add(Random.Next(1000, 5000), () => DoChatDisrespect(name));
                            }
                        }
                    }
                    break;
            }
        }

        public static void DoEmote()
        {
            if (Utils.GameTimeTickCount - LastEmoteSpam > Random.Next(5000, 15000))
            {
                LastEmoteSpam = Utils.GameTimeTickCount;
                if (Menu.Item("usepackets").GetValue<bool>())
                {
                    Game.SendEmote(Emote.Laugh);
                    return;
                }
                Game.Say(Menu.Item("mode").GetValue<StringList>().SelectedValue == "MASTERY" ? "/masterybadge" : "/l");
            }
        }

        public static void DoChatDisrespect(string theTarget)
        {
            if (Utils.GameTimeTickCount - LastChat > Random.Next(5000, 20000))
            {
                LastChat = Utils.GameTimeTickCount;
                if (Menu.Item("guccimode").GetValue<bool>() && Random.Next(0,1) == 1)
                {
                    switch (Random.Next(0, 15))
                    {
                        case 0:
                            Game.Say(String.Format("/all HAHA {0} that was a refreshing experience!", theTarget));
                            break;
                        case 1:
                            Game.Say(String.Format("/all LOL {0} no match for me!", theTarget));
                            break;
                        case 2:
                            Game.Say(String.Format("/all Fantastic performance right there {0}!", theTarget));
                            break;
                        case 3:
                            Game.Say(String.Format("/all Can't touch this {0}", theTarget));
                            break;
                        case 4:
                            Game.Say(String.Format("/all {0}, you have been reformed!", theTarget));
                            break;
                        case 5:
                            Game.Say(String.Format("/all Completely smashed there {0}", theTarget));
                            break;
                        case 6:
                            Game.Say(String.Format("/all haha pathetic {0}", theTarget));
                            break;
                        case 7:
                            Game.Say(String.Format("/all true display of skill {0}", theTarget));
                            break;
                        case 8:
                            Game.Say(String.Format("/all better luck next time {0}", theTarget));
                            break;
                        case 9:
                            Game.Say(String.Format("/all Nice try for a monkey {0}", theTarget));
                            break;
                        case 10:
                            Game.Say(String.Format("/all I see you've set aside this special time to humiliate yourself in public {0}", theTarget));
                            break;
                        case 11:
                            Game.Say(String.Format("/all Who lit the fuse on your tampon {0}?", theTarget));
                            break;
                        case 12:
                            Game.Say(String.Format("/all I like you {0}. You remind me of myself when I was young and stupid. ", theTarget));
                            break;
                        case 13:
                            Game.Say(String.Format("/all {0}, I'll try being nicer if you'll try being more intelligent.", theTarget));
                            break;
                        case 14:
                            Game.Say(String.Format("/all {0}, if you have something to say raise your hand... then place it over your mouth. ", theTarget));
                            break;
                        case 15:
                            Game.Say(String.Format("/all Somewhere out there is a tree, tirelessly producing oxygen so you can breathe. I think you owe it an apology, {0}", theTarget));
                            break;
                    }
                    return;
                }
                Game.Say("/all " + KnownDisrespectStarts[Random.Next(0, KnownDisrespectStarts.Length - 1)] +
                         (Random.Next(1, 2) == 1 ? theTarget : "") +
                         KnownDisrespectEndings[Random.Next(0, KnownDisrespectEndings.Length - 1)]);
            }
        }
    }
}
