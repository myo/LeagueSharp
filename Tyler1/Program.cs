using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using SharpDX.IO;

namespace Tyler1
{
    class Program
    {
        private static Menu Menu;
        public static MenuBool AutoCatch;
        public static MenuBool CatchOnlyCloseToMouse;
        public static MenuSlider MaxDistToMouse;
        public static MenuBool OnlyCatchIfSafe;
        public static MenuSlider MinQLaneclearManaPercent;
        public static Menu EMenu;
        public static MenuBool ECombo;
        public static MenuBool EGC;
        public static MenuBool EInterrupt;
        public static Menu RMenu;
        public static MenuBool RKS;
        public static MenuBool RKSOnlyIfCantAA;
        public static MenuSlider RIfHit;
        public static MenuBool WCombo;
        public static MenuBool UseItems;
        public static Menu DrawingMenu;
        public static MenuBool DrawAXELocation;
        public static MenuBool DrawAXECatchRadius;
        public static MenuBool DrawAXELine;
        public static MenuColor DrawingColor;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static Spell Q, W, E, R;
        static Items.Item BOTRK, Bilgewater, Yomamas, Mercurial, QSS;
        public static Color color = Color.DarkOrange;
        public static float MyRange = 550f;
        //private static int _lastCatchAttempt;
        private static MenuBool R1vs1;

        private static Dictionary<int, GameObject> Reticles;

        private static int AxesCount
        {
            get
            {
                var data = Player.GetBuff("dravenspinningattack");
                if (data == null || data.Count == -1)
                {
                    return 0;
                }
                return data.Count == 0 ? 1 : data.Count;
            }
        }

        private static int TotalAxesCount
        {
            get
            {
                return (ObjectManager.Player.HasBuff("dravenspinning") ? 1 : 0)
                       + (ObjectManager.Player.HasBuff("dravenspinningleft") ? 1 : 0) + Reticles.Count;
            }
        }

        static void Main(string[] args)
        {
            Bootstrap.Init(new string[1]);
            Events.OnLoad += Load;
        }

        private static void Load(object sender, EventArgs args)
        {
            DelayAction.Add(1500, () =>
            {
                if (ObjectManager.Player.CharData.BaseSkinName != "Draven") return;
                InitSpells();
                FinishLoading();
                Reticles = new Dictionary<int, GameObject>();
                GameObject.OnCreate += OnCreate;
                GameObject.OnDelete += OnDelete;
            });
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            var itemToDelete = Reticles.FirstOrDefault(ret => ret.Value.NetworkId == sender.NetworkId);
            if (itemToDelete.Key != null)
            {
                Reticles.Remove(itemToDelete.Key);
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Equals("Draven_Base_Q_reticle_self.troy") && !sender.IsDead)
            {
                Reticles.Add(Variables.TickCount, sender);
            }
        }

        private static void InitSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1050);
            E.SetSkillshot(0.25f, 130, 1400, false, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R, 3000);
            R.SetSkillshot(0.25f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            BOTRK = new Items.Item(3153, 550);
            Bilgewater = new Items.Item(3144, 550);
            Yomamas = new Items.Item(3142, 400);
            Mercurial = new Items.Item(3139, 22000);
            QSS = new Items.Item(3140, 22000);
        }

        private static void FinishLoading()
        {
            LeagueSharp.Drawing.OnDraw += Draw;
            Game.OnUpdate += OnUpdate;
            Events.OnGapCloser += OnGapcloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
            DelayAction.Add(3000, () => MyRange = Variables.Orbwalker.GetAutoAttackRange(Player));
            Variables.Orbwalker.Enabled = true;
            DelayAction.Add(1000, () => Variables.Orbwalker.Enabled = true);
            DelayAction.Add(5000, () => Variables.Orbwalker.Enabled = true);
            DelayAction.Add(10000, () => Variables.Orbwalker.Enabled = true);
            Menu = new Menu("tyler1", "Tyler1", true);
            AutoCatch = Menu.Add(new MenuBool("tyler1auto", "Auto catch axes?", true));
            CatchOnlyCloseToMouse = Menu.Add(new MenuBool("tyler1onlyclose", "Catch only axes close to mouse?", true));
            MaxDistToMouse = Menu.Add(new MenuSlider("tyler1maxdist", "Max axe distance to mouse", 500, 250, 1250));
            OnlyCatchIfSafe = Menu.Add(new MenuBool("tyler1safeaxes", "Only catch axes if safe (anti melee)", false));
            MinQLaneclearManaPercent =
                Menu.Add(new MenuSlider("tyler1QLCMana", "Min Mana Percent for Q Laneclear", 60, 0, 100));
            EMenu = Menu.Add(new Menu("tyler1E", "E Settings: "));
            ECombo = EMenu.Add(new MenuBool("tyler1ECombo", "Use E in Combo", true));
            EGC = EMenu.Add(new MenuBool("tyler1EGC", "Use E on Gapcloser", true));
            EInterrupt = EMenu.Add(new MenuBool("tyler1EInterrupt", "Use E to Interrupt", true));
            RMenu = Menu.Add(new Menu("tyler1R", "R Settings:"));
            RKS = RMenu.Add(new MenuBool("tyler1RKS", "Use R to steal kills", true));
            RKSOnlyIfCantAA = RMenu.Add(new MenuBool("tyler1RKSOnlyIfCantAA", "Use R KS only if can't AA", true));
            RIfHit = RMenu.Add(new MenuSlider("tyler1RIfHit", "Use R if it will hit X enemies", 2, 1, 5));
            R1vs1 = RMenu.Add(new MenuBool("tyler1R1v1", "Always use R in 1v1", true));

            WCombo = Menu.Add(new MenuBool("tyler1WCombo", "Use W in Combo", true));
            UseItems = Menu.Add(new MenuBool("tyler1Items", "Use Items?", true));

            DrawingMenu = Menu.Add(new Menu("tyler1DrawSettings", "Draw Settings:"));
            DrawAXECatchRadius = DrawingMenu.Add(new MenuBool("tyler1AxeCatchDraw", "Draw Axe Catch Radius", true));
            DrawAXELocation = DrawingMenu.Add(new MenuBool("tyler1AxeLocationDraw", "Draw Axe Location", true));
            DrawAXELine = DrawingMenu.Add(new MenuBool("tyler1AxeLineDraw", "Draw Line to Axe Position", true));
            DrawingColor = DrawingMenu.Add(new MenuColor("tyler1DrawingColor", "Drawing Color", ColorBGRA.FromRgba(Color.Red.ToRgba())));

            Menu.Attach();
        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                var target = Variables.TargetSelector.GetTarget(E);
                if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.LaneClear) Farm();
                if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo && target != null)
                {
                    Combo();
                    RCombo();
                }
                CatchAxes();
                KS();
                if (W.IsReady() && Player.HasBuffOfType(BuffType.Slow) &&
                    target.Distance(ObjectManager.Player) <= MyRange) W.Cast();
                R1V1(target);

            }
            catch (Exception ex)
            {
            }
        }

        private static void R1V1(Obj_AI_Hero target)
        {
            if (R1vs1 && target != null && target.IsHPBarRendered && target.Distance(ObjectManager.Player) < 650 && !ShouldntUlt(target))
            {
                if (target.HealthPercent > ObjectManager.Player.HealthPercent &&
                    (target.MaxHealth <= ObjectManager.Player.MaxHealth + 300 ||
                     noobchamps.Contains(target.CharData.BaseSkinName)))
                {
                    var pred = R.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        R.Cast(pred.UnitPosition);
                    }
                }
            }
        }

        private static List<string> noobchamps = new List<string>
        {
            "Ahri",
            "Anivia",
            "Annie",
            "Ashe",
            "Azir",
            "Brand",
            "Caitlyn",
            "Cassiopeia",
            "Corki",
            "Draven",
            "Ezreal",
            "Graves",
            "Jinx",
            "Kalista",
            "Karma",
            "Karthus",
            "Katarina",
            "Kennen",
            "KogMaw",
            "Leblanc",
            "Kindred",
            "Lucian",
            "Lux",
            "Malzahar",
            "MasterYi",
            "MissFortune",
            "Orianna",
            "Quinn",
            "Sivir",
            "Syndra",
            "Talon",
            "Teemo",
            "Tristana",
            "TwistedFate",
            "Twitch",
            "Varus",
            "Vayne",
            "Veigar",
            "Velkoz",
            "Viktor",
            "Xerath",
            "Zed",
            "Ziggs",
            "Soraka",
            "Akali",
            "Diana",
            "Ekko",
            "Fiddlesticks",
            "Fiora",
            "Fizz",
            "Heimerdinger",
            "Illaoi",
            "Jayce",
            "Kassadin",
            "Kayle",
            "KhaZix",
            "Kindred",
            "Lissandra",
            "Mordekaiser",
            "Nidalee",
            "Riven",
            "Shaco",
            "Vladimir",
            "Yasuo",
            "Zilean"
        };

        /// <summary>
        /// Those buffs make the target either unkillable or a pain in the ass to kill, just wait until they end
        /// </summary>
        private static List<string> UndyingBuffs = new List<string>
        {
            "JudicatorIntervention",
            "UndyingRage",
            "FerociousHowl",
            "ChronoRevive",
            "ChronoShift",
            "lissandrarself",
            "kindredrnodeathbuff"
        };

        private static bool ShouldntUlt(Obj_AI_Hero target)
        {
            //Dead or not a hero
            if (target == null || !target.IsHPBarRendered) return true;
            //Undying
            if (UndyingBuffs.Any(buff => target.HasBuff(buff))) return true;
            //Blitzcrank
            if (target.CharData.BaseSkinName == "Blitzcrank" && !target.HasBuff("BlitzcrankManaBarrierCD")
                && !target.HasBuff("ManaBarrier"))
            {
                return true;
            }
            //Sivir
            return target.CharData.BaseSkinName == "Sivir" && target.HasBuffOfType(BuffType.SpellShield) ||
                   target.HasBuffOfType(BuffType.SpellImmunity);
        }

        private static void RCombo()
        {
            var target = Variables.TargetSelector.GetTarget(E);
            if (target != null && target.IsHPBarRendered && !target.IsDead && !target.IsZombie)
            {
                var pred = R.GetPrediction(target);
                if (pred.Hitchance > HitChance.High && pred.AoeTargetsHit.Count >= RIfHit.Value)
                {
                    R.Cast(pred.UnitPosition);
                }
            }
        }

        private static void Farm()
        {
            if (ObjectManager.Player.ManaPercent < MinQLaneclearManaPercent.Value) return;
            if (
                ObjectManager.Get<Obj_AI_Minion>()
                    .Any(m => m.IsHPBarRendered && m.IsEnemy && m.Distance(ObjectManager.Player) < MyRange))
            {
                if (TotalAxesCount < 2) Q.Cast();
            }
        }

        private static void Combo()
        {
            var target = Variables.TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target.Distance(Player) < MyRange + 100)
            {
                if (TotalAxesCount < 2) Q.Cast();
                if (WCombo && W.IsReady() && !Player.HasBuff("dravenfurybuff")) W.Cast();
            }
            if (ECombo && E.IsReady() && target.IsValidTarget(750))
            {
                var pred = E.GetPrediction(target);
                if (pred.Hitchance >= HitChance.High)
                    E.Cast(pred.UnitPosition);
            }

            if (UseItems)
            {
                if (target.IsValidTarget(MyRange))
                {
                    if (Yomamas.IsReady) Yomamas.Cast();
                    if (Bilgewater.IsReady) Bilgewater.Cast(target);
                    if (BOTRK.IsReady) BOTRK.Cast(target);
                }
                //QSS
                if (Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Fear) ||
                    Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Taunt) ||
                    Player.HasBuffOfType(BuffType.Blind))
                {
                    if (Mercurial.IsReady) DelayAction.Add(100, () => Mercurial.Cast());
                    if (QSS.IsReady) DelayAction.Add(100, () => QSS.Cast());
                }
            }
        }

        private static void CatchAxes()
        {
            Vector3 Mouse = Game.CursorPos;
            if (!ObjectManager
                .Get<GameObject>(
                ).Any(x => x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead) || !AutoCatch)
            {
                Variables.Orbwalker.SetMovementState(true);
            }
            if (AutoCatch)
            {
                foreach (
                    var reticle in
                        Reticles
                            .Where(
                                x => !x.Value.IsDead &&
                                     (!x.Value.Position.IsUnderEnemyTurret() ||
                                      (Mouse.IsUnderEnemyTurret() && ObjectManager.Player.IsUnderEnemyTurret())))
                            .OrderBy(ret => ret.Key))
                {
                    var AXE = reticle.Value;
                    if (OnlyCatchIfSafe &&
                        GameObjects.EnemyHeroes.Count(
                            e => e.IsHPBarRendered && e.IsMelee && e.ServerPosition.Distance(AXE.Position) < 350) >= 1)
                    {
                        break;
                    }
                    if (CatchOnlyCloseToMouse && AXE.Distance(Mouse) > MaxDistToMouse.Value)
                    {
                        Variables.Orbwalker.SetMovementState(true);

                        if (GameObjects.EnemyHeroes.Count(
                            e => e.IsHPBarRendered && e.IsMelee && e.ServerPosition.Distance(AXE.Position) < 350) >= 1)
                        {
                            //user probably doesn't want to go there, try the next reticle
                            break;
                        }
                        //maybe user just has potato reaction time
                        return;
                    }
                    if (AXE.Distance(Player.ServerPosition) > 80 && Variables.Orbwalker.CanMove(30, false))
                    {
                        Variables.Orbwalker.Move(AXE.Position.Randomize());
                        Variables.Orbwalker.SetMovementState(false);
                        //DelayAction.Add(300, () => Variables.Orbwalker.SetMovementState(true));
                    }
                    if (AXE.Distance(Player.ServerPosition) <= 80)
                    {
                        Variables.Orbwalker.SetMovementState(true);
                    }
                }
            }
        }


        /// <summary>
        /// Will need to add an actual missile check for the axes in air instead of brosciencing
        /// </summary>
        private static void KS()
        {
            if (!RKS) return;
            foreach (
                var enemy in
                    GameObjects.EnemyHeroes.Where(
                        e =>
                            e.IsHPBarRendered && e.Distance(ObjectManager.Player) < 3000 &&
                            (e.Distance(ObjectManager.Player) > MyRange + 150 || !RKSOnlyIfCantAA)))
            {
                if (enemy.Health < R.GetDamage(enemy) && !ShouldntUlt(enemy))
                {
                    var pred = R.GetPrediction(enemy);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        R.Cast(pred.UnitPosition);
                    }
                }
            }
        }

        private static void Draw(EventArgs args)
        {
            if (Player.IsDead) return;
            var reticles =
                ObjectManager.Get<GameObject>()
                    .Where(x => x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead).ToArray();
            Color lColor = Color.Red;
            if (reticles.Any())
            {
                var PlayerPosToScreen = LeagueSharp.Drawing.WorldToScreen(ObjectManager.Player.Position);

                if (DrawAXELocation.Value)
                    foreach (var AXE in reticles)
                    {
                        var AXEToScreen = LeagueSharp.Drawing.WorldToScreen(AXE.Position);
                        Render.Circle.DrawCircle(AXE.Position, 140, lColor, 8);
                    }

                LeagueSharp.Drawing.DrawLine(PlayerPosToScreen, LeagueSharp.Drawing.WorldToScreen(reticles[0].Position), 8, lColor);

                if (DrawAXELine.Value)
                    for (int i = 0; i < reticles.Length; i++)
                    {
                        if (i < reticles.Length - 1)
                        {
                            LeagueSharp.Drawing.DrawLine(LeagueSharp.Drawing.WorldToScreen(reticles[i].Position),
                                LeagueSharp.Drawing.WorldToScreen(reticles[i + 1].Position), 8, lColor);
                        }
                    }
                if (DrawAXECatchRadius.Value)
                    if (CatchOnlyCloseToMouse && MaxDistToMouse.Value < 700 &&
                        ObjectManager.Get<GameObject>()
                            .Any(x => x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead))
                    {
                        Render.Circle.DrawCircle(Game.CursorPos, MaxDistToMouse.Value, lColor, 8);
                    }
            }
        }

        private static void OnGapcloser(object sender, Events.GapCloserEventArgs gapcloser)
        {
            if (EGC && E.IsReady() && gapcloser.Sender.Distance(ObjectManager.Player) < 800)
            {
                var pred = E.GetPrediction(gapcloser.Sender);
                if (pred.Hitchance > HitChance.High)
                {
                    E.Cast(pred.UnitPosition);
                }
            }
        }

        private static void OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs args)
        {
            if (EInterrupt && E.IsReady() && args.Sender.Distance(ObjectManager.Player) < 950)
            {
                E.Cast(args.Sender.Position);
            }
        }
    }
}