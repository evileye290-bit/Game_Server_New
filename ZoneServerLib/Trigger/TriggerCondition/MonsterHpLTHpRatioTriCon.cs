using CommonUtility;
using Logger;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class MonsterHpLTHpRatioTriCon : BaseTriCon
    {
        private readonly int monsterId;
        private readonly int ratio;
        private long hpValue = 0;

        public MonsterHpLTHpRatioTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] param = conditionParam.Split(':');
            if (param.Length != 2)
            {
                Log.Warn($"create MonsterDamageToTalGTHpRatioTriCon failed: invalid param {conditionParam}");
                return;
            }

            if (!int.TryParse(param[0], out monsterId) || !int.TryParse(param[1], out ratio))
            {
                Log.Warn($"create MonsterDamageToTalGTHpRatioTriCon failed: invalid param {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.MonsterTotalDamage, out param))
            {
                return false;
            }

            Monster monster = trigger.CurMap.MonsterList.Values.Where(x => x.Generator.Id == monsterId).FirstOrDefault();
            if (monster == null) return false;

            if (hpValue == 0)
            {
                hpValue = (long)(monster.GetNatureValue(NatureType.PRO_MAX_HP) * (ratio * 0.0001f));
            }

            Dictionary<int, int> monsterDamage = param as Dictionary<int, int>;
            if (monsterDamage == null) return false;

            return monsterDamage.ContainsKey(monsterId) && monster.GetNatureValue(NatureType.PRO_HP) <= hpValue;
        }
    }
}
