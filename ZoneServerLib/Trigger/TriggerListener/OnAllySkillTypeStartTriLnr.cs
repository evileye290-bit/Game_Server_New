using CommonUtility;

namespace ZoneServerLib
{
    public class OnAllySkillTypeStartTriLnr : BaseTriLnr
    {
        public OnAllySkillTypeStartTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
            Owner.CurDungeon?.AddBridgeTriggerMessageListener(TriggerMessageType.AllySkillTypeStart, Owner);
        }

        protected override void ParseMessage(object message)
        {
            SkillStartMsg msg = message as SkillStartMsg;
            if (msg == null) return;

            trigger.RecordParam(TriggerParamKey.BuildAllySkillTypeStartKey((int)msg.Model.Type), message);
        }

    }
}
