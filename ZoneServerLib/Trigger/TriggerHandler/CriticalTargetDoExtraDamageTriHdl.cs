using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CriticalTargetDoExtraDamageTriHdl : BaseTriHdl
    {
        private readonly float growth = 0;
        private readonly float damage = 0;
        public CriticalTargetDoExtraDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2)
            {
                Log.Warn("init CriticalTargetDoExtraDamageTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }
            if (!float.TryParse(param[0], out growth) || !float.TryParse(param[1], out damage))
            {
                Log.Warn("init CriticalTargetDoExtraDamageTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }                          
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.Critical, out param))
            {
                return;
            }
            CriticalTriMsg msg = param as CriticalTriMsg;
            if (msg == null || msg.Target == null || msg.Model == null)
            {
                return;
            }
            Skill skill = Owner.SkillManager.GetSkill(msg.Model.Id);
            if (skill == null)
            {
                return;
            }
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            int finalDamage = (int)trigger.CalcParam(growth, damage, skillLevelGrowth);
            //msg.Target.DoSpecDamage(Owner, DamageType.Extra, finalDamage);
            msg.Target.AddNatureBaseValue(NatureType.PRO_EXTRA_DAMAGE_ONCE, finalDamage);
            SetThisFspHandled();
        }
    }
}
