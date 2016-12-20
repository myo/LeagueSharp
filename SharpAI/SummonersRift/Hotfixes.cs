using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAI.SummonersRift.Data;
using SharpAI.Utility;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.Utils;
using NLog;
using SharpDX;
using Random = SharpAI.Utility.Random;

namespace SharpAI.SummonersRift
{
    // this stuff needs better fixing to it but it would take me more lines and eventually time to complete so atm #hotfixed
    public static class Hotfixes
    {
        private static int _lastMovementCommand = 0;
        public static bool AttackedByMinionsFlag = false;
        public static bool AttackedByTurretFlag = false;
        public static int _lastAfkCheckTime = 0;
        public static Vector3 _lastAfkCheckPosition;
        public static void Load()
        {
            Events.OnLoad += (obj, loadArgs) =>
            {
                Shop.Main.Init();
                Obj_AI_Base.OnIssueOrder += (sender, issueOrderArgs) =>
                {
                    if (SessionBasedData.Loaded && sender.IsMe)
                    {
                        if (issueOrderArgs.Order == GameObjectOrder.MoveTo)
                        {
                            //no walking to fountain.
                            if (issueOrderArgs.TargetPosition == Vector3.Zero || issueOrderArgs.TargetPosition.X == 0)
                            {
                                issueOrderArgs.Process = false;
                                return;
                            }
                            //no walking to cursor pos
                            if (issueOrderArgs.TargetPosition.Distance(Game.CursorPos) < 50)
                            {
                                issueOrderArgs.Process = false;
                                return;
                            }
                            //force stay in fountain until 95% hp
                            if (ObjectManager.Player.InFountain() && ObjectManager.Player.HealthPercent < 95)
                            {
                                issueOrderArgs.Process = false;
                                return;
                            }
                            //no walking into turrets if no minions under it
                            if (!ObjectManager.Player.IsUnderEnemyTurret() && issueOrderArgs.TargetPosition.IsDangerousPosition())
                            {
                                issueOrderArgs.Process = false;
                                return;
                            }
                            //humanizer shit
                            if (Environment.TickCount - _lastMovementCommand > Utility.Random.GetRandomInteger(300, 1100))
                            {
                                _lastMovementCommand = Environment.TickCount;
                                return;
                            }
                        }
                        if (issueOrderArgs.Target != null)
                        {
                            //no hitting heroes under enemy turrets
                            if (issueOrderArgs.Target is Obj_AI_Hero)
                            {
                                if (ObjectManager.Player.IsUnderEnemyTurret())
                                {
                                    issueOrderArgs.Process = false;
                                    return;
                                }
                            }
                            //no hitting jg camps
                            if (issueOrderArgs.Target is Obj_AI_Minion && (issueOrderArgs.Target as Obj_AI_Minion).Team == GameObjectTeam.Neutral)
                            {
                                Logging.Log("skipped hitting jg camp");
                                issueOrderArgs.Process = false;
                                return;
                            }
                        }
                    }
                };
                Spellbook.OnCastSpell += (sender, castSpellArgs) =>
                {
                    if (castSpellArgs.Slot == SpellSlot.Recall)
                    {
                        Variables.Orbwalker.ActiveMode = OrbwalkingMode.Combo;
                    }
                };
                Obj_AI_Base.OnProcessSpellCast += (sender, spellCastArgs) =>
                {
                    if (Variables.Orbwalker.ActiveMode != OrbwalkingMode.Combo && spellCastArgs.Target != null && spellCastArgs.Target.IsMe)
                    {
                        if (sender is Obj_AI_Minion)
                        {
                            AttackedByMinionsFlag = true;
                            DelayAction.Add(1350, () => AttackedByMinionsFlag = false);
                        }
                        if (sender is Obj_AI_Turret)
                        {
                            AttackedByTurretFlag = true;
                            DelayAction.Add(1500, () =>
                            {
                                if (!ObjectManager.Player.IsUnderEnemyTurret())
                                {
                                    AttackedByTurretFlag = false;
                                }
                                else
                                {
                                    DelayAction.Add(2250, () => AttackedByTurretFlag = false);
                                }
                            });
                        }
                    }
                };
                Game.OnUpdate += args =>
                {
                    if (ObjectManager.Player.IsRecalling() ||
                           (ObjectManager.Player.InFountain() && ObjectManager.Player.HealthPercent < 95))
                    {
                        Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                        return;
                    }
                    if (ObjectManager.Player.Position.IsDangerousPosition())
                    {
                        AttackedByMinionsFlag = true;
                        DelayAction.Add(350, () => AttackedByMinionsFlag = false);
                    }
                    
                };
                Variables.Orbwalker.OnAction += (sender, args) =>
                {
                    if (ObjectManager.Player.IsRecalling())
                    {
                        args.Process = false;
                        Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                    }
                    if (Environment.TickCount - _lastAfkCheckTime > 30000)
                    {
                        _lastAfkCheckTime = Environment.TickCount;
                        if (_lastAfkCheckPosition.Distance(ObjectManager.Player.ServerPosition) < 400)
                        {
                            var pos = new Utility.Geometry.Circle(ObjectManager.Player.Position.ToVector2(), 500).ToPolygon().GetRandomPointInPolygon();
                            pos.WalkToPoint(OrbwalkingMode.None, true);
                        }
                        _lastAfkCheckPosition = ObjectManager.Player.ServerPosition;
                    }
                };
            };
        }
    }
}
