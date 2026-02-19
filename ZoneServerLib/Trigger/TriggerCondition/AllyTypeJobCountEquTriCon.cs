using CommonUtility;
using Logger;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class AllyTypeJobCountEquTriCon : BaseTriCon
    {
        private readonly JobType jobType;
        private readonly int countLimit, count;
        public AllyTypeJobCountEquTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            List<int> param = conditionParam.ToList(':');
            if (param.Count != 2)
            {
                Log.Warn($"init AllyTypeJobCountTriCon failed: invalid skill id {conditionParam}");
                return;
            }
            jobType = (JobType)param[0];
            countLimit = param[1];


            List<FieldObject> fieldList = new List<FieldObject>();
            SkillSplashChecker.GetAllyInMap(owner, owner.CurrentMap, fieldList);
            count = fieldList.Where(x =>
            {
                if (x is Hero)
                {
                    Hero hero = x as Hero;
                    return hero.GetJobType() == jobType;
                }
                return false;
            }).Count();

        }

        public override bool Check()
        {
            return count == countLimit;
        }
    }
    
}
