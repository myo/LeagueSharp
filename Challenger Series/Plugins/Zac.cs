using System;
using System.Collections.Generic;
using System.Linq;
using Challenger_Series.Utils;
using LeagueSharp;
using LeagueSharp.SDK;
using SharpDX;
using Color = System.Drawing.Color;
using Challenger_Series.Utils;
using System.Windows.Forms;
using LeagueSharp.Data.Enumerations;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using Menu = LeagueSharp.SDK.UI.Menu;

namespace Challenger_Series.Plugins
{
    public class Zac : CSPlugin
    {
        public Zac()
        {
            base.Q = new Spell(SpellSlot.Q);
            base.W = new Spell(SpellSlot.W, 1100);
            base.W.SetSkillshot(250f, 75f, 1500f, true, SkillshotType.SkillshotLine);
            base.E = new Spell(SpellSlot.E, 25000);
            base.R = new Spell(SpellSlot.R, 1400);
            base.R.SetSkillshot(250f, 80f, 1500f, false, SkillshotType.SkillshotLine);
            InitMenu();
            Obj_AI_Hero.OnDoCast += OnDoCast;
            Orbwalker.OnAction += OnAction;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Events.OnGapCloser += EventsOnOnGapCloser;
            Events.OnInterruptableTarget += OnInterruptableTarget;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            this.OnLoadingFinished();
        }

        private int DelayOnUpdate;
        void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name == "BlobDrop")
            {
                this.Blobs.RemoveAll(o => o.NetworkId == sender.NetworkId);
            }
        }
        
        private List<GameObject> Blobs = new List<GameObject>(); 

        void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "BlobDrop" || sender.Name.Contains("Zac_Base_P_Chunk") || sender.Name.Contains("Zac_Base_W_Chunk_Splat"))
            {
                this.Blobs.Add(sender);
                if (sender.Distance(ObjectManager.Player) < 35)
                {
                    this.DelayOnUpdate = Variables.TickCount;
                }
            }
            else
            {
                if (sender.Name.Contains("Zac"))
                {
                    Game.PrintChat(sender.Name);
                }
            }
        }

        private void OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs args)
        {
        }

        private void EventsOnOnGapCloser(object sender, Events.GapCloserEventArgs args)
        {
        }

        public override void OnDraw(EventArgs args)
        {
            foreach (var obj in this.Blobs)
            {
                Render.Circle.DrawCircle(obj.Position,
                    150,
                    Color.Red);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            Orbwalker.SetMovementState(W.IsReady() || !this.Blobs.Any());
            Orbwalker.SetAttackState(!this.Blobs.Any());
            if (W.IsReady() && (GameObjects.EnemyHeroes.Any(h=> h.IsHPBarRendered && h.Distance(ObjectManager.Player) < 325) || GameObjects.Jungle.Any(mob => mob.IsHPBarRendered && mob.Distance(ObjectManager.Player) < 325)))
            {
                W.Cast();
                return;
            }
            if (Variables.TickCount - this.DelayOnUpdate < 250)
            {
                Game.PrintChat("Delayed");
                return;
            }
            if (this.Blobs.Any())
            {
                var targetBlob = Blobs.OrderBy(b => b.Position.Distance(ObjectManager.Player.Position)).FirstOrDefault();
                if (targetBlob == null) return;
                if (targetBlob.Distance(ObjectManager.Player) > 35)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, targetBlob.Position.Randomize(-15, 15));
                }
                else
                {
                    DelayAction.Add(ObjectManager.Player.Distance(targetBlob.Position)/335,
                        () =>
                            {
                                this.Blobs.RemoveAll(b => b.NetworkId == targetBlob.NetworkId);
                            });
                }
            }
        }

        private void OnAction(object sender, OrbwalkingActionArgs orbwalkingActionArgs)
        {
        }

        private void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
        }

        private Menu ComboMenu;
        private MenuBool UseWCombo;
        public void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("Zaccombomenu", "Combo Settings: "));
            UseWCombo = ComboMenu.Add(new MenuBool("Zacwcombo", "Use W", true));
            MainMenu.Attach();
        }

    }
}
