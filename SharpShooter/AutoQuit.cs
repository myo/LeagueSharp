using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter
{
    internal class AutoQuit
    {
        internal static void Load()
        {
            MenuProvider.MenuInstance.AddItem(new MenuItem("QuitTheGameAfterGameOver",
                "Auto Quit the game after game over")).SetValue(false);
            Game.OnEnd += Game_OnEnd;
        }

        private static void Game_OnEnd(GameEndEventArgs args)
        {
            if (MenuProvider.MenuInstance.Item("QuitTheGameAfterGameOver").GetValue<bool>())
            {
                Task.Run(
                    async () =>
                    {
                        await Task.Delay(5000);
                        Game.Quit();
                    });
            }
        }
    }
}