using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnSkillTypeHitTriLnr : BaseTriLnr
    {
        public OnSkillTypeHitTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            SkillHitMsg param = message as SkillHitMsg;
            if(param == null )
            {
                return;
            }
            
            trigger.RecordParam(TriggerParamKey.BuildSkillTypeHitKey((int)param.Model.Type), param);

            //技能命中次数
            string key = TriggerParamKey.BuildSkillTypeHitCountKey((int)param.Model.Type);
            object param1;
            if (trigger.TryGetParam(key, out param1))
            {
                int count = 0;
                if (param1 is int)
                {
                    count = (int)param1;
                }
                count++;
                trigger.RecordParam(key, count);
            }
        }

    }
}
