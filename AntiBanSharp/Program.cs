using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using SharpDX;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace AntiBanSharp
{
    class Program
    {
        static Menu _menu;
        private static MenuSlider _minClicks;
        private static MenuSlider _maxClicks;
        private static MenuBool _enabled;
        private static MenuBool _draw;
        static Random _random;
        private static Dictionary<string, int> _lastCommandT;
        private static bool _thisMovementCommandHasBeenTamperedWith = false;
        private static int _blockedCount = 0;

        static double GimmeNextRandomizedRandomizerToRektTrees(int min, int max)
        {
            var x = _random.Next(min, max);
            var y = _random.Next(min, max);
            if (_random.Next(0, 1) > 0)
            {
                return x;
            }
            if (1 == 1)
            {
                return (x + y)/2d;
            }
            return y;
        }

        private static Vector3 Randomize(Vector3 p)
        {
            return new Vector2(p.X + _random.Next(-10, 10), p.Y + _random.Next(-10, 10)).ToVector3();
        }
        static void Main(string[] args)
        {
            _random = new Random(DateTime.Now.Millisecond);
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
            Events.OnLoad += (loadSender, loadArgs) =>
            {
                _menu = new Menu("AntiBan# Ultimate", "antibansharpmenu", true);
                _minClicks = _menu.Add(new MenuSlider("MinClicks", "Min clicks per second", _random.Next(6, 7), 1, 7));
                _maxClicks = _menu.Add(new MenuSlider("MaxClicks", "Max clicks per second", _random.Next(0, 1) > 0 ? (int)Math.Floor(GimmeNextRandomizedRandomizerToRektTrees(7,11)) : (int)Math.Ceiling(GimmeNextRandomizedRandomizerToRektTrees(7,11)), 7, 15));
                _draw = _menu.Add(
                    new MenuBool("ShowBlockedClicks", "Display blocked clicks!", false)); 
                _enabled = _menu.Add(new MenuBool("Enabled", "Enable?", true));
                _menu.Attach();
                Drawing.OnDraw += onDrawArgs =>
                {
                    if (_draw)
                    {
                        Drawing.DrawText(Drawing.Width - 190, 100, System.Drawing.Color.Lime, "Blocked " + _blockedCount + " clicks");
                    }
                };
            };
            Obj_AI_Base.OnIssueOrder += (sender, issueOrderEventArgs) =>
            {
                if (sender.IsMe && !issueOrderEventArgs.IsAttackMove)
                {
                    if (!_enabled) return;


                    var orderName = issueOrderEventArgs.Order.ToString();
                    var order = _lastCommandT.FirstOrDefault(e => e.Key == orderName);
                    if (Variables.TickCount - order.Value <
                        GimmeNextRandomizedRandomizerToRektTrees(
                            1000/_maxClicks,
                            1000/_minClicks))
                    {
                        _blockedCount += 1;
                        issueOrderEventArgs.Process = false;
                        return;
                    }
                    if (issueOrderEventArgs.Order == GameObjectOrder.MoveTo && !Render.OnScreen(Drawing.WorldToScreen(issueOrderEventArgs.TargetPosition)))
                    {
                        issueOrderEventArgs.Process = false;
                    }
                    if (issueOrderEventArgs.Order == GameObjectOrder.MoveTo &&
                        Extensions.IsValid(issueOrderEventArgs.TargetPosition) && !_thisMovementCommandHasBeenTamperedWith)
                    {
                        _thisMovementCommandHasBeenTamperedWith = true;
                        issueOrderEventArgs.Process = false;
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                            Randomize(issueOrderEventArgs.TargetPosition));
                    }
                    _thisMovementCommandHasBeenTamperedWith = false;
                    _lastCommandT.Remove(orderName);
                    _lastCommandT.Add(orderName, Variables.TickCount);
                }
            };
            Spellbook.OnCastSpell += (sender, eventArgs) =>
            {
                if (!_enabled) return;
                
                //get spelldata
                SpellDatabaseEntry sData = null;
                foreach (var spellDataEntry in SpellData.Entries)
                {
                    var spellDatabaseEntry = SpellDatabase.GetByName(spellDataEntry.Name);
                    if (spellDatabaseEntry != null && spellDatabaseEntry.Slot == eventArgs.Slot)
                    {
                        sData = spellDatabaseEntry;
                    }
                }

                //if getting spell data wasn't succesful, don't go any further
                if (sData == null) return;

                //if the spell is a position cast type, randomize the position
                if (sData != null && sData.CastType != null && sData.CastType.Any(castTypeFlag => castTypeFlag == CastType.Position))
                {
                    if (!Render.OnScreen(Drawing.WorldToScreen(eventArgs.StartPosition)))
                    {
                        eventArgs.Process = false;
                    }
                    //if the spell isn't registered isn't humanized (as known by our dict), humanize it
                    if (_lastCommandT.FirstOrDefault(e => e.Key == "spellcast" + eventArgs.Slot).Value == 0)
                    {
                        _lastCommandT.Remove("spellcast" + eventArgs.Slot);
                        _lastCommandT.Add("spellcast" + eventArgs.Slot, Variables.TickCount);
                        eventArgs.Process = false;
                        ObjectManager.Player.Spellbook.CastSpell(eventArgs.Slot,
                            Randomize(eventArgs.StartPosition));
                        return;
                    }
                    //the spell was humanized so cast it.
                    _lastCommandT.Remove("spellcast" + eventArgs.Slot);
                    _lastCommandT.Add("spellcast" + eventArgs.Slot, 0);
                }
            };
            Game.OnChat += gameChatEventArgs =>
            {
                if (gameChatEventArgs.Sender.IsMe && _enabled)
                {
                    if (Variables.TickCount - _lastCommandT.FirstOrDefault(e => e.Key == "lastchat").Value <
                        _random.Next(100, 200))
                    {
                        gameChatEventArgs.Process = false;
                    }
                    _lastCommandT.Remove("lastchat");
                    _lastCommandT.Add("lastchat", Variables.TickCount);
                }
            };
        }
    }
}
