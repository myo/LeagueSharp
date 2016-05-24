using System;
using System.Collections.Generic;
using System.Linq;
using SharpAI.Enums;
using SharpAI.Utility;
using LeagueSharp;
using LeagueSharp.SDK;
using SharpDX;
using Geometry = SharpAI.Utility.Geometry;

namespace SharpAI.SummonersRift.Data
{
    public static class StaticData
    {
        private static Dictionary<Lane, Geometry.Polygon> _teamOrderLaneZones = new Dictionary<Lane, Geometry.Polygon>();
        private static Dictionary<Lane, Geometry.Polygon> _teamChaosLaneZones = new Dictionary<Lane, Geometry.Polygon>();

        private static Dictionary<Lane, Geometry.Polygon> _teamNeutralLaneZones =
            new Dictionary<Lane, Geometry.Polygon>();

        private static Dictionary<Lane, Geometry.Polygon> _wholeLanePolygon = new Dictionary<Lane, Geometry.Polygon>();

        static StaticData()
        {
            foreach (var lane in Enum.GetValues(typeof(Lane)))
            {
                var laneCastedAsLane = (Lane) lane;
                // for order
                var orderTurretsList = Turrets.GetTurretsPosition(GameObjectTeam.Order, laneCastedAsLane).ToArray();
                var orderPolygons = new List<Geometry.Polygon>();
                for (var i = 0; i < orderTurretsList.Length - 1; i++)
                {
                    // circle around turrets
                    orderPolygons.Add(new Geometry.Circle(orderTurretsList[i], 950).ToPolygon());
                    // rectangle from this turret to next one
                    orderPolygons.Add(new Geometry.Rectangle(orderTurretsList[i],
                        orderTurretsList[i + 1], 700).ToPolygon());
                }
                // add a circle around last turret too
                orderPolygons.Add(new Geometry.Circle(orderTurretsList.LastOrDefault(), 950).ToPolygon());
                _teamOrderLaneZones.Add(laneCastedAsLane, orderPolygons.ClipperUnion().PathsToPolygon());
                // for team chaos
                var chaosTurretsList = Turrets.GetTurretsPosition(GameObjectTeam.Chaos, laneCastedAsLane).ToArray();
                var chaosPolygons = new List<Geometry.Polygon>();
                for (var i = 0; i < chaosTurretsList.Length - 1; i++)
                {
                    // circle around turrets
                    chaosPolygons.Add(new Geometry.Circle(chaosTurretsList[i], 950).ToPolygon());
                    // rectangle from this turret to next one
                    chaosPolygons.Add(new Geometry.Rectangle(chaosTurretsList[i],
                        chaosTurretsList[i + 1], 700).ToPolygon());
                }
                // add a circle around last turret too
                chaosPolygons.Add(new Geometry.Circle(chaosTurretsList.LastOrDefault(), 950).ToPolygon());
                _teamChaosLaneZones.Add(laneCastedAsLane, chaosPolygons.ClipperUnion().PathsToPolygon());
                // for neutral zones
                if (laneCastedAsLane != Lane.Base)
                {
                    var neutralCenter =
                        Turrets.GetTurretsPosition(GameObjectTeam.Neutral, laneCastedAsLane).LastOrDefault();
                    //#TODO should probably get the polygon end-point but cba
                    var blueTeamTurretPos =
                        Turrets.GetTurretsPosition(GameObjectTeam.Order, laneCastedAsLane)
                            .LastOrDefault()
                            .Extend(neutralCenter, 880);
                    var redTeamTurretPos =
                        Turrets.GetTurretsPosition(GameObjectTeam.Chaos, laneCastedAsLane)
                            .LastOrDefault()
                            .Extend(neutralCenter, 880);
                    var poly1 = new Geometry.Rectangle(blueTeamTurretPos, neutralCenter, 700);
                    var poly2 = new Geometry.Rectangle(neutralCenter, redTeamTurretPos, 700);
                    var union = new List<Geometry.Polygon> {poly1.ToPolygon(), poly2.ToPolygon()}.ClipperUnion();
                    _teamNeutralLaneZones.Add(laneCastedAsLane, union.PathsToPolygon());
                }
            }
            // add a polygon for each lane, without minding the team.
            foreach (var lane in Enum.GetValues(typeof(Lane)))
            {
                var laneCastedAsLane = (Lane) lane;
                if (laneCastedAsLane == Lane.Mid || laneCastedAsLane == Lane.Bot || laneCastedAsLane == Lane.Top)
                {
                    var polygons = new List<Geometry.Polygon>();
                    polygons.Add(_teamNeutralLaneZones.FirstOrDefault(entry => entry.Key == laneCastedAsLane).Value);
                    polygons.Add(_teamOrderLaneZones.FirstOrDefault(entry => entry.Key == laneCastedAsLane).Value);
                    polygons.Add(_teamChaosLaneZones.FirstOrDefault(entry => entry.Key == laneCastedAsLane).Value);
                    _wholeLanePolygon.Add(laneCastedAsLane, polygons.ClipperUnion().PathsToPolygon());
                }
            }
            /*
            // Here we WIPE the turret out from our lane if it's DELETED
            GameObject.OnDelete += (sender, deleteArgs) =>
            {
                if (sender == null)
                {
                    Logging.Log("[Polygon Cache] Turret was deleted but sender was null");
                }
                if (sender is Obj_AI_Turret)
                {
                    Logging.Log("[Polygon Cache] A turret was deleted");
                    // we gather some information about the dead turret
                    var teamLaneTuple = Turrets.GetTurretTeamAndLaneByPosition(sender.Position);
                    var team = teamLaneTuple.Item1;
                    var lane = teamLaneTuple.Item2;
                    var position = teamLaneTuple.Item3;
                    // check if its worth editing the lane poly
                    if (lane == Lane.Base || lane == Lane.Unknown)
                    {
                        // escape null refs produced by the code below :roto2_think:
                        return;
                    }
                    // we backup the lane to perform operations on its polygon
                    var backup = GetLanePolygon(team, lane);
                    if (backup != null)
                    {
                        Logging.Log("[Polygon Cache] backup of a deleted turret was created");
                    }
                    // do the work for each team separately (cuz when we wipe a team's region it should become neutral)
                    // first blue
                    if (team == GameObjectTeam.Order)
                    {
                        Logging.Log(
                            "[Polygon Cache] Attempting to remove lane from team order, the collection currently has " +
                            _teamChaosLaneZones.Count + " lanes");

                        _teamChaosLaneZones.Remove(lane);

                        Logging.Log("[Polygon Cache] Removed lane from team order, the collection currently has " +
                                    _teamChaosLaneZones.Count + " lanes");
                        // we first cut the first circle poly:
                        var turretCircle = new Geometry.Circle(position, 850).ToPolygon();
                        Logging.Log("[Polygon Cache] Trying to execute XOR");
                        try
                        {
                            var step1 = backup.ClipperXor(turretCircle).PathsToPolygon();
                            Logging.Log("[Polygon Cache] XOR executed succesfuly");

                            var allTurretsInLane = Turrets.GetTurretsPosition(team, lane);
                            var thisTurretIndex = allTurretsInLane.IndexOf(position);
                            // and add it to neutral zone
                            _teamNeutralLaneZones.Add(lane, turretCircle);
                            // then we remove the space between the removed turret and the one that should still be alive (if any)
                            if (thisTurretIndex - 1 >= 0)
                            {
                                var rectFromThisToNextAlive = new Geometry.Rectangle(position,
                                    allTurretsInLane[thisTurretIndex - 1], 700).ToPolygon();
                                var step2 = step1.ClipperXor(rectFromThisToNextAlive).PathsToPolygon();
                                _teamOrderLaneZones.Add(lane, step2);
                                _teamNeutralLaneZones.Add(lane, rectFromThisToNextAlive);
                                return;
                            }
                            _teamOrderLaneZones.Add(lane, step1);
                        }
                        catch (Exception e)
                        {
                            Logging.Log("[Polygon Cache] Caught Exception: " + e);
                        }
                    }
                    // then red
                    if (team == GameObjectTeam.Chaos)
                    {
                        _teamChaosLaneZones.Remove(lane);

                        // we first cut the first circle poly:
                        var turretCircle = new Geometry.Circle(position, 850).ToPolygon();
                        try
                        {
                            var step1 = backup.ClipperXor(turretCircle).PathsToPolygon();
                            var allTurretsInLane = Turrets.GetTurretsPosition(team, lane);
                            var thisTurretIndex = allTurretsInLane.IndexOf(position);
                            // and add it to neutral zone
                            _teamNeutralLaneZones.Add(lane, turretCircle);
                            // then we remove the space between the removed turret and the one that should still be alive (if any)
                            if (thisTurretIndex - 1 >= 0)
                            {
                                var rectFromThisToNextAlive = new Geometry.Rectangle(position,
                                    allTurretsInLane[thisTurretIndex - 1], 700).ToPolygon();
                                var step2 = step1.ClipperXor(rectFromThisToNextAlive).PathsToPolygon();
                                _teamChaosLaneZones.Add(lane, step2);
                                _teamNeutralLaneZones.Add(lane, rectFromThisToNextAlive);
                                return;
                            }
                            _teamChaosLaneZones.Add(lane, step1);
                        }
                        catch (Exception e)
                        {
                            Logging.Log("[Polygon Cache] Caugh Exception: " + e);
                        }
                    }
                }
            };*/
        }

        public static Geometry.Polygon GetWholeLane(Lane lane)
        {
            return _wholeLanePolygon.FirstOrDefault(entry => entry.Key == lane).Value;
        }

        public static Geometry.Polygon GetLastTurretInLanePolygon(GameObjectTeam team, Lane lane)
        {
            var turret = Turrets.GetTurretsPosition(team, lane);
            return new Geometry.Circle(turret.LastOrDefault(), 850).ToPolygon();
        }

        /// <summary>
        /// Returns a polygon containing the lane itself.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <param name="lane">The lane.</param>
        /// <returns>A polygon containing the lane itself.</returns>
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
                case GameObjectTeam.Neutral:
                {
                    return _teamNeutralLaneZones.FirstOrDefault(entry => entry.Key == lane).Value;
                }
                default:
                {
                    return null;
                }
            }
        }

        public static Geometry.Polygon GetLanePolygonExtendedToFarthestMinion(GameObjectTeam laneZoneTeam, Lane lane,
            GameObjectTeam minionTeam, int maxFieldDistance = 1000)
        {
            var farthestTurret = Turrets.GetTurretsPosition(laneZoneTeam, lane).Last();
            var mostdamaged = Minions.GetMostDamagedMinion(minionTeam, lane);
            var farthestMinion = mostdamaged != null
                ? mostdamaged
                : Minions.GetMinionsInLane(minionTeam, lane)
                    .OrderBy(m => m.Distance(farthestTurret))
                    .FirstOrDefault();
            if (farthestMinion != null)
            {
                return
                    new Geometry.Rectangle(farthestMinion.Position.ToVector2(),
                        farthestMinion.Position.ToVector2().Extend(farthestTurret, maxFieldDistance), 700).ToPolygon();
            }
            return GetLanePolygon(laneZoneTeam, lane);
        }

        public static Lane ChooseBestLane()
        {
            var allies = GameObjects.AllyHeroes.Where(h => !h.IsMe);
            if (!allies.Any(
                h =>
                    h.Position.IsInside(GetWholeLane(Lane.Mid))))
            {
                return Lane.Mid;
            }
            if (allies.Count(
                h =>
                    h.Position.IsInside(GetWholeLane(Lane.Bot))) < 2)
            {
                return Lane.Bot;
            }
            if (allies.Count(
                h =>
                    h.Position.IsInside(GetWholeLane(Lane.Top))) < 2)
            {
                return Lane.Top;
            }
            return Lane.Mid;
        }
    }

    // This is our minion cache thingy. We use it to store minions in a lane for faster processing ( :klappa: )
    public static class Minions
    {
        // this is bad and should be only initialized if we're running a SummonerRift instance. #TODO
        static Minions()
        {
            Game.OnUpdate += OnUpdate;
            GameObject.OnDelete += OnDelete;
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion)
            {
                _minionsInLanes.Remove(sender.NetworkId);
            }
        }

        // we're gonna add an update delay, we don't really need to add the minion as soon as it enters the lane, performance is better
        private static int _lastUpdate;

        // how much fps does this actually drop? a mistery. #TODO
        private static void OnUpdate(EventArgs args)
        {
            // we're actually gonna remove the dead minions before ondelete does it #fixedcache
            var minionsToRemove = new List<int>();
            foreach (var minion in GameObjects.Minions)
            {
                if (_minionsInLanes.ContainsKey(minion.NetworkId))
                {
                    if (minion.IsDead)
                    {
                        minionsToRemove.Add(minion.NetworkId);
                    }
                    continue;
                }
                // we're only gonna consider top, mid and bot since we might add lane,jg etc
                if (minion.Position.IsInside(StaticData.GetWholeLane(Lane.Top)))
                {
                    _minionsInLanes.Add(minion.NetworkId,
                        new SharpAIMinion {Lane = Lane.Top, MinionPtr = minion, NetworkId = minion.NetworkId});
                }
                if (minion.Position.IsInside(StaticData.GetWholeLane(Lane.Mid)))
                {
                    _minionsInLanes.Add(minion.NetworkId,
                        new SharpAIMinion {Lane = Lane.Mid, MinionPtr = minion, NetworkId = minion.NetworkId});
                }
                if (minion.Position.IsInside(StaticData.GetWholeLane(Lane.Bot)))
                {
                    _minionsInLanes.Add(minion.NetworkId,
                        new SharpAIMinion {Lane = Lane.Bot, MinionPtr = minion, NetworkId = minion.NetworkId});
                }
            }
            foreach (var minionNetworkId in minionsToRemove)
            {
                _minionsInLanes.Remove(minionNetworkId);
            }
        }

        private static Dictionary<int, SharpAIMinion> _minionsInLanes = new Dictionary<int, SharpAIMinion>();

        public static IEnumerable<Obj_AI_Minion> GetMinionsInLane(GameObjectTeam team, Lane lane)
        {
            return from entry in _minionsInLanes
                where entry.Value.Team == team && entry.Value.Lane == lane
                select entry.Value.MinionPtr;
        }

        public static IEnumerable<Obj_AI_Minion> GetAllyMinionsInLane(Lane lane)
        {
            return from entry in _minionsInLanes
                where !entry.Value.IsDead && entry.Value.Team == ObjectManager.Player.Team && entry.Value.Lane == lane
                select entry.Value.MinionPtr;
        }
        public static IEnumerable<Obj_AI_Minion> GetEnemyMinionsInLane(Lane lane)
        {
            return from entry in _minionsInLanes
                   where !entry.Value.IsDead && entry.Value.Team == (ObjectManager.Player.Team == GameObjectTeam.Order ? GameObjectTeam.Chaos : GameObjectTeam.Order) && entry.Value.Lane == lane
                   select entry.Value.MinionPtr;
        }

        public static Obj_AI_Minion GetMostDamagedMinion(GameObjectTeam team, Lane lane)
        {
            return GetMinionsInLane(team, lane).OrderBy(m => m.Health).FirstOrDefault(minion => minion.Health > 0);
        }


        /// <summary>
        /// Class helpful for better organizing minions in lanes
        /// </summary>
        public class SharpAIMinion
        {
            /// <summary>
            /// The minion's network id.
            /// </summary>
            public int NetworkId;

            /// <summary>
            /// The minion.
            /// </summary>
            public Obj_AI_Minion MinionPtr;

            /// <summary>
            /// The minion's IsDead flag.
            /// </summary>
            public bool IsDead => MinionPtr.IsDead;

            /// <summary>
            /// The minion's Team.
            /// </summary>
            public GameObjectTeam Team => MinionPtr.Team;

            /// <summary>
            /// The lane in which the minion can be currently found.
            /// </summary>
            public Lane Lane;
        }
    }

    /// <summary>
    /// Since riot messed up the cool naming system they had, I have to use this instead. Which I guess is better cause I'll never have to update this. -imsosharp
    /// </summary>
    public static class Turrets
    {
        private static List<Vector2> orderBase = new List<Vector2>
        {
            new Vector2(1748, 2271), // left
            new Vector2(2178, 1808) // right
        };

        private static List<Vector2> chaosBase = new List<Vector2>
        {
            new Vector2(12611, 13084), // right
            new Vector2(13053, 12612) // left
        };

        private static List<Vector2> orderTop = new List<Vector2>
        {
            new Vector2(1170, 4287), // inhib
            new Vector2(1513, 6700), // tier 1 
            new Vector2(981, 10441) // tier 2
        };

        private static List<Vector2> chaosTop = new List<Vector2>
        {
            new Vector2(10481, 13651), // inhib
            new Vector2(7943, 13412), // tier 1 
            new Vector2(4318, 13876) // tier 2
        };

        private static List<Vector2> orderMid = new List<Vector2>
        {
            new Vector2(3638, 3697), // inhib
            new Vector2(5070, 4832), // tier 1
            new Vector2(5865, 6462) // tier 2
        };

        private static List<Vector2> chaosMid = new List<Vector2>
        {
            new Vector2(11135, 11208), // inhib
            new Vector2(9768, 10114), // tier 1
            new Vector2(8955, 8510) // tier 2
        };

        private static List<Vector2> orderBot = new List<Vector2>
        {
            new Vector2(4282, 1254), // inhib
            new Vector2(6919, 1484), // tier 1
            new Vector2(10504, 1030) // tier 2
        };

        private static List<Vector2> chaosBot = new List<Vector2>
        {
            new Vector2(13625, 10573), // inhib
            new Vector2(13327, 8226), // tier 1
            new Vector2(13866, 4505) // tier 2
        };

        /// <summary>
        /// Gives you the ability to extract Vector2's from each turret in game (doesn't account for dead ones)
        /// </summary>
        /// <param name="team">The team of the turrets</param>
        /// <param name="lane">The lane of the turrets</param>
        /// <returns>A List of Vector2's representing positions of turrets in that specific lane.</returns>
        public static List<Vector2> GetTurretsPosition(GameObjectTeam team, Lane lane)
        {
            switch (team)
            {
                case GameObjectTeam.Order:
                {
                    switch (lane)
                    {
                        case Lane.Base:
                        {
                            return orderBase;
                        }
                        case Lane.Bot:
                        {
                            return orderBot;
                        }
                        case Lane.Mid:
                        {
                            return orderMid;
                        }
                        case Lane.Top:
                        {
                            return orderTop;
                        }
                    }
                    break;
                }
                case GameObjectTeam.Chaos:
                {
                    switch (lane)
                    {
                        case Lane.Base:
                        {
                            return chaosBase;
                        }
                        case Lane.Bot:
                        {
                            return chaosBot;
                        }
                        case Lane.Mid:
                        {
                            return chaosMid;
                        }
                        case Lane.Top:
                        {
                            return chaosTop;
                        }
                    }
                    break;
                }
                case GameObjectTeam.Neutral:
                {
                    switch (lane)
                    {
                        case Lane.Top:
                        {
                            return new List<Vector2> {new Vector2(2232, 12730)};
                        }
                        case Lane.Mid:
                        {
                            return new List<Vector2> {new Vector2(7464, 7464)};
                        }
                        case Lane.Bot:
                        {
                            return new List<Vector2> {new Vector2(12642, 2517)};
                        }
                    }
                    break;
                }
                default:
                    return ObjectManager.Player.Team == GameObjectTeam.Order ? orderMid : chaosMid;
            }
            return ObjectManager.Player.Team == GameObjectTeam.Order ? orderMid : chaosMid;

        }

        public static Tuple<GameObjectTeam, Lane, Vector2> GetTurretTeamAndLaneByPosition(Vector3 position)
        {
            var oba = orderBase.FirstOrDefault(p => p.Distance(position) < 100);
            if (oba != null && !oba.IsZero)
            {
                return new Tuple<GameObjectTeam, Lane, Vector2>(GameObjectTeam.Order, Lane.Base, oba);
            }
            var cba = chaosBase.FirstOrDefault(p => p.Distance(position) < 100);
            if (cba != null && !cba.IsZero)
            {
                return new Tuple<GameObjectTeam, Lane, Vector2>(GameObjectTeam.Chaos, Lane.Base, cba);
            }
            var ot = orderTop.FirstOrDefault(p => p.Distance(position) < 100);
            if (ot != null && !ot.IsZero)
            {
                return new Tuple<GameObjectTeam, Lane, Vector2>(GameObjectTeam.Order, Lane.Top, ot);
            }
            var ct = chaosTop.FirstOrDefault(p => p.Distance(position) < 100);
            if (ct != null && !ct.IsZero)
            {
                return new Tuple<GameObjectTeam, Lane, Vector2>(GameObjectTeam.Chaos, Lane.Top, ct);
            }
            var om = orderMid.FirstOrDefault(p => p.Distance(position) < 100);
            if (om != null && !om.IsZero)
            {
                return new Tuple<GameObjectTeam, Lane, Vector2>(GameObjectTeam.Order, Lane.Mid, om);
            }
            var cm = chaosMid.FirstOrDefault(p => p.Distance(position) < 100);
            if (cm != null && !cm.IsZero)
            {
                return new Tuple<GameObjectTeam, Lane, Vector2>(GameObjectTeam.Chaos, Lane.Mid, cm);
            }
            var obo = orderBase.FirstOrDefault(p => p.Distance(position) < 100);
            if (obo != null && !obo.IsZero)
            {
                return new Tuple<GameObjectTeam, Lane, Vector2>(GameObjectTeam.Order, Lane.Bot, obo);
            }
            var cbo = orderBase.FirstOrDefault(p => p.Distance(position) < 100);
            if (cbo != null && !cbo.IsZero)
            {
                return new Tuple<GameObjectTeam, Lane, Vector2>(GameObjectTeam.Chaos, Lane.Bot, cbo);
            }
            return new Tuple<GameObjectTeam, Lane, Vector2>(GameObjectTeam.Unknown, Lane.Unknown, Vector2.Zero);
        }

        public static IEnumerable<Obj_AI_Turret> GetTurrets(GameObjectTeam team, Lane lane)
        {
            switch (team)
            {
                case GameObjectTeam.Order:
                {
                    switch (lane)
                    {
                        case Lane.Base:
                        {
                            return
                                GameObjects.Turrets.Where(
                                    turret =>
                                        orderBase.Any(
                                            orderBaseTurretPosition =>
                                                orderBaseTurretPosition.Distance(turret.Position) < 250));
                        }
                        case Lane.Bot:
                        {
                            return GameObjects.Turrets.Where(
                                turret =>
                                    orderBot.Any(
                                        orderBotTurretPosition => orderBotTurretPosition.Distance(turret.Position) < 250));
                        }
                        case Lane.Mid:
                        {
                            return GameObjects.Turrets.Where(
                                turret =>
                                    orderMid.Any(
                                        orderMidPosition => orderMidPosition.Distance(turret.Position) < 250));
                        }
                        case Lane.Top:
                        {
                            return GameObjects.Turrets.Where(
                                turret =>
                                    orderTop.Any(
                                        orderTopTurretPosition => orderTopTurretPosition.Distance(turret.Position) < 250));
                        }
                    }
                    break;
                }
                case GameObjectTeam.Chaos:
                {
                    switch (lane)
                    {
                        case Lane.Base:
                        {
                            return
                                GameObjects.Turrets.Where(
                                    turret =>
                                        chaosBase.Any(
                                            chaosBaseTurretPosition =>
                                                chaosBaseTurretPosition.Distance(turret.Position) < 250));
                        }
                        case Lane.Bot:
                        {
                            return GameObjects.Turrets.Where(
                                turret =>
                                    chaosBot.Any(
                                        chaosBotTurretPosition => chaosBotTurretPosition.Distance(turret.Position) < 250));
                        }
                        case Lane.Mid:
                        {
                            return GameObjects.Turrets.Where(
                                turret =>
                                    chaosMid.Any(
                                        chaosMidPosition => chaosMidPosition.Distance(turret.Position) < 250));
                        }
                        case Lane.Top:
                        {
                            return GameObjects.Turrets.Where(
                                turret =>
                                    chaosTop.Any(
                                        chaosTopTurretPosition => chaosTopTurretPosition.Distance(turret.Position) < 250));
                        }
                    }
                    break;
                }
                default:
                    return GameObjects.AllyTurrets;
            }
            return GameObjects.AllyTurrets;
        }
    }
}