using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter
{
    internal class LasthitIndicator
    {
        internal static void Load()
        {
            MenuProvider.Champion.Drawings.AddItem("Draw Minion Lasthit", new Circle(true, Color.GreenYellow), false);
            MenuProvider.Champion.Drawings.AddItem("Draw Minion NearKill", new Circle(true, Color.Gray), false);

            Drawing.OnDraw += Drawing_OnDraw;

            Console.WriteLine("SharpShooter: LasthitIndicator Loaded.");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var drawMinionLastHit = MenuProvider.Champion.Drawings.GetCircleValue("Draw Minion Lasthit", false);
                var drawMinionNearKill = MenuProvider.Champion.Drawings.GetCircleValue("Draw Minion NearKill", false);

                if (drawMinionLastHit.Active || drawMinionNearKill.Active)
                {
                    var xMinions =
                        MinionManager.GetMinions(
                            ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + 300,
                            MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                    foreach (var xMinion in xMinions)
                    {
                        if (drawMinionLastHit.Active &&
                            ObjectManager.Player.GetAutoAttackDamage(xMinion, true) >= xMinion.Health)
                            Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius - 20,
                                drawMinionLastHit.Color, 3);
                        else if (drawMinionNearKill.Active &&
                                 ObjectManager.Player.GetAutoAttackDamage(xMinion, true)*2 >= xMinion.Health)
                            Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius - 20,
                                drawMinionNearKill.Color, 3);
                    }
                }
            }
        }
    }
}