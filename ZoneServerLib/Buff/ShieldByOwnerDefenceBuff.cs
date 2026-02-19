using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{

    public class ShieldByOwnerDefenceBuff : BaseBuff
    {
        int shield = 0;
        public ShieldByOwnerDefenceBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            shield = (int)(owner.GetNatureValue(NatureType.PRO_DEF) * (m * 0.0001f));
            AddListener(TriggerMessageType.ShieldBreakUp, OnShieldBreakUp);
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_SHIELD_MAX_HP, shield, Model.Notify);
            owner.AddNatureAddedValue(NatureType.PRO_SHIELD_HP, shield, Model.Notify);
            owner.BroadCastHp();
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.ShieldBreakUp, OnShieldBreakUp);
            owner.AddNatureAddedValue(NatureType.PRO_SHIELD_MAX_HP, shield * -1);
            owner.AddNatureAddedValue(NatureType.PRO_SHIELD_HP, shield * -1);
            owner.BroadCastHp();
        }

        protected override void SendBuffEndMsg()
        {
            if (owner.SubcribedMessage(TriggerMessageType.BuffEnd))
            {
                // buff结束的原因是 被打碎
                BuffEndTriMsg msg = new BuffEndTriMsg(Id, owner.GetNatureValue(NatureType.PRO_SHIELD_HP) <= 0 ? BuffEndReason.Damage : BuffEndReason.Time);
                owner.DispatchMessage(TriggerMessageType.BuffEnd, msg);
            }
        }

        private void OnShieldBreakUp(object param)
        {
            isEnd = true;
            if (owner != null)
            {
                MSG_ZGC_BUFF_SPEC_END notify = new MSG_ZGC_BUFF_SPEC_END();
                notify.InstanceId = owner.InstanceId;
                notify.BuffId = Id;
                owner.BroadCast(notify);
            }
        }
    }
}
