using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace PaintSharp
{
    public class Program
    {
        public static Menu MainMenu;
        public static Dictionary<int, PaintSharpPoint> Points;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += l =>
                {
                    MainMenu = new Menu("PaintSharp", "paintsharp", true);
                    MainMenu.AddItem(new MenuItem("paintsharp.brushsize", "Point Size").SetValue(new Slider(30, 10, 150)));
                    MainMenu.AddItem(new MenuItem("paintsharp.duration", "Life-time of a Point (s)").SetValue(new Slider(10, 0, 60)));
                    MainMenu.AddItem(new MenuItem("paintsharp.color", "Current Point Color").SetValue(new Circle(true, Color.Turquoise)));
                    MainMenu.AddItem(new MenuItem("paintsharp.add", "Draw!").SetValue(new KeyBind(73, KeyBindType.Press)));
                    MainMenu.AddItem(new MenuItem("paintsharp.enabled", "Enabled (Draw Points?)").SetValue(true));
                    MainMenu.AddToMainMenu();
                    Points = new Dictionary<int, PaintSharpPoint>();
                };
            Drawing.OnDraw += d =>
                {
                    if (Points.Values.Any())
                    {
                        foreach (var point in Points.Values)
                        {
                            Drawing.DrawCircle(point.Position, point.Size, point.Color);
                        }
                    }
                };
            Game.OnUpdate += u =>
                {
                    if (MainMenu.Item("paintsharp.enabled").GetValue<bool>() && MainMenu.Item("paintsharp.add").GetValue<KeyBind>().Active)
                    {
                        try
                        {
                            var tickCount = Utils.GameTimeTickCount;
                            Points.Add(tickCount, new PaintSharpPoint(Game.CursorPos, MainMenu.Item("paintsharp.brushsize").GetValue<Slider>().Value, MainMenu.Item("paintsharp.color").GetValue<Circle>().Color));
                            Utility.DelayAction.Add(MainMenu.Item("paintsharp.duration").GetValue<Slider>().Value * 1000, () => Points.Remove(tickCount));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                };
        }
    }
    
    public class PaintSharpPoint
    {
        private Vector3 _position;
        private int _size;
        private Color _color;
        public PaintSharpPoint(Vector3 pointPosition, int pointSize, Color pointColor)
        {
            _position = pointPosition;
            _size = pointSize;
            _color = pointColor;
        }
        public int Size
        {
            get { return _size; }
        }
        public Color Color
        {
            get { return _color;  }
        }
        public Vector3 Position
        {
            get { return _position; }
        }
    }
}
