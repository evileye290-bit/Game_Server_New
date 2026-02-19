using System.Collections.Generic;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class EnhanceIdSkillDamageRatioTriHdl : BaseTriHdl
    {
        private readonly int skillId = 0;
        private readonly int ratio;
        public EnhanceIdSkillDamageRatioTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 2)
            {
                Log.Warn("init enhance skill damage ratio tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            if (!int.TryParse(paramArr[0], out skillId) || !int.TryParse(paramArr[1], out ratio))
            {
                Log.Warn("init enhance skill damage ratio tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            Owner?.SkillManager?.AddSkillEnhanceDamageRatio(skillId, ratio);
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
