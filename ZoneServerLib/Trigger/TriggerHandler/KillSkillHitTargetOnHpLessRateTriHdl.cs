using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class KillSkillHitTargetOnHpLessRateTriHdl : BaseTriHdl
    {
        readonly int skillType;
        readonly float hpLessRate;

        public KillSkillHitTargetOnHpLessRateTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int hprate = 0;
            string[] param = handlerParam.Split(':');
            if (param.Length != 2 || !int.TryParse(param[0], out skillType) || !int.TryParse(param[1], out hprate))
            {
                Log.Error($"init KillSkillHitTargetOnHpLessRateTriHdl error : param {handlerParam}");
                return;
            }

            hpLessRate = (float)(hprate * 0.0001f);
        }

        public override void Handle()
        {
            object param =null;
            if (!trigger.TryGetParam(TriggerParamKey.BuildSkillTypeHitKey(skillType), out param))
            {
                return;
            }

            SkillHitMsg msg = param as SkillHitMsg;
            if (msg == null)
            {
                return;
            }

            foreach (var kv in msg.TargetList)
            {
                if (kv.IsDead || kv.IsMonster) continue;

                long hp = kv.GetNatureValue(NatureType.PRO_HP);
                long maxHp = kv.GetNatureValue(NatureType.PRO_MAX_HP);

                if (hp < maxHp * hpLessRate)
                {
                    if (kv.InBuffState(BuffType.Invincible)) continue;

                    //同一帧不连续触发
                    if (ThisFpsHadHandled()) return;
                    SetThisFspHandled();

                    kv.UpdateHp(DamageType.Skill, hp * -1, trigger.Owner);
                    //kv.DoSpecDamage(trigger.Owner, DamageType.Skill, maxHp * 2);
                }
            }

        }
    }
}
