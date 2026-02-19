using CommonUtility;

namespace ZoneServerLib
{
    class TriggerOwnerDieTriHdl : BaseTriHdl
    {
        public TriggerOwnerDieTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
                : base(trigger, handlerType, handlerParam)
        {
        }

        public override void Handle()
        {
            if (trigger.CurMap == null)
            {
                return;
            }

            Monster monster = Owner as Monster;
            if (monster != null && !monster.IsDead)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();

                //更新怪血量，让怪死亡
                monster.UpdateHp(DamageType.Time, monster.GetNatureValue(NatureType.PRO_MAX_HP) * -1, Owner);
            }
        }
    }
}
