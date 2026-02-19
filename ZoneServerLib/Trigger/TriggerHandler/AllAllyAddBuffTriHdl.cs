using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class AllAllyAddBuffTriHdl : BaseTriHdl
    {
        private readonly int buffId = 0;
        public AllAllyAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out buffId))
            {
                Log.Warn("init AllAllyAddBuffTriHdl failed, invalid handler param {0}", handlerParam);
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
                    kv.Value.AddBuff(Owner, buffId, trigger.GetFixedParam_SkillLevelGrowth());
                }
            }       
        }
    }
}
