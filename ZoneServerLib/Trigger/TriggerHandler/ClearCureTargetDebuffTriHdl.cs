using CommonUtility;

namespace ZoneServerLib
{
    public class ClearCureTargetDebuffTriHdl : BaseTriHdl
    {
        public ClearCureTargetDebuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.CastCureBuff, out param) &&
                !trigger.TryGetParam(TriggerParamKey.SkillAddCureBuff, out param))
            {
                return;
            }
            (param as FieldObject)?.CleanAllDebuff();

            SetThisFspHandled();
        }
    }
}
