using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;
using SharpDX;
using Geometry = AutoSharp.Utility.Geometry;

namespace AutoSharp.Data
{
    public enum Lane
    {
        Bot = 0,
        Mid = 1,
        Top = 2
    }

    public static class StaticData
    {
        private static Dictionary<Lane, Geometry.Polygon> _teamOrderLaneZones = new Dictionary<Lane, Geometry.Polygon>();
        private static Dictionary<Lane, Geometry.Polygon> _teamChaosLaneZones = new Dictionary<Lane, Geometry.Polygon>();

        public static void Initialize()
        {
            foreach (var lane in Enum.GetValues(typeof (Lane)))
            {
                var laneCastedAsLane = (Lane) lane;
                var turretsList = Turrets.GetTurrets(GameObjectTeam.Order, laneCastedAsLane).ToArray();
                var poly = new Geometry.Polygon();
                for (var i = 0; i < turretsList.Length - 1; i++)
                {
                    foreach (
                        var point in
                            new Geometry.Circle(turretsList[i].Position.ToVector2(), 950).ToPolygon().ToClipperPath())
                    {
                        poly.Add(new Vector2(point.X, point.Y));
                    }

                    var rectFromThisToNextTurret = new Geometry.Rectangle(turretsList[i].Position.ToVector2(),
                        turretsList[i + 1].Position.ToVector2(), 700);
                    foreach (var point in rectFromThisToNextTurret.ToPolygon().ToClipperPath())
                    {
                        poly.Add(new Vector2(point.X, point.Y));
                    }
                }
                _teamOrderLaneZones.Add(laneCastedAsLane, poly);
            }
            foreach (var lane in Enum.GetValues(typeof (Lane)))
            {
                var laneCastedAsLane = (Lane) lane;
                var turretsList = Turrets.GetTurrets(GameObjectTeam.Chaos, laneCastedAsLane).ToArray();
                var poly = new Geometry.Polygon();
                for (var i = 0; i < turretsList.Length - 1; i++)
                {
                    foreach (
                        var point in
                            new Geometry.Circle(turretsList[i].Position.ToVector2(), 950).ToPolygon().ToClipperPath())
                    {
                        poly.Add(new Vector2(point.X, point.Y));
                    }

                    var rectFromThisToNextTurret = new Geometry.Rectangle(turretsList[i].Position.ToVector2(),
                        turretsList[i + 1].Position.ToVector2(), 700);
                    foreach (var point in rectFromThisToNextTurret.ToPolygon().ToClipperPath())
                    {
                        poly.Add(new Vector2(point.X, point.Y));
                    }
                }
                _teamChaosLaneZones.Add(laneCastedAsLane, poly);
            }
        }

        public static Geometry.Polygon GetLanePolygon(GameObjectTeam team, Lane lane)
        {
            switch (team)
            {
                case GameObjectTeam.Order:
                {
                    return _teamOrderLaneZones.FirstOrDefault(entry => entry.Key == lane).Value;
                }
                case GameObjectTeam.Chaos:
                {
                    return _teamChaosLaneZones.FirstOrDefault(entry => entry.Key == lane).Value;
                }
                default:
                {
                    return null;
                }
            }
        }
    }

    public static class Turrets
    {
        private static string TurretBaseName = "Turret";

        private static string GetTurretLaneNaming(Lane? lane)
        {
            switch (lane)
            {
                case Lane.Bot:
                    return "_R"; //Right
                case Lane.Mid:
                    return "_C"; //Center
                case Lane.Top:
                    return "_L"; //Left
                default:
                    return "";
            }
        }

        public static string GetTurretName(GameObjectTeam team, Lane? lane = null, int? number = null)
        {
            var result = TurretBaseName;
            result += team == GameObjectTeam.Order ? "_T1" : "_T2";
            result += GetTurretLaneNaming(lane);
            result += number != null ? "_" + number : "";
            return result;
        }

        public static IEnumerable<Obj_AI_Turret> GetTurrets(GameObjectTeam team, Lane? lane = null, int? number = null)
        {
            return
                GameObjects.Get<Obj_AI_Turret>()
                    .Where(turret => turret.Name.Contains(GetTurretName(team, lane, number)));
        }
    }
}
