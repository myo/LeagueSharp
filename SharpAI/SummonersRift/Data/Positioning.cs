using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAI.Utility;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Utils;
using SharpDX;
using Geometry = SharpAI.Utility.Geometry;

namespace SharpAI.SummonersRift.Data
{
    public static class Positioning
    {
        public static Vector3 GetFarmingPosition()
        {
            var lasthittable =
                Minions.GetMinionsInLane(SessionBasedData.EnemyTeam, SessionBasedData.MyLane)
                    .OrderBy(m => m.Health)
                    .FirstOrDefault(m => m.Health < ObjectManager.Player.GetAutoAttackDamage(m)*1.5);
            if (lasthittable != null)
            {
                return GetLastHitPosition(lasthittable);
            }
            var ourMinion =
                ObjectManager.Get<Obj_AI_Minion>().Where(m=>m.IsAlly && !m.IsDead && m.Position.IsInside(SessionBasedData.CurrentLanePolygon)).OrderBy(m => m.Distance(GameObjects.AllyNexus))
                    .LastOrDefault();
            if (ourMinion != null)
            {
                return
                    new Geometry.Circle(ourMinion
                        .Position.Extend(GameObjects.AllyNexus.Position, Utility.Random.GetRandomInteger(350, 550)).ToVector2(),
                        Utility.Random.GetRandomInteger(100, 350)).ToPolygon().GetRandomPointInPolygon();
            }
            return Game.CursorPos;
        }

        public static Vector3 GetLastHitPosition(Obj_AI_Minion lasthittableresult)
        {
            var myRange = (int)ObjectManager.Player.AttackRange;
            return
                new Utility.Geometry.Circle(lasthittableresult.Position.Extend(ObjectManager.Player.ServerPosition,
                    Utility.Random.GetRandomInteger(myRange - 250, myRange - 150)).ToVector2(), 250).ToPolygon()
                    .GetRandomPointInPolygon();
        }

        public static Vector3 GetTeamfightPosition()
        {
            return GetFarmingPosition(); //AllyZone.ClipperXor(EnemyZone).PathsToPolygon().GetRandomPointInPolygon(); need to fix XOR :roto2: --fixed in 2.0
        }

        #region REWORKED BROSCIENCE FROM AIM XD        

        /// <summary>
        /// Returns a list of points in the Ally Zone
        /// </summary>
        internal static Geometry.Polygon AllyZone
        {
            get
            {
                var teamPolygons = GameObjects.AllyHeroes.
                    Where(
                        h =>
                            !h.IsDead && !h.IsMe && !h.InFountain() &&
                            h.Position.CountAllyHeroesInRange(1000) > 2).
                    Select(ally => new Geometry.Circle(ally.Position.ToVector2(), ally.AttackRange).
                        ToPolygon()).ToList();
                var teamPaths = teamPolygons.ClipperUnion();
                return teamPaths.PathsToPolygon();
            }
        }

        /// <summary>
        /// Returns a list of points in the Enemy Zone
        /// </summary>
        internal static Geometry.Polygon EnemyZone
        {
            get
            {
                var teamPolygons = GameObjects.EnemyHeroes.
                    Where(
                        h =>
                            !h.IsDead && h.IsMelee &&
                            h.Position.IsInside(StaticData.GetWholeLane(SessionBasedData.CurrentLane))).
                    Select(enemy => new Geometry.Circle(enemy.Position.ToVector2(), enemy.AttackRange).
                        ToPolygon()).ToList();
                var teamPaths = teamPolygons.ClipperUnion();
                return teamPaths.PathsToPolygon();
            }
        }

        #endregion
    }
}