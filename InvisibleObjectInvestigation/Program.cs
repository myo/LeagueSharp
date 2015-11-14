//This code is copyright (c) LeagueSharp 2015. Please do not remove this line.

using System;
using System.Diagnostics;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;

namespace InvisibleObjectAutisticInvestigation
{
    public static class Program
    {
        public static string LogFile;
        public static string LogFile2;
        public static string LogFile3;
        public static string LogFile4;
        public static Stopwatch Stopwatch;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                //Define the logfile location
                LogFile = Config.AppDataDirectory + "\\Investigation Logs\\" + DateTime.Now.ToString("yy-MM-dd") + " " + DateTime.Now.ToString("HH-mm-ss") + " - All Objects.txt";
                LogFile2 = Config.AppDataDirectory + "\\Investigation Logs\\" + DateTime.Now.ToString("yy-MM-dd") + " " + DateTime.Now.ToString("HH-mm-ss") + " - Objects Close To Me.txt";
                LogFile3 = Config.AppDataDirectory + "\\Investigation Logs\\" + DateTime.Now.ToString("yy-MM-dd") + " " + DateTime.Now.ToString("HH-mm-ss") + " - Only Missiles.txt";
                LogFile4 = Config.AppDataDirectory + "\\Investigation Logs\\" + DateTime.Now.ToString("yy-MM-dd") + " " + DateTime.Now.ToString("HH-mm-ss") + " - Fake Heroes.txt";

                //Create a stopwatch which we will use to emulate in-game time.
                Stopwatch = new Stopwatch();
                Stopwatch.Start();

                //Create the AppData Directory, if it doesn't exist.
                if (!Directory.Exists(Config.AppDataDirectory + "\\Investigation Logs\\"))
                {
                    Directory.CreateDirectory(Config.AppDataDirectory + "\\Investigation Logs\\");
                }

                //Show the user a message
                Game.PrintChat("The investigation log for this game can be found at " + LogFile);

                //Subscribe to OnCreate to do the magic
                GameObject.OnCreate += OnCreate;
            };
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (!File.Exists(LogFile))
            {
                File.Create(LogFile);
            }
            if (!File.Exists(LogFile2))
            {
                File.Create(LogFile2);
            }
            if (!File.Exists(LogFile3))
            {
                File.Create(LogFile3);
            }
            if (!File.Exists(LogFile4))
            {
                File.Create(LogFile4);
            }

            int dist =
                (int)sender.Position.Distance(ObjectManager.Player.ServerPosition);

            using (var sw = new StreamWriter(LogFile, true))
            {
                //store the current stopwatch millisecond for accurate results
                long elapsedTime = Stopwatch.ElapsedMilliseconds;
                //compute elapsed minutes
                long elapsedMinutes = elapsedTime / 60000;
                //create a variable to store the seconds in
                long elapsedSeconds = 0;
                //compute the elapsed seconds and store it in the variable previously created
                Math.DivRem(elapsedTime, 60000, out elapsedSeconds);
                elapsedSeconds /= 1000;

                //write everything to the stream
                sw.WriteLine("[" + elapsedMinutes + ":" + elapsedSeconds + "] " + sender.Name +
                             " has been created at a distance of " + dist + " units away from me");
                //close the stream
                sw.Close();
            }
            if (dist < 1000)
            {
                using (var sw = new StreamWriter(LogFile2, true))
                {
                    //store the current stopwatch millisecond for accurate results
                    long elapsedTime = Stopwatch.ElapsedMilliseconds;
                    //compute elapsed minutes
                    long elapsedMinutes = elapsedTime / 60000;
                    //create a variable to store the seconds in
                    long elapsedSeconds = 0;
                    //compute the elapsed seconds and store it in the variable previously created
                    Math.DivRem(elapsedTime, 60000, out elapsedSeconds);
                    elapsedSeconds /= 1000;

                    //write everything to the stream
                    sw.WriteLine("[" + elapsedMinutes + ":" + elapsedSeconds + "] " + sender.Name +
                                 " has been created at a distance of " + dist + " units away from me");
                    //close the stream
                    sw.Close();
                }
            } 
            if (sender is Obj_SpellMissile)
            {
                using (var sw = new StreamWriter(LogFile3, true))
                {
                    //store the current stopwatch millisecond for accurate results
                    long elapsedTime = Stopwatch.ElapsedMilliseconds;
                    //compute elapsed minutes
                    long elapsedMinutes = elapsedTime / 60000;
                    //create a variable to store the seconds in
                    long elapsedSeconds = 0;
                    //compute the elapsed seconds and store it in the variable previously created
                    Math.DivRem(elapsedTime, 60000, out elapsedSeconds);
                    elapsedSeconds /= 1000;

                    //write everything to the stream
                    sw.WriteLine("[" + elapsedMinutes + ":" + elapsedSeconds + "] " + sender.Name +
                                 " has been created at a distance of " + dist + " units away from me");
                    //close the stream
                    sw.Close();
                }
            }
            if (sender is Obj_AI_Base)
            {
                using (var sw = new StreamWriter(LogFile4, true))
                {
                    //store the current stopwatch millisecond for accurate results
                    long elapsedTime = Stopwatch.ElapsedMilliseconds;
                    //compute elapsed minutes
                    long elapsedMinutes = elapsedTime / 60000;
                    //create a variable to store the seconds in
                    long elapsedSeconds = 0;
                    //compute the elapsed seconds and store it in the variable previously created
                    Math.DivRem(elapsedTime, 60000, out elapsedSeconds);
                    elapsedSeconds /= 1000;

                    //write everything to the stream
                    sw.WriteLine("[" + elapsedMinutes + ":" + elapsedSeconds + "] " + sender.Name +
                                 " has been created at a distance of " + dist + " units away from me");
                    //close the stream
                    sw.Close();
                }
            }
        }
    }
}
