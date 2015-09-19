using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRAVEN_Draven.MyUtils;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace DRAVEN_Draven.MyLogic.Q
{
    public static class DravenDecision
    {
        public static int QBuffCount, QReticleCount, QTotalCount = 0;
        public static Vector3 BestReticle;

        public static Vector3 WhichAxeShouldITake(Obj_AI_Base target = null)
        {
           
            switch (QReticleCount)
            {
                case 0:
                    return target.IsValidTarget() ? target.GetPositioning() : Game.CursorPos;
                default:
                    return BestReticle.Randomize(-25, 25);
            }
        }

        public static void OnUpdate(EventArgs args)
        {
            var qBuffTotal = ObjectManager.Player.Buffs.FirstOrDefault(b => b.Name == "dravenspinningattack");
            var qBuffLeft = ObjectManager.Player.Buffs.FirstOrDefault(b => b.Name == "dravenspinningleft");
            QBuffCount = qBuffTotal != null ? qBuffTotal.Count : 0;
            QReticleCount = Draven.QReticles.Count;
            QTotalCount = QBuffCount + QReticleCount;

            if (qBuffLeft != null && qBuffLeft.Count == 1)
            {
                Draven.QReticles.Clear();
            }
            if (QReticleCount == 1)
            {
                BestReticle = Draven.QReticles.FirstOrDefault().Value.Position;
            }
            if (QReticleCount == 2 && QBuffCount == 0)
            {
                BestReticle = Draven.QReticles.OrderBy(ret => ret.Key).FirstOrDefault().Value.Position;
            }
            if (QReticleCount == 2 && QBuffCount == 1)
            {
                BestReticle = Draven.QReticles.OrderByDescending(ret => ret.Key).FirstOrDefault().Value.Position;
            }
            if (QReticleCount >= 2 && QBuffCount >= 2)
            {
                Draven.QReticles.Clear();
            }
        }
    }
}
