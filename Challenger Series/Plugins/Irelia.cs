using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Utils;

namespace Challenger_Series
{
    public class Irelia : CSPlugin
    {
        public Irelia()
        {
            base.Q = new Spell(SpellSlot.Q, 650);
            base.W = new Spell(SpellSlot.W);
            base.E = new Spell(SpellSlot.E, 425);
            base.R = new Spell(SpellSlot.R, 1000);
            base.R.SetSkillshot(100, 50, 1600, false, SkillshotType.SkillshotLine);

            InitMenu();
            Game.OnUpdate += OnUpdate;
            Orbwalker.OnAction += OnOrbwalkerAction;
        }

        private void OnOrbwalkerAction(object sender, OrbwalkingActionArgs args)
        {
            if (args.Type == OrbwalkingType.BeforeAttack)
            {
                if (args.Target is Obj_AI_Hero && UseWComboBool)
                {
                    W.Cast();
                }
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            base.OnUpdate(args);
            var target = Variables.TargetSelector.GetTarget(1000, DamageType.Physical);
            if (target != null)
            {
                if (ObjectManager.Player.HasBuff("ireliatranscendentbladesspell") && UseRComboBool)
                {
                    R.Cast(R.GetPrediction(target).UnitPosition);
                }
            }
            if (Orbwalker.ActiveMode == OrbwalkingMode.Combo)
            {
                if (Q.IsReady())
                {
                    var killableEnemy =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .FirstOrDefault(
                                hero =>
                                    hero.IsEnemy && !hero.IsDead && hero.Health < Q.GetDamage(hero) &&
                                    hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 650);
                    if (killableEnemy != null && killableEnemy.IsValidTarget())
                    {
                        Q.Cast(killableEnemy);
                    }

                    var qMode = UseQComboStringList.SelectedValue;
                    if (qMode == "CHALLENGER")
                    {
                        var distBetweenMeAndTarget =
                            ObjectManager.Player.ServerPosition.Distance(target.ServerPosition);
                        if (distBetweenMeAndTarget > MinDistForQGapcloser)
                        {
                            if (distBetweenMeAndTarget < 650)
                            {
                                Q.Cast(target);
                            }
                            else
                            {
                                var minionGapclosingMode = QGapcloseModeStringList.SelectedValue;
                                if (minionGapclosingMode == "ONLY-CLOSEST-TO-TARGET")
                                {
                                    var gapclosingMinion =
                                        ObjectManager.Get<Obj_AI_Minion>()
                                            .Where(
                                                m =>
                                                    m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <
                                                    650 &&
                                                    m.IsEnemy &&
                                                    m.ServerPosition.Distance(target.ServerPosition) <
                                                    distBetweenMeAndTarget && m.Health > 1 &&
                                                    m.Health < Q.GetDamage(m))
                                            .OrderBy(m => m.Position.Distance(target.ServerPosition))
                                            .FirstOrDefault();
                                    if (gapclosingMinion != null)
                                    {
                                        Q.Cast(gapclosingMinion);
                                    }
                                }
                                else
                                {
                                    var firstGapclosingMinion =
                                        ObjectManager.Get<Obj_AI_Minion>()
                                            .Where(
                                                m =>
                                                    m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <
                                                    650 && m.IsEnemy &&
                                                    m.ServerPosition.Distance(target.ServerPosition) <
                                                    distBetweenMeAndTarget &&
                                                    m.Health > 1 && m.Health < Q.GetDamage(m))
                                            .OrderByDescending(m => m.Position.Distance(target.ServerPosition))
                                            .FirstOrDefault();
                                    if (firstGapclosingMinion != null)
                                    {
                                        Q.Cast(firstGapclosingMinion);
                                    }
                                }
                            }
                        }
                    }
                    if (qMode == "BRONZE")
                    {
                        var distBetweenMeAndTarget =
                            ObjectManager.Player.ServerPosition.Distance(target.ServerPosition);
                        if (distBetweenMeAndTarget < 650)
                        {
                            Q.Cast(target);
                        }
                        else
                        {
                            var firstGapclosingMinion =
                                        ObjectManager.Get<Obj_AI_Minion>()
                                            .Where(
                                                m =>
                                                    m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <
                                                    650 && m.IsEnemy &&
                                                    m.ServerPosition.Distance(target.ServerPosition) <
                                                    distBetweenMeAndTarget &&
                                                    m.Health > 1 && m.Health < Q.GetDamage(m))
                                            .OrderByDescending(m => m.Position.Distance(target.ServerPosition))
                                            .FirstOrDefault();
                            if (firstGapclosingMinion != null)
                            {
                                Q.Cast(firstGapclosingMinion);
                            }
                        }
                    }
                }
                if (E.IsReady())
                {
                    var killableEnemy =
                           ObjectManager.Get<Obj_AI_Hero>()
                               .FirstOrDefault(
                                   hero =>
                                       hero.IsEnemy && !hero.IsDead && hero.Health < E.GetDamage(hero) &&
                                       hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 425 && hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) > ObjectManager.Player.GetRealAutoAttackRange());
                    if (!Q.IsReady() && UseEKSBool)
                    {
                        E.Cast(killableEnemy);
                    }

                    var eMode = UseEComboStringList.SelectedValue;
                    if (eMode == "CHALLENGER")
                    {
                        if (ObjectManager.Player.HealthPercent <= target.HealthPercent)
                        {
                            E.Cast(target);
                        }
                        if (target.HealthPercent < ObjectManager.Player.HealthPercent &&
                            target.MoveSpeed > ObjectManager.Player.MoveSpeed &&
                            ObjectManager.Player.ServerPosition.Distance(target.ServerPosition) > 300)
                        {
                            E.Cast(target);
                        }
                    }
                    if (eMode == "BRONZE")
                    {
                        E.Cast(target);
                    }
                }
            }
            if (Orbwalker.ActiveMode == OrbwalkingMode.LaneClear)
            {
                var farmMode = QFarmModeStringList.SelectedValue;
                switch (farmMode)
                {
                    case "ONLY-UNKILLABLE":
                    {
                        var unkillableMinion =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .FirstOrDefault(
                                    m =>
                                        m.IsEnemy && m.Position.Distance(ObjectManager.Player.ServerPosition) < 650 &&
                                        m.Position.Distance(ObjectManager.Player.Position) >
                                        ObjectManager.Player.AttackRange && m.Health > 1 &&
                                        m.Health < 25);
                        if (unkillableMinion != null)
                        {
                            Q.Cast(unkillableMinion);
                        }
                        break;
                    }
                    case "ALWAYS":
                    {
                        var killableMinion =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .FirstOrDefault(
                                    m =>
                                        m.IsEnemy && m.Position.Distance(ObjectManager.Player.ServerPosition) < 650 &&
                                        m.Health > 1 && m.Health < Q.GetDamage(m));
                        if (killableMinion != null)
                        {
                            Q.Cast(killableMinion);
                        }
                        break;
                    }
                    case "NEVER":
                    {
                        break;
                    }
                }
            }

        }

        private MenuList<string> UseQComboStringList;
        private MenuBool UseWComboBool;
        private MenuList<string> UseEComboStringList;
        private MenuBool UseEKSBool;
        private MenuBool UseRComboBool;
        private MenuList<string> QGapcloseModeStringList;
        private MenuSlider MinDistForQGapcloser;
        private MenuList<string> QFarmModeStringList;
        private void InitMenu()
        {
            UseQComboStringList = MainMenu.Add(new MenuList<string>("useqcombo", "Q Combo MODE: ", new [] {"CHALLENGER", "BRONZE", "NEVER"}));
            UseWComboBool = MainMenu.Add(new MenuBool("usewcombo", "Use W Combo", true));
            UseEComboStringList = MainMenu.Add(new MenuList<string>("useecombo", "Use E Combo", new [] {"CHALLENGER", "BRONZE", "NEVER"}));
            UseEKSBool = MainMenu.Add(new MenuBool("useeks", "Use E KS if Q on CD", true));
            UseRComboBool = MainMenu.Add(new MenuBool("usercombo", "Use R Combo", true));
            QGapcloseModeStringList =
                MainMenu.Add(new MenuList<string>("qgc", "Q Gapcloser Mode",
                    new[] {"ONLY-CLOSEST-TO-TARGET", "ALL-KILLABLE-MINIONS"}));
            MinDistForQGapcloser =
                MainMenu.Add(new MenuSlider("mindistqgapcloser", "Min Distance for Q Gapclose", 350, 325, 625));
            QFarmModeStringList = MainMenu.Add(new MenuList<string>("useqfarm", "Q Farm Mode: ", new[] { "ONLY-UNKILLABLE", "ALWAYS", "NEVER" }));
            MainMenu.Attach();
        }
    }
}
