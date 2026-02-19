using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ExtraRatioCureOnceTriHdl : BaseTriHdl
    {     
        readonly float growthFactor;//成长系数
        readonly int baseCure;//成长治疗基础
        readonly double hpRatio;//如hp大于0则需满足血量大于此百分比

        public ExtraRatioCureOnceTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 3)
            {
                Log.WarnLine($"ExtraRatioCureOnceTriHdl param error need params leng 2, current param {handlerParam}");
            }
            else
            {
                growthFactor = float.Parse(param[0]);
                baseCure = int.Parse(param[1]);
                hpRatio = double.Parse(param[2]);
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.CastCureBuff, out param))
            {
                return;
            }
            if (hpRatio > 0 && Owner.GetHpRate() < hpRatio)
            {
                return;
            }
            FieldObject target = param as FieldObject;
            if (target != null)
            {
                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                int cureValue = (int)trigger.CalcParam(growthFactor, baseCure, skillLevelGrowth);
                Owner.AddNatureAddedValue(NatureType.PRO_CURE_ENHANCE_ONCE, cureValue);
            }
            SetThisFspHandled();
        }
    }
}
