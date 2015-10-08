using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Poppy.MyUtils;
using Color = System.Drawing.Color;

namespace PRADA_Poppy.MyLogic.Others
{
    public static partial class Events
    {
        public static void OnDraw(EventArgs args)
        {
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
