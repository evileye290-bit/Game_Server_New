using CommonUtility;

namespace ZoneServerLib
{
    public class TimeTriCon : BaseTriCon
    {
        readonly float triggerTime = 0;
        float elapsedTime = 0;
        public TimeTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            triggerTime = float.Parse(conditionParam);
            if (trigger.Caster == null)
            {
                DungeonMap dungeon = trigger.CurMap as DungeonMap;
                if (dungeon != null)
                {
                    triggerTime += dungeon.SpeedUpFinishDelayTime;
                }
            }
        }

        public override void Update(float dt)
        {
            if (ready)
            {
                return;
            }
            elapsedTime += dt;
            if (elapsedTime >= triggerTime)
            {
                //Logger.Log.Warn($"time------------ {dt} elapsedtime {elapsedTime} triggertime {triggerTime} triggerid {trigger.Model.Id}");
                ready = true;
                trigger.TryHandle();
            }
        }

        public override void Reset()
        {
            base.Reset();
            elapsedTime = 0;
        }
        public override bool Check()
        {
            return elapsedTime >= triggerTime;
        }
    }
}
