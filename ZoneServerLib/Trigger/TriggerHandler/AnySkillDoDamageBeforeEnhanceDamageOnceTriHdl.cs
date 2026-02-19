using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class AnySkillDoDamageBeforeEnhanceDamageOnceTriHdl : BaseTriHdl
    {
        readonly float growthFactor;//成长系数
        readonly int baseCure;//成长治疗基础

        public AnySkillDoDamageBeforeEnhanceDamageOnceTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2)
            {
                Log.WarnLine($"AnySkillDoDamageBeforeEnhanceDamageOnceTriHdl param error need params length 2, current param {handlerParam}");
            }
            else
            {
                growthFactor = float.Parse(param[0]);
                baseCure = int.Parse(param[1]);
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg != null)
            {
                if (ThisFpsHadHandled(Owner, msg.SkillId)) return;

                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                int cureValue = (int)trigger.CalcParam(growthFactor, baseCure, skillLevelGrowth);
                Owner.AddNatureBaseValue(NatureType.PRO_DAM_ENHANCE_RATIO_ONCE, cureValue);

                SetThisFspHandled(Owner, msg.SkillId);
            }
        }
    }
}