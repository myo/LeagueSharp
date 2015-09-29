using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using HERMES_Kalista.MyLogic.Others;

namespace HERMES_Kalista.MyInitializer
{
    public static partial class HERMESLoader
    {
        public static void Draw()
        {
            Utility.HpBarDamageIndicator.DamageToUnit = GetRealDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            CustomDamageIndicator.Initialize(GetRealDamage);
            Drawing.OnDraw += args =>
            {
                    Render.Circle.DrawCircle(
                        ObjectManager.Player.Position,
                        1000,
                        Color.LightGreen);

                CustomDamageIndicator.DrawingColor = Color.LightGreen;

                    foreach (var source in
                        HeroManager.Enemies.Where(x => ObjectManager.Player.Distance(x) <= 2000f && !x.IsDead))
                    {
                        var currentPercentage = GetRealDamage(source) * 100 / source.Health;

                        Drawing.DrawText(
                            Drawing.WorldToScreen(source.Position)[0],
                            Drawing.WorldToScreen(source.Position)[1],
                            currentPercentage >= 100 ? Color.DarkRed : Color.White,
                            currentPercentage >= 100
                                ? "Killable With E"
                                : "Current Damage: " + currentPercentage + "%");
                    }
            };
        }

        public static float GetRealDamage(Obj_AI_Hero target)
        {
            if (target.HasBuff("ferocioushowl"))
            {
                return Program.E.GetDamage(target) * 0.7f;
            }

            return Program.E.GetDamage(target);
        }
    }
}