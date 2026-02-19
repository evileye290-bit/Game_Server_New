using CommonUtility;
using ServerFrame;

namespace ZoneServerLib
{
    public class BattleEndTriCon : BaseTriCon
    {
        readonly float triggerTime = 0;//剩余时间小于该时间则通知

        public BattleEndTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            triggerTime = float.Parse(conditionParam);
        }

        public override void Update(float dt)
        {
            if (ready)
            {
                return;
            }
            DungeonMap dungeon = trigger.CurMap as DungeonMap;
            if (dungeon == null || dungeon.State != DungeonState.Started)
            {
                return;
            }

            if (dungeon.StopTime <= triggerTime)
            {
                ready = true;
                trigger.TryHandle();
            }
        }

        public override bool Check()
        {
            return true;
        }
    }
}
