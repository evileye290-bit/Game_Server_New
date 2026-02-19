using CommonUtility;
using Logger;
using ScriptFighting;

namespace ZoneServerLib
{
    public class DamageCasterAddBuffTriHdl : BaseTriHdl
    {
        private readonly int buffId;
        private readonly int ratio;
        public DamageCasterAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 2)
            {
                Log.Warn("damage caster add buff failed: invalid param {0}", handlerParam);
                return;
            }

            if (!int.TryParse(paramArr[0], out buffId) || !int.TryParse(paramArr[1], out ratio))
            {
                Log.Warn("damage caster add buff failed: invalid param {0}", handlerParam);
                return;
            }

            ratio = trigger.CalcParam(TriggerHandlerType.DamageCasterAddBuff, ratio);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.OnceDamage, out param)) return;

            DamageTriMsg msg = param as DamageTriMsg;
            if (msg == null || msg.Caster == null || trigger.Owner == null) return;

            if (ratio > RAND.Range(0, 10000))
            { 
                msg.Caster.AddBuff(trigger.Caster, buffId, trigger.GetFixedParam_SkillLevelGrowth());
            }
            SetThisFspHandled();
        }
    }
}
