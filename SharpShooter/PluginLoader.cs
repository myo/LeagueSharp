using System;
using LeagueSharp;

namespace SharpShooter
{
    internal class PluginLoader
    {
        internal static bool LoadPlugin(string pluginName)
        {
            if (CanLoadPlugin(pluginName))
            {
                DynamicInitializer.NewInstance(Type.GetType("SharpShooter.Plugins." + ObjectManager.Player.ChampionName));
                return true;
            }

            return false;
        }

        internal static bool CanLoadPlugin(string pluginName)
        {
            return Type.GetType("SharpShooter.Plugins." + ObjectManager.Player.ChampionName) != null;
        }
    }
}