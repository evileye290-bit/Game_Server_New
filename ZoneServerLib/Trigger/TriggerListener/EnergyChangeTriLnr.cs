using CommonUtility;

namespace ZoneServerLib
{
    public class EnergyChangeTriLnr : BaseTriLnr
    {
        public EnergyChangeTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
            DungeonMap dungeon = trigger.CurMap as DungeonMap;
            if (dungeon == null)
            {
                Logger.Log.Warn($"trigger owned map {trigger.Model.Id} is not dungeon");
                return;
            }

            //自身能量触发也需要通知
            dungeon.AddBridgeTriggerMessageListener(TriggerMessageType.EnergyChange, trigger.Owner, true);
        }

        protected override void ParseMessage(object message)
        {
            EnergyChangeMsg msg = message as EnergyChangeMsg;
            if (msg == null) return;

            trigger.RecordParam(TriggerParamKey.EnergyChangeTarget, msg);
        }
    }
}