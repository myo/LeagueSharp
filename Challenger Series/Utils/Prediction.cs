using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK;
using LeagueSharp.SDK.UI;
using SharpDX;
using HitChance = LeagueSharp.SDK.Enumerations.HitChance;
using PredictionInput = LeagueSharp.Common.PredictionInput;
using SkillshotType = LeagueSharp.SDK.Enumerations.SkillshotType;
using Spell = LeagueSharp.SDK.Spell;

namespace Challenger_Series.Utils
{
    public static class Prediction
    {
        public static MenuList<string> PredictionMode;
        public static Tuple<LeagueSharp.SDK.Enumerations.HitChance, Vector3, List<Obj_AI_Base>> GetPrediction(Obj_AI_Hero target, Spell spell)
        {
            switch (Utils.Prediction.PredictionMode.SelectedValue)
            {
                case "SDK":
                    {
                        var pred = spell.GetPrediction(target);
                        return new Tuple<LeagueSharp.SDK.Enumerations.HitChance, Vector3, List<Obj_AI_Base>>(pred.Hitchance, pred.UnitPosition, pred.CollisionObjects);
                    }
                default:
                    {

                        var pred = LeagueSharp.Common.Prediction.GetPrediction(target, spell.Delay, spell.Width, spell.Speed);
                        return new Tuple<LeagueSharp.SDK.Enumerations.HitChance, Vector3, List<Obj_AI_Base>>((HitChance)((int)pred.Hitchance), pred.UnitPosition, pred.CollisionObjects);
                    }
            }
        }

        public static LeagueSharp.Common.SkillshotType GetCommonSkillshotType(LeagueSharp.SDK.Enumerations.SkillshotType sdkType)
        {
            switch (sdkType)
            {
                case SkillshotType.SkillshotCircle:
                    return LeagueSharp.Common.SkillshotType.SkillshotCircle;
                case SkillshotType.SkillshotCone:
                    return LeagueSharp.Common.SkillshotType.SkillshotCone;
                case SkillshotType.SkillshotLine:
                    return LeagueSharp.Common.SkillshotType.SkillshotLine;
                default:
                    return LeagueSharp.Common.SkillshotType.SkillshotLine;
            }
        }
    }
}
