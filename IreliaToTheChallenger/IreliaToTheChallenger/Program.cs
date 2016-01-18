using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace IreliaToTheChallenger
{
    public static class Program
    {
        public static Spell Q, W, E, R;
        public static Menu MainMenu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Load;
        }
        public static void Load(EventArgs args)
        {
            if (ObjectManager.Player.CharData.BaseSkinName != "Irelia") return;
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);
            R.SetSkillshot(100, 50, 1600, false, SkillshotType.SkillshotLine);

            MainMenu = new Menu("Irelia To The Challenger", "ittc", true);
            MainMenu.AddItem(new MenuItem("ittc.qfarm", "Q FARM Mode: ").SetValue(new StringList(new[] { "ONLY-UNKILLABLE", "ALWAYS", "NEVER" })));
            MainMenu.AddItem(new MenuItem("ittc.mindistanceforqgapclose", "MIN DISTANCE FOR Q GAPCLOSER").SetValue(new Slider(450, 325, 625)));
            MainMenu.AddItem(new MenuItem("ittc.qminiongapclose", "Q MINION GAPCLOSER Mode: ").SetValue(new StringList(new[] { "ONLY-CLOSEST-TO-TARGET", "ALL-KILLABLE-MINIONS" })));

            var orbwalkerMenu = MainMenu.AddSubMenu(new Menu("Orbwalker", "orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            MainMenu.AddToMainMenu();

            Game.OnUpdate += Mechanics;
            Orbwalking.BeforeAttack += UseW;
            Drawing.OnDraw += DrawR;
        }
        public static void DrawR(EventArgs args)
        {
            if (R.IsReady())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 1000 && hero.Health > 1))
                {
                    var enemyPositionToScreen = Drawing.WorldToScreen(enemy.Position);
                    var dmg = R.GetDamage(enemy);
                    Drawing.DrawText(enemyPositionToScreen.X - 20, enemyPositionToScreen.Y - 30, dmg > enemy.Health ? Color.Gold : Color.Red, "R DMG: " + Math.Round(dmg) + " (" + Math.Round(dmg / enemy.Health) + "%)");
                }
            }
        }

        public static void UseW(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target.IsValid<Obj_AI_Hero>())
            {
                W.Cast();
            }
        }
        public static void Mechanics(EventArgs args)
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
            if (target != null)
            {
                if (ObjectManager.Player.HasBuff("ireliatranscendentbladesspell"))
                {
                    R.Cast(R.GetPrediction(target).UnitPosition);
                }
                if (E.IsReady())
                {
                    if (ObjectManager.Player.HealthPercent <= target.HealthPercent)
                    {
                        E.Cast(target);
                    }
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    if (Q.IsReady())
                    {
                        var killableEnemy = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(hero => hero.IsEnemy && !hero.IsDead && hero.Health < Q.GetDamage(hero) && hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 650);
                        if (killableEnemy != null && killableEnemy.IsValidTarget())
                        {
                            Q.Cast(killableEnemy);
                        }
                        var distBetweenMeAndTarget = ObjectManager.Player.ServerPosition.Distance(target.ServerPosition);
                        if (distBetweenMeAndTarget > MainMenu.Item("ittc.mindistanceforqgapclose").GetValue<Slider>().Value)
                        {
                            if (distBetweenMeAndTarget < 650)
                            {
                                Q.Cast(target);
                            }
                            else
                            {
                                var minionGapclosingMode = MainMenu.Item("ittc.qminiongapclose").GetValue<StringList>().SelectedValue;
                                if (minionGapclosingMode == "ONLY-CLOSEST-TO-TARGET")
                                {
                                    var gapclosingMinion = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 650 &&
                                        m.IsEnemy && m.ServerPosition.Distance(target.ServerPosition) < distBetweenMeAndTarget && m.Health > 1 && m.Health < Q.GetDamage(m)).OrderBy(m => m.Position.Distance(target.ServerPosition)).FirstOrDefault();
                                    if (gapclosingMinion != null)
                                    {
                                        Q.Cast(gapclosingMinion);
                                    }
                                }
                                else
                                {
                                    var firstGapclosingMinion = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 650 &&
                                    m.Health > 1 && m.Health < Q.GetDamage(m)).OrderByDescending(m => m.Position.Distance(target.ServerPosition)).FirstOrDefault();
                                    if (firstGapclosingMinion != null)
                                    {
                                        Q.Cast(firstGapclosingMinion);
                                    }
                                }
                            }
                        }
                    }
                    if (target.HealthPercent < ObjectManager.Player.HealthPercent && target.MoveSpeed > ObjectManager.Player.MoveSpeed)
                    {
                        E.Cast(target);
                    }
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var farmMode = MainMenu.Item("ittc.qfarm").GetValue<StringList>().SelectedValue;
                switch (farmMode)
                {
                    case "ONLY-UNKILLABLE":
                        {
                            var unkillableMinion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(m => m.IsEnemy && m.Position.Distance(ObjectManager.Player.ServerPosition) < 650 && !Orbwalking.InAutoAttackRange(m) && m.Health > 1 && m.Health < Q.GetDamage(m));
                            if (unkillableMinion != null)
                            {
                                Q.Cast(unkillableMinion);
                            }
                            break;
                        }
                    case "ALWAYS":
                        {
                            var killableMinion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(m => m.IsEnemy && m.Position.Distance(ObjectManager.Player.ServerPosition) < 650 && m.Health > 1 && m.Health < Q.GetDamage(m));
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
    }
}
