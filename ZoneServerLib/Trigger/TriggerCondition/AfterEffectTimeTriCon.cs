using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class AfterEffectTimeTriCon : BaseTriCon
    {
        readonly float triggerTime = 0;
        float elapsedTime = 0;
        bool firstEffect = true;
        public AfterEffectTimeTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            triggerTime = float.Parse(conditionParam);
        }

        public override void Update(float dt)
        {
            if (ready)
            {
                return;
            }
            elapsedTime += dt;
            if (elapsedTime >= triggerTime)
            {
                ready = true;
            }
        }

        public override void Reset()
        {
            base.Reset();
            elapsedTime = 0;
        }
        public override bool Check()
        {
            if (firstEffect)
            {
                firstEffect = false;
                return true;
            }
            return ready;
        }
    }
}
