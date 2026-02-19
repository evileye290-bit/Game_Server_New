using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class TypeSkillDamageBeforeTriCon : BaseTriCon
    {
        private readonly int skillType;
        public TypeSkillDamageBeforeTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillType))
            {
                Log.Warn($"init TypeSkillDamageBeforeTriCon condition failed: invalid skill type {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg == null)
            {
                return false;
            }

            SkillModel model = SkillLibrary.GetSkillModel(msg.SkillId);
            if (model == null)
            {
                return false;
            }
            return skillType == (int)(model.Type);
        }
    }
}

