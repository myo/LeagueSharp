using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;

namespace DRAVEN_Draven.MyUtils
{
    public static class Extensions
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static Vector3 GetPositioning(this Obj_AI_Base target)
        {
            var cursorPos = Game.CursorPos;
            //if the target is not a melee and he's alone he's not really a danger to us, proceed to 1v1 him :^ )
            if (!target.IsMelee && Heroes.Player.CountEnemiesInRange(800) == 1) return cursorPos;
            if (!cursorPos.IsDangerousPosition()) return cursorPos;


            var aRC = new Geometry.Circle(Heroes.Player.ServerPosition.To2D(), 575).ToPolygon().ToClipperPath();
            var targetPosition = Prediction.GetPrediction(target, 1800f).UnitPosition;
            var pList = new List<Vector3>();

            if (!cursorPos.IsDangerousPosition() || Player.UnderTurret() || Game.CursorPos.UnderTurret(true) || Player.CountEnemiesInRange(1400) == 1) return cursorPos;

            foreach (var p in aRC)
            {
                var v3 = new Vector2(p.X, p.Y).To3D();

                if (!v3.IsDangerousPosition() && v3.Distance(targetPosition) > 375 && v3.Distance(targetPosition) < 600) pList.Add(v3);
            }
            return pList.Count > 1 ? pList.OrderBy(el => el.Distance(cursorPos)).FirstOrDefault() : Vector3.Zero;
        }

        public static Vector3 Randomize(this Vector3 pos)
        {
            var r = new Random(Environment.TickCount);
            return new Vector2(pos.X + r.Next(-150, 150), pos.Y + r.Next(-150, 150)).To3D();
        }

        public static bool IsDangerousPosition(this Vector3 pos)
        {
            return
                HeroManager.Enemies.Any(
                    e => e.IsMelee && e.IsValidTarget() && e.IsVisible &&
                        e.Distance(pos) < Program.ComboMenu.Item("QMinDist").GetValue<Slider>().Value) ||
                Traps.EnemyTraps.Any(t => pos.Distance(t.Position) < 125);
        }

        public static bool IsKillable(this Obj_AI_Hero hero)
        {
            return Player.GetAutoAttackDamage(hero) * 2 < hero.Health;
        }

        public static bool IsCollisionable(this Vector3 pos)
        {
            return NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall) ||
                (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.Combo && NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Building));
        }
        public static bool IsValidState(this Obj_AI_Hero target)
        {
            return !target.HasBuffOfType(BuffType.SpellShield) && !target.HasBuffOfType(BuffType.SpellImmunity) &&
                   !target.HasBuffOfType(BuffType.Invulnerability);
        }

        public static int CountHerosInRange(this Obj_AI_Hero target, bool checkteam, float range = 1200f)
        {
            var objListTeam =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        x => x.IsValidTarget(range, false));

            return objListTeam.Count(hero => checkteam ? hero.Team != target.Team : hero.Team == target.Team);
        }
    }
}
