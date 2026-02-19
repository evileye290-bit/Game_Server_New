using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    // 某个类型的技能成功命中target
    public class SkillTypeKillEnemyTriCon : BaseTriCon
    {
        private readonly int skillType;
        public SkillTypeKillEnemyTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillType))
            {
                Log.Warn($"init cast skill kill enemy condition failed: invalid skill type {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.KillEnemy, out param))
            {
                return false;
            }

            KillEnemyTriMsg msg = param as KillEnemyTriMsg;
            if (msg == null)
            {
                return false;
            }

            SkillModel model = SkillLibrary.GetSkillModel(msg.SkillId);
            if(model == null)
            {
                return false;
            }
            return skillType == (int)(model.Type);
        }
    }
}
