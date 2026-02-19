using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DoExtraDamageByPoisonBuffCountTriHdl : BaseTriHdl
    {
        private readonly float growth = 0;
        private readonly float damage = 0;

        public DoExtraDamageByPoisonBuffCountTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2)
            {
                Log.Warn("init DoExtraDamageByPoisonBuffCount failed, invalid handler param {0}", handlerParam);
                return;
            }
            if (!float.TryParse(param[0], out growth) || !float.TryParse(param[1], out damage))
            {
                Log.Warn("init DoExtraDamageByPoisonBuffCount failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param) && !trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return;
            }
            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg == null || msg.FieldObject == null || msg.SkillId == 0)
            {
                return;
            }

            if (ThisFpsHadHandled(msg.FieldObject, msg.SkillId)) return;

            Skill skill = Owner.SkillManager.GetSkill(msg.SkillId);
            if (skill == null)
            {
                return;
            }
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            int unitDamage = (int)trigger.CalcParam(growth, damage, skillLevelGrowth);
            if (msg.FieldObject.BuffManager == null)
            {
                return;
            }
            int count = msg.FieldObject.BuffManager.GetPoisonBuffCount();
            msg.FieldObject.AddNatureBaseValue(NatureType.PRO_EXTRA_DAMAGE_ONCE, count * unitDamage);
            SetThisFspHandled(msg.FieldObject, msg.SkillId);
        }
    }

}
