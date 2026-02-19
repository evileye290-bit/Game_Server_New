using CommonUtility;

namespace ZoneServerLib
{
    public class OnEnemyDeadTriLnr : BaseTriLnr
    {
        public OnEnemyDeadTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.AddCounter(TriggerCounter.EnemyDead);
        }
    }
}
