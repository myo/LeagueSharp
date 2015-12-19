using System;
using LeagueSharp;

namespace SharpShooter
{
    internal class Initializer
    {
        internal static void Initialize()
        {
            Console.WriteLine("SharpShooter: HelloWorld!");

            MenuProvider.Initialize();

            if (PluginLoader.LoadPlugin(ObjectManager.Player.ChampionName))
            {
                MenuProvider.Champion.Drawings.AddItem(" ");
                OrbwalkerTargetIndicator.Load();
                LasthitIndicator.Load();
                Activator.Load();
            }

            MenuProvider.SupportedChampions.AddItem("1. Ashe");
            MenuProvider.SupportedChampions.AddItem("2. Caitlyn");
            MenuProvider.SupportedChampions.AddItem("3. Corki");
            MenuProvider.SupportedChampions.AddItem("4. Ezreal");
            MenuProvider.SupportedChampions.AddItem("5. Graves");
            MenuProvider.SupportedChampions.AddItem("6. Jinx");
            MenuProvider.SupportedChampions.AddItem("7. Kalista");
            MenuProvider.SupportedChampions.AddItem("8. KogMaw");
            MenuProvider.SupportedChampions.AddItem("9. Lucian");
            MenuProvider.SupportedChampions.AddItem("10. MissFortune");
            MenuProvider.SupportedChampions.AddItem("11. Sivir");
            MenuProvider.SupportedChampions.AddItem("12. Tristana");
            MenuProvider.SupportedChampions.AddItem("13. Twitch");
            MenuProvider.SupportedChampions.AddItem("14. Varus");
            MenuProvider.SupportedChampions.AddItem("15. Vayne");
            MenuProvider.SupportedChampions.AddItem("16. Teemo");
            MenuProvider.SupportedChampions.AddItem("17. Ryze");
            MenuProvider.SupportedChampions.AddItem("18. Blitzcrank");
            MenuProvider.SupportedChampions.AddItem("19. Karthus");
            MenuProvider.SupportedChampions.AddItem("20. Kindred");
            MenuProvider.SupportedChampions.AddItem("21. Draven");
            MenuProvider.SupportedChampions.AddItem("22. TwistedFate");
            MenuProvider.SupportedChampions.AddItem("23. illaoi");
            MenuProvider.SupportedChampions.AddItem("24. Lulu");

            AutoQuit.Load();

            Console.WriteLine("SharpShooter: Initialized.");
        }
    }
}