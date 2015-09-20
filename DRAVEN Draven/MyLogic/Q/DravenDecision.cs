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

        public static Vector3 WhichAxeShouldITake(Obj_AI_Base target = null)
        {
            var qBuffTotal = ObjectManager.Player.Buffs.FirstOrDefault(b => b.Name == "dravenspinningattack");
            var qBuffLeft = ObjectManager.Player.Buffs.FirstOrDefault(b => b.Name == "dravenspinningleft");
            QBuffCount = qBuffTotal != null ? qBuffTotal.Count : 0;
            QReticleCount = Draven.QReticles.Count;
            QTotalCount = QBuffCount + QReticleCount;
            switch (QReticleCount)
            {
                case 0:
                    return target != null ? target.GetPositioning() : Game.CursorPos;
                case 1:
                    return Draven.QReticles.FirstOrDefault().Value.Position.Randomize(-25, 25);
                default:
                    return Draven.QReticles.OrderBy(entry => entry.Key).FirstOrDefault().Value.Position.Randomize(-25, 25);
            }
        }
    }
}
