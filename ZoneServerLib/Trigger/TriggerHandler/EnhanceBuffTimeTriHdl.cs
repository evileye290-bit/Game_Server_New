using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class EnhanceBuffTimeTriHdl : BaseTriHdl
    {
        private readonly int buffId;
        private readonly float time;
        public EnhanceBuffTimeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] info = handlerParam.Split('|');
            if (info.Length != 2)
            { 
                Log.Error($"EnhanceBuffTimeTriHdl error, handlerParam {handlerParam}");
                return;
            }

            if (!int.TryParse(info[0], out buffId) || !float.TryParse(info[1], out time))
            {
                Log.Error($"EnhanceBuffTimeTriHdl error, handlerParam {handlerParam}");
            }
        }

        public override void Handle()
        {
            BaseBuff buff= trigger.Owner.BuffManager.GetBuff(buffId);
            buff?.AddTime(time);

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
