using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class EnhanceCastedBuffParamCTriHdl : BaseTriHdl
    {
        private readonly int buffId = 0;
        private readonly int num;
        public EnhanceCastedBuffParamCTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 2)
            {
                Log.Warn("init EnhanceCastedBuffParamCTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            if (!int.TryParse(paramArr[0], out buffId) || !int.TryParse(paramArr[1], out num))
            {
                Log.Warn("init EnhanceCastedBuffParamCTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.BuildCastBuffKey(buffId), out param))
            {
                return;
            }
            BaseBuff buff = param as BaseBuff;
            if (buff == null || buff.Id != buffId)
            {
                return;
            }

            buff.AddParamC(num * 1f);
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
