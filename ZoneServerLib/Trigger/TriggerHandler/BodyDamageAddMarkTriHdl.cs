using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class BodyDamageAddMarkTriHdl : BaseTriHdl
    {
        private readonly int markId;
        public BodyDamageAddMarkTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out markId))
            {
                Log.Warn("in add mark tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object obj;
            if (trigger.TryGetParam(TriggerParamKey.BodyDamage, out obj))
            {
                BodyDamageMsg msg = obj as BodyDamageMsg;
                if (msg != null)
                {
                    Owner.AddMark(msg.Caster, markId, 1);
                }
            }
            SetThisFspHandled();
        }
    }
}
