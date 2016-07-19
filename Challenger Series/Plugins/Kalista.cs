using System;
using System.Collections.Generic;
using System.Linq;
using Challenger_Series.Utils;
using LeagueSharp;
using LeagueSharp.SDK;
using SharpDX;
using Color = System.Drawing.Color;
using System.Windows.Forms;
using LeagueSharp.Data.Enumerations;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using Menu = LeagueSharp.SDK.UI.Menu;

namespace Challenger_Series.Plugins
{
    using Collision = LeagueSharp.SDK.Collision;

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
            DelayedOnUpdate += OnUpdate;
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
                if (Q.IsReady())
                {
                    this.QLogic(orbwalkingActionArgs.Target);
                    if (UseQStackTransferBool)
                    {
                        this.QLogic(orbwalkingActionArgs.Target);
                    }
                }
            }
            if (orbwalkingActionArgs.Type == OrbwalkingType.BeforeAttack)
            {
                if (FocusWBuffedEnemyBool)
                {
                    Orbwalker.ForceTarget =
                        ValidTargets.FirstOrDefault(
                            h =>
                            h.Distance(ObjectManager.Player.ServerPosition) < 600
                            && h.HasBuff("kalistacoopstrikemarkally"));
                }
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            base.OnUpdate(args);

            if (E.IsReady()) this.ELogic();
            if (Orbwalker.ActiveMode == OrbwalkingMode.Combo && Q.IsReady() && Orbwalker.CanMove)
            {
                foreach (var enemy in ValidTargets.Where(e => e.Distance(ObjectManager.Player) < 900))
                {
                    var pred = Q.GetPrediction(enemy);
                    if (pred.Hitchance >= HitChance.High && !pred.CollisionObjects.Any())
                    {
                        Q.Cast(pred.UnitPosition);
                    }
                }
            }

            #region Orbwalk On Minions
            if (OrbwalkOnMinions && Orbwalker.ActiveMode == OrbwalkingMode.Combo && ValidTargets.Count(e => e.InAutoAttackRange()) == 0 && ObjectManager.Player.InventoryItems.Any(i => (int)i.IData.Id == 3085))
            {
                var minion =
                    GameObjects.EnemyMinions.Where(m => m.InAutoAttackRange()).OrderBy(m => m.Health).FirstOrDefault();
                if (minion != null)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }
            #endregion Orbwalk On Minions
        }

        public override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            if (DrawERangeBool)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position,
                    1000,
                    Color.LightGreen);
            }
            if (DrawRRangeBool)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position,
                    1400,
                    Color.DarkRed);
            }

            if (DrawEDamage)
            {
                HpBarDamageIndicator.DamageToUnit = GetFloatRendDamage;
            }
            HpBarDamageIndicator.Enabled = DrawEDamage;
        }

        private Menu ComboMenu;
        private Menu WomboComboMenu;
        private MenuBool BalistaBool;
        private MenuBool TalistaBool;
        private MenuBool SalistaBool;
        //private MenuKeyBind UseQWalljumpKey;
        private MenuSlider UseQManaSlider;
        private MenuBool FocusWBuffedEnemyBool;
        //private MenuBool UseEBeforeYouDieBool;
        private MenuBool UseRAllySaverBool;
        private MenuBool UseREngageBool;
        private MenuBool UseRCounterEngageBool;
        private MenuBool UseRInterruptBool;
        //private MenuBool OrbwalkOnMinionsBool;
        private Menu HarassMenu;
        private MenuBool UseQStackTransferBool;
        private MenuSlider UseQStackTransferMinStacksSlider;
        private MenuBool UseEIfResettedByAMinionBool;
        private MenuSlider EResetByAMinionMinManaSlider;
        private MenuSlider MinEnemyStacksForEMinionResetSlider;
        private Menu FarmMenu;
        private MenuBool AlwaysUseEIf2MinionsKillableBool;
        private Menu RendSmiteMenu;
        private Menu RendDamageMenu;
        private MenuSlider ReduceRendDamageBySlider;
        private Menu DrawMenu;
        private MenuBool DrawERangeBool;
        private MenuBool DrawRRangeBool;
        private MenuBool DrawEDamage;
        private MenuBool OrbwalkOnMinions;

        private void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("kalicombomenu", "Combo Settings: "));
            WomboComboMenu = ComboMenu.Add(new Menu("kaliwombos", "Wombo Combos: "));
            BalistaBool = WomboComboMenu.Add(new MenuBool("kalibalista", "Balista", true));
            TalistaBool = WomboComboMenu.Add(new MenuBool("kalitalista", "Talista", true));
            SalistaBool = WomboComboMenu.Add(new MenuBool("kalisalista", "Salista", true));
            OrbwalkOnMinions = ComboMenu.Add(new MenuBool("kaliorbwalkonminions", "Orbwalk On Minions", false));
            UseQManaSlider = ComboMenu.Add(new MenuSlider("kaliuseqmanaslider", "Use Q if Mana% > ", 20));
            //UseQWalljumpKey = ComboMenu.Add(new MenuKeyBind("useqwalljump", "Q Walljump Key", Keys.N, KeyBindType.Press));
            FocusWBuffedEnemyBool = ComboMenu.Add(new MenuBool("kalifocuswbuffedenemy", "Focus Enemy with W Buff", true));
            //UseEBeforeYouDieBool = ComboMenu.Add(new MenuBool("kaliuseebeforedeath", "Use E Before You Die", false));
            UseRAllySaverBool = ComboMenu.Add(new MenuBool("kaliusersaveally", "Use R to save Soulbound", true));
            UseREngageBool = ComboMenu.Add(new MenuBool("userengage", "Use R to engage", false));
            UseRCounterEngageBool = ComboMenu.Add(new MenuBool("kaliusercounternengage", "Use R counter-engage", true));
            UseRInterruptBool = ComboMenu.Add(new MenuBool("kaliuserinterrupt", "Use R to Interrupt"));
            HarassMenu = MainMenu.Add(new Menu("kaliharassmenu", "Harass Settings: "));
            UseQStackTransferBool = HarassMenu.Add(new MenuBool("kaliuseqstacktransfer", "Use Q Stack Transfer"));
            UseQStackTransferMinStacksSlider =
                HarassMenu.Add(new MenuSlider("kaliuseqstacktransferminstacks", "Min stacks for Stack Transfer", 3, 0,
                    15));
            UseEIfResettedByAMinionBool =
                HarassMenu.Add(new MenuBool("useeresetharass", "Use E if resetted by a minion"));
            EResetByAMinionMinManaSlider =
                HarassMenu.Add(new MenuSlider("useeresetmana", "Use E Reset by Minion if Mana% > ", 90));
            MinEnemyStacksForEMinionResetSlider =
                HarassMenu.Add(new MenuSlider("useeresetminenstacks", "Use E Reset if Enemy stacks > ", 3, 0, 25));
            FarmMenu = MainMenu.Add(new Menu("kalifarmmenu", "Farm Settings"));
            AlwaysUseEIf2MinionsKillableBool =
                FarmMenu.Add(new MenuBool("alwaysuseeif2minkillable", "Always use E if resetted with no mana cost", true));
            RendDamageMenu = MainMenu.Add(new Menu("kalirenddmgmenu", "Adjust Rend (E) DMG Prediction: "));
            ReduceRendDamageBySlider =
                RendDamageMenu.Add(new MenuSlider("kalirendreducedmg", "Reduce E DMG by: ", 15, 0, 300));
            DrawMenu = MainMenu.Add(new Menu("kalidrawmenu", "Drawing Settings: "));
            DrawERangeBool = DrawMenu.Add(new MenuBool("drawerangekali", "Draw E Range", true));
            DrawRRangeBool = DrawMenu.Add(new MenuBool("kalidrawrrange", "Draw R Range", true));
            DrawEDamage = DrawMenu.Add(new MenuBool("kalidrawedmg", "Draw E Damage", true));
            MainMenu.Attach();
        }

        #region Champion Logic
        void QLogic(AttackableUnit target = null)
        {
            if (target != null)
            {
                if (Orbwalker.ActiveMode == OrbwalkingMode.Combo)
                {
                    var hero = target as Obj_AI_Hero;
                    if (hero != null)
                    {
                        if (hero.IsHPBarRendered)
                        {
                            var pred = Q.GetPrediction(hero);
                            if (pred.Hitchance >= HitChance.High)
                            {
                                Q.Cast(pred.UnitPosition);
                                return;
                            }
                        }
                    }
                    else
                    {
                        foreach (var tar in ValidTargets.Where(t => t.Distance(ObjectManager.Player) < 900))
                        {
                            if (ObjectManager.Player.ManaPercent > UseQManaSlider.Value)
                            {
                                var pred = Q.GetPrediction(tar);
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    Q.Cast(pred.UnitPosition);
                                    return;
                                }
                            }
                        }
                    }
                }
                if (Orbwalker.ActiveMode == OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == OrbwalkingMode.Hybrid)
                {
                    var minion = target as Obj_AI_Minion;
                    if (minion != null && GetRendBuff(minion).Count >= UseQStackTransferMinStacksSlider
                        && target.Health < Q.GetDamage(minion))
                    {
                        foreach (var enemy in ValidTargets.Where(en => en.Distance(ObjectManager.Player) < 900))
                        {
                            var pred = Q.GetPrediction(enemy, false);
                            if (pred.Hitchance >= HitChance.High
                                && pred.CollisionObjects.All(co => co is Obj_AI_Minion && co.Health < Q.GetDamage(co))
                                && pred.CollisionObjects.Any(m => m.NetworkId == target.NetworkId))
                            {
                                Q.Cast(pred.UnitPosition);
                            }
                        }
                    }
                }
            }
        }

        void ELogic()
        {
            if (ValidTargets.Any(t => IsRendKillable(t)))
            {
                E.Cast();
            }
            if (GameObjects.JungleLarge.Any(IsRendKillable)
                || ObjectManager.Get<Obj_AI_Minion>().Any(
                    m =>
                    (m.CharData.BaseSkinName.Contains("Baron")
                    || m.CharData.BaseSkinName.Contains("Dragon") || m.CharData.Name.Contains("Crab") || m.CharData.Name.Contains("Herald")) && this.IsRendKillable(m)))
            {
                E.Cast();
            }
            if (AlwaysUseEIf2MinionsKillableBool && GameObjects.EnemyMinions.Count(IsRendKillable) >= 2)
            {
                E.Cast();
            }
            if (UseEIfResettedByAMinionBool && ObjectManager.Player.ManaPercent > EResetByAMinionMinManaSlider.Value)
            {
                if (
                    ValidTargets.Any(e =>
                            e.Distance(ObjectManager.Player.ServerPosition) > 615 &&
                            GetRendBuff(e).Count >= MinEnemyStacksForEMinionResetSlider.Value) &&
                    GameObjects.EnemyMinions.Any(m => IsRendKillable(m)))
                {
                    E.Cast();
                }
            }
            /*if (GameObjects.EnemyMinions.Any(m => m.CharData.BaseSkinName.Contains("MinionSiege") && IsRendKillable(m)))
            {
                E.Cast();
            }*/
            if ((Orbwalker.ActiveMode == OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == OrbwalkingMode.LastHit) &&
                GameObjects.EnemyMinions.Any(
                    m => IsRendKillable(m) &&
                    Health.GetPrediction(m, (int)((Game.Ping / 2) + ObjectManager.Player.AttackCastDelay * 1000)) < 1
                    && Health.GetPrediction(m, (int)((Game.Ping / 2) + 100)) > 1))
            {
                E.Cast();
            }
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
                foreach (var enemy in ValidTargets.Where(en => en.IsValidTarget(1000) && en.IsFacing(ObjectManager.Player)))
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

        #region Damages


        private static readonly float[] RawRendDamage = { 20, 30, 40, 50, 60 };
        private static readonly float[] RawRendDamageMultiplier = { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f };
        private static readonly float[] RawRendDamagePerSpear = { 10, 14, 19, 25, 32 };
        private static readonly float[] RawRendDamagePerSpearMultiplier = { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f };
        
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

        private bool ShouldntRend(Obj_AI_Hero target)
        {
            //Dead or not a hero
            if (target == null || !target.IsHPBarRendered) return false;
            //Undying
            if (this.UndyingBuffs.Any(buff => target.HasBuff(buff))) return true;
            //Blitzcrank
            if (target.CharData.BaseSkinName == "Blitzcrank" && !target.HasBuff("BlitzcrankManaBarrierCD")
                && !target.HasBuff("ManaBarrier"))
            {
                return true;
            }
            //SpellShield
            return target.HasBuffOfType(BuffType.SpellShield) || target.HasBuffOfType(BuffType.SpellImmunity);
        }

        private BuffInstance GetRendBuff(Obj_AI_Base target)
        {
            return target.Buffs.FirstOrDefault(b => b.Name == "kalistaexpungemarker");
        }

        private bool HasRendBuff(Obj_AI_Base target)
        {
            return this.GetRendBuff(target) != null;
        }

        private double GetTotalHealthWithShieldsApplied(Obj_AI_Base target)
        {
            return target.Health + target.AllShield;
        }

        public bool IsRendKillable(Obj_AI_Base target)
        {
            // Validate unit
            if (target == null) { return false; }
            if (!HasRendBuff(target)) { return false; }
            if (target is Obj_AI_Hero && target.Health > 1)
            {
                if (ShouldntRend((Obj_AI_Hero)target)) return false;
            }

            // Take into account all kinds of shields
            var totalHealth = GetTotalHealthWithShieldsApplied(target);

            var dmg = GetRendDamage(target) - ReduceRendDamageBySlider;

            if (target.Name.Contains("Baron") && ObjectManager.Player.HasBuff("barontarget"))
            {
                dmg *= 0.5f;
            }
            //you deal -7% dmg to dragon for each killed dragon
            if (target.Name.Contains("Dragon") && ObjectManager.Player.HasBuff("s5test_dragonslayerbuff"))
            {
                dmg *= (1f - (0.075f * ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff")));
            }
            return dmg > totalHealth;
        }

        public float GetFloatRendDamage(Obj_AI_Base target)
        {
            return (float)GetRendDamage(target, -1);
        }
        public double GetRendDamage(Obj_AI_Base target)
        {
            return GetRendDamage(target, -1);
        }

        public double GetRendDamage(Obj_AI_Base target, int customStacks = -1, BuffInstance rendBuff = null)
        {
            // Calculate the damage and return
            return ObjectManager.Player.CalculateDamage(target, DamageType.Physical, GetRawRendDamage(target, customStacks, rendBuff)); 
        }

        public float GetRawRendDamage(Obj_AI_Base target, int customStacks = -1, BuffInstance rendBuff = null)
        {
            rendBuff = rendBuff ?? GetRendBuff(target);
            var stacks = (customStacks > -1 ? customStacks : rendBuff != null ? rendBuff.Count : 0) - 1;
            if (stacks > -1)
            {
                var index = E.Level - 1;
                return RawRendDamage[index] + stacks * RawRendDamagePerSpear[index] +
                       ObjectManager.Player.TotalAttackDamage * (RawRendDamageMultiplier[index] + stacks * RawRendDamagePerSpearMultiplier[index]);
            }

            return 0;
        }
#endregion
    }
}