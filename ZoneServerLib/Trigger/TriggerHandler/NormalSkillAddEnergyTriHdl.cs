using System;
using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class NormalSkillAddEnergyTriHdl : BaseTriHdl
    {
        readonly int energy;
        public NormalSkillAddEnergyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            energy = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Owner.SkillManager.AddNormalSkillEnergy(energy, true);
        }
    }
}
