using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class DeadEnemyHaveTypeBuffTriCon : BaseTriCon
    {
        private readonly int buffType = 0;
        private List<int> buffTypeList = new List<int>();
        public DeadEnemyHaveTypeBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] buffIds = conditionParam.Split('|');
            foreach (var buffIdStr in buffIds)
            {
                if (!int.TryParse(buffIdStr, out buffType))
                {
                    Log.Warn($"init DeadEnemyHaveTypeBuffTriCon failed: invalid skill param {conditionParam}");
                    continue;
                }
                buffTypeList.Add(buffType);
            }
        }

        public override bool Check()
        {
            object param = null;
            if (!trigger.TryGetParam(TriggerParamKey.FieldObjectDead, out param))
            {
                return false;
            }
            FieldObject fieldObject = param as FieldObject;
            if (fieldObject == null || !fieldObject.IsEnemy(trigger.Owner))
            {
                return false;
            }
            foreach (var buffType in buffTypeList)
            {
                if (fieldObject?.BuffManager.GetBuffByType(buffType) != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
