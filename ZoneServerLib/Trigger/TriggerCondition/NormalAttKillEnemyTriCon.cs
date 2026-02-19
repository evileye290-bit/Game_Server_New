using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    // 普攻击杀目标
    public class NormalAttKillEnemyTriCon : BaseTriCon
    {
        public NormalAttKillEnemyTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
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
            return model.IsNormalAttack();
        }
    }
}
