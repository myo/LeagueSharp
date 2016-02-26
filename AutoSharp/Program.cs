using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoSharp.Data;
using LeagueSharp;
using SharpDX;
using Color=System.Drawing.Color;

namespace AutoSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            StaticData.Initialize();
            Drawing.OnDraw += (drawArgs) =>
            {
                StaticData.GetLanePolygon(GameObjectTeam.Order, Lane.Mid).Draw(Color.DarkGreen, 15);
            };
        }
    }
}
