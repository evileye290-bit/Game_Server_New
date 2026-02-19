using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class CureBySkillDamageTriHdl : BaseTriHdl
    {
        readonly int skillId, ratio;

        public CureBySkillDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 2)
            {
                Log.WarnLine($"CureBySkillDamageTriHdl param error need params leng 2, current param {handlerParam}");
                return;
            }

            if (!int.TryParse(paramArr[0], out skillId) || !int.TryParse(paramArr[1], out ratio))
            {
                Log.WarnLine($"CureBySkillDamageTriHdl param error need params leng 2, current param {handlerParam}");
                return;
            }

            ratio = trigger.CalcParam(TriggerHandlerType.CureBySkillDamage, ratio);
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.BuildOneSkillDamageKey(skillId), out param))
            {
                return;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg != null)
            {
                int hp = (int)(ratio * 0.0001f * msg.Damage);

                Owner.AddHp(Owner, hp);
            }
        }
    }
}
