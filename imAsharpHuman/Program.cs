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
        private static int _lastMoveT = 0;
        private static int _lastAttackT = 0;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += gameLoadEventArgs =>
            {
                _random = new Random(Environment.TickCount);
                _menu = new Menu("imAsharpHuman", "imasharphumanmenu", true);
                _menu.AddItem(new MenuItem("MinClicks", "Min clicks per second").SetValue(new Slider(6, 1, 6)));
                _menu.AddItem(new MenuItem("MaxClicks", "Max clicks per second").SetValue(new Slider(9, 7, 15)));
                _menu.AddToMainMenu();
            };
            Obj_AI_Base.OnIssueOrder += (sender, issueOrderEventArgs) =>
            {
                if (sender.IsMe)
                {
                    if (issueOrderEventArgs.Order == GameObjectOrder.MoveTo)
                    {
                        if (Utils.GameTimeTickCount - _lastMoveT <
                            _random.Next(1000/_menu.Item("MaxClicks").GetValue<Slider>().Value,
                                1000/_menu.Item("MinClicks").GetValue<Slider>().Value) + _random.Next(-10, 10))
                        {
                            issueOrderEventArgs.Process = false;
                            return;
                        }
                        _lastMoveT = Utils.GameTimeTickCount;
                    }
                    if (issueOrderEventArgs.Order == GameObjectOrder.AttackUnit)
                    {

                        if (Utils.GameTimeTickCount - _lastAttackT <
                            _random.Next(1000 / _menu.Item("MaxClicks").GetValue<Slider>().Value,
                                1000 / _menu.Item("MinClicks").GetValue<Slider>().Value) + _random.Next(-10, 10))
                        {
                            issueOrderEventArgs.Process = false;
                            return;
                        }
                        _lastAttackT = Utils.GameTimeTickCount;
                    }
                }
            };
        }
    }
}
