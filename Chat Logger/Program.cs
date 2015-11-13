//This code is copyright (c) LeagueSharp 2015. Please do not remove this line.

using System;
using System.Diagnostics;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;

namespace Chat_Logger
{
    public static class Program
    {
        public static string LogFile;
        public static Stopwatch Stopwatch;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                //Define the logfile location
                LogFile = Config.AppDataDirectory + "\\Chat Logs\\" + DateTime.Now.ToString("yy-mm-dd") + " " + DateTime.Now.ToString("HH:mm:ss tt") + " - " + ObjectManager.Player.ChampionName + ".txt";
                
                //Create a stopwatch which we will use to emulate in-game time.
                Stopwatch = new Stopwatch();
                Stopwatch.Start();

                //Create the AppData Directory, if it doesn't exist.
                if (!Directory.Exists(Config.AppDataDirectory + "\\Chat Logs\\"))
                {
                    Directory.CreateDirectory(Config.AppDataDirectory + "\\Chat Logs\\");
                }

                //Show the user a message
                Game.PrintChat("The chat log for this game can be found at " + LogFile);

                //Subscribe to OnChat to do the magic
                Game.OnChat += OnChat;
            };
        }

        private static void OnChat(GameChatEventArgs args)
        {
            if (!File.Exists(LogFile))
            {
                File.Create(LogFile);
            }

            using (var sw = new StreamWriter(LogFile, true))
            {
                //store the current stopwatch millisecond for accurate results
                long elapsedTime = Stopwatch.ElapsedMilliseconds;
                //compute elapsed minutes
                long elapsedMinutes = elapsedTime/60000;
                //create a variable to store the seconds in
                long elapsedSeconds = 0;
                //compute the elapsed seconds and store it in the variable previously created
                Math.DivRem(elapsedTime, 60000, out elapsedSeconds);
                elapsedSeconds /= 1000;
                
                //write everything to the stream
                sw.WriteLine("[" + elapsedMinutes + ":" + elapsedSeconds + "] " + args.Sender.Name + " (" + args.Sender.ChampionName + "): " + args.Message);
                //close the stream
                sw.Close();
            }
        }
    }
}
