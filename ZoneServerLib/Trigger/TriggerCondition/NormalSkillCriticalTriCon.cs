using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class NormalSkillCriticalTriCon : BaseTriCon
    {
        public NormalSkillCriticalTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.Critical, out param))
            {
                return false;
            }

            CriticalTriMsg msg = param as CriticalTriMsg;
            if (msg == null)
            {
                return false;
            }

            SkillModel model = SkillLibrary.GetSkillModel(msg.Model.Id);
            if (model == null)
            {
                return false;
            }
            return model.IsNormalSkill();
        }
    }
}
