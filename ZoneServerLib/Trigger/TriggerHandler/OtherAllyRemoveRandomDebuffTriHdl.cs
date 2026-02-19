using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;

namespace ZoneServerLib
{
    public class OtherAllyRemoveRandomDebuffTriHdl : BaseTriHdl
    {
        public OtherAllyRemoveRandomDebuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam) 
            : base(trigger, handlerType, handlerParam)
        {
        }
        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            foreach (var hero in Owner.CurrentMap.HeroList)
            {
                if (hero.Value.IsAlly(Owner) && hero.Value != Owner)
                {
                    hero.Value.RemoveRandomDebuff();
                }
            }
        }

    }
}
