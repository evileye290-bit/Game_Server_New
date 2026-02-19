using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ExtendDebuffTimeTriHdl : BaseTriHdl
    {
        private readonly float extendTime;
        public ExtendDebuffTimeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!float.TryParse(handlerParam, out extendTime))
            {
                Log.Warn("init ExtendDebuffTimeTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.CastDeBuff, out param))
            {
                return;
            }

            BaseBuff buff = param as BaseBuff;
            if (buff != null)
            {
                buff.SetBuffDuringTime(buff.S + extendTime);
            }
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
