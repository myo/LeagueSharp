using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using HERMES_Kalista.MyUtils;
using Color = System.Drawing.Color;

namespace HERMES_Kalista.MyLogic.Others
{
    public static partial class Events
    {
        public static void OnDraw(EventArgs args)
        {
            foreach (var hero in HeroManager.Enemies.Where(h => h.IsValidTarget() && h.Distance(Heroes.Player) < 1400))
            {
                var WDMG = Program.W.GetDamage(hero);
                var AADMG = Heroes.Player.GetAutoAttackDamage(hero);
                var AAOnly = (int)(hero.Health / AADMG);
                var Combined = (int)((hero.Health - ((AAOnly/3)*WDMG))/AADMG);
                Drawing.DrawText(hero.HPBarPosition.X + 5, hero.HPBarPosition.Y - 30,
                    Combined <= 3 ? Color.Gold : Color.White,
                    "AAs to kill: " + Combined);
            }

            if (Program.DrawingsMenu.Item("drawenemywaypoints").GetValue<bool>())
            {
                foreach (var e in HeroManager.Enemies.Where(en => en.IsVisible && !en.IsDead && en.Distance(Heroes.Player) < 2500))
                {
                    var ip = Drawing.WorldToScreen(e.Position); //start pos

                    var wp = Utility.GetWaypoints(e);
                    var c = wp.Count - 1;
                    if (wp.Count() <= 1) break;

                    var w = Drawing.WorldToScreen(wp[c].To3D()); //endpos

                    Drawing.DrawLine(ip.X, ip.Y, w.X, w.Y, 2, Color.Red);
                }
            }
        }
    }
}
