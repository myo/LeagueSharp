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
                _random = new Random(Environment.TickCount - Utils.GameTimeTickCount);
                _menu = new Menu("imAsharpHuman PRO", "iashpromenu", true);
                _menu.AddItem(new MenuItem("iashpromenu.MinClicks", "Min clicks per second").SetValue(new Slider(_random.Next(5,6), 1, 6)).DontSave());
                _menu.AddItem(new MenuItem("iashpromenu.MaxClicks", "Max clicks per second").SetValue(new Slider(_random.Next(7, 11), 7, 15)).DontSave());
                _menu.AddToMainMenu();
            };
            Obj_AI_Base.OnIssueOrder += (sender, issueOrderEventArgs) =>
            {
                if (sender.IsMe)
                {
                    if (issueOrderEventArgs.Order == GameObjectOrder.MoveTo)
                    {
                        if (Utils.GameTimeTickCount - _lastMoveT <
                            _random.Next(1000/_menu.Item("iashpromenu.MaxClicks").GetValue<Slider>().Value,
                                1000/_menu.Item("iashpromenu.MinClicks").GetValue<Slider>().Value))
                        {
                            issueOrderEventArgs.Process = false;
                            return;
                        }
                        _lastMoveT = Utils.GameTimeTickCount;
                    }
                    if (issueOrderEventArgs.Order == GameObjectOrder.AttackUnit)
                    {
                        if (Utils.GameTimeTickCount - _lastAttackT <
                            _random.Next(1000 / _menu.Item("iashpromenu.MaxClicks").GetValue<Slider>().Value,
                                1000 / _menu.Item("iashpromenu.MinClicks").GetValue<Slider>().Value))
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
