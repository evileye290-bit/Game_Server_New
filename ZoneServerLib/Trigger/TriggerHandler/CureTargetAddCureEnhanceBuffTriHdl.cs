using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class CureTargetAddBuffTriHdl : BaseTriHdl
    {
        private readonly int buffId;
        public CureTargetAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out buffId))
            {
                Log.Warn("init CureBuffTargetAddBuffTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.CastCureBuff, out param) &&
                !trigger.TryGetParam(TriggerParamKey.SkillAddCureBuff, out param))
            {
                return;
            }
            FieldObject target = param as FieldObject;
            if (target == null)
            {
                return;
            }
            if (ThisFpsHadHandled(target)) return;
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            target.AddBuff(Owner, buffId, skillLevelGrowth);
            SetThisFspHandled(target);
        }
    }
}
