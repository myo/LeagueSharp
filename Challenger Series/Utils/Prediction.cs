using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDKEx;
using LeagueSharp.SDKEx.UI;
using SharpDX;
using PredictionInput = LeagueSharp.Common.PredictionInput;
using SkillshotType = LeagueSharp.SDKEx.Enumerations.SkillshotType;
using Spell = LeagueSharp.SDKEx.Spell;

namespace Challenger_Series.Utils
{
    public static class Prediction
    {
        public static MenuList<string> PredictionMode;
        public static object GetPrediction(this Spell spell, Obj_AI_Base target)
        {
            switch (PredictionMode.SelectedValue)
            {
                case "SDKEx":
                {
                    return spell.GetPrediction(target);
                }
                case "Common":
                {
                        var commonSpell = new LeagueSharp.Common.Spell(spell.Slot, spell.Range);
                        commonSpell.SetSkillshot(spell.Delay, spell.Width, spell.Speed, spell.Collision, GetCommonSkillshotType(spell.Type));
                    return commonSpell.GetPrediction(target);
                }
                default:
                {
                    return spell.GetPrediction(target);
                }
            }
        }

        public static LeagueSharp.Common.SkillshotType GetCommonSkillshotType(LeagueSharp.SDKEx.Enumerations.SkillshotType sdkType)
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
