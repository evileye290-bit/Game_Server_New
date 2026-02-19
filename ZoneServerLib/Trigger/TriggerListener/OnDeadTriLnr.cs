using CommonUtility;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class OnDeadTriLnr : BaseTriLnr
    {
        public OnDeadTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            FieldObject fieldObject = message as FieldObject;
            if (fieldObject == null)
            {
                return;
            }

            trigger.RecordParam(TriggerParamKey.Dead, fieldObject);

            //分别记录各种fieldobject dead 参数
            switch (fieldObject.FieldObjectType)
            {
                case TYPE.MONSTER:
                    {
                        //trigger.RecordParam(TriggerParamKey.Dead, fieldObject);
                    }
                    break;
                case TYPE.ROBOT:
                case TYPE.PC:
                    {
                        int count = trigger.GetParam_PlayerDeadCount();
                        trigger.RecordParam(TriggerParamKey.PlayerDeadCount, ++count);
                    }
                    break;
                case TYPE.HERO:
                    {
                        int count = trigger.GetParam_HeroDeadCount();
                        trigger.RecordParam(TriggerParamKey.HeroDeadCount, ++count);
                    }
                    break;
            }
        }
    }
}
