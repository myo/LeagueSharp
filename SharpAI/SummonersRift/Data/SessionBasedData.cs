using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAI.Enums;
using LeagueSharp;
using LeagueSharp.SDK;
using Geometry = SharpAI.Utility.Geometry;

namespace SharpAI.SummonersRift.Data
{
    public static class SessionBasedData
    {
        public static int LoadTick { get; set; } = 0;
        public static bool Loaded => (LoadTick > 0);
        public static Lane MyLane { get; set; } = Lane.Unknown;
        public static Geometry.Polygon MyLanePolygon => StaticData.GetWholeLane(CurrentLane);
        public static Lane CurrentLane { get; set; } = Lane.Unknown;
        public static Geometry.Polygon CurrentLanePolygon => StaticData.GetWholeLane(CurrentLane);



        public static GameObjectTeam MyTeam => ObjectManager.Player.Team;
        public static GameObjectTeam EnemyTeam
            => (ObjectManager.Player.Team == GameObjectTeam.Order ? GameObjectTeam.Chaos : GameObjectTeam.Order);
    }
}
