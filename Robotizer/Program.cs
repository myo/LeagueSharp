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
        private static bool _thisMovementCommandHasBeenTamperedWith = false;
        private static int _blockedCount = 0;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += gameLoadEventArgs =>
            {
                Game.PrintChat("Enjoy free ban friend!");
                Game.PrintChat("If you don't want to get banned there's still time.");
                Game.PrintChat("Uncheck this assembly and press F5");
                _random = new Random(Environment.TickCount - Utils.GameTimeTickCount);
                _lastCommandT = new Dictionary<string, int>();
                foreach (var order in Enum.GetValues(typeof(GameObjectOrder)))
                {
                    _lastCommandT.Add(order.ToString(), 0);
                }
                foreach (var spellslot in Enum.GetValues(typeof(SpellSlot)))
                {
                    _lastCommandT.Add("spellcast" + spellslot.ToString(), 0);
                }
                _lastCommandT.Add("lastchat", 0);
                _menu = new Menu("imAsharpHuman PRO", "iashmenu", true);
                _menu.AddItem(new MenuItem("MinClicks", "Min clicks per second").SetValue(new Slider(30, 30, 30)));
                _menu.AddItem(new MenuItem("MaxClicks", "Max clicks per second").SetValue(new Slider(30, 30, 60)));
                _menu.AddItem(
                    new MenuItem("ShowBlockedClicks", "Show me how many clicks you generated!").SetValue(true));
                _menu.AddToMainMenu();
                Drawing.OnDraw += onDrawArgs =>
                {
                    if (_menu.Item("ShowBlockedClicks").GetValue<bool>())
                    {
                        Drawing.DrawText(Drawing.Width - 180, 100, System.Drawing.Color.Lime, "Generated " + _blockedCount + " clicks");
                    }
                };
            };
            Obj_AI_Base.OnIssueOrder += (sender, issueOrderEventArgs) =>
            {
                if (sender.IsMe && issueOrderEventArgs.Order == GameObjectOrder.MoveTo)
                {
                    Utility.DelayAction.Add(
                        _random.Next(1000/_menu.Item("MaxClicks").GetValue<Slider>().Value,
                            1000/_menu.Item("MinClicks").GetValue<Slider>().Value),
                        () =>
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, issueOrderEventArgs.TargetPosition);
                        });
                }
            };
        }
    }
}
