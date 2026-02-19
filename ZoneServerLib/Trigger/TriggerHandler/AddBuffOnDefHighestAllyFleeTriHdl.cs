using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class AddBuffOnDefHighestAllyFleeTriHdl : BaseTriHdl
    {
        private readonly int buffId;
        private readonly float cd;
        private MessageDispatcher messageDispatcher;

        private DateTime nextTime = DateTime.Now;

        public AddBuffOnDefHighestAllyFleeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramStr = handlerParam.Split(':');
            if (paramStr.Length != 2)
            {
                Logger.Log.Error($"init AddBuffOnDefHighestAllyFleeTriHdl error, param {handlerParam}");
                return;
            }
            buffId = Convert.ToInt32(paramStr[0]);
            cd = float.Parse(paramStr[1]);
        }

        public override void Handle()
        {
            List<FieldObject> fieldObjects = new List<FieldObject>();
            SkillSplashChecker.GetAllyInMap(Owner, CurMap, fieldObjects);
            Owner.FilterTarget_MaxDefAlly(fieldObjects);

            FieldObject fieldObject = fieldObjects.FirstOrDefault();
            if (fieldObject == null) return;
            messageDispatcher = fieldObject.GetDispatcher();

            messageDispatcher?.AddListener(TriggerMessageType.DodgeSkill, Listener);
        }

        private void Listener(Object obj)
        {
            if (Owner.IsDead)
            {
                messageDispatcher?.RemoveListener(TriggerMessageType.DodgeSkill, Listener);
                return;
            }

            if (DungeonMap?.CurrTime >= nextTime)
            {
                nextTime = DungeonMap.CurrTime.AddSeconds(cd);
                Owner.BuffManager.AddBuff(trigger.Owner, buffId, 1);
            }
        }
    }
}

