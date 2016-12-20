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
        private static float GetMinDistanceFromEnemies()
        {
            var amount = 0f;
            var enemies =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => !h.IsDead && h.IsEnemy && h.IsVisible && h.Distance(ObjectManager.Player) < 1100);
            if (!enemies.Any())
            {
                return 0;
            }
            if (enemies.Count() >
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(h => !h.IsDead && !h.IsMe && h.IsAlly && h.Distance(ObjectManager.Player) < 800) + 1)
            {
                amount = 950;
            }
            var enemyHigherLevelThanMe = enemies.FirstOrDefault(e => e.Level > ObjectManager.Player.Level);
            if (enemyHigherLevelThanMe != null)
            {
                amount = Math.Min(enemyHigherLevelThanMe.AttackRange + 100, 600);
            }
            if (ObjectManager.Player.IsUnderAllyTurret())
            {
                amount -= 800;
            }
            return amount > 0 ?amount:0;
        }
        public static Vector3 GetFarmingPosition()
        {
            var lasthittable =
                Minions.GetMinionsInLane(SessionBasedData.EnemyTeam, SessionBasedData.MyLane)
                    .OrderBy(m => m.Health)
                    .FirstOrDefault(m => m.Health > 1 && m.Health < ObjectManager.Player.GetAutoAttackDamage(m)*2);
            var farthestTurret = Turrets.GetTurretsPosition(ObjectManager.Player.Team, SessionBasedData.CurrentLane).Last();
            if (lasthittable != null)
            {
                return GetLastHitPosition(lasthittable);
            }
            var ourMinion =
                ObjectManager.Get<Obj_AI_Minion>().Where(m=>m.IsAlly && !m.IsDead && m.Position.IsInside(SessionBasedData.CurrentLanePolygon)).OrderBy(m => m.Distance(GameObjects.AllyNexus))
                    .LastOrDefault();
            if (ourMinion != null && ourMinion is Obj_AI_Minion)
            {
                return
                    new Geometry.Circle(ourMinion
                        .ServerPosition.Extend(GameObjects.AllyNexus.Position, Math.Min(Utility.Random.GetRandomInteger(100, 350)+GetMinDistanceFromEnemies(), 1350)).ToVector2(),
                        Utility.Random.GetRandomInteger(100, 350)).ToPolygon().GetRandomPointInPolygon();
            }
            return Game.CursorPos;
        }

        public static Vector3 GetLastHitPosition(Obj_AI_Minion lasthittableresult)
        {
            var myRange = (int)ObjectManager.Player.AttackRange;
            return lasthittableresult.ServerPosition.Extend(ObjectManager.Player.ServerPosition, Math.Min(myRange - Utility.Random.GetRandomInteger(85, myRange/2) + GetMinDistanceFromEnemies(), 1350));
        }

        public static Vector3 GetTeamfightPosition()
        {
            var notInEnemyZone = AllyZone.Points.FirstOrDefault(p => !EnemyZone.Points.Contains(p));
            if (notInEnemyZone != null && notInEnemyZone != Vector2.Zero)
            {
                return notInEnemyZone.ToVector3();
            }
            var farthestFromEnemyZone =
                AllyZone.Points.OrderByDescending(
                    p =>
                        p.Distance(
                            EnemyZone.Points.OrderBy(ep => ep.Distance(ObjectManager.Player.Position)).LastOrDefault()))
                    .LastOrDefault();
            if (farthestFromEnemyZone != null && farthestFromEnemyZone != Vector2.Zero)
            {
                return farthestFromEnemyZone.ToVector3();
            }
            return GetFarmingPosition();
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
                            h.ServerPosition.CountAllyHeroesInRange(1000) > 2).
                    Select(ally => new Geometry.Circle(ally.ServerPosition.ToVector2(), ally.AttackRange).
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
                            h.ServerPosition.IsInside(StaticData.GetWholeLane(SessionBasedData.CurrentLane))).
                    Select(enemy => new Geometry.Circle(enemy.ServerPosition.ToVector2(), enemy.AttackRange).
                        ToPolygon()).ToList();
                var teamPaths = teamPolygons.ClipperUnion();
                return teamPaths.PathsToPolygon();
            }
        }

        #endregion
    }
}