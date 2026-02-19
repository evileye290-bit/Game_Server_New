using CommonUtility;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class MonsterAllDeadTriCon : BaseTriCon
    {
        readonly Dictionary<int, int> monsterDeadCheckList;//<ZoneMonsterId, count>
        readonly Dictionary<int, int> monsterCheckList;//<ZoneMonsterId, count>
        readonly List<int> deadMonsterList;// 
        public MonsterAllDeadTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            monsterCheckList = new Dictionary<int, int>();
            deadMonsterList = new List<int>();
            string[] monsterArr = conditionParam.Split('|');
            foreach(var monsterStr in monsterArr)
            {
                if(string.IsNullOrEmpty(monsterStr))
                {
                    continue;
                }
                string[] param = monsterStr.Split(':');
                if(param.Length != 2)
                {
                    continue;
                }
                monsterCheckList.Add(int.Parse(param[0]), int.Parse(param[1]));
            }
            monsterDeadCheckList = new Dictionary<int, int>(monsterCheckList);
        }

        public override bool Check()
        {
            object deadObject = null;
            if(!trigger.TryGetParam(TriggerParamKey.Dead, out deadObject))
            {
                return false;
            }

            Monster monster = deadObject as Monster;
            if(monster == null)
            {
                return false;
            }
            // 可能是Hero或Player Dead触发，而上次记录的TriggerParamKey.Dead对应参数并没有擦出，依旧为上次死亡的monster
            // 需要排除
            if (deadMonsterList.Contains(monster.InstanceId))
            {
                return false;
            }

            int deadMonsterId = monster.Generator.Id;
            int aliveCount = 0;
            if(!monsterCheckList.TryGetValue(deadMonsterId, out aliveCount))
            {
                return false;
            }

            --monsterCheckList[deadMonsterId];
            deadMonsterList.Add(monster.InstanceId);

            foreach(var kv in monsterCheckList)
            {
                if(kv.Value > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public override void Reset()
        {
            base.Reset();
            monsterCheckList.Clear();
            monsterDeadCheckList.ForEach(item => monsterCheckList.Add(item.Key, item.Value));
        }
    }
}
