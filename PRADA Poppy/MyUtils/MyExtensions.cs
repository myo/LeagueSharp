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

namespace PRADA_Poppy.MyUtils
{
    public static class Extensions
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static bool IsCondemnable(this Obj_AI_Hero hero)
        {
            if (!hero.IsValidTarget(550f) || hero.HasBuffOfType(BuffType.SpellShield) ||
                hero.HasBuffOfType(BuffType.SpellImmunity) || hero.IsDashing()) return false;

            //values for pred calc pP = player position; p = enemy position; pD = push distance
            var pP = Heroes.Player.ServerPosition;
            var p = hero.ServerPosition;
            var pD = Program.ComboMenu.Item("EPushDist").GetValue<Slider>().Value;
            var mode = Program.ComboMenu.Item("EMode").GetValue<StringList>().SelectedValue;


            if (mode == "PRADASMART" && (p.Extend(pP, -pD).IsCollisionable() || p.Extend(pP, -pD / 2f).IsCollisionable() ||
                 p.Extend(pP, -pD / 3f).IsCollisionable()))
            {
                if (!hero.CanMove ||
                    (hero.IsWindingUp))
                    return true;

                var enemiesCount = ObjectManager.Player.CountEnemiesInRange(1200);
                if (enemiesCount > 1 && enemiesCount <= 3)
                {
                    var prediction = Program.E.GetPrediction(hero);
                    for (var i = 15; i < pD; i += 75)
                    {
                        var posFlags = NavMesh.GetCollisionFlags(
                            prediction.UnitPosition.To2D()
                                .Extend(
                                    pP.To2D(),
                                    -i)
                                .To3D());
                        if (posFlags.HasFlag(CollisionFlags.Wall) || posFlags.HasFlag(CollisionFlags.Building))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    var hitchance = Program.ComboMenu.Item("EHitchance").GetValue<Slider>().Value;
                    var angle = 0.20 * hitchance;
                    const float travelDistance = 0.5f;
                    var alpha = new Vector2((float)(p.X + travelDistance * Math.Cos(Math.PI / 180 * angle)),
                        (float)(p.X + travelDistance * Math.Sin(Math.PI / 180 * angle)));
                    var beta = new Vector2((float)(p.X - travelDistance * Math.Cos(Math.PI / 180 * angle)),
                        (float)(p.X - travelDistance * Math.Sin(Math.PI / 180 * angle)));

                    for (var i = 15; i < pD; i += 100)
                    {
                        if (pP.To2D().Extend(alpha,
                                i)
                            .To3D().IsCollisionable() && pP.To2D().Extend(beta, i).To3D().IsCollisionable()) return true;
                    }
                    return false;
                }
            }

            if (mode == "PRADAPERFECT" &&
                (p.Extend(pP, -pD).IsCollisionable() || p.Extend(pP, -pD/2f).IsCollisionable() ||
                 p.Extend(pP, -pD/3f).IsCollisionable()))
            {
                if (!hero.CanMove ||
                    (hero.IsWindingUp))
                    return true;

                var hitchance = Program.ComboMenu.Item("EHitchance").GetValue<Slider>().Value;
                var angle = 0.20*hitchance;
                const float travelDistance = 0.5f;
                var alpha = new Vector2((float) (p.X + travelDistance*Math.Cos(Math.PI/180*angle)),
                    (float) (p.X + travelDistance*Math.Sin(Math.PI/180*angle)));
                var beta = new Vector2((float) (p.X - travelDistance*Math.Cos(Math.PI/180*angle)),
                    (float) (p.X - travelDistance*Math.Sin(Math.PI/180*angle)));

                for (var i = 15; i < pD; i += 100)
                {
                    if (pP.To2D().Extend(alpha,
                            i)
                        .To3D().IsCollisionable() && pP.To2D().Extend(beta, i).To3D().IsCollisionable()) return true;
                }
                return false;
            }

            if (mode == "OLDPRADA")
            {
                    if (!hero.CanMove ||
                        (hero.IsWindingUp))
                        return true;

                    var hitchance = Program.ComboMenu.Item("EHitchance").GetValue<Slider>().Value;
                    var angle = 0.20 * hitchance;
                    const float travelDistance = 0.5f;
                    var alpha = new Vector2((float)(p.X + travelDistance * Math.Cos(Math.PI / 180 * angle)),
                        (float)(p.X + travelDistance * Math.Sin(Math.PI / 180 * angle)));
                    var beta = new Vector2((float)(p.X - travelDistance * Math.Cos(Math.PI / 180 * angle)),
                        (float)(p.X - travelDistance * Math.Sin(Math.PI / 180 * angle)));

                    for (var i = 15; i < pD; i += 100)
                    {
                        if (pP.To2D().Extend(alpha,
                                i)
                            .To3D().IsCollisionable() || pP.To2D().Extend(beta, i).To3D().IsCollisionable()) return true;
                    }
                    return false;
            }

            if (mode == "MARKSMAN")
            {
                var prediction = Program.E.GetPrediction(hero);
                return NavMesh.GetCollisionFlags(
                    prediction.UnitPosition.To2D()
                        .Extend(
                            pP.To2D(),
                            -pD)
                        .To3D()).HasFlag(CollisionFlags.Wall) ||
                       NavMesh.GetCollisionFlags(
                           prediction.UnitPosition.To2D()
                               .Extend(
                                   pP.To2D(),
                                   -pD / 2f)
                               .To3D()).HasFlag(CollisionFlags.Wall);
            }

            if (mode == "SHARPSHOOTER")
            {
                var prediction = Program.E.GetPrediction(hero);
                for (var i = 15; i < pD; i += 100)
                {
                    var posCF = NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.To2D()
                            .Extend(
                                pP.To2D(),
                                -i)
                            .To3D());
                    if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                    {
                        return true;
                    }
                }
                return false;
            }

            if (mode == "GOSU")
            {
                var prediction = Program.E.GetPrediction(hero);
                for (var i = 15; i < pD; i += 75)
                {
                    var posCF = NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.To2D()
                            .Extend(
                                pP.To2D(),
                                -i)
                            .To3D());
                    if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                    {
                        return true;
                    }
                }
                return false;
            }

            if (mode == "VHR")
            {
                var prediction = Program.E.GetPrediction(hero);
                for (var i = 15; i < pD; i += (int)hero.BoundingRadius) //:frosty:
                {
                    var posCF = NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.To2D()
                            .Extend(
                                pP.To2D(),
                                -i)
                            .To3D());
                    if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                    {
                        return true;
                    }
                }
                return false;
            }

            if (mode == "PRADALEGACY")
            {
                var prediction = Program.E.GetPrediction(hero);
                for (var i = 15; i < pD; i += 75)
                {
                    var posCF = NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.To2D()
                            .Extend(
                                pP.To2D(),
                                -i)
                            .To3D());
                    if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                    {
                        return true;
                    }
                }
                return false;
            }

            if (mode == "FASTEST" && (p.Extend(pP, -pD).IsCollisionable() || p.Extend(pP, -pD/2f).IsCollisionable() ||
                                     p.Extend(pP, -pD/3f).IsCollisionable()))
            {
                return true;
            }

            return false;
        }

        public static Vector3 GetTumblePos(this Obj_AI_Base target)
        {
            //if the target is not a melee and he's alone he's not really a danger to us, proceed to 1v1 him :^ )
            if (!target.IsMelee && Heroes.Player.CountEnemiesInRange(800) == 1) return Game.CursorPos;

            var aRC = new Geometry.Circle(Heroes.Player.ServerPosition.To2D(), 300).ToPolygon().ToClipperPath();
            var cursorPos = Game.CursorPos;
            var targetPosition = target.ServerPosition;
            var pList = new List<Vector3>();
            var additionalDistance = (0.106 + Game.Ping / 2000f) * target.MoveSpeed;

            if (!cursorPos.IsDangerousPosition()) return cursorPos;

            foreach (var p in aRC)
            {
                var v3 = new Vector2(p.X, p.Y).To3D();

                if (target.IsFacing(Heroes.Player))
                {
                    if (!v3.IsDangerousPosition() && v3.Distance(targetPosition) < 550) pList.Add(v3);
                }
                else
                {
                    if (!v3.IsDangerousPosition() && v3.Distance(targetPosition) < 550 - additionalDistance) pList.Add(v3);
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
            return pList.Count > 1 ? pList.OrderByDescending(el => el.Distance(cursorPos)).FirstOrDefault() : Vector3.Zero;
        }

        public static int VayneWStacks(this Obj_AI_Base o)
        {
            if (o == null) return 0;
            if (o.Buffs.FirstOrDefault(b => b.Name.Contains("vaynesilver")) == null || !o.Buffs.Any(b => b.Name.Contains("vaynesilver"))) return 0;
            return o.Buffs.FirstOrDefault(b => b.Name.Contains("vaynesilver")).Count;
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
                        e.Distance(pos) < Program.ComboMenu.Item("QMinDist").GetValue<Slider>().Value) ||
                Traps.EnemyTraps.Any(t => pos.Distance(t.Position) < 125) ||
                (pos.UnderTurret(true) && !Player.UnderTurret(true)) || pos.IsWall();
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
