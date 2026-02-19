using CommonUtility;

namespace ZoneServerLib
{
    class AnySkilDamageCasterAddBuffTriHdl : BaseTriHdl
    {
        readonly int buffId;

        public AnySkilDamageCasterAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int.TryParse(handlerParam, out buffId);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDamage, out param))
            {
                return;
            }

            SkillDamageTriMsg msg = param as SkillDamageTriMsg;
            if (msg != null)
            {
                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();

                msg.Caster.AddBuff(trigger.Owner, buffId, skillLevelGrowth);
            }
        }
    }
}

