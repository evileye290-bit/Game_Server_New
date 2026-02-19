using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CureTargetDoExtraCureTriHdl : BaseTriHdl
    {
        private readonly float growth = 0;
        private readonly float cureVal = 0;
        public CureTargetDoExtraCureTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2)
            {
                Log.Warn("init CureTargetDoExtraCureTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }
            if (!float.TryParse(param[0], out growth) || !float.TryParse(param[1], out cureVal))
            {
                Log.Warn("init CureTargetDoExtraCureTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.SkillAddCureBuff, out param))
            {
                return;
            }

            FieldObject target = param as FieldObject;
            if (target != null)
            {
                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                int hp = (int)trigger.CalcParam(growth, cureVal, skillLevelGrowth);

                //此处不能使用Docure 否则自己会触发自己造成死循环
                target.AddHp(Owner, hp);
            }
            SetThisFspHandled();
        }
    }
}
