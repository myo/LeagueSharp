using System;
using System.Linq;
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

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static void OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Mastery Emote Spammer", "masteryemotespammermenu", true);
            Menu.AddItem(new MenuItem("mode", "Mode").SetValue(new StringList(new[] { "MASTERY", "LAUGH" })));
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
            Menu.AddToMainMenu();
            Random = new Random();
            FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
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
        }

        public static void DoEmote()
        {
            if (Utils.GameTimeTickCount - LastEmoteSpam > Random.Next(5000, 15000))
            {
                LastEmoteSpam = Utils.GameTimeTickCount;
                if (Menu.Item("mode").GetValue<StringList>().SelectedValue == "MASTERY")
                {
                    Game.Say("/masterybadge");
                }
                else
                {
                    Game.Say("/l");
                }
            }
        }
    }
}
