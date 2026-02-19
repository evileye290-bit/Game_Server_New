using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class EnhanceTypeBuffTimeTriHdl: BaseTriHdl
    {
        private readonly BuffType buffType;
        private readonly float time;
        public EnhanceTypeBuffTimeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int buff = 0;
            string[] param = handlerParam.Split(':');
            if (param.Length!=2 || !float.TryParse(param[1], out time) || !int.TryParse(param[0], out buff))
            {
                Log.Error($"EnhanceTypeBuffTimeTriHdl error, handlerParam {handlerParam}");
                return;
            }

            buffType = (BuffType)buff;
        }

        public override void Handle()
        {
            trigger.Owner?.BuffManager.EnhanceTypefBuffTime(buffType, time);
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}

