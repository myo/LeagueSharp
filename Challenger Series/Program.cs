#region License
/* Copyright (c) LeagueSharp 2016
 * No reproduction is allowed in any way unless given written consent
 * from the LeagueSharp staff.
 * 
 * Author: imsosharp
 * Date: 2/20/2016
 * File: Program.cs
 */
#endregion License

using Challenger_Series.Plugins;
using LeagueSharp;
using LeagueSharp.SDK;

namespace Challenger_Series
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.Init();
            Events.OnLoad += (sender, eventArgs) =>
            {
                switch (ObjectManager.Player.ChampionName)
                {
                    case "Soraka":
                        new Soraka();
                        break;
                    case "Vayne":
                        new Vayne();
                        break;
                    case "Irelia":
                        new Irelia();
                        break;
                    case "Kalista":
                        new Kalista();
                        break;
                    case "KogMaw":
                        new KogMaw();
                        break;
                    case "Ashe":
                        new Ashe();
                        break;
                    case "Caitlyn":
                        new Caitlyn();
                        break;
                    case "Lucian":
                        new Lucian();
                        break;
                    case "Ezreal":
                        new Ezreal();
                        break;
                    default:
                        break;
                }
            };
        }
    }
}
