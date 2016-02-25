#region License
/* Copyright (c) LeagueSharp 2016
 * No reproduction is allowed in any way unless given written consent
 * from the LeagueSharp staff.
 * 
 * Author: imsosharp
 * Date: 2/24/2016
 * File: Kalista.cs
 */
#endregion License

using System;
using System.Collections.Generic;
using System.Linq;
using Challenger_Series.Utils;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Wrappers.Damages;
using SharpDX;
using Collision = LeagueSharp.SDK.Collision;
using Color = System.Drawing.Color;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace Challenger_Series.Plugins
{
    public class Kalista : CSPlugin
    {
        public Kalista()
        {
            base.Q = new Spell(SpellSlot.Q, 1150f);
            base.W = new Spell(SpellSlot.W, 5000);
            base.E = new Spell(SpellSlot.E, 1000f);
            base.R = new Spell(SpellSlot.R, 1400f);
            base.Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);
            InitMenu();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += HpBarDamageIndicator.Drawing_OnDraw;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnAction += OnOrbwalkerAction;
            Obj_AI_Base.OnProcessSpellCast += UltLogic_OnSpellcast;
            Game.OnUpdate += UltLogic_OnUpdate;
        }

        private void OnOrbwalkerAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
            if (orbwalkingActionArgs.Type == OrbwalkingType.AfterAttack)
            {
                Orbwalker.ForceTarget = null;
                if (Orbwalker.ActiveMode == OrbwalkingMode.Combo && orbwalkingActionArgs.Target is Obj_AI_Hero)
                {
                    var target = orbwalkingActionArgs.Target as Obj_AI_Hero;
                    if (ObjectManager.Player.ManaPercent > UseQManaSlider.Value)
                    {
                        if (UseQIfECanKillBool && IsPierceRendComboKillable(target))
                        {
                            var predictedPos = Q.GetPrediction(target).UnitPosition;
                            if (predictedPos.Distance(ObjectManager.Player.ServerPosition) < 1100)
                            {
                                Q.Cast(predictedPos);
                            }
                        }
                    }
                }
                if (UseEIfResettedByAMinionBool && ObjectManager.Player.ManaPercent > EResetByAMinionMinManaSlider.Value)
                {
                    if (
                        GameObjects.EnemyHeroes.Any(
                            e =>
                                e.Distance(ObjectManager.Player.ServerPosition) > 615 &&
                                GetRendBuff(e).Count >= MinEnemyStacksForEMinionResetSlider.Value) &&
                        GameObjects.EnemyMinions.Any(m => IsRendKillable(m)))
                    {
                        E.Cast();
                    }
                }
                if (orbwalkingActionArgs.Target is Obj_AI_Minion && GetJungleCampsOnCurrentMap() != null &&
                    GetJungleCampsOnCurrentMap().Contains(orbwalkingActionArgs.Target.Name))
                {
                    if (RendSmiteMenu[orbwalkingActionArgs.Target.Name].GetValue<MenuBool>())
                    {
                        var minion = orbwalkingActionArgs.Target as Obj_AI_Minion;
                        if (IsRendKillable(minion))
                        {
                            E.Cast();
                        }
                    }
                }
                if (UseQStackTransferBool && orbwalkingActionArgs.Target is Obj_AI_Minion)
                {
                    var target = orbwalkingActionArgs.Target as Obj_AI_Minion;
                    if (GetRendBuff(target).Count >= UseQStackTransferMinStacksSlider && IsRendKillable(target))
                    {
                        var pi = new PredictionInput();
                        pi.Delay = 0.25f;
                        pi.Radius = 40f;
                        pi.Range = 1100;
                        pi.Speed = 1400;
                        pi.From = target.Position;
                        pi.Type = SkillshotType.SkillshotLine;
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(en => en.Distance(ObjectManager.Player) > 1050 && en.Health > 1))
                        {
                            var prediction = Movement.GetPrediction(enemy, 0.25f, 40, 1400);
                            var posLists = new List<Vector3>();
                            posLists.Add(ObjectManager.Player.ServerPosition);
                            posLists.Add(prediction.UnitPosition);
                            var collision = Collision.GetCollision(posLists, pi);
                            if (collision.Where(colobject => colobject is Obj_AI_Minion).All(m => IsRendKillable(m)))
                            {
                                Q.Cast(prediction.UnitPosition);
                            }
                        }
                    }
                }
            }
            if (orbwalkingActionArgs.Type == OrbwalkingType.BeforeAttack)
            {
                if (Orbwalker.ActiveMode == OrbwalkingMode.Combo && FocusWBuffedEnemyBool)
                {
                    var wMarkedEnemy =
                        GameObjects.EnemyHeroes.FirstOrDefault(
                            h => h.Distance(ObjectManager.Player.ServerPosition) < 600 && h.HasBuff("kalistacoopstrikemarkally"));
                    if (wMarkedEnemy != null && wMarkedEnemy.IsValidTarget())
                    {
                        Orbwalker.ForceTarget = wMarkedEnemy;
                    }
                }
                if (Orbwalker.ActiveMode != OrbwalkingMode.Combo && FocusWBuffedEnemyInHarassBool)
                {
                    var wMarkedEnemy =
                        GameObjects.EnemyHeroes.FirstOrDefault(
                            h => h.Distance(ObjectManager.Player.ServerPosition) < 600 && h.HasBuff("kalistacoopstrikemarkally"));
                    if (wMarkedEnemy != null && wMarkedEnemy.IsValidTarget())
                    {
                        Orbwalker.ForceTarget = wMarkedEnemy;
                    }
                }
                if (Orbwalker.ActiveMode == OrbwalkingMode.LaneClear &&
                    orbwalkingActionArgs.Target.Type != GameObjectType.obj_AI_Hero)
                {
                    if (FocusWBuffedMinions)
                    {
                        Orbwalker.ForceTarget =
                            GameObjects.EnemyMinions.FirstOrDefault(
                                m =>
                                    m.Distance(ObjectManager.Player.ServerPosition) < 615 && m.HasBuff("kalistacoopstrikemarkally") &&
                                    m.Health < ObjectManager.Player.GetAutoAttackDamage(m) + W.GetDamage(m));
                    }
                }
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            base.OnUpdate(args);
            if (UseEBool)
            {
                if (GameObjects.EnemyHeroes.Any(IsRendKillable))
                {
                    E.Cast();
                }
            }
            if (AlwaysUseEIf2MinionsKillableBool && GameObjects.EnemyMinions.Count(IsRendKillable) > 1)
            {
                E.Cast();
            }
            if (Orbwalker.ActiveMode == OrbwalkingMode.Combo)
            {
                var target = TargetSelector.GetTarget(800);
                if (target == null) return;
                if (ObjectManager.Player.ManaPercent > UseQManaSlider.Value)
                {
                    if (target.Distance(ObjectManager.Player) > 585 && target.Distance(ObjectManager.Player) < 1100 &&
                        UseQCantAABool)
                    {
                        var predictedPos = Q.GetPrediction(target).UnitPosition;
                        Q.Cast(predictedPos);
                    }
                }
            }
        }

        public override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            if (DrawERangeBool)
            {
                Drawing.DrawCircle(
                    ObjectManager.Player.Position,
                    1000,
                    Color.LightGreen);
            }
            if (DrawRRangeBool)
            {
                Drawing.DrawCircle(
                    ObjectManager.Player.Position,
                    1400,
                    Color.DarkRed);
            }

            if (DrawEDamage)
            {
                HpBarDamageIndicator.DamageToUnit = GetRendDmg;
            }
            HpBarDamageIndicator.Enabled = DrawEDamage;
        }

        private Menu ComboMenu;
        private Menu WomboComboMenu;
        private MenuBool BalistaBool;
        private MenuBool TalistaBool;
        private MenuBool SalistaBool;
        //private MenuKeyBind UseQWalljumpKey;
        private MenuBool UseQCantAABool;
        private MenuBool UseQIfECanKillBool;
        private MenuSlider UseQManaSlider;
        private MenuBool FocusWBuffedEnemyBool;
        private MenuBool UseEBool;
        //private MenuBool UseEBeforeYouDieBool;
        private MenuBool UseRAllySaverBool;
        private MenuBool UseREngageBool;
        private MenuBool UseRCounterEngageBool;
        private MenuBool UseRInterruptBool;
        //private MenuBool OrbwalkOnMinionsBool;
        private Menu HarassMenu;
        private MenuBool UseQStackTransferBool;
        private MenuSlider UseQStackTransferMinStacksSlider;
        private MenuBool FocusWBuffedEnemyInHarassBool;
        private MenuBool UseEIfResettedByAMinionBool;
        private MenuSlider EResetByAMinionMinManaSlider;
        private MenuSlider MinEnemyStacksForEMinionResetSlider;
        private Menu FarmMenu;
        private MenuBool FocusWBuffedMinions;
        private MenuBool AlwaysUseEIf2MinionsKillableBool;
        private Menu RendSmiteMenu;
        private Menu RendDamageMenu;
        private MenuSlider ReduceRendDamageBySlider;
        private MenuSlider IncreaseRendDamageBySlider;
        private Menu DrawMenu;
        private MenuBool DrawERangeBool;
        private MenuBool DrawRRangeBool;
        private MenuBool DrawEDamage;

        private void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("kalicombomenu", "Combo Settings: "));
            WomboComboMenu = ComboMenu.Add(new Menu("kaliwombos", "Wombo Combos: "));
            BalistaBool = WomboComboMenu.Add(new MenuBool("kalibalista", "Balista", true));
            TalistaBool = WomboComboMenu.Add(new MenuBool("kalitalista", "Talista", true));
            SalistaBool = WomboComboMenu.Add(new MenuBool("kalisalista", "Salista", true));
            UseQCantAABool = ComboMenu.Add(new MenuBool("kaliuseqcombo", "Use Q if cant AA", true));
            UseQIfECanKillBool = ComboMenu.Add(new MenuBool("kaliuseqecombo", "Use Q > E combo", true));
            UseQManaSlider = ComboMenu.Add(new MenuSlider("kaliuseqmanaslider", "Use Q if Mana% > ", 20));
            //UseQWalljumpKey = ComboMenu.Add(new MenuKeyBind("useqwalljump", "Q Walljump Key", Keys.N, KeyBindType.Press));
            FocusWBuffedEnemyBool = ComboMenu.Add(new MenuBool("kalifocuswbuffedenemy", "Focus Enemy with W Buff", true));
            UseEBool = ComboMenu.Add(new MenuBool("kaliuseecombo", "Use E if can kill enemy", true));
            //UseEBeforeYouDieBool = ComboMenu.Add(new MenuBool("kaliuseebeforedeath", "Use E Before You Die", false));
            UseRAllySaverBool = ComboMenu.Add(new MenuBool("kaliusersaveally", "Use R to save Soulbound", true));
            UseREngageBool = ComboMenu.Add(new MenuBool("userengage", "Use R to engage", true));
            UseRCounterEngageBool = ComboMenu.Add(new MenuBool("kaliusercounternengage", "Use R counter-engage", true));
            UseRInterruptBool = ComboMenu.Add(new MenuBool("kaliuserinterrupt", "Use R to Interrupt"));
            //OrbwalkOnMinionsBool = ComboMenu.Add(new MenuBool("kaliorbonminions", "Orbwalk On Minions?", false));
            HarassMenu = MainMenu.Add(new Menu("kaliharassmenu", "Harass Settings: "));
            UseQStackTransferBool = HarassMenu.Add(new MenuBool("kaliuseqstacktransfer", "Use Q Stack Transfer"));
            UseQStackTransferMinStacksSlider =
                HarassMenu.Add(new MenuSlider("kaliuseqstacktransferminstacks", "Min stacks for Stack Transfer", 3, 0,
                    15));
            FocusWBuffedEnemyInHarassBool =
                HarassMenu.Add(new MenuBool("kalifocuswharass", "Focus W Buffed Enemy", true));
            UseEIfResettedByAMinionBool =
                HarassMenu.Add(new MenuBool("useeresetharass", "Use E if resetted by a minion"));
            EResetByAMinionMinManaSlider =
                HarassMenu.Add(new MenuSlider("useeresetmana", "Use E Reset by Minion if Mana% > ", 50));
            MinEnemyStacksForEMinionResetSlider =
                HarassMenu.Add(new MenuSlider("useeresetminenstacks", "Use E Reset if Enemy stacks > ", 3, 0, 25));
            FarmMenu = MainMenu.Add(new Menu("kalifarmmenu", "Farm Settings"));
            FocusWBuffedMinions = FarmMenu.Add(new MenuBool("focuswbufminions", "Focus minions with W buff", false));
            AlwaysUseEIf2MinionsKillableBool =
                FarmMenu.Add(new MenuBool("alwaysuseeif2minkillable", "Always use E if resetted with no mana cost", true));
            RendSmiteMenu = MainMenu.Add(new Menu("kalirendsmitemenu", "Rend (E) Smite: "));

            if (GetJungleCampsOnCurrentMap() != null)
            {
                foreach (var mob in GetJungleCampsOnCurrentMap())
                {
                    RendSmiteMenu.Add(new MenuBool(mob, mob, true));
                }
            }
            RendDamageMenu = MainMenu.Add(new Menu("kalirenddmgmenu", "Adjust Rend (E) DMG Prediction: "));
            ReduceRendDamageBySlider =
                RendDamageMenu.Add(new MenuSlider("kalirendreducedmg", "Reduce E DMG by: ", 0, 0, 300));
            IncreaseRendDamageBySlider =
                RendDamageMenu.Add(new MenuSlider("kalirendincreasedmg", "Increse E DMG by: ", 0, 0, 300));
            DrawMenu = MainMenu.Add(new Menu("kalidrawmenu", "Drawing Settings: "));
            DrawERangeBool = DrawMenu.Add(new MenuBool("drawerangekali", "Draw E Range", true));
            DrawRRangeBool = DrawMenu.Add(new MenuBool("kalidrawrrange", "Draw R Range", true));
            DrawEDamage = DrawMenu.Add(new MenuBool("kalidrawedmg", "Draw E Damage", true));
            MainMenu.Attach();
        }

        #region Champion Logic

        /// <summary>
        /// Those buffs make the target either unkillable or a pain in the ass to kill, just wait until they end
        /// </summary>
        private List<string> UndyingBuffs = new List<string>
        {
            "JudicatorIntervention",
            "UndyingRage",
            "FerociousHowl",
            "ChronoRevive",
            "ChronoShift",
            "lissandrarself",
            "kindredrnodeathbuff"
        };

        private List<string> GetJungleCampsOnCurrentMap()
        {
            switch ((int) Game.MapId)
            {
                //Summoner's Rift
                case 11:
                {
                    return SRMobs;
                }
                //Twisted Treeline
                case 10:
                {
                    return TTMobs;
                }
            }
            return null;
        }

        /// <summary>
        /// Summoner's Rift Jungle "Big" Mobs
        /// </summary>
        private List<string> SRMobs = new List<string>
        {
            "SRU_Baron12.1.1",
            "SRU_Blue1.1.1",
            "SRU_Blue7.1.1",
            "Sru_Crab15.1.1",
            "Sru_Crab16.1.1",
            "SRU_Dragon6.1.1",
            "SRU_Gromp13.1.1",
            "SRU_Gromp14.1.1",
            "SRU_Krug5.1.2",
            "SRU_Krug11.1.2",
            "SRU_Murkwolf2.1.1",
            "SRU_Murkwolf8.1.1",
            "SRU_Razorbeak3.1.1",
            "SRU_Razorbeak9.1.1",
            "SRU_Red4.1.1",
            "SRU_Red10.1.1"
        };

        /// <summary>
        /// Twisted Treeline Jungle "Big" Mobs
        /// </summary>
        private List<string> TTMobs = new List<string>
        {
            "TT_NWraith1.1.1",
            "TT_NGolem2.1.1",
            "TT_NWolf3.1.1",
            "TT_NWraith4.1.1",
            "TT_NGolem5.1.1",
            "TT_NWolf6.1.1",
            "TT_Spiderboss8.1.1"
        };

        //#TODO: Check E Damage every patch
        public bool IsPierceRendComboKillable(Obj_AI_Base target)
        {
            //If target doesn't have rend buff or is too far away, rend will most likely not kill him.
            if (E.Level == 0 || !target.HasBuff("kalistaexpungemarker") || target.Distance(ObjectManager.Player.ServerPosition) > 985)
                return false;

            //take shields into account
            var actualTargetHealth = target.Health + target.AllShield;

            //check if target isn't dead
            if (actualTargetHealth > 1)
            {
                //if the target is a champion check for spellshields
                if (target is Obj_AI_Hero)
                {
                    var objaihero_target = target as Obj_AI_Hero;
                    if (objaihero_target.Buffs.Any(buff => UndyingBuffs.Contains(buff.Name)) ||
                        objaihero_target.HasBuffOfType(BuffType.SpellShield))
                    {
                        return false;
                    }
                }
                //get the rend damage
                var dmg = E.GetDamage(target, Damage.DamageStage.Default) + E.GetDamage(target, Damage.DamageStage.Buff) +
                          Q.GetDamage(target) + E.GetDamage(target)/2;
                //exhaust reduces target damage by 40%
                if (ObjectManager.Player.HasBuff("SummonerExhaustSlow") || ObjectManager.Player.HasBuff("summonerexhaust"))
                {
                    dmg *= 0.6f;
                }
                //the barontarget buff reduces the damage to baron by 50%
                if (target.Name.Contains("Baron") && ObjectManager.Player.HasBuff("barontarget"))
                {
                    dmg *= 0.5f;
                }
                //you deal -7% dmg to dragon for each killed dragon
                if (target.Name.Contains("Dragon") && ObjectManager.Player.HasBuff("s5test_dragonslayerbuff"))
                {
                    dmg *= (1f - (0.07f*ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff")));
                }
                //check if damage > target hp + all shields affecting target
                return dmg > actualTargetHealth;
            }
            return false;
        }

        /// <summary>
        /// Checks if the target is killable by Rend(E)
        /// </summary>
        public bool IsRendKillable(Obj_AI_Base target)
        {
            //take shields into account
            var actualTargetHealth = target.Health + target.AllShield;

            //check if target isn't dead
            if (actualTargetHealth > 1)
            {
                //if the target is a champion check for spellshields
                if (target is Obj_AI_Hero)
                {
                    var objaihero_target = target as Obj_AI_Hero;
                    if (objaihero_target.Buffs.Any(buff => UndyingBuffs.Contains(buff.Name)) ||
                        objaihero_target.HasBuffOfType(BuffType.SpellShield))
                    {
                        return false;
                    }
                }
                var dmg = GetRendDmg(target);
                //check if damage > target hp + all shields affecting target
                dmg -= ReduceRendDamageBySlider.Value;
                dmg += IncreaseRendDamageBySlider.Value;
                return dmg > actualTargetHealth;
            }
            return false;
        }

        public float GetRendDmg(Obj_AI_Base target)
        {
            if (E.Level == 0 || !target.HasBuff("kalistaexpungemarker") || target.Distance(ObjectManager.Player.ServerPosition) > 985) return 0;
            //get the rend damage
            var dmg = E.GetDamage(target) + E.GetDamage(target, Damage.DamageStage.Buff);
            //exhaust reduces target damage by 40%
            if (ObjectManager.Player.HasBuff("SummonerExhaustSlow"))
            {
                dmg *= 0.6f;
            }
            //the barontarget buff reduces the damage to baron by 50%
            if (target.Name.Contains("Baron") && ObjectManager.Player.HasBuff("barontarget"))
            {
                dmg *= 0.5f;
            }
            //you deal -7% dmg to dragon for each killed dragon
            if (target.Name.Contains("Dragon") && ObjectManager.Player.HasBuff("s5test_dragonslayerbuff"))
            {
                dmg *= (1f - (0.1f*ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff")));
            }
            return dmg;
        }

        public static BuffInstance GetRendBuff(Obj_AI_Base target)
        {
            return target.Buffs.Find(b => b.Caster.IsMe && b.DisplayName.ToLower() == "kalistaexpungemarker");
        }

        #region Ult Logic

        private static Obj_AI_Hero SoulboundAlly;
        private static Dictionary<float, float> IncomingDamageToSoulboundAlly = new Dictionary<float, float>();
        private static Dictionary<float, float> InstantDamageOnSoulboundAlly = new Dictionary<float, float>();

        public static float AllIncomingDamageToSoulbound
        {
            get
            {
                return IncomingDamageToSoulboundAlly.Sum(e => e.Value) + InstantDamageOnSoulboundAlly.Sum(e => e.Value);
            }
        }

        public void UltLogic_OnSpellcast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (R.IsReady())
                {
                    if (SoulboundAlly != null)
                    {
                        var sdata = SpellDatabase.GetByName(args.SData.Name);
                        if (UseRCounterEngageBool && sdata != null &&
                            (args.End.Distance(ObjectManager.Player.ServerPosition) < 550 || args.Target.IsMe) &&
                            sdata.SpellTags != null &&
                            sdata.SpellTags.Any(st => st == SpellTags.Dash || st == SpellTags.Blink))
                        {
                            R.Cast();
                        }
                        if (UseRInterruptBool && sdata != null && sdata.SpellTags != null &&
                            sdata.SpellTags.Any(st => st == SpellTags.Interruptable) && sender.Distance(ObjectManager.Player.ServerPosition) < sdata.Range)
                        {
                            R.Cast();
                        }
                        if (UseRAllySaverBool)
                        {
                            if (args.Target != null &&
                                args.Target.NetworkId == SoulboundAlly.NetworkId)
                            {
                                if (args.SData.ConsideredAsAutoAttack)
                                {
                                    IncomingDamageToSoulboundAlly.Add(
                                        SoulboundAlly.ServerPosition.Distance(sender.ServerPosition)/
                                        args.SData.MissileSpeed +
                                        Game.Time, (float) sender.GetAutoAttackDamage(SoulboundAlly));
                                    return;
                                }
                                if (sender is Obj_AI_Hero)
                                {
                                    var attacker = (Obj_AI_Hero) sender;
                                    var slot = attacker.GetSpellSlot(args.SData.Name);

                                    if (slot != SpellSlot.Unknown)
                                    {
                                        var igniteSlot = attacker.GetSpellSlot("SummonerDot");
                                        if (slot == igniteSlot && args.Target != null &&
                                            args.Target.NetworkId == SoulboundAlly.NetworkId)
                                        {
                                            InstantDamageOnSoulboundAlly.Add(Game.Time + 2,
                                                (float)
                                                    attacker.GetSpellDamage(SoulboundAlly,
                                                        attacker.GetSpellSlot("SummonerDot")));
                                            return;
                                        }
                                        if (slot.HasFlag(SpellSlot.Q | SpellSlot.W | SpellSlot.E | SpellSlot.R))
                                        {
                                            InstantDamageOnSoulboundAlly.Add(Game.Time + 2,
                                                (float) attacker.GetSpellDamage(SoulboundAlly, slot));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UltLogic_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsRecalling() || ObjectManager.Player.InFountain())
                return;

            if (SoulboundAlly == null)
            {
                SoulboundAlly = GameObjects.AllyHeroes.FirstOrDefault(a => a.HasBuff("kalistacoopstrikeally"));
                return;
            }
            if (UseRAllySaverBool && AllIncomingDamageToSoulbound > SoulboundAlly.Health &&
                SoulboundAlly.CountEnemyHeroesInRange(800) > 0)
            {
                R.Cast();
            }
            if ((SoulboundAlly.ChampionName == "Blitzcrank" || SoulboundAlly.ChampionName == "Skarner" ||
                 SoulboundAlly.ChampionName == "TahmKench"))
            {
                foreach (
                    var unit in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                h =>
                                    h.IsEnemy && h.IsHPBarRendered && h.Distance(ObjectManager.Player.ServerPosition) > 700 &&
                                    h.Distance(ObjectManager.Player.ServerPosition) < 1400)
                    )
                {
                    if ((unit.HasBuff("rocketgrab2") && BalistaBool) ||
                        (unit.HasBuff("tahmkenchwdevoured") && TalistaBool) ||
                        (unit.HasBuff("skarnerimpale") && SalistaBool))
                    {
                        R.Cast();
                    }
                }
            }
            if (UseREngageBool)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(en => en.Distance(ObjectManager.Player.ServerPosition) < 1000 && en.IsFacing(ObjectManager.Player)))
                {
                    var waypoints = enemy.GetWaypoints();
                    if (waypoints.LastOrDefault().Distance(ObjectManager.Player.ServerPosition) < 400)
                    {
                        R.Cast();
                    }
                }
            }
        }

        #endregion Ult Logic

        #endregion Champion Logic
    }
}