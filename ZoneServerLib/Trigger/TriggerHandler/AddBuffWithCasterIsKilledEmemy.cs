using CommonUtility;

namespace ZoneServerLib
{
    class AddBuffWithCasterIsKilledEmemy : BaseTriHdl
    {
        readonly int buffId;

        public AddBuffWithCasterIsKilledEmemy(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int.TryParse(handlerParam, out buffId);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            object param = null;
            if (!trigger.TryGetParam(TriggerParamKey.KillEnemy, out param)) return;
            
            KillEnemyTriMsg msg = param as KillEnemyTriMsg;
            if (msg == null) return;

            FieldObject field = CurMap.GetFieldObject(msg.DeadInstanceId);
            if (field == null) return;

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            trigger.Owner?.AddBuff(field, buffId, skillLevelGrowth);
        }
    }
}

