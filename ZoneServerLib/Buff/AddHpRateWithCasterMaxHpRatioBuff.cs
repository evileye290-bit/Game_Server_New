using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public class AddHpRateWithCasterMaxHpRatioBuff : BaseBuff
    {
        public AddHpRateWithCasterMaxHpRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.BuffEnd, OnBuffEnd);
        }

        protected override void Update(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime < deltaTime)
            {
                return;
            }

            elapsedTime = 0;
            if (owner.BuffManager.HaveBuff((int)buffModel.N.Value))
            {
                if (!owner.HpGreaterThanRate((int)c))
                {
                    owner.AddHp(owner, (long)(caster.GetNatureValue(NatureType.PRO_MAX_HP) * (m * 0.0001f)));
                }
            }
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.BuffEnd, OnBuffEnd);
        }

        private void OnBuffEnd(object param)
        {
            BuffEndTriMsg msg = param as BuffEndTriMsg;
            if (msg != null && msg.BuffId == n)
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
}
