using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class AnySkilDamageCureSelfTriHdl : BaseTriHdl
    {
        readonly float growthFactor;//成长系数
        readonly int baseCure;//成长治疗基础

        public AnySkilDamageCureSelfTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2)
            {
                Log.WarnLine($"AnySkilDamageCureSelfTriHdl param error need params leng 2, current param {handlerParam}");
            }
            else
            {
                growthFactor = float.Parse(param[0]);
                baseCure = int.Parse(param[1]);
            }
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
                int hp = (int)(trigger.CalcParam(growthFactor, baseCure, skillLevelGrowth) * 0.0001f * msg.Damage);

                trigger.Owner.AddHp(trigger.Caster, hp);
            }
        }
    }
}

