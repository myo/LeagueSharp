using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Vayne.MyUtils;
using SharpDX;
using Color = System.Drawing.Color;

namespace PRADA_Vayne.MyLogic.Q
{
    public static class WallTumble
    {
        private static bool _canWallTumble;
        private static Vector3 _dragPreV3 = new Vector2(12050, 4828).To3D();
        private static Vector3 _dragAftV3 = new Vector2(11510, 4470).To3D();
        private static Vector3 _midPreV3 = new Vector2(6962, 8952).To3D();
        private static Vector3 _midAftV3 = new Vector2(6667, 8794).To3D();

        public static void OnLoad(EventArgs args)
        {
            _canWallTumble = (Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift);
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        public static void OnDraw(EventArgs args)
        {
            if (Program.DrawingsMenu.Item("streamingmode").GetValue<bool>())
            {
                return;
            }

            if (_canWallTumble && Heroes.Player.Distance(_dragPreV3) < 3000)
                Drawing.DrawCircle(_dragPreV3, 75, Color.Gold);
            if (_canWallTumble && Heroes.Player.Distance(_midPreV3) < 3000)
                Drawing.DrawCircle(_midPreV3, 75, Color.Gold);
        }

        public static void OnUpdate(EventArgs args)
        {
            if (_canWallTumble && Program.ComboMenu.Item("QWall").GetValue<bool>() && Program.Q.IsReady() && Heroes.Player.Distance(_dragPreV3) < 500)
            {
                DragWallTumble();
            }

            /*if (_canWallTumble && Program.ComboMenu.Item("QWall").GetValue<bool>() && Program.Q.IsReady() && Heroes.Player.Distance(_midPreV3) < 500)
            {
                MidWallTumble();
            }*/
        }

        private static void DragWallTumble()
        {
            if (Heroes.Player.Distance(_dragPreV3) < 100)
            {
                Heroes.Player.IssueOrder(GameObjectOrder.MoveTo, _dragPreV3.Randomize(0, 1));
            }
            if (Heroes.Player.Distance(_dragPreV3) < 5)
            {
                Program.Orbwalker.SetMovement(false);
                var tumblePos = _dragAftV3;
                Tumble.Cast(tumblePos);
                Utility.DelayAction.Add(100 + Game.Ping / 2, () =>
                {
                    Heroes.Player.IssueOrder(GameObjectOrder.MoveTo, _dragAftV3.Randomize(0, 1));
                    Program.Orbwalker.SetMovement(true);
                });
            }
        }

        private static void MidWallTumble()
        {
            if (!Program.Q.IsReady()) return;
            if (Heroes.Player.Distance(_midPreV3) < 100)
            {
                Heroes.Player.IssueOrder(GameObjectOrder.MoveTo, _midPreV3.Randomize(0, 1));
            }
            if (Heroes.Player.Distance(_midPreV3) < 5)
            {
                Program.Orbwalker.SetMovement(false);
                var tumblePos = _midAftV3;
                Tumble.Cast(tumblePos);
                Utility.DelayAction.Add(100 + Game.Ping / 2, () =>
                {
                    Heroes.Player.IssueOrder(GameObjectOrder.MoveTo, _midAftV3.Randomize(0, 1));
                    Program.Orbwalker.SetMovement(true);
                });
            }
        }
    }
}
