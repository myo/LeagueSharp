using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PimpMyGame
{
    using System.IO;

    using LeagueSharp;
    using LeagueSharp.SDK;

    using PimpMyGame.Properties;

    public class InstanceStats
    {
        public int Kills;

        public int Assists;

        public int TripleKills;

        public int PentaKills;
    }
    class Program
    {
        private static List<byte[]> _beforeSpawn = new List<byte[]> {Resources.beforespawn_0};
        private static List<byte[]> _onDead = new List<byte[]> { Resources.dead_0, Resources.dead_1, Resources.dead_2, Resources.dead_3, Resources.dead_4, Resources.dead_5 };
        private static List<byte[]> _onKill = new List<byte[]> { Resources.kill_0, Resources.kill_1, Resources.kill_2, Resources.kill_3, Resources.kill_4, Resources.kill_5, Resources.kill_6, Resources.kill_7, Resources.kill_8, Resources.kill_9 };
        private static List<byte[]> _onTriple = new List<byte[]> { Resources.triple_0, Resources.triple_1, Resources.triple_2, Resources.triple_3, Resources.triple_4, Resources.triple_5, Resources.triple_6 };
        private static List<byte[]> _onPenta = new List<byte[]> { Resources.penta_0, Resources.penta_1, Resources.penta_2, Resources.penta_3, Resources.penta_4, Resources.penta_5, Resources.penta_6 };

        private static SoundPlayer _activeSoundPlayer;

        private static Random _rand;

        private static InstanceStats _instance;
        
        private 
        static void Main(string[] args)
        {
            FileOperations.CreateCustomFolders();
            _rand = new Random();
            Game.OnNotify += OnNotify;
            Game.OnChat += OnChat;
            Events.OnLoad += OnLoad;
            Obj_AI_Base.OnDoCast += OnDoCast;
            _activeSoundPlayer = new SoundPlayer(Resources.darudeloadingstorm, "loadingstorm.mp3");
        }

        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //if (sender.IsMe && args.)
        }

        private static void OnLoad(object sender, EventArgs eventArgs)
        {
            _instance = new InstanceStats {Assists=ObjectManager.Player.Assists, Kills = ObjectManager.Player.ChampionsKilled, PentaKills = ObjectManager.Player.PentaKills, TripleKills = ObjectManager.Player.TripleKills};
        }

        private static void OnChat(GameChatEventArgs args)
        {
            if (args.Message == ":stopmusic:")
            {
                Game.PrintChat("Music has been stopped.");
                _activeSoundPlayer = new SoundPlayer(Resources.triple_0, Resources.triple_0.Length + ".mp3");
            }
        }

        private static void OnNotify(GameNotifyEventArgs args)
        {
            if (ObjectManager.Player.PentaKills > _instance.PentaKills && (args.EventId == GameEventId.OnChampionPentaAssist || args.EventId == GameEventId.OnChampionPentaKill))
            {
                if (_activeSoundPlayer != null)
                {
                    _activeSoundPlayer = null;
                    Trashcan.KillOpenScripts();
                }
                _instance.PentaKills = ObjectManager.Player.PentaKills;
                _instance.TripleKills = ObjectManager.Player.TripleKills;
                _instance.Kills = ObjectManager.Player.ChampionsKilled;
                var sound = getRandomCollectionMember(_onPenta);
                _activeSoundPlayer = new SoundPlayer(sound, sound.Length + ".mp3");
                return;
            }
            if (ObjectManager.Player.TripleKills > _instance.TripleKills && (args.EventId == GameEventId.OnChampionTripleKill || args.EventId == GameEventId.OnChampionTripleAssist))
            {
                if (_activeSoundPlayer != null)
                {
                    _activeSoundPlayer = null;
                    Trashcan.KillOpenScripts();
                }
                _instance.PentaKills = ObjectManager.Player.PentaKills;
                _instance.TripleKills = ObjectManager.Player.TripleKills;
                _instance.Kills = ObjectManager.Player.ChampionsKilled;
                var sound = getRandomCollectionMember(_onTriple);
                _activeSoundPlayer = new SoundPlayer(sound, sound.Length + ".mp3");
                return;
            }
            if (ObjectManager.Player.ChampionsKilled > _instance.Kills && args.EventId == GameEventId.OnKill)
            {
                if (_activeSoundPlayer != null)
                {
                    _activeSoundPlayer = null;
                    Trashcan.KillOpenScripts();
                }
                _instance.PentaKills = ObjectManager.Player.PentaKills;
                _instance.TripleKills = ObjectManager.Player.TripleKills;
                _instance.Kills = ObjectManager.Player.ChampionsKilled;
                var sound = getRandomCollectionMember(_onKill);
                _activeSoundPlayer = new SoundPlayer(sound, sound.Length + ".mp3");
                return;
            }
        }

        private static byte[] getRandomCollectionMember(List<byte[]> collection)
        {
            return collection[_rand.Next(0, collection.Count - 1)];
        }
    }
}
