﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAI.Enums;
using SharpAI.SummonersRift.Data;
using SharpAI.Utility;
using LeagueSharp;
using LeagueSharp.SDK;
using TreeSharp;
using Action = TreeSharp.Action;

namespace SharpAI.SummonersRift.Logic
{
    public static class PickALane
    {
        static bool ShouldTakeAction()
        {
            return SessionBasedData.MyLane == Lane.Unknown || (Environment.TickCount - SessionBasedData.LoadTick < 105000 && ObjectManager.Get<Obj_AI_Hero>().Count(h=>h.IsAlly && !h.IsDead && h.Position.IsInside(SessionBasedData.MyLanePolygon)) > 0);
        }

        static Action TakeAction()
        {
            return new Action(a =>
            {
                Logging.Log("SWITCHED MODE TO PICKALANE");
                SessionBasedData.MyLane = StaticData.ChooseBestLane();
                SessionBasedData.CurrentLane = SessionBasedData.MyLane;
            });
        }
        
        public static Composite BehaviorComposite => new Decorator(t=>ShouldTakeAction(), TakeAction());
    }
}