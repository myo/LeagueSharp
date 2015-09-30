using LeagueSharp;
using LeagueSharp.Common;

namespace Humanizer
{
    public static class Program
    {
        public static Menu Config;
        public static int ProcessedMovementCommandTick;
        public static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                Config = new Menu("IAmAHumanSharp", "iamhumansharp", true);
                Config.AddItem(new MenuItem("delay", "Delay (in milliseconds):").SetValue(new Slider(100, 0, 125)));
            };
            Obj_AI_Base.OnIssueOrder += (sender, eventArgs) =>
            {
                if (sender.IsMe && eventArgs.Order == GameObjectOrder.MoveTo)
                {
                    if (Utils.TickCount - ProcessedMovementCommandTick < Config.Item("delay").GetValue<Slider>().Value)
                    {
                        eventArgs.Process = false;
                    }
                    else
                    {
                        ProcessedMovementCommandTick = Utils.TickCount;
                    }
                }
            };

        }
    }
}
