using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public abstract class AliveCountTriCon : BaseTriCon
    {
        public AliveCountTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
        }

        public override bool Check()
        {
            return false;
        }

        protected int GetAllyCount()
        {
            List<FieldObject> fieldList = new List<FieldObject>();
            SkillSplashChecker.GetAllyInMap(trigger.Owner, trigger.CurMap, fieldList);
            return fieldList.Count;
        }

        protected int GetEnemyCount()
        {
            List<FieldObject> fieldList = new List<FieldObject>();
            SkillSplashChecker.GetEnemyInMap(trigger.Owner, trigger.CurMap, fieldList);
            return fieldList.Count;
        }
    }


    public abstract class AliveCountGTTriCon : AliveCountTriCon
    {
        private readonly bool isAlly;
        private readonly int count;
        public AliveCountGTTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam, bool isAlly)
            : base(trigger, conditionType, conditionParam)
        {
            this.isAlly = isAlly;
            if (!int.TryParse(conditionParam, out count))
            {
                Log.Warn($"init AliveCountGTTriCon failed: invalid buffId {conditionParam}");
            }
        }

        public override bool Check()
        {
            if (isAlly)
            {
                return GetAllyCount() > count;
            }
            else
            { 
                return GetEnemyCount() > count;
            }
        }
    }

    public abstract class AliveCountEQTriCon : AliveCountTriCon
    {
        private readonly bool isAlly;
        private readonly int count;
        public AliveCountEQTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam, bool isAlly)
            : base(trigger, conditionType, conditionParam)
        {
            this.isAlly = isAlly;
            if (!int.TryParse(conditionParam, out count))
            {
                Log.Warn($"init AliveCountETTriCon failed: invalid buffId {conditionParam}");
            }
        }

        public override bool Check()
        {
            if (isAlly)
            {
                return GetAllyCount() == count;
            }
            else
            {
                return GetEnemyCount() == count;
            }
        }
    }

    public abstract class AliveCountLETriCon : AliveCountTriCon
    {
        private readonly bool isAlly;
        private readonly int count;
        public AliveCountLETriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam, bool isAlly)
            : base(trigger, conditionType, conditionParam)
        {
            this.isAlly = isAlly;
            if (!int.TryParse(conditionParam, out count))
            {
                Log.Warn($"init AliveCountLETriCon failed: invalid buffId {conditionParam}");
            }
        }

        public override bool Check()
        {
            if (isAlly)
            {
                return GetAllyCount() < count;
            }
            else
            {
                return GetEnemyCount() < count;
            }
        }
    }

    public class AllyAliveCountGTTriCon : AliveCountGTTriCon
    {
        public AllyAliveCountGTTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam, true)
        {
        }
    }

    public class AllyAliveCountEQTriCon : AliveCountEQTriCon
    {
        public AllyAliveCountEQTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam, true)
        {
        }
    }

    public class AllyAliveCountLETriCon : AliveCountLETriCon
    {
        public AllyAliveCountLETriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam, true)
        {
        }
    }

    public class EnemyAliveCountGTTriCon : AliveCountGTTriCon
    {
        public EnemyAliveCountGTTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam, false)
        {
        }
    }

    public class EnemyAliveCountEQTriCon : AliveCountEQTriCon
    {
        public EnemyAliveCountEQTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam, false)
        {
        }
    }

    public class EnemyAliveCountLETriCon : AliveCountLETriCon
    {
        public EnemyAliveCountLETriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam, false)
        {
        }
    }
}

