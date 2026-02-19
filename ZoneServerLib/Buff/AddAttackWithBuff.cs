using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class AddAttackWithBuff:BaseBuff
    {
        float addAtk = 0;
        int buffId = 0;
        int buffId2 = 0;
        public AddAttackWithBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.BuffEnd, OnBuffEnd);
            addAtk = c;
            buffId = n;
            buffId2 = m;
        }

        protected override void Start()
        {
            owner.AddNatureRatio(NatureType.PRO_ATK, (int)addAtk, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureRatio(NatureType.PRO_ATK, (int)addAtk * -1);
            RemoveListener(TriggerMessageType.BuffEnd, OnBuffEnd);
        }

        protected override void SendBuffEndMsg()
        {
            //if (owner.SubcribedMessage(TriggerMessageType.BuffEnd))
            //{
            //    // buff结束的原因是 被打碎
            //    BuffEndTriMsg msg = new BuffEndTriMsg(Id, owner.GetNatureValue(NatureType.PRO_SHIELD_HP) <= 0 ? BuffEndReason.Damage : BuffEndReason.Time);
            //    owner.DispatchMessage(TriggerMessageType.BuffEnd, msg);
            //}
        }

        private void OnBuffEnd(object param)
        {
            
            BuffEndTriMsg msg = param as BuffEndTriMsg;
            if (msg != null&&(msg.BuffId== buffId)||msg.BuffId==buffId2)
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
