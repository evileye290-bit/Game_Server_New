using CommonUtility;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class MonsterAnyDeadTriCon : BaseTriCon
    {
        readonly List<int> monsterList;//ZoneMonster.xml id
        public MonsterAnyDeadTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int monsterId = 0;
            monsterList = new List<int>();
            string[] monsterArr = conditionParam.Split('|');
            foreach(var monsterStr in monsterArr)
            {
                if(int.TryParse(monsterStr, out monsterId))
                {
                    monsterList.Add(monsterId);
                }
            }
        }

        public override bool Check()
        {
            if(trigger.CurMap == null)
            {
                return false;
            }

            Monster monster = null;
            Dictionary<int, Monster> monstersInMap = trigger.CurMap.MonsterList;
            foreach (var item in monstersInMap)
            {
                monster = item.Value;
                if (monsterList.Contains(monster.Generator.Id) && monster.IsDead)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
