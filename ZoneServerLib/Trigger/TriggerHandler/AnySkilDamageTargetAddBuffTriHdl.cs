using CommonUtility;

namespace ZoneServerLib
{
    class AnySkilDamageTargetAddBuffTriHdl : BaseTriHdl
    {
        readonly int buffId;
        public AnySkilDamageTargetAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam) 
            : base(trigger, handlerType, handlerParam)
        {
            buffId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param) &&
                !trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg != null)
            {
                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                msg.FieldObject.AddBuff(Owner, buffId, skillLevelGrowth);
            }
        }
    }
}

