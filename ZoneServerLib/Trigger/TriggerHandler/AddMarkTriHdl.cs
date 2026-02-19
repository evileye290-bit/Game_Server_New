using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AddMarkTriHdl : BaseTriHdl
    {
        private readonly int markId, markCount;
        public AddMarkTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramList = handlerParam.Split(':');
            if (paramList.Length == 1)
            {
                markId = int.Parse(paramList[0]);
                markCount = 1;
            }
            else
            {
                markId = int.Parse(paramList[0]);
                markCount = int.Parse(paramList[1]);
            }
        }

        public override void Handle()
        {
            Owner.AddMark(trigger.Caster, markId, markCount);

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id{Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
