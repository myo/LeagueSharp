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
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;

namespace Challenger_Series
{
    public abstract class CSPlugin
    {
        public MenuBool MyoModeOn;
        public Menu CrossAssemblySettings;
        public CSPlugin()
        {
            MainMenu = new Menu("challengerseries", ObjectManager.Player.ChampionName + " To The Challenger", true, ObjectManager.Player.ChampionName);
            CrossAssemblySettings = MainMenu.Add(new Menu("crossassemblysettings", "AIO Settings: "));
            MyoModeOn = CrossAssemblySettings.Add(new MenuBool("myomode", "Anti-TOXIC", false));
            LeagueSharp.SDK.Core.Utils.DelayAction.Add(15000, () => Orbwalker.Enabled = true);
            Game.OnChat += args => 
            {
                if (MyoModeOn)
                {
                    var msg = args.Message.ToLower();
                    if (msg.Contains("mid") || msg.Contains("top") || msg.Contains("bot") || msg.Contains("jungle") ||
                        msg.Contains("jg") || msg.Contains("supp") || msg.Contains("adc"))
                        args.Process = false;
                    foreach (var ally in GameObjects.AllyHeroes)
                    {
                        if (args.Sender.IsMe && args.Message.ToLower().Contains(ally.CharData.BaseSkinName.ToLower()))
                            args.Process = false;
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

        public List<Obj_AI_Hero> ValidTargets
        {
            get { return GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget() && !enemy.IsZombie).ToList(); }
        }

        public Orbwalker Orbwalker { get; } = Variables.Orbwalker;
        public TargetSelector TargetSelector { get; } = Variables.TargetSelector;
        public Menu MainMenu { get; set; }
        public virtual void OnUpdate(EventArgs args) { }
        public virtual void OnProcessSpellCast(GameObject sender, GameObjectProcessSpellCastEventArgs args) { }
        public virtual void OnDraw(EventArgs args) { }
        public virtual void InitializeMenu() { }
    }
}
