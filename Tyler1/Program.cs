using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Color = System.Drawing.Color;
using System.Drawing;
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
        public static MenuSlider MaxQAxes;
        public static MenuSlider MinQLaneclearManaPercent;
        public static Menu EMenu;
        public static MenuBool ECombo;
        public static MenuBool EGC;
        public static MenuBool EInterrupt;
        public static Menu RMenu;
        public static MenuBool RKS;
        public static MenuSlider RIfHit;
        public static MenuBool WCombo;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static Spell Q, W, E, R;
        static Items.Item BOTRK, Bilgewater, Yomamas, Mercurial, QSS;
        public static Color color = Color.DarkOrange;
        public static float MyRange = 550f;
        private static int _lastCatchAttempt;

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
                return AxesCount + ObjectManager.Get<GameObject>()
                    .Count(
                        x =>
                            x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead);
            }
        }

        static void Main(string[] args)
        {
            Events.OnLoad += Load;
        }

        private static void Load(object sender, EventArgs args)
        {
            DelayAction.Add(1500, () =>
            {
                if (ObjectManager.Player.CharData.BaseSkinName != "Draven") return;
                InitSpells();
                FinishLoading();
            });
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
            Drawing.OnDraw += Draw;
            Game.OnUpdate += OnUpdate;
            Events.OnGapCloser += OnGapcloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
            DelayAction.Add(3000, () => MyRange = Variables.Orbwalker.GetAutoAttackRange(Player));
            Variables.Orbwalker.Enabled = true;
            DelayAction.Add(1000, ()=>Variables.Orbwalker.Enabled = true);
            DelayAction.Add(5000, () => Variables.Orbwalker.Enabled = true);
            DelayAction.Add(10000, () => Variables.Orbwalker.Enabled = true);
            Menu = new Menu("tyler1", "Tyler1", true);
            AutoCatch = Menu.Add(new MenuBool("tyler1auto", "Auto catch axes?", true));
            CatchOnlyCloseToMouse = Menu.Add(new MenuBool("tyler1onlyclose", "Catch only axes close to mouse?", true));
            MaxDistToMouse = Menu.Add(new MenuSlider("tyler1maxdist", "Max axe distance to mouse", 500, 250, 1250));
            OnlyCatchIfSafe = Menu.Add(new MenuBool("tyler1safeaxes", "Only catch axes if safe (anti melee)", false));
            MaxQAxes = Menu.Add(new MenuSlider("tyler1MaxQs", "Max Q Axes", 5, 1, 5));
            MinQLaneclearManaPercent = Menu.Add(new MenuSlider("tyler1QLCMana", "Min Mana Percent for Q Laneclear", 60, 0, 100));
            EMenu = Menu.Add(new Menu("tyler1E", "E Settings: "));
            ECombo = EMenu.Add(new MenuBool("tyler1ECombo", "Use E in Combo", true));
            EGC = EMenu.Add(new MenuBool("tyler1EGC", "Use E on Gapcloser", true));
            EInterrupt = EMenu.Add(new MenuBool("tyler1EInterrupt", "Use E to Interrupt", true));
            RMenu = Menu.Add(new Menu("tyler1R", "R Settings:"));
            RKS = RMenu.Add(new MenuBool("tyler1RKS", "Use R to steal kills", true));
            RIfHit = RMenu.Add(new MenuSlider("tyler1RIfHit", "Use R if it will hit X enemies", 2, 1, 5));
            WCombo = Menu.Add(new MenuBool("tyler1WCombo", "Use W in Combo", true));
            Menu.Attach();
        }

        private static void OnUpdate(EventArgs args)
        {
            var target = Variables.TargetSelector.GetTarget(E);
            try
            {
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
            }
            catch (Exception ex)
            {
            }
        }

        private static void RCombo()
        {
            var target = Variables.TargetSelector.GetTarget(E);
            if (target != null && target.IsHPBarRendered)
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
            if (ObjectManager.Get<Obj_AI_Minion>().Any(m => m.IsHPBarRendered && m.Distance(ObjectManager.Player) < MyRange))
            {
                if (AxesCount == 0 && Q.IsReady()) Q.Cast();
            }
        }

        private static void Combo()
        {
            var target = Variables.TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target.Distance(Player) < MyRange + 100)
            {
                if (AxesCount < 1 && TotalAxesCount <= MaxQAxes.Value) Q.Cast();
                if (WCombo && W.IsReady() && !Player.HasBuff("dravenfurybuff")) W.Cast();
            }
            if (ECombo && E.IsReady() && target.IsValidTarget(750)) E.Cast(target.ServerPosition);

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
                    var AXE in
                        ObjectManager.Get<GameObject>()
                            .Where(
                                x =>
                                    x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead  &&
                                    (!x.Position.IsUnderEnemyTurret() || Mouse.IsUnderEnemyTurret()))
                            .OrderBy(a => a.Distance(ObjectManager.Player)))
                {
                    if (OnlyCatchIfSafe &&
                        GameObjects.EnemyHeroes.Count(
                            e => e.IsHPBarRendered && e.IsMelee && e.ServerPosition.Distance(AXE.Position) < 350) >= 1)
                    {
                        break;
                    }
                    if (CatchOnlyCloseToMouse && AXE.Distance(Mouse) > MaxDistToMouse.Value)
                    {
                        Variables.Orbwalker.SetMovementState(true);
                        break;
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
        
        

    private static void KS()
    {
        if (!RKS) return;
            foreach (
                var enemy in
                    GameObjects.EnemyHeroes.Where(e => e.IsHPBarRendered && e.Distance(ObjectManager.Player) < 3000))
            {
                if (enemy.Health < R.GetDamage(enemy))
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
            foreach (var AXE in ObjectManager.Get<GameObject>().Where(x => x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead))
            {
                var AXEToScreen = Drawing.WorldToScreen(AXE.Position);
                var PlayerPosToScreen = Drawing.WorldToScreen(ObjectManager.Player.Position);
                Render.Circle.DrawCircle(AXE.Position, 140, Color.Red, 8);
                Drawing.DrawLine(PlayerPosToScreen, AXEToScreen, 8, Color.Red);
            }
            if (CatchOnlyCloseToMouse && MaxDistToMouse.Value < 700 && ObjectManager.Get<GameObject>().Any(x => x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead))
            {
                Render.Circle.DrawCircle(Game.CursorPos, MaxDistToMouse.Value, Color.Red, 8);
            }
        }
        private static void OnGapcloser(object sender, Events.GapCloserEventArgs gapcloser)
        {
            if (EGC && E.IsReady())
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
