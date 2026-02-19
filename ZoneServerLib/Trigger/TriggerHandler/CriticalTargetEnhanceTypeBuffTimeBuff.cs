using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class CriticalTargetEnhanceTypeBuffTimeBuff : BaseTriHdl
    {
        private readonly BuffType buffType;
        private readonly float time;
        public CriticalTargetEnhanceTypeBuffTimeBuff(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int buff = 0;
            string[] param = handlerParam.Split(':');
            if (param.Length!=2 || !float.TryParse(param[1], out time) || !int.TryParse(param[0], out buff))
            {
                Log.Error($"CriticalTargetEnhanceTypeBuffTimeBuff error, handlerParam {handlerParam}");
                return;
            }

            buffType = (BuffType)buff;
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.Critical, out param))
            {
                return;
            }
            CriticalTriMsg msg = param as CriticalTriMsg;
            if (msg == null || msg.Target == null || msg.Model == null)
            {
                return;
            }
            Skill skill = Owner.SkillManager.GetSkill(msg.Model.Id);
            if (skill == null)
            {
                return;
            }

            msg.Target.BuffManager.EnhanceTypefBuffTime(buffType, time);

            SetThisFspHandled();
        }
    }
}

