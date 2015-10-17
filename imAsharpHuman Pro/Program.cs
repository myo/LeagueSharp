using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace imAsharpHuman
{
    class Program
    {
        static Menu _menu;
        static Random _random;
        private static Dictionary<string, int> _lastCommandT;
        private static int _blockedCount = 0;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += gameLoadEventArgs =>
            {
                _random = new Random(Environment.TickCount - Utils.GameTimeTickCount);
                _lastCommandT = new Dictionary<string, int>();
                foreach (var order in Enum.GetValues(typeof (GameObjectOrder)))
                {
                    _lastCommandT.Add(order.ToString(), 0);
                    Console.WriteLine(order.ToString());
                }
                _menu = new Menu("imAsharpHuman PRO", "iashpromenu", true);
                _menu.AddItem(new MenuItem("iashpromenu.MinClicks", "Min clicks per second").SetValue(new Slider(_random.Next(3,6), 1, 6)).DontSave());
                _menu.AddItem(new MenuItem("iashpromenu.MaxClicks", "Max clicks per second").SetValue(new Slider(_random.Next(6, 11), 6, 15)).DontSave());
                _menu.AddItem(
                    new MenuItem("iashpromenu.ShowBlockedClicks", "Show me how many clicks you blocked!").SetValue(true));
                _menu.AddToMainMenu();
                Drawing.OnDraw += onDrawArgs =>
                {
                    if (_menu.Item("iashpromenu.ShowBlockedClicks").GetValue<bool>())
                    {
                        Drawing.DrawText(Drawing.Width - 180, 100, System.Drawing.Color.Lime, "Blocked " + _blockedCount + " clicks");
                    }
                };
            };
            Obj_AI_Base.OnIssueOrder += (sender, issueOrderEventArgs) =>
            {
                if (sender.IsMe)
                {
                    var orderName = issueOrderEventArgs.Order.ToString();
                    var order = _lastCommandT.FirstOrDefault(e => e.Key == orderName);
                        if (Utils.GameTimeTickCount - order.Value <
                            _random.Next(1000/_menu.Item("iashpromenu.MaxClicks").GetValue<Slider>().Value,
                                1000/_menu.Item("iashpromenu.MinClicks").GetValue<Slider>().Value) + _random.Next(-10, 10))
                        {
                            _blockedCount += 1;
                            issueOrderEventArgs.Process = false;
                            return;
                        }
                    _lastCommandT.Remove(orderName);
                    _lastCommandT.Add(orderName, Utils.GameTimeTickCount);
                }
            };
        }
    }
}
