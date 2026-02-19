using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public class ShieldBuff : BaseBuff
    {
        private long shieldHp = 0;
        public ShieldBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            shieldHp = m;
        }

        protected override void Start()
        {
            BaseBuff baseBuff = owner.BuffManager.GetOneBuffByType(BuffType.EnhanceShieldHp);
            if (baseBuff != null)
            {
                m = (int)(m * (1 + baseBuff.C * 0.0001f));
                shieldHp = m;
            }

            owner.AddNatureAddedValue(NatureType.PRO_SHIELD_MAX_HP, m, Model.Notify);
            owner.AddNatureAddedValue(NatureType.PRO_SHIELD_HP, m, Model.Notify);
            owner.BroadCastHp();
            
            AddListener(TriggerMessageType.ShieldDamage, OnShieldDamage);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_SHIELD_MAX_HP, -m, Model.Notify);
            if (shieldHp > 0)
            {
                //移除剩余盾的血量
                owner.AddNatureAddedValue(NatureType.PRO_SHIELD_HP, -shieldHp, Model.Notify);
            }

            RemoveListener();

            owner.BroadCastHp();
        }

        protected override void SendBuffEndMsg()
        {
            if(owner.SubcribedMessage(TriggerMessageType.BuffEnd))
            {
                // buff结束的原因是 被打碎
                BuffEndTriMsg msg = new BuffEndTriMsg(Id, shieldHp <= 0 ? BuffEndReason.Damage : BuffEndReason.Time);
                owner.DispatchMessage(TriggerMessageType.BuffEnd, msg);
            }
        }

        private void OnShieldDamage(object param)
        {
            if (isEnd) return;
            
            ShieldDamageTriMsg msg = param as ShieldDamageTriMsg;
            if(msg == null) return;

            if (shieldHp <= msg.Damage)
            {
                //护盾抵挡了部分伤害
                msg.Damage -= shieldHp;
                shieldHp = 0;

                //盾被打碎
                isEnd = true;
                RemoveListener();

                if(owner == null) return;
                
                MSG_ZGC_BUFF_SPEC_END notify = new MSG_ZGC_BUFF_SPEC_END();
                notify.InstanceId = owner.InstanceId;
                notify.BuffId = Id;
                owner.BroadCast(notify);
            }
            else
            {
                //护盾抵挡了所有的伤害
                shieldHp -= msg.Damage;
                msg.Damage = 0;
            }
        }

        private void RemoveListener()
        {
            RemoveListener(TriggerMessageType.ShieldDamage, OnShieldDamage);
        }
    }
}
