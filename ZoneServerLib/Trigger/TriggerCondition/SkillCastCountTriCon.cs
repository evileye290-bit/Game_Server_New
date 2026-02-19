using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class SkillCastCountTriCon : BaseTriCon
    {
        private int skillId;
        private int count;
        public SkillCastCountTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] info = conditionParam.Split(':');
            if (info.Length != 2 || !int.TryParse(info[0], out skillId) || !int.TryParse(info[1], out count))
            {
                Log.Warn($"init SkillCastCountTriCon failed: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {    
            int castCount = trigger.GetParam_SkillCastCount(skillId);
            if (castCount < count)
            {
                return false;
            }
            else
            {
                trigger.RecordParam(TriggerParamKey.BuildSkillCastCount(skillId), 0);
                return true;
            }          
        }
    }
}
