using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class RandomGetBuffTriHdl : BaseTriHdl
    {
        readonly int buffId, ratio;
        private SortedDictionary<int, int> buffIds;
        private int count = 0;

        public RandomGetBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split('|');
            if (paramArr.Length < 2)
            {
                Log.WarnLine($"RandomGetBuffTriHdl param error need params leng 3, current param {handlerParam}");
                return;
            }
            buffIds = new SortedDictionary<int, int>();
            foreach (var param in paramArr)
            {
                string[] unitParam = param.Split(':');
                if (unitParam.Length != 2 || !int.TryParse(unitParam[0], out ratio) || !int.TryParse(unitParam[1], out buffId))
                {
                    Log.WarnLine($"RandomGetBuffTriHdl param error, current param {handlerParam}");
                    return;
                }
                if (!buffIds.ContainsValue(buffId))
                {
                    count += ratio;
                    buffIds.Add(count, buffId);
                }
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            FieldObject addTarget = Owner;
            FieldObject caster = Owner;

            object param;
            if (trigger.TryGetParam(TriggerParamKey.BodyDamage, out param))
            {
                BodyDamageMsg bodyMsg = param as BodyDamageMsg;
                if (bodyMsg == null || bodyMsg.Caster == null)
                {
                    return;
                }
                caster = bodyMsg.Caster;
            }
            else if (trigger.TryGetParam(TriggerParamKey.Critical, out param))
            {
                CriticalTriMsg criMsg = param as CriticalTriMsg;
                if (criMsg == null || criMsg.Target == null)
                {
                    return;
                }
                addTarget = criMsg.Target;
            }
            else if (trigger.TryGetParam(TriggerParamKey.BuildSkillTypeStartKey((int)SkillType.Normal_Skill_1), out param))
            {
                int paramInt;
                if (!int.TryParse(param.ToString(), out paramInt))
                {
                    return;
                }
            }

            int rand = RAND.Range(0, count);
            foreach (var kv in buffIds)
            {
                if (kv.Key >= rand)
                {
                    int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                    addTarget.AddBuff(caster, kv.Value, skillLevelGrowth);
                    return;
                }
            }
        }
    }
}
