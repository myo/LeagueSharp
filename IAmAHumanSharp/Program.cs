using LeagueSharp;
using LeagueSharp.Common;

namespace Humanizer
{
    public static class Program
    {
        public static Menu Config;
        public static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                Config = new Menu("IAmAHumanSharp", "iamhumansharp", true);
                Config.AddItem(new MenuItem("delay", "Delay (in milliseconds):").SetValue(new Slider(50, 0, 100)));
            };
            Obj_AI_Base.OnIssueOrder += (sender, eventArgs) =>
            {
                if (sender.IsMe)
                {
                    if (eventArgs.Order == GameObjectOrder.AttackUnit)
                    {
                        Utility.DelayAction.Add(Config.Item("delay").GetValue<Slider>().Value,
                            () => ObjectManager.Player.IssueOrder(eventArgs.Order, eventArgs.Target));
                    }
                    if (eventArgs.Order == GameObjectOrder.MoveTo)
                    {
                        Utility.DelayAction.Add(Config.Item("delay").GetValue<Slider>().Value,
                            () => ObjectManager.Player.IssueOrder(eventArgs.Order, eventArgs.TargetPosition));
                    }
                }
            };

        }
    }
}
