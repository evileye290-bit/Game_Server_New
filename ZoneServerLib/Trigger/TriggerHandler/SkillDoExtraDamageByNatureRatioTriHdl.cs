using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class SkillDoExtraDamageByNatureRatioTriHdl : BaseTriHdl
    {
        private readonly int skillId;
        private readonly NatureType natureType;
        private readonly float ratio;

        public SkillDoExtraDamageByNatureRatioTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            List<int> param = handlerParam.ToList(':');
            if (param.Count != 3)
            {
                Log.Warn("init skill SkillDoExtraDamageByNatureRatioTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            skillId = param[0];
            natureType = (NatureType)param[1];
            ratio = param[2] * 0.0001f;
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg?.SkillId == skillId)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled(msg.FieldObject, msg.SkillId)) return;

                long damage = (long)(Owner.GetNatureValue(natureType) * ratio);
                msg.FieldObject?.DoSpecDamage(Owner, DamageType.Extra, damage);

                SetThisFspHandled(msg.FieldObject, msg.SkillId);
            }
        }
    }
}
