using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AddBodyEnergyTrlHdl : BaseTriHdl
    {
        int energy = 0;
        public AddBodyEnergyTrlHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            var param = StringSplit.ParseToFloatPair(handlerParam);
            int skillLevel = trigger.GetFixedParam_SkillLevel();

            energy = (int)trigger.CalcParam(TriggerHandlerType.AddBodyEnergy, param, skillLevel);
        }

        public override void Handle()
        {
            Log.Debug("hero instanceId {0} AddBodyEnergyTrlHdl done!", Owner.InstanceId);
            if (Owner.SkillManager.HasBodySkill)
            {
                if (!Owner.IsDead && !Owner.InRealBody)
                {
                    Owner.SkillManager.AddBodyEnergy(energy, true);
                }
            }
            else
            {
               trigger.Stop();
            }
        }
    }
}
