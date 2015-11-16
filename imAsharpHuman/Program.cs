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
                _menu.AddItem(new MenuItem("MinClicks", "Min clicks per second").SetValue(new Slider(6, 1, 7)));
                _menu.AddItem(new MenuItem("MaxClicks", "Max clicks per second").SetValue(new Slider(11, 7, 15))); 
                _menu.AddItem(new MenuItem("Spells", "Humanize Spells?").SetValue(true));
                _menu.AddItem(new MenuItem("Attacks", "Humanize Attacks?").SetValue(true));
                _menu.AddItem(new MenuItem("Movement", "Humanize Movement?").SetValue(true));
                _menu.AddItem(new MenuItem("Chat", "Humanize Chat?").SetValue(true));
                _menu.AddItem(
                    new MenuItem("ShowBlockedClicks", "Show me how many clicks you blocked!").SetValue(true));
                _menu.AddToMainMenu();
                Drawing.OnDraw += onDrawArgs =>
                {
                    if (_menu.Item("ShowBlockedClicks").GetValue<bool>())
                    {
                        Drawing.DrawText(Drawing.Width - 180, 100, System.Drawing.Color.Lime, "Blocked " + _blockedCount + " clicks");
                    }
                };
            };
            Obj_AI_Base.OnIssueOrder += (sender, issueOrderEventArgs) =>
            {
                if (sender.IsMe && !issueOrderEventArgs.IsAttackMove)
                {
                    if (issueOrderEventArgs.Order == GameObjectOrder.AttackUnit ||
                        issueOrderEventArgs.Order == GameObjectOrder.AttackTo &&
                        !_menu.Item("Attacks").GetValue<bool>()) return;
                    if (issueOrderEventArgs.Order == GameObjectOrder.MoveTo &&
                        !_menu.Item("Movement").GetValue<bool>()) return;


                    var orderName = issueOrderEventArgs.Order.ToString();
                    var order = _lastCommandT.FirstOrDefault(e => e.Key == orderName);
                    if (Utils.GameTimeTickCount - order.Value <
                        _random.Next(1000 / _menu.Item("MaxClicks").GetValue<Slider>().Value,
                            1000 / _menu.Item("MinClicks").GetValue<Slider>().Value) + _random.Next(-10, 10) && !issueOrderEventArgs.TargetPosition.IsWall())
                    {
                        _blockedCount += 1;
                        issueOrderEventArgs.Process = false;
                        return;
                    }
                    if (issueOrderEventArgs.Order == GameObjectOrder.MoveTo &&
                        issueOrderEventArgs.TargetPosition.IsValid() && !_thisMovementCommandHasBeenTamperedWith)
                    {
                        _thisMovementCommandHasBeenTamperedWith = true;
                        issueOrderEventArgs.Process = false;
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                            issueOrderEventArgs.TargetPosition.Randomize(-10, 10));
                    }
                    _thisMovementCommandHasBeenTamperedWith = false;
                    _lastCommandT.Remove(orderName);
                    _lastCommandT.Add(orderName, Utils.GameTimeTickCount);
                }
            };
            Spellbook.OnCastSpell += (sender, eventArgs) =>
            {
                return;


                var cN = ObjectManager.Player.ChampionName;
                if (sender.Owner.IsMe && cN != "Viktor" && cN != "Rumble" &&
                    eventArgs.StartPosition.Distance(ObjectManager.Player.ServerPosition, true) > 50 * 50 &&
                    eventArgs.StartPosition.Distance(ObjectManager.Player.Position, true) > 50 * 50 &&
                    eventArgs.Target == null)
                {
                    if (_lastCommandT.FirstOrDefault(e => e.Key == "spellcast" + eventArgs.Slot).Value == 0)
                    {
                        _lastCommandT.Remove("spellcast" + eventArgs.Slot);
                        _lastCommandT.Add("spellcast" + eventArgs.Slot, Utils.GameTimeTickCount);
                        eventArgs.Process = false;
                        ObjectManager.Player.Spellbook.CastSpell(eventArgs.Slot,
                            eventArgs.StartPosition.Randomize(-10, 10));
                        return;
                    }
                    _lastCommandT.Remove("spellcast" + eventArgs.Slot);
                    _lastCommandT.Add("spellcast" + eventArgs.Slot, 0);
                }
            };
            Game.OnChat += gameChatEventArgs =>
            {
                if (gameChatEventArgs.Sender.IsMe && _menu.Item("Chat").GetValue<bool>())
                {
                    if (Utils.GameTimeTickCount - _lastCommandT.FirstOrDefault(e => e.Key == "lastchat").Value <
                        _random.Next(100, 200))
                    {
                        gameChatEventArgs.Process = false;
                    }
                    _lastCommandT.Remove("lastchat");
                    _lastCommandT.Add("lastchat", Utils.GameTimeTickCount);
                }
            };
        }
    }
}
