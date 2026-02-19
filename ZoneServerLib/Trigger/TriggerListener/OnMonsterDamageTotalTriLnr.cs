using CommonUtility;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class OnMonsterDamageTotalTriLnr : BaseTriLnr
    {
        public OnMonsterDamageTotalTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            MonsterDamageTriMsg msg = message as MonsterDamageTriMsg;

            if (msg == null) return;

            object totalDamageParam;
            Dictionary<int, long> monsterDamage;

            if (trigger.TryGetParam(TriggerParamKey.MonsterTotalDamage, out totalDamageParam))
            {
                monsterDamage = totalDamageParam as Dictionary<int, long>;
                if (monsterDamage == null)
                {
                    monsterDamage = new Dictionary<int, long>();
                    monsterDamage[msg.MonsterId] = msg.Damage;
                }
                else
                {
                    monsterDamage[msg.MonsterId] += msg.Damage;
                }
            }
            else
            {
                monsterDamage = new Dictionary<int, long>();
                monsterDamage[msg.MonsterId] = msg.Damage;
            }

            trigger.RecordParam(TriggerParamKey.MonsterTotalDamage, monsterDamage);
        }

    }
}
