//This code is copyright (c) LeagueSharp 2015. Please do not remove this line.

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Ping_Assistant
{
    public static class Program
    {
        public static int LastPing;
        public static int NumberOfPings;
        public static Menu Config;
        public static Random Rand = new Random(Environment.TickCount);
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                Config = new Menu("Ping Assitant", "pingassistant", true);
                Config.AddItem(new MenuItem("maxpings", "Max Pings A Time").SetValue(new Slider(6, 1, 6)));
                Config.AddItem(new MenuItem("pingtarget", "Ping Target You Should Focus").SetValue(true));
                Config.AddItem(new MenuItem("pinghelp", "Ping To Ask For Help").SetValue(true));
                Config.AddItem(new MenuItem("pingdanger", "Ping Danger").SetValue(true));
                Config.AddItem(new MenuItem("pingganks", "Ping Ganks").SetValue(true));
                Config.AddToMainMenu();

                Game.OnUpdate += OnUpdate;
            };
        }

        static void OnUpdate(EventArgs args)
        {
            if (Utils.GameTimeTickCount - LastPing > Rand.Next(10000, 60000)) 
                NumberOfPings = 0;


            if (Config.Item("pingtarget").GetValue<bool>() && ObjectManager.Player.CountEnemiesInRange(1000) <= ObjectManager.Player.CountAlliesInRange(1000))
            {
                PingTarget(ObjectManager.Get<Obj_AI_Hero>().Where(h=>h.IsEnemy && h.IsValidTarget() && h.Distance(ObjectManager.Player) < 1000).OrderBy(TargetSelector.GetPriority).ThenBy(e=>e.Health).FirstOrDefault());
                return;
            }
            if (ObjectManager.Player.CountEnemiesInRange(1000) > ObjectManager.Player.CountAlliesInRange(1000))
            {
                if (Config.Item("pinghelp").GetValue<bool>() && ObjectManager.Player.CountAlliesInRange(2500) >= ObjectManager.Player.CountEnemiesInRange(1000))
                {
                    PingGround(GetPointNearMe(800), PingCategory.AssistMe);
                    return;
                }
                if (Config.Item("pingdanger").GetValue<bool>())
                    PingGround(GetPointNearMe(800), PingCategory.Danger);
            }
        }

        private static Vector3 GetPointNearMe(int range)
        {
            var circle = new Geometry.Circle(ObjectManager.Player.ServerPosition.To2D(), range).ToPolygon().ToClipperPath();
            var point = circle.OrderBy(p => Rand.Next()).FirstOrDefault();
            return new Vector2(point.X, point.Y).To3D();
        }

        private static void PingTarget(Obj_AI_Hero target)
        {
            if (!target.IsValidTarget(1000)) return;
            if (Utils.GameTimeTickCount - LastPing < Rand.Next(100, 1100) || NumberOfPings >= Rand.Next(Math.Max(1, Config.Item("maxpings").GetValue<Slider>().Value / 2), Config.Item("maxpings").GetValue<Slider>().Value)) return;
            LastPing = Utils.GameTimeTickCount;
            NumberOfPings++;
            Game.SendPing(PingCategory.Normal, target);
        }

        private static void PingGround(Vector3 point, PingCategory pingtype)
        {
            if (point.Distance(ObjectManager.Player.ServerPosition) > 1000) return;
            if (Utils.GameTimeTickCount - LastPing < Rand.Next(100, 1100) || NumberOfPings >= Rand.Next(Math.Max(1, Config.Item("maxpings").GetValue<Slider>().Value/2), Config.Item("maxpings").GetValue<Slider>().Value)) return;
            LastPing = Utils.GameTimeTickCount;
            NumberOfPings++;
            Game.SendPing(pingtype, point);
        }
    }
}
