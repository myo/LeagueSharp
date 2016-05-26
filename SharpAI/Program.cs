using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAI.Enums;
using SharpAI.SummonersRift;
using SharpAI.SummonersRift.Data;
using SharpAI.Utility;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using SharpDX;
using Color=System.Drawing.Color;

namespace SharpAI
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.Init();
            Hacks.UseGameObjectCache = true;
            Events.OnLoad += (sender, loadArgs) =>
            {
                if (Game.MapId != GameMapId.SummonersRift)
                {
                    // halt
                    return;
                }
                SessionBasedData.LoadTick = ObjectManager.Get<Obj_AI_Minion>().Any(m => m.CharData.BaseSkinName.Contains("Minion"))
                    ? Environment.TickCount - 190000
                    : Environment.TickCount;
                Hotfixes.Load();
                Logging.Log("LOADED " + SessionBasedData.LoadTick);
                Game.OnUpdate += (updateArgs) =>
                {
                    if (Environment.TickCount - SessionBasedData.LoadTick > 15000)
                    {
                        Tree.Seed();
                        Tree.Water();
                    }
                    else
                    {
                        Logging.Log("WAITING FOR GAME START");
                    }
                };
                Game.OnEnd += endArgs =>
                {
                    Task.Run(
                        async () =>
                        {
                            await Task.Delay(5000);
                            Game.Quit();
                        });
                };
            };
        }
    }
}
