using CommonUtility;

namespace ZoneServerLib
{
    public class BaseTriCon
    {
        protected BaseTrigger trigger;
        protected FieldObject owner
        { get { return trigger.Owner; } }

        protected TriggerCondition conditionType = TriggerCondition.None;
        string conditionParam;

        protected bool ready = false;

        public BaseTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
        {
            this.trigger = trigger;
            this.conditionType = conditionType;
            this.conditionParam = conditionParam;
        }

        public virtual bool Check()
        {
            return false;
        }

        // 单位 秒
        public virtual void Update(float dt)
        {
        }

        public virtual void Reset()
        {
            ready = false;
        }
    }
}
