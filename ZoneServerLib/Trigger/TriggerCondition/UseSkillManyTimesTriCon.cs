using CommonUtility;

namespace ZoneServerLib
{
    class UseSkillManyTimesTriCon : BaseTriCon
    {
        private readonly int needTime = 0;
        private int usedTime = 0;

        public UseSkillManyTimesTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            needTime = int.Parse(conditionParam);
        }

        public override bool Check()
        {
            ++usedTime;
            return usedTime >= needTime;
        }

        public override void Reset()
        {
            usedTime = 0;
        }
    }
}
