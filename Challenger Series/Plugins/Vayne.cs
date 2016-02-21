#region License
/* Copyright (c) LeagueSharp 2016
 * No reproduction is allowed in any way unless given written consent
 * from the LeagueSharp staff.
 * 
 * Author: imsosharp
 * Date: 2/21/2016
 * File: Vayne.cs
 */
#endregion License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers.Damages;
using SharpDX;
using Challenger_Series.Utils;
using LeagueSharp.SDK.Core.Utils;
using Geometry = Challenger_Series.Utils.Geometry;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
using Color = System.Drawing.Color;

namespace Challenger_Series
{
    public class Vayne : CSPlugin
    {

        #region ctor
        public Vayne()
        {
            base.Q = new Spell(SpellSlot.Q, 300);
            base.W = new Spell(SpellSlot.W);
            base.E = new Spell(SpellSlot.E, 550);
            base.R = new Spell(SpellSlot.R);

            base.E.SetSkillshot(0.42f, 50f, 1300f, false, SkillshotType.SkillshotLine);
            CachedGapclosers = new List<Tuple<string, SpellDatabaseEntry>>();
            CachedCrowdControl = new List<Tuple<string, SpellDatabaseEntry>>();
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                foreach (var spell in enemy.Spellbook.Spells)
                {
                    var sdata = SpellDatabase.GetByName(spell.Name);
                    if (sdata != null)
                    {
                        if (sdata.SpellTags == null)
                        {
                            Game.PrintChat(enemy.ChampionName + " " + spell.Name + " is broken in SDK, report to imsosharp.");
                            break;
                        }
                        if (
                            sdata.SpellTags.Any(
                                st => st == SpellTags.Dash || st == SpellTags.Blink))
                        {
                            CachedGapclosers.Add(new Tuple<string, SpellDatabaseEntry>(enemy.CharData.BaseSkinName,
                                sdata));
                        }
                        if (sdata.SpellTags.Any(st => st == SpellTags.CrowdControl))
                        {
                            CachedCrowdControl.Add(new Tuple<string, SpellDatabaseEntry>(enemy.CharData.BaseSkinName,
                                sdata));
                        }
                    }
                }
            }
            InitMenu();
            Game.OnUpdate += OnUpdate;
            Orbwalker.OnAction += OnOrbwalkingAction;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
        }
        #endregion

        #region Cache bik
        public List<Tuple<string, SpellDatabaseEntry>> CachedGapclosers;
        public List<Tuple<string, SpellDatabaseEntry>> CachedCrowdControl;
        #endregion

        #region Events

        public override void OnUpdate(EventArgs args)
        {
            base.OnUpdate(args);
            if (UseEBool)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(550)))
                {
                    if (IsCondemnable(enemy))
                    {
                        E.CastOnUnit(enemy);
                    }
                }
            }
            if (SemiAutomaticCondemnKey.Active)
            {
                foreach (
                    var hero in
                        GameObjects.EnemyHeroes.Where(
                            h => h.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 550))
                {
                    var prediction = E.GetPrediction(hero);
                    for (var i = 40; i < 425; i += 125)
                    {
                        var flags = NavMesh.GetCollisionFlags(
                            prediction.UnitPosition.ToVector2()
                                .Extend(ObjectManager.Player.ServerPosition.ToVector2(),
                                    -i)
                                .ToVector3());
                        if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
                        {
                            E.CastOnUnit(hero);
                            return;
                        }
                    }
                }
            }
            if (UseEInterruptBool)
            {
                var possibleChannelingTarget =
                    GameObjects.EnemyHeroes.FirstOrDefault(
                        e =>
                            e.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 550 &&
                            e.IsCastingInterruptableSpell());
                if (possibleChannelingTarget.IsValidTarget())
                {
                    E.CastOnUnit(possibleChannelingTarget);
                }
            }
        }

        public override void OnProcessSpellCast(GameObject sender, GameObjectProcessSpellCastEventArgs args)
        {
            base.OnProcessSpellCast(sender, args);
            if (sender is Obj_AI_Hero && sender.IsEnemy)
            {
                if (args.SData.Name == "summonerflash" && args.End.Distance(ObjectManager.Player.ServerPosition) < 350)
                {
                    E.CastOnUnit((Obj_AI_Hero) sender);
                }
                var sdata = SpellDatabase.GetByName(args.SData.Name);
                if (sdata != null)
                {
                    if (UseEAntiGapcloserBool &&
                        ObjectManager.Player.Distance(args.Start.Extend(args.End, sdata.Range)) < 350 &&
                        sdata.SpellTags.Any(st => st == SpellTags.Dash || st == SpellTags.Blink))
                    {
                        E.CastOnUnit((Obj_AI_Hero) sender);
                    }
                    if (UseEInterruptBool && sdata.SpellTags.Any(st => st == SpellTags.Interruptable) &&
                        ObjectManager.Player.Distance(sender) < 550)
                    {
                        E.CastOnUnit((Obj_AI_Hero) sender);
                    }
                }
            }
        }

        public override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            if (DrawEnemyWaypointsBool)
            {
                foreach (
                    var e in
                        GameObjects.EnemyHeroes.Where(
                            en => en.IsVisible && !en.IsDead && en.Distance(ObjectManager.Player) < 2500))
                {
                    var ip = Drawing.WorldToScreen(e.Position); //start pos

                    var wp = MathUtils.GetWaypoints(e);
                    var c = wp.Count - 1;
                    if (wp.Count() <= 1) break;

                    var w = Drawing.WorldToScreen(wp[c].ToVector3()); //endpos

                    Drawing.DrawLine(ip.X, ip.Y, w.X, w.Y, 2, Color.Red);
                }
            }
        }

        private void OnOrbwalkingAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
            if (orbwalkingActionArgs.Type == OrbwalkingType.AfterAttack)
            {
                Orbwalker.ForceTarget = null;
                if (Orbwalker.ActiveMode != OrbwalkingMode.Combo)
                {
                    var possible2WTarget = GameObjects.EnemyHeroes.FirstOrDefault(
                        h =>
                            h.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 500 &&
                            h.GetBuffCount("vaynesilvereddebuff") == 2);
                    if (possible2WTarget.IsValidTarget() && UseEAs3rdWProcBool && (sender as Obj_AI_Hero).GetWaypoints().LastOrDefault().Distance(ObjectManager.Player.ServerPosition) < 550)
                    {
                        E.Cast(possible2WTarget);
                    }
                }
                if (orbwalkingActionArgs.Target is Obj_AI_Hero && UseQBool)
                {
                    if (Q.IsReady())
                    {
                        var tg = orbwalkingActionArgs.Target as Obj_AI_Hero;
                        if (tg != null)
                        {
                            var mode = QModeStringList.SelectedValue;
                            var tumblePosition = Game.CursorPos;
                            switch (mode)
                            {
                                case "PRADA":
                                    tumblePosition = GetTumblePos(tg);
                                    break;
                                default:
                                    tumblePosition = Game.CursorPos;
                                    break;
                            }
                            if (tumblePosition.Distance(ObjectManager.Player.Position) > 2000) return;
                            Q.Cast(tumblePosition);
                        }
                    }
                }
                if (orbwalkingActionArgs.Target is Obj_AI_Minion && Orbwalker.ActiveMode == OrbwalkingMode.LaneClear)
                {
                    if (E.IsReady())
                    {
                        var tg = orbwalkingActionArgs.Target as Obj_AI_Minion;
                        if (tg.Name.Contains("SRU_") && tg.IsValidTarget() && UseEJungleFarm)
                        {
                            E.CastOnUnit(tg);
                        }
                    }
                    if (
                        UseQFarm &&
                        GameObjects.EnemyMinions.Count(
                            m =>
                                m.Position.Distance(ObjectManager.Player.Position) < 550 &&
                                m.Health < ObjectManager.Player.GetAutoAttackDamage(m) + Q.GetDamage(m)) > 1 &&
                        !IsDangerousPosition(Game.CursorPos))
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }
            }
            if (orbwalkingActionArgs.Type == OrbwalkingType.BeforeAttack)
            {
                var possible2WTarget = GameObjects.EnemyHeroes.FirstOrDefault(
                    h =>
                        h.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 500 &&
                        h.GetBuffCount("vaynesilvereddebuff") == 2);
                if (TryToFocus2WBool && possible2WTarget.IsValidTarget())
                {
                    Orbwalker.ForceTarget = possible2WTarget;
                }
                if (ObjectManager.Player.HasBuff("vaynetumblefade") && DontAttackWhileInvisibleAndMeelesNearBool)
                {
                    if (
                        GameObjects.EnemyHeroes.Any(
                            e => e.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 350 && e.IsMelee))
                    {
                        orbwalkingActionArgs.Process = false;
                    }
                }
                if (ObjectManager.Player.HasBuff("vaynetumblebonus") && orbwalkingActionArgs.Target is Obj_AI_Minion &&
                    UseQBonusOnEnemiesNotCS)
                {
                    var possibleTarget = Variables.TargetSelector.GetTarget(-1f, DamageType.Physical);
                    if (possibleTarget != null && possibleTarget.InAutoAttackRange())
                    {
                        Orbwalker.ForceTarget = possibleTarget;
                        Orbwalker.Attack(possibleTarget);
                        orbwalkingActionArgs.Process = false;
                    }
                }
                var possibleNearbyMeleeChampion =
                    GameObjects.EnemyHeroes.FirstOrDefault(
                        e => e.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 350);

                if (possibleNearbyMeleeChampion.IsValidTarget())
                {
                    if (Q.IsReady() && UseQBool)
                    {
                        Q.Cast(ObjectManager.Player.ServerPosition.Extend(possibleNearbyMeleeChampion.ServerPosition,
                            -350));
                        orbwalkingActionArgs.Process = false;
                    }
                    if (UseEWhenMeleesNearBool && !Q.IsReady() && E.IsReady())
                    {
                        var possibleMeleeChampionsGapclosers = from tuplet in CachedGapclosers
                            where tuplet.Item1 == possibleNearbyMeleeChampion.CharData.BaseSkinName
                            select tuplet.Item2;
                        if (possibleMeleeChampionsGapclosers.FirstOrDefault() != null)
                        {
                            if (
                                possibleMeleeChampionsGapclosers.Any(
                                    gapcloserEntry =>
                                        possibleNearbyMeleeChampion.Spellbook.GetSpell(gapcloserEntry.Slot).IsReady()))
                            {
                                return;
                            }
                        }
                        if (
                            possibleNearbyMeleeChampion.GetWaypoints()
                                .LastOrDefault()
                                .Distance(ObjectManager.Player.ServerPosition) < 1000)
                        {
                            E.CastOnUnit(possibleNearbyMeleeChampion);
                        }
                    }
                }
            }
        }

        #endregion

        #region Menu

        private Menu ComboMenu;
        private Menu HarassMenu;
        private Menu FarmMenu;
        private Menu DrawMenu;
        private Menu CondemnMenu;
        private MenuBool UseQBool;
        private MenuList<string> QModeStringList;
        private MenuBool TryToFocus2WBool;
        private MenuBool UseEBool;
        private MenuSlider EDelaySlider;
        private MenuSlider EPushDistanceSlider;
        private MenuSlider EHitchanceSlider;
        private MenuList<string> EModeStringList;
        private MenuBool UseEInterruptBool;
        private MenuBool UseEAntiGapcloserBool;
        private MenuBool UseEWhenMeleesNearBool;
        private MenuBool UseEAs3rdWProcBool;
        private MenuBool DontAttackWhileInvisibleAndMeelesNearBool;
        private MenuBool UseRBool;
        private MenuBool UseQBonusOnEnemiesNotCS;
        private MenuBool UseQFarm;
        private MenuBool UseEJungleFarm;
        private MenuKeyBind SemiAutomaticCondemnKey;
        private MenuBool DrawEnemyWaypointsBool;

        private void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("combomenu", "Combo Settings: "));
            CondemnMenu = ComboMenu.Add(new Menu("condemnmenu", "Condemn Settings: "));
            HarassMenu = MainMenu.Add(new Menu("harassmenu", "Harass Settings: "));
            FarmMenu = MainMenu.Add(new Menu("farmmenu", "Farm Settings: "));
            DrawMenu = MainMenu.Add(new Menu("drawmenu", "Drawing Settings: "));
            UseQBool = ComboMenu.Add(new MenuBool("useq", "Auto Q", true));
            QModeStringList =
                ComboMenu.Add(new MenuList<string>("qmode", "Q Mode: ",
                    new[] {"PRADA", "MARKSMAN", "VHR", "SharpShooter"}));
            TryToFocus2WBool = ComboMenu.Add(new MenuBool("focus2w", "Try To Focus 2W", false));
            UseEBool = CondemnMenu.Add(new MenuBool("usee", "Auto E", true));
            EDelaySlider = CondemnMenu.Add(new MenuSlider("edelay", "E Delay: ", 0, 0, 100));
            EModeStringList =
                CondemnMenu.Add(new MenuList<string>("emode", "E Mode: ",
                    new[]
                    {
                        "PRADASMART", "PRADAPERFECT", "MARKSMAN", "SHARPSHOOTER", "GOSU", "VHR", "PRADALEGACY",
                        "FASTEST",
                        "OLDPRADA"
                    }));
            UseEInterruptBool = CondemnMenu.Add(new MenuBool("useeinterrupt", "Use E To Interrupt", true));
            UseEAntiGapcloserBool = CondemnMenu.Add(new MenuBool("useeantigapcloser", "Use E AntiGapcloser", true));
            UseEWhenMeleesNearBool = CondemnMenu.Add(new MenuBool("useewhenmeleesnear", "Use E when Melee near", true));
            EPushDistanceSlider = CondemnMenu.Add(new MenuSlider("epushdist", "E Push Distance: ", 450, 300, 475));
            EHitchanceSlider = CondemnMenu.Add(new MenuSlider("ehitchance", "Condemn Hitchance", 50, 0, 100));
            SemiAutomaticCondemnKey =
                CondemnMenu.Add(new MenuKeyBind("semiautoekey", "Semi Automatic Condemn", Keys.E, KeyBindType.Press));
            DontAttackWhileInvisibleAndMeelesNearBool =
                ComboMenu.Add(new MenuBool("dontattackwhileinvisible", "Smart Invisible Attacking", true));
            UseRBool = ComboMenu.Add(new MenuBool("user", "Use R In Combo", false));
            UseEAs3rdWProcBool =
                HarassMenu.Add(new MenuBool("usee3rdwproc", "Use E as 3rd W Proc Before LVL: ", true));
            UseQBonusOnEnemiesNotCS =
                HarassMenu.Add(new MenuBool("useqonenemiesnotcs", "Use Q Bonus On ENEMY not CS", false));
            UseQFarm = FarmMenu.Add(new MenuBool("useqfarm", "Use Q"));
            UseEJungleFarm = FarmMenu.Add(new MenuBool("useejgfarm", "Use E Jungle", true));
            DrawEnemyWaypointsBool = DrawMenu.Add(new MenuBool("drawenemywaypoints", "Draw Enemy Waypoints", true));
            MainMenu.Attach();
        }

        #endregion Menu

        #region ChampionLogic

        public bool IsCondemnable(Obj_AI_Hero hero)
        {
            if (!hero.IsValidTarget(550f) || hero.HasBuffOfType(BuffType.SpellShield) ||
                hero.HasBuffOfType(BuffType.SpellImmunity) || hero.IsDashing()) return false;

            //values for pred calc pP = player position; p = enemy position; pD = push distance
            var pP = ObjectManager.Player.ServerPosition;
            var p = hero.ServerPosition;
            var pD = EPushDistanceSlider.Value;
            var mode = EModeStringList.SelectedValue;


            if (mode == "PRADASMART" && (IsCollisionable(p.Extend(pP, -pD)) || IsCollisionable(p.Extend(pP, -pD/2f)) ||
                                         IsCollisionable(p.Extend(pP, -pD/3f))))
            {
                if (!hero.CanMove ||
                    (hero.IsWindingUp))
                    return true;

                var enemiesCount = ObjectManager.Player.CountEnemyHeroesInRange(1200);
                if (enemiesCount > 1 && enemiesCount <= 3)
                {
                    var prediction = E.GetPrediction(hero);
                    for (var i = 15; i < pD; i += 75)
                    {
                        var posFlags = NavMesh.GetCollisionFlags(
                            prediction.UnitPosition.ToVector2()
                                .Extend(
                                    pP.ToVector2(),
                                    -i)
                                .ToVector3());
                        if (posFlags.HasFlag(CollisionFlags.Wall) || posFlags.HasFlag(CollisionFlags.Building))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    var hitchance = EHitchanceSlider.Value;
                    var angle = 0.20*hitchance;
                    const float travelDistance = 0.5f;
                    var alpha = new Vector2((float) (p.X + travelDistance*Math.Cos(Math.PI/180*angle)),
                        (float) (p.X + travelDistance*Math.Sin(Math.PI/180*angle)));
                    var beta = new Vector2((float) (p.X - travelDistance*Math.Cos(Math.PI/180*angle)),
                        (float) (p.X - travelDistance*Math.Sin(Math.PI/180*angle)));

                    for (var i = 15; i < pD; i += 100)
                    {
                        if (IsCollisionable(pP.ToVector2().Extend(alpha,
                            i)
                            .ToVector3()) && IsCollisionable(pP.ToVector2().Extend(beta, i).ToVector3())) return true;
                    }
                    return false;
                }
            }

            if (mode == "PRADAPERFECT" &&
                (IsCollisionable(p.Extend(pP, -pD)) || IsCollisionable(p.Extend(pP, -pD/2f)) ||
                 IsCollisionable(p.Extend(pP, -pD/3f))))
            {
                if (!hero.CanMove ||
                    (hero.IsWindingUp))
                    return true;

                var hitchance = EHitchanceSlider.Value;
                var angle = 0.20*hitchance;
                const float travelDistance = 0.5f;
                var alpha = new Vector2((float) (p.X + travelDistance*Math.Cos(Math.PI/180*angle)),
                    (float) (p.X + travelDistance*Math.Sin(Math.PI/180*angle)));
                var beta = new Vector2((float) (p.X - travelDistance*Math.Cos(Math.PI/180*angle)),
                    (float) (p.X - travelDistance*Math.Sin(Math.PI/180*angle)));

                for (var i = 15; i < pD; i += 100)
                {
                    if (IsCollisionable(pP.ToVector2().Extend(alpha,
                        i)
                        .ToVector3()) && IsCollisionable(pP.ToVector2().Extend(beta, i).ToVector3())) return true;
                }
                return false;
            }

            if (mode == "OLDPRADA")
            {
                if (!hero.CanMove ||
                    (hero.IsWindingUp))
                    return true;

                var hitchance = EHitchanceSlider.Value;
                var angle = 0.20*hitchance;
                const float travelDistance = 0.5f;
                var alpha = new Vector2((float) (p.X + travelDistance*Math.Cos(Math.PI/180*angle)),
                    (float) (p.X + travelDistance*Math.Sin(Math.PI/180*angle)));
                var beta = new Vector2((float) (p.X - travelDistance*Math.Cos(Math.PI/180*angle)),
                    (float) (p.X - travelDistance*Math.Sin(Math.PI/180*angle)));

                for (var i = 15; i < pD; i += 100)
                {
                    if (IsCollisionable(pP.ToVector2().Extend(alpha,
                        i)
                        .ToVector3()) || IsCollisionable(pP.ToVector2().Extend(beta, i).ToVector3())) return true;
                }
                return false;
            }

            if (mode == "MARKSMAN")
            {
                var prediction = E.GetPrediction(hero);
                return NavMesh.GetCollisionFlags(
                    prediction.UnitPosition.ToVector2()
                        .Extend(
                            pP.ToVector2(),
                            -pD)
                        .ToVector3()).HasFlag(CollisionFlags.Wall) ||
                       NavMesh.GetCollisionFlags(
                           prediction.UnitPosition.ToVector2()
                               .Extend(
                                   pP.ToVector2(),
                                   -pD/2f)
                               .ToVector3()).HasFlag(CollisionFlags.Wall);
            }

            if (mode == "SHARPSHOOTER")
            {
                var prediction = E.GetPrediction(hero);
                for (var i = 15; i < pD; i += 100)
                {
                    var posCF = NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.ToVector2()
                            .Extend(
                                pP.ToVector2(),
                                -i)
                            .ToVector3());
                    if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                    {
                        return true;
                    }
                }
                return false;
            }

            if (mode == "GOSU")
            {
                var prediction = E.GetPrediction(hero);
                for (var i = 15; i < pD; i += 75)
                {
                    var posCF = NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.ToVector2()
                            .Extend(
                                pP.ToVector2(),
                                -i)
                            .ToVector3());
                    if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                    {
                        return true;
                    }
                }
                return false;
            }

            if (mode == "VHR")
            {
                var prediction = E.GetPrediction(hero);
                for (var i = 15; i < pD; i += (int) hero.BoundingRadius) //:frosty:
                {
                    var posCF = NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.ToVector2()
                            .Extend(
                                pP.ToVector2(),
                                -i)
                            .ToVector3());
                    if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                    {
                        return true;
                    }
                }
                return false;
            }

            if (mode == "PRADALEGACY")
            {
                var prediction = E.GetPrediction(hero);
                for (var i = 15; i < pD; i += 75)
                {
                    var posCF = NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.ToVector2()
                            .Extend(
                                pP.ToVector2(),
                                -i)
                            .ToVector3());
                    if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                    {
                        return true;
                    }
                }
                return false;
            }

            if (mode == "FASTEST" &&
                (IsCollisionable(p.Extend(pP, -pD)) || IsCollisionable(p.Extend(pP, -pD/2f)) ||
                 IsCollisionable(p.Extend(pP, -pD/3f))))
            {
                return true;
            }

            return false;
        }

        public Vector3 GetAggressiveTumblePos(Obj_AI_Base target)
        {
            var cursorPos = Game.CursorPos;

            if (!IsDangerousPosition(cursorPos)) return cursorPos;
            //if the target is not a melee and he's alone he's not really a danger to us, proceed to 1v1 him :^ )
            if (!target.IsMelee && ObjectManager.Player.CountEnemyHeroesInRange(800) == 1) return cursorPos;

            var aRC =
                new Geometry.Circle(ObjectManager.Player.ServerPosition.ToVector2(), 300).ToPolygon().ToClipperPath();
            var targetPosition = target.ServerPosition;


            foreach (var p in aRC)
            {
                var v3 = new Vector2(p.X, p.Y).ToVector3();
                var dist = v3.Distance(targetPosition);
                if (dist > 325 && dist < 450)
                {
                    return v3;
                }
            }
            return Vector3.Zero;
        }

        public Vector3 GetTumblePos(Obj_AI_Base target)
        {
            if (Orbwalker.ActiveMode != OrbwalkingMode.Combo)
                return GetAggressiveTumblePos(target);

            var cursorPos = Game.CursorPos;
            var targetCrowdControl = from tuplet in CachedCrowdControl
                                                   where tuplet.Item1 == target.CharData.BaseSkinName
                                                   select tuplet.Item2;

            if (!IsDangerousPosition(cursorPos) && !(targetCrowdControl.FirstOrDefault() != null && targetCrowdControl.Any(
                        crowdControlEntry =>
                            target.Spellbook.GetSpell(crowdControlEntry.Slot).IsReady()))) return cursorPos;

            //if the target is not a melee and he's alone he's not really a danger to us, proceed to 1v1 him :^ )
            if (!target.IsMelee && ObjectManager.Player.CountEnemyHeroesInRange(800) == 1) return cursorPos;
            var targetWaypoints = MathUtils.GetWaypoints(target);
            if (targetWaypoints[targetWaypoints.Count - 1].Distance(ObjectManager.Player.ServerPosition) > 550)
                return Vector3.Zero;

            var aRC =
                new Geometry.Circle(ObjectManager.Player.ServerPosition.ToVector2(), 300).ToPolygon().ToClipperPath();
            var targetPosition = target.ServerPosition;
            var pList = (from p in aRC
                select new Vector2(p.X, p.Y).ToVector3()
                into v3
                let dist = v3.Distance(targetPosition)
                where !IsDangerousPosition(v3) && dist < 500
                select v3).ToList();

            if (ObjectManager.Player.UnderTurret() || ObjectManager.Player.CountEnemyHeroesInRange(800) == 1 ||
                cursorPos.CountEnemyHeroesInRange(450) <= 1)
            {
                return pList.Count > 1 ? pList.OrderBy(el => el.Distance(cursorPos)).FirstOrDefault() : Vector3.Zero;
            }
            return pList.Count > 1
                ? pList.OrderByDescending(el => el.Distance(cursorPos)).FirstOrDefault()
                : Vector3.Zero;
        }

        public static int VayneWStacks(Obj_AI_Base o)
        {
            if (o == null) return 0;
            if (o.Buffs.FirstOrDefault(b => b.Name.Contains("vaynesilver")) == null ||
                !o.Buffs.Any(b => b.Name.Contains("vaynesilver"))) return 0;
            return o.Buffs.FirstOrDefault(b => b.Name.Contains("vaynesilver")).Count;
        }

        public static Vector3 Randomize(Vector3 pos)
        {
            var r = new Random(Environment.TickCount);
            return new Vector2(pos.X + r.Next(-150, 150), pos.Y + r.Next(-150, 150)).ToVector3();
        }

        public static bool IsDangerousPosition(Vector3 pos)
        {
            return GameObjects.EnemyHeroes.Any(
                e => e.IsValidTarget() && e.IsVisible &&
                     (e.Distance(pos) < 375) && (e.GetWaypoints().LastOrDefault().Distance(pos) > 550)) ||
                     (pos.UnderTurret(true) && !ObjectManager.Player.UnderTurret(true)) || pos.IsWall();
        }

        public static bool IsKillable(Obj_AI_Hero hero)
        {
            return ObjectManager.Player.GetAutoAttackDamage(hero)*2 < hero.Health;
        }

        public static bool IsCollisionable(Vector3 pos)
        {
            return NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall) ||
                   (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Building));
        }

        public static bool IsValidState(Obj_AI_Hero target)
        {
            return !target.HasBuffOfType(BuffType.SpellShield) && !target.HasBuffOfType(BuffType.SpellImmunity) &&
                   !target.HasBuffOfType(BuffType.Invulnerability);
        }

        public static int CountHerosInRange(Obj_AI_Hero target, bool checkteam, float range = 1200f)
        {
            var objListTeam =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        x => x.IsValidTarget(range, false));

            return objListTeam.Count(hero => checkteam ? hero.Team != target.Team : hero.Team == target.Team);
        }

        #endregion
    }
}