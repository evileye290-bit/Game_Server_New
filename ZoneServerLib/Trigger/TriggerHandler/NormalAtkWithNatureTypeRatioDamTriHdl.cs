using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class NormalAtkWithNatureTypeRatioDamTriHdl : BaseTriHdl
    {
        private readonly float growth;
        private readonly int natureType;
        private readonly int ratio;
        public NormalAtkWithNatureTypeRatioDamTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 3)
            {
                Log.WarnLine($"NormalAtkWithNatureTypeRatioDamTriHdl param error need params leng 2, current param {handlerParam}");
            }
            else
            {
                growth = float.Parse(param[0]);
                natureType = int.Parse(param[1]);
                ratio = int.Parse(param[2]);
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.NormalAttackStart, out param))
            {
                return;
            }

            int targetId;
            if (!int.TryParse(param.ToString(), out targetId))
            {
                return;
            }

            //魂环技能等级
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            long damage = (long)(Owner.GetNatureValue((NatureType)natureType) * (ratio * 0.0001f));
            int extraDamage = (int)trigger.CalcParam(growth, damage, skillLevelGrowth);
            FieldObject target = Owner.CurrentMap.GetFieldObject(targetId);
            if (target != null)
            {
                target.AddNatureBaseValue(NatureType.PRO_EXTRA_DAMAGE_ONCE, extraDamage);
            }
        }
    }
}
