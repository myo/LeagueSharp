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
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Wrappers.Damages;

namespace Challenger_Series
{
    public abstract class CSPlugin
    {
        public MenuSlider DecreaseDamageToMinionsBy;
        public MenuBool MyoModeOn;
        public MenuBool DrawEnemyWaypoints;
        public Menu CrossAssemblySettings;
        public CSPlugin()
        {
            MainMenu = new Menu("challengerseries", ObjectManager.Player.ChampionName + " To The Challenger", true, ObjectManager.Player.ChampionName);
            CrossAssemblySettings = MainMenu.Add(new Menu("crossassemblysettings", "Challenger Utils: "));
            MyoModeOn = CrossAssemblySettings.Add(new MenuBool("myomode", "Anti-TOXIC", false));
            DrawEnemyWaypoints =
                CrossAssemblySettings.Add(new MenuBool("drawenemywaypoints", "Draw Enemy Waypoints", true));
            DecreaseDamageToMinionsBy = CrossAssemblySettings.Add(new MenuSlider("decreasedamagetominionsby", "Decrease Damage To Minions By: ", 0, 0, 20));

            LeagueSharp.SDK.Core.Utils.DelayAction.Add(15000, () => Orbwalker.Enabled = true);
            Game.OnChat += args => 
            {
                if (MyoModeOn && args.Sender.IsMe)
                {
                    var msg = args.Message.ToLower();
                    if (msg.Contains("mid") || msg.Contains("top") || msg.Contains("bot") || msg.Contains("jungle") ||
                        msg.Contains("jg") || msg.Contains("supp") || msg.Contains("adc"))
                        args.Process = false;
                    foreach (var ally in GameObjects.AllyHeroes)
                    {
                        if (msg.Contains(ally.CharData.BaseSkinName.ToLower()))
                            args.Process = false;
                    }
                }
            };
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

            Orbwalker.OnAction+=(sender, orbwalkingArgs) =>
                {
                    if (orbwalkingArgs.Type == OrbwalkingType.BeforeAttack && orbwalkingArgs.Target is Obj_AI_Minion)
                    {
                        var target = orbwalkingArgs.Target as Obj_AI_Minion;
                        if (target.Health < 100 && target.Health > ObjectManager.Player.GetAutoAttackDamage(target) - DecreaseDamageToMinionsBy.Value)
                        {
                            orbwalkingArgs.Process = false;
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

        public IEnumerable<Obj_AI_Hero> ValidTargets { get {return GameObjects.EnemyHeroes.Where(enemy=>enemy.Health > 5 && enemy.IsVisible);}}

        public Orbwalker Orbwalker { get; } = Variables.Orbwalker;
        public TargetSelector TargetSelector { get; } = Variables.TargetSelector;
        public Menu MainMenu { get; set; }
        public virtual void OnUpdate(EventArgs args) { }
        public virtual void OnProcessSpellCast(GameObject sender, GameObjectProcessSpellCastEventArgs args) { }
        public virtual void OnDraw(EventArgs args) { }
        public virtual void InitializeMenu() { }
    }
}
