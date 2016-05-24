using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;

namespace SharpAI.Utility
{
    public static class Logging
    {
        private static string _lastMessage = "";

        public static void Log(string message)
        {
            if (message != _lastMessage)
            {
                _lastMessage = message;
                Game.PrintChat(Environment.TickCount + " " + message);
            }
        }
    }
}
