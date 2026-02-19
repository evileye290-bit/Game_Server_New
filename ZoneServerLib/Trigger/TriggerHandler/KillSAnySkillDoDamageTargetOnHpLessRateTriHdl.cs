using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class KillSAnySkillDoDamageTargetOnHpLessRateTriHdl : BaseTriHdl
    {
        readonly float hpLessRate;

        public KillSAnySkillDoDamageTargetOnHpLessRateTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int hprate = 0;
            if (!int.TryParse(handlerParam, out hprate))
            {
                Log.Error($"init KillSAnySkillDoDamageTargetOnHpLessRateTriHdl error : param {handlerParam}");
                return;
            }

            hpLessRate = (float)(hprate * 0.0001f);
        }

        public override void Handle()
        {
            object param = null;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param)) return;

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg == null) return;

            if (msg.FieldObject.IsDead) return;

            FieldObject field = msg.FieldObject;

            long hp = field.GetNatureValue(NatureType.PRO_HP);
            long maxHp = field.GetNatureValue(NatureType.PRO_MAX_HP);

            if (hp < maxHp * hpLessRate)
            {
                if (field.InBuffState(BuffType.Invincible)) return;

                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();
                field.UpdateHp(DamageType.Extra, hp * -1, trigger.Owner);
            }

        }
    }
}
