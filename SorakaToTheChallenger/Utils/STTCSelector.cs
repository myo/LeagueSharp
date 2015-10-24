#region LICENSE

/*
 Copyright 2014 - 2014 LeagueSharp
 TargetSelector.cs is part of LeagueSharp.Common.
 
 LeagueSharp.Common is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 LeagueSharp.Common is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with LeagueSharp.Common. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace SorakaToTheChallenger.Utils
{
    public static class STTCSelector
    {

        #region Enum


        #endregion

        /// <summary>
        ///     Sets the priority of the hero
        /// </summary>
        public static void SetPriority(Obj_AI_Hero hero, int newPriority)
        {
            if (Program.PriorityMenu.Item("TargetSelector" + hero.ChampionName + "Priority") == null)
            {
                return;
            }
            var p = Program.PriorityMenu.Item("TargetSelector" + hero.ChampionName + "Priority").GetValue<Slider>();
            p.Value = Math.Max(1, Math.Min(5, newPriority));
            Program.PriorityMenu.Item("TargetSelector" + hero.ChampionName + "Priority").SetValue(p);
        }

        /// <summary>
        ///     Returns the priority of the hero
        /// </summary>
        public static float GetPriority(Obj_AI_Hero hero)
        {
            var p = 1;
            if (Program.PriorityMenu.Item("TargetSelector" + hero.ChampionName + "Priority") != null)
            {
                p = Program.PriorityMenu.Item("TargetSelector" + hero.ChampionName + "Priority").GetValue<Slider>().Value;
            }

            switch (p)
            {
                case 2:
                    return 1.5f;
                case 3:
                    return 1.75f;
                case 4:
                    return 2f;
                case 5:
                    return 2.5f;
                default:
                    return 1f;
            }
        }

        private static int GetPriorityFromDb(string championName)
        {
            string[] p1 =
            {
                "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
                "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
                "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona",
                "Soraka", "Taric", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra"
            };

            string[] p2 =
            {
                "Aatrox", "Darius", "Elise", "Evelynn", "Galio", "Gangplank", "Gragas", "Irelia", "Jax",
                "Lee Sin", "Maokai", "Morgana", "Nocturne", "Pantheon", "Poppy", "Rengar", "Rumble", "Ryze", "Swain",
                "Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao", "RekSai"
            };

            string[] p3 =
            {
                "Akali", "Diana", "Ekko", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce", "Kassadin",
                "Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir", "Yasuo",
                "Zilean"
            };

            string[] p4 =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
                "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
                "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
                "Zed", "Ziggs"
            };

            if (p1.Contains(championName))
            {
                return 1;
            }
            if (p2.Contains(championName))
            {
                return 2;
            }
            if (p3.Contains(championName))
            {
                return 3;
            }
            return p4.Contains(championName) ? 4 : 1;
        }


        internal static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += args =>
            {
                foreach (var ally in HeroManager.Allies.Where(a => !a.IsMe))
                {
                    Program.PriorityMenu.AddItem(
                        new MenuItem("TargetSelector" + ally.ChampionName + "Priority", ally.ChampionName)
                            .SetValue(
                                new Slider(GetPriorityFromDb(ally.ChampionName), 1, 5)));
                }
            };
        }

        private static void autoPriorityItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (!e.GetNewValue<bool>())
            {
                return;
            }
            foreach (var ally in HeroManager.Enemies)
            {
                Program.PriorityMenu.Item("TargetSelector" + ally.ChampionName + "Priority")
                    .SetValue(new Slider(GetPriorityFromDb(ally.ChampionName), 5, 1));
            }
        }
    }
}