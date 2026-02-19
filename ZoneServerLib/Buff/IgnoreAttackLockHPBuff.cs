using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    //免疫致命伤害，锁顶血量值1
    public class IgnoreAttackLockHPBuff : BaseBuff
    {
        public IgnoreAttackLockHPBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void SendBuffEndMsg()
        {
            if (owner.SubcribedMessage(TriggerMessageType.BuffEnd))
            {
                BuffEndTriMsg msg = new BuffEndTriMsg(Id, LeftTime <= 0 ? BuffEndReason.Time : BuffEndReason.Damage);
                owner.DispatchMessage(TriggerMessageType.BuffEnd, msg);
            }
        }
    }
}
