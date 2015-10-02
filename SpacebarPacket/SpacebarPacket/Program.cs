using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SpacebarPacket
{
    class Program
    {
        public static Dictionary<int, byte[]> NormalPackets;
        public static Dictionary<int, byte[]> SpacebarPackets;
        public static Menu OrbwalkerMenu;
        public static Orbwalking.Orbwalker Orbwalker;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += e =>
            {
                OrbwalkerMenu = new Menu("Orbwalker", "orbwalker");
                Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);
            };
            Game.OnProcessPacket += eventArgs =>
            {
                if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                {
                    if (!NormalPackets.Any(pair => pair.Key == eventArgs.PacketData.Length))
                    {
                        NormalPackets.Add(eventArgs.PacketData.Length, eventArgs.PacketData);
                    }
                }
                else
                {
                    if (!SpacebarPackets.Any(pair => pair.Key == eventArgs.PacketData.Length))
                    {
                        SpacebarPackets.Add(eventArgs.PacketData.Length, eventArgs.PacketData);
                    }
                }
            };
            Drawing.OnDraw += d =>
            {
                Drawing.DrawText(Drawing.Width - 200, 100, Color.LimeGreen, "Normal packets: " + NormalPackets.Count);
                Drawing.DrawText(Drawing.Width - 200, 110, Color.Crimson, "[32] packets: " + SpacebarPackets.Count);
            };
        }
    }
}
