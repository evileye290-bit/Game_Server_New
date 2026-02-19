using CommonUtility;

namespace ZoneServerLib
{
    public class OnAllyCastTypeBuffTriLnr : BaseTriLnr
    {
        public OnAllyCastTypeBuffTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
            Owner.CurDungeon?.AddBridgeTriggerMessageListener(TriggerMessageType.AllyCastTypeBuff, Owner);
        }

        protected override void ParseMessage(object message)
        {
            BaseBuff buff = message as BaseBuff;
            if (buff == null) return;

            trigger.RecordParam(TriggerParamKey.BuildAllyCastTypeBuffKey((int)buff.BuffType), message);
        }

    }
}
