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
        public static Dictionary<int, byte[]> DiffPackets;
        public static Menu OrbwalkerMenu;
        public static Orbwalking.Orbwalker Orbwalker;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += e =>
            {
                OrbwalkerMenu = new Menu("Orbwalker", "orbwalker", true);
                Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);
                OrbwalkerMenu.AddToMainMenu();
                NormalPackets = new Dictionary<int, byte[]>();
                SpacebarPackets = new Dictionary<int, byte[]>();
                DiffPackets = new Dictionary<int, byte[]>();

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
                    Drawing.DrawText(Drawing.Width - 200, 110, Color.LimeGreen, "Spacebar packets: " + SpacebarPackets.Count);
                    Drawing.DrawText(Drawing.Width - 200, 120, Color.Crimson, "Total unique packets: " + DiffPackets.Count);
                    Drawing.DrawText(Drawing.Width - 200, 130, Color.Crimson, "Not unique packets: " + (SpacebarPackets.Count + NormalPackets.Count - DiffPackets.Count));
                };
                Game.OnUpdate += a =>
                {
                    foreach (var np in NormalPackets)
                    {
                        if (!DiffPackets.Any(r => r.Key == np.Key))
                        {
                            DiffPackets.Add(np.Key, np.Value);
                        }
                    }
                    foreach (var sp in SpacebarPackets)
                    {
                        if (!DiffPackets.Any(r => r.Key == sp.Key))
                        {
                            DiffPackets.Add(sp.Key, sp.Value);
                        }
                    }
                };
            };
        }
    }
}
