using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAI.SummonersRift.Logic;
using LeagueSharp;
using TreeSharp;

namespace SharpAI.SummonersRift
{
    public static class Tree
    {
        private static Composite _root = new PrioritySelector(Recall.BehaviorComposite, PickALane.BehaviorComposite, WalkToLane.BehaviorComposite, Teamfight.BehaviorComposite, Fight.BehaviorComposite, Objectives.BehaviorComposite, PrivillegeCheck.BehaviorComposite, Push.BehaviorComposite, Freeze.BehaviorComposite, FailSafe.BehaviorComposite);

        public static void Seed(string[] args = null)
        {
            _root.Start(args);
        }
        public static void Water(string[] args = null)
        {
            _root.Start(args);
            _root.Tick(args);
        }
    }
}
