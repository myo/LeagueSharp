#region License
/* Copyright (c) LeagueSharp 2016
 * No reproduction is allowed in any way unless given written consent
 * from the LeagueSharp staff.
 * 
 * Author: imsosharp
 * Date: 2/20/2016
 * File: CSPlugin.cs
 */
#endregion License

using System;
using System.Collections.Generic;
using System.Linq;
using Challenger_Series.Utils;
using LeagueSharp;
using LeagueSharp.SDK;
using SharpDX;
using Color = System.Drawing.Color;
using Challenger_Series.Utils;
using System.Windows.Forms;
using LeagueSharp.Data.Enumerations;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using Menu = LeagueSharp.SDK.UI.Menu;

namespace Challenger_Series
{

    using Challenger_Series.Properties;

    public abstract class CSPlugin
    {
        public MenuBool DrawEnemyWaypoints;
        public Menu CrossAssemblySettings;
        public MenuBool PlayUrfThemeNextGame;
        public CSPlugin()
        {
            MainMenu = new Menu("challengerseries", ObjectManager.Player.ChampionName + " To The Challenger", true, ObjectManager.Player.ChampionName);
            CrossAssemblySettings = MainMenu.Add(new Menu("crossassemblysettings", "Challenger Utils: "));
            DrawEnemyWaypoints =
                CrossAssemblySettings.Add(new MenuBool("drawenemywaypoints", "Draw Enemy Waypoints", true));
            PlayUrfThemeNextGame = CrossAssemblySettings.Add(new MenuBool("playsoundatstart", "Play URF Theme Next Game", true));
            DelayAction.Add(15000, () => Orbwalker.Enabled = true);
            Drawing.OnDraw += args =>
            {
                if (DrawEnemyWaypoints)
                {
                    foreach (
                        var e in
                            ValidTargets.Where(
                                en => en.Distance(ObjectManager.Player) < 5000))
                    {
                        var ip = Drawing.WorldToScreen(e.Position); //start pos

                        var wp = e.GetWaypoints();
                        var c = wp.Count - 1;
                        if (wp.Count() <= 1) break;

                        var w = Drawing.WorldToScreen(wp[c].ToVector3()); //endpos

                        Drawing.DrawLine(ip.X, ip.Y, w.X, w.Y, 2, Color.Red);
                        Drawing.DrawText(w.X, w.Y, Color.Red, e.CharData.BaseSkinName);
                    }
                }
            };
        }

        #region Spells
        public Spell Q { get; set; }
        public Spell Q2 { get; set; }
        public Spell W { get; set; }
        public Spell W2 { get; set; }
        public Spell E { get; set; }
        public Spell E2 { get; set; }
        public Spell R { get; set; }
        public Spell R2 { get; set; }
        #endregion Spells

        public IEnumerable<Obj_AI_Hero> ValidTargets { get {return GameObjects.EnemyHeroes.Where(enemy=>enemy.IsHPBarRendered);}}

        public Orbwalker Orbwalker { get; } = Variables.Orbwalker;
        public TargetSelector TargetSelector { get; } = Variables.TargetSelector;
        public Menu MainMenu { get; set; }
        public virtual void OnUpdate(EventArgs args) { }
        public virtual void OnProcessSpellCast(GameObject sender, GameObjectProcessSpellCastEventArgs args) { }
        public virtual void OnDraw(EventArgs args) { }
        public virtual void InitializeMenu() { }
    }
}
