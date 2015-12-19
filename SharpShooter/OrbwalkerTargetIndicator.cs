using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter
{
    internal class OrbwalkerTargetIndicator
    {
        internal static void Load()
        {
            MenuProvider.Champion.Drawings.AddItem("Draw AutoAttack Target", new Circle(true, Color.Red), false);

            Drawing.OnDraw += Drawing_OnDraw;

            Console.WriteLine("SharpShooter: OrbwalkerTargetindicator Loaded.");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
                if (MenuProvider.Champion.Drawings.GetCircleValue("Draw AutoAttack Target", false).Active)
                {
                    var orbwalkerTarget = MenuProvider.Orbwalker.GetTarget();

                    if (orbwalkerTarget.IsValidTarget())
                        Render.Circle.DrawCircle(orbwalkerTarget.Position, orbwalkerTarget.BoundingRadius,
                            MenuProvider.Champion.Drawings.GetCircleValue("Draw AutoAttack Target", false).Color);
                }
        }
    }
}