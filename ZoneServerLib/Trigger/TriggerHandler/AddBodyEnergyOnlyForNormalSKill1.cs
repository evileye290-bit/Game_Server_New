using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    /// <summary>
    /// 用于主动技能加武魂真身能量，走handler55无法区分是否需要广播前端，此处不广播
    /// </summary>
    public class AddBodyEnergyOnlyForNormalSKill1 : BaseTriHdl
    {
        int energy = 0;
        public AddBodyEnergyOnlyForNormalSKill1(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            var param = StringSplit.ParseToFloatPair(handlerParam);
            int skillLevel = trigger.GetFixedParam_SkillLevel();

            energy = (int)trigger.CalcParam(TriggerHandlerType.AddBodyEnergy, param, skillLevel);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            Log.Debug("hero instanceId {0} AddBodyEnergyTrlHdl done!", Owner.InstanceId);
            if (Owner.SkillManager.HasBodySkill)
            {
                if (!Owner.IsDead && !Owner.InRealBody)
                {
                    Owner.SkillManager.AddBodyEnergy(energy);
                }
            }
            else
            {
               trigger.Stop();
            }
        }
    }
}
