using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class AddTrigger4AllyTriHdl:BaseTriHdl
    {
        private readonly int triggerId = 0;
        public AddTrigger4AllyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out triggerId))
            {
                Log.Warn("init add trigger tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            foreach (var kv in Owner.CurDungeon.HeroList)
            {
                if (kv.Value.IsAlly(Owner))
                {
                    kv.Value.AddTriggerCreatedBySkill(triggerId, 1, Owner);
                }
            }
        }
    }
}
