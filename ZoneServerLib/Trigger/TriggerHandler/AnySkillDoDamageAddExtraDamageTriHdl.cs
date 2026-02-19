using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AnySkillDoDamageAddExtraDamageTriHdl : BaseTriHdl
    {
        private readonly float growth = 0;
        private readonly float damage = 0;
        public AnySkillDoDamageAddExtraDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2)
            {
                Log.Warn("init AnySkillDoDamageAddExtraDamageTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }
            if (!float.TryParse(param[0], out growth) || !float.TryParse(param[1], out damage))
            {
                Log.Warn("init AnySkillDoDamageAddExtraDamageTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return;
            }


            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg != null && msg.FieldObject != null)
            {
                if (ThisFpsHadHandled(msg.FieldObject, msg.SkillId)) return;

                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                int finalDamage = (int)trigger.CalcParam(growth, damage, skillLevelGrowth);
                msg.FieldObject.AddNatureBaseValue(NatureType.PRO_EXTRA_DAMAGE_ONCE, finalDamage);

                SetThisFspHandled(msg.FieldObject, msg.SkillId);
            }
        }
    }
}
