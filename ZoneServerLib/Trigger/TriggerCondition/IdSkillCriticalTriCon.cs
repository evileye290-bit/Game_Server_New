using CommonUtility;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class IdSkillCriticalTriCon : BaseTriCon
    {
        private int skillId;

        public IdSkillCriticalTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            skillId = conditionParam.ToInt();
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
            return model?.Id == skillId;
        }
    }
}
