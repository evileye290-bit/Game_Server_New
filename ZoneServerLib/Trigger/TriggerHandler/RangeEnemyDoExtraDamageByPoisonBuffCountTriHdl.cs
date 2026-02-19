using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class RangeEnemyDoExtraDamageByPoisonBuffCountTriHdl : BaseTriHdl
    {
        private readonly float growth, damage, range;

        public RangeEnemyDoExtraDamageByPoisonBuffCountTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 3)
            {
                Log.Warn("init DoExtraDamageByPoisonBuffCount failed, invalid handler param {0}", handlerParam);
                return;
            }
            if (!float.TryParse(param[0], out growth) || !float.TryParse(param[1], out damage) || !float.TryParse(param[2], out range))
            {
                Log.Warn("init DoExtraDamageByPoisonBuffCount failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            int unitDamage = (int)trigger.CalcParam(growth, damage, skillLevelGrowth);

            List<FieldObject> targetList = new List<FieldObject>();
            Owner.GetEnemyInSplash(Owner, SplashType.Circle, Owner.Position, new Vec2(0, 0), range, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                int count = target.BuffManager.GetPoisonBuffCount();
                target.AddNatureBaseValue(NatureType.PRO_EXTRA_DAMAGE_ONCE, count * unitDamage);
            }

            SetThisFspHandled();
        }
    }
}
