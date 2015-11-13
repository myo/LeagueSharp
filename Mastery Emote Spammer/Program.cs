using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mastery_Badge_Spammer
{
    public static class Program
    {
        public static Menu Menu;
        public static int LastMasteryBadgeSpam = 0;
        public static int MyKills = 0;
        public static int MyAssits = 0;
        public static int MyDeaths = 0;
        public static Random Random;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static void OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Mastery Emote Spammer", "masteryemotespammermenu", true);
            Menu.AddItem(new MenuItem("onkill", "After Kill").SetValue(true));
            Menu.AddItem(new MenuItem("onassist", "After Assist").SetValue(true));
            Menu.AddItem(new MenuItem("ondeath", "After Death").SetValue(false));
            Menu.AddItem(new MenuItem("neardead", "Near Dead Bodies").SetValue(true));
            Menu.AddItem(new MenuItem("ondodgedskillshot", "After you dodge a skillshot").SetValue(true));
            Menu.AddToMainMenu();
            Random = new Random();
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var sData = SpellDatabase.GetByName(args.SData.Name);
            if (Menu.Item("ondodgedskillshot").GetValue<bool>() && sender.IsEnemy && sData != null &&
                ObjectManager.Player.Distance(sender) < sData.Range)
            {
                Utility.DelayAction.Add(sData.Delay, DoEmote);
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
            if (Utils.GameTimeTickCount - LastMasteryBadgeSpam > Random.Next(5000, 15000))
            {
                LastMasteryBadgeSpam = Utils.GameTimeTickCount;
                Game.Say("/masterybadge");
            }
        }
    }
}
