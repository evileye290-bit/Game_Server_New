using CommonUtility;
using Logger;
using System.Linq;

namespace ZoneServerLib
{
    public class TargetInSkillRangeMoreDamageGreaterTriHdl : BaseTriHdl
    {
        private readonly float growthFactor;
        private readonly float multiFactor;
        private readonly int damage;

        public TargetInSkillRangeMoreDamageGreaterTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 3)
            {
                Log.WarnLine($"TargetInSkillRangeMoreDamageGreaterTriHdl param error need params leng 2, current param {handlerParam}");
            }
            else
            {
                growthFactor = float.Parse(param[0]);
                multiFactor = float.Parse(param[1]);
                damage = int.Parse(param[2]);
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.TargetInSkillRange, out param))
            {
                return;
            }
            TargetInSkillRangeMsg msg = param as TargetInSkillRangeMsg;
            if (msg == null || msg.Target == null)
            {
                return;
            }

            if (ThisFpsHadHandled(Owner, msg.SkillId)) return;

            //魂环技能等级
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            int unitDamage = (int)trigger.CalcParam(growthFactor, damage, skillLevelGrowth);
            int totalDamage = (int)(unitDamage * multiFactor * (msg.Target.Count - 1));

            foreach (var target in msg.Target)
            {
                target.AddNatureBaseValue(NatureType.PRO_FIXED_DAM_ONCE, totalDamage);
            }

            SetThisFspHandled(Owner, msg.SkillId);
        }
    }
}
