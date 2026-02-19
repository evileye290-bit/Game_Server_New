using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class AnySkilDoDamageCureSelfTriHdl : BaseTriHdl
    {
        readonly int growthFactor;//成长系数
        readonly int baseCure;//成长治疗基础

        public AnySkilDoDamageCureSelfTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2)
            {
                Log.WarnLine($"AnySkilDoDamageCureSelfTriHdl param error need params leng 2, current param {handlerParam}");
            }
            else
            {
                growthFactor = int.Parse(param[0]);
                baseCure = int.Parse(param[1]);
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg != null)
            {
                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                int hp = (int)(trigger.CalcParam(growthFactor, baseCure, skillLevelGrowth) * 0.0001f * msg.Damage);

                Owner.AddHp(Owner, hp);
            }

            SetThisFspHandled();
        }
    }
}

