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
using LeagueSharp.SDK.Utils;
using SharpDX;
using Color=System.Drawing.Color;

namespace SharpAI
{
    class Program
    {
        public static string myocry;
        static void Main(string[] args)
        {
            myocry = "You are quite the l33t revuresuzurez if u got this far, plz consider not dumping/sharing/porting/feeding it to your cat, cheers!";
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
            };
        }
    }
}
