using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class AnySkilDoDamageBeforeAddDamageOnecTriHdl : BaseTriHdl
    {
        readonly float growthFactor;//成长系数
        readonly int baseCure;//成长治疗基础

        public AnySkilDoDamageBeforeAddDamageOnecTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2)
            {
                Log.WarnLine($"AnySkilDoDamageBeforeAddDamageOnec param error need params leng 2, current param {handlerParam}");
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
            if (msg != null && msg.FieldObject != null)
            {
                if (ThisFpsHadHandled(msg.FieldObject, msg.SkillId)) return;

                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                int cureValue = (int)trigger.CalcParam(growthFactor, baseCure, skillLevelGrowth);
                msg.FieldObject.AddNatureBaseValue(NatureType.PRO_ADD_DMG_ONCE, cureValue);

                SetThisFspHandled(msg.FieldObject, msg.SkillId);
            }

        }
    }
}

