using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HERMES_Kalista.MyLogic.Others;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;

namespace HERMES_Kalista.MyUtils
{
    public static class Extensions
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static Vector3 GetKitePos(this AttackableUnit t)
        {
            var target = t as Obj_AI_Base;
            //if the target is not a melee and he's alone he's not really a danger to us, proceed to 1v1 him :^ )
            if (!target.IsMelee && Heroes.Player.CountEnemiesInRange(800) == 1) return Game.CursorPos;

            var aRC = new Geometry.Circle(Heroes.Player.ServerPosition.To2D(), 300).ToPolygon().ToClipperPath();
            var cursorPos = Game.CursorPos;
            var targetPosition = target.ServerPosition;
            var pList = new List<Vector3>();
            var additionalDistance = (0.106 + Game.Ping/2000f)*target.MoveSpeed;


            foreach (var p in aRC)
            {
                var v3 = new Vector2(p.X, p.Y).To3D();

                if (target.IsFacing(Heroes.Player))
                {
                    if (!v3.IsDangerousPosition() && v3.Distance(targetPosition) < 500) pList.Add(v3);
                }
                else
                {
                    if (!v3.IsDangerousPosition() && v3.Distance(targetPosition) < 500 - additionalDistance)
                        pList.Add(v3);
                }
            }
            if (Heroes.Player.UnderTurret() || Heroes.Player.CountEnemiesInRange(800) == 1)
            {
                return pList.Count > 1 ? pList.OrderBy(el => el.Distance(cursorPos)).FirstOrDefault() : Vector3.Zero;
            }
            if (!cursorPos.IsDangerousPosition())
            {
                return pList.Count > 1 ? pList.OrderBy(el => el.Distance(cursorPos)).FirstOrDefault() : Vector3.Zero;
            }
            return pList.Count > 1
                ? pList.OrderByDescending(el => el.Distance(cursorPos)).FirstOrDefault()
                : Vector3.Zero;
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
                    e => e.IsValidTarget() && e.IsVisible &&
                         e.Distance(pos) < 350) ||
                Traps.EnemyTraps.Any(t => pos.Distance(t.Position) < 125) ||
                (pos.UnderTurret(true) && !Player.UnderTurret(true)) || pos.IsWall();
        }

        public static bool IsKillable(this Obj_AI_Hero hero)
        {
            return Player.GetAutoAttackDamage(hero)*2 < hero.Health;
        }

        public static bool IsCollisionable(this Vector3 pos)
        {
            return NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall) ||
                   (Program.Orbwalker.ActiveMode == MyOrbwalker.OrbwalkingMode.Combo &&
                    NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Building));
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

        public static bool HasUndyingBuff(this Obj_AI_Hero target)
        {
            // Various buffs
            if (target.Buffs.Any(
                b => b.IsValid &&
                     (b.DisplayName == "Chrono Shift" /* Zilean R */||
                      b.DisplayName == "JudicatorIntervention" /* Kayle R */||
                      b.DisplayName == "Undying Rage" /* Tryndamere R */)))
            {
                return true;
            }

            // Poppy R
            if (target.ChampionName == "Poppy")
            {
                if (
                    HeroManager.Allies.Any(
                        o =>
                            !o.IsMe &&
                            o.Buffs.Any(
                                b =>
                                    b.Caster.NetworkId == target.NetworkId && b.IsValid &&
                                    b.DisplayName == "PoppyDITarget")))
                {
                    return true;
                }
            }

            return target.IsInvulnerable;
        }

        public static bool HasSpellShield(this Obj_AI_Hero target)
        {
            // Various spellshields
            return target.HasBuffOfType(BuffType.SpellShield) || target.HasBuffOfType(BuffType.SpellImmunity);
        }

        public static List<T> MakeUnique<T>(this List<T> list) where T : Obj_AI_Base, new()
        {
            var uniqueList = new List<T>();

            foreach (var entry in list.Where(entry => uniqueList.All(e => e.NetworkId != entry.NetworkId)))
            {
                uniqueList.Add(entry);
            }

            list.Clear();
            list.AddRange(uniqueList);

            return list;
        }
    }
}
