using CommonUtility;

namespace ZoneServerLib
{
    public class CriticalTargetReduceBodyEnergyTriHdl : BaseTriHdl
    {
        private readonly int energy = 0;
        public CriticalTargetReduceBodyEnergyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            var param = StringSplit.ParseToFloatPair(handlerParam);
            int skillLevel = trigger.GetFixedParam_SkillLevel();

            energy = (int)trigger.CalcParam(TriggerHandlerType.CriticalTargetReduceBodyEnergy, param, skillLevel) * -1;
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

            if (msg.Target.SkillManager.HasBodySkill && !msg.Target.IsDead && !msg.Target.InRealBody)
            {
                msg.Target.SkillManager.AddBodyEnergy(energy, true, true);
            }
            SetThisFspHandled();
        }
    }
}