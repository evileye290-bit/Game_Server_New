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
    partial class FieldObject
    {
        protected BuffManager buffManager;
        public BuffManager BuffManager
        { get { return buffManager; } }

        public void InitBuffManager()
        {
            buffManager = new BuffManager(this);
        }

        public void AddBuff(FieldObject caster, int buffId, int skillLevel, int pileCount = 1)
        {
            if(IsDead || buffManager == null)
            {
                return;
            }
            buffManager.AddBuff(caster, buffId, skillLevel, pileCount);
        }

        public void AddBuffDelay(FieldObject caster, int buffId, int skillLevel, float delayTime)
        {
            if (IsDead || buffManager == null)
            {
                return;
            }
            buffManager.AddBuffDelay(caster, buffId, skillLevel, delayTime);
        }

        public bool InBuffState(BuffType buffType)
        {
            if (buffManager == null)
            {
                return false;
            }
            return buffManager.InBuffState(buffType);
        }

        public bool BeControlled()
        {
            return InBuffState(BuffType.Dizzy) || InBuffState(BuffType.Fixed) 
                || InBuffState(BuffType.Disarm) || InBuffState(BuffType.Silent);
        }

        public bool HasDebuff()
        {
            return buffManager.HaveDeBuff();
        }

        public int GetBuffTotal_M(BuffType buffType)
        {
            if (buffManager == null) return 0;
            return buffManager.GetBuffTotal_M(buffType);
        }

        public void RemoveRandomDebuff()
        {
            if (buffManager == null) return;
            buffManager.RemoveRandomDebuff();
        }

        public void RemoveRandomBuff()
        {
            if (buffManager == null) return;
            buffManager.RemoveRandomBuff();
        }

        public bool InShieldBuff()
        {
            if (buffManager == null) return false;

            return buffManager.InBuffState(BuffType.Shield) ||
                buffManager.InBuffState(BuffType.Shield_Spider) ||
                buffManager.InBuffState(BuffType.Shield_WhiteTiger_Self) ||
                buffManager.InBuffState(BuffType.Shield_WhiteTiger_Ally) 
                //|| buffManager.InBuffState(BuffType.ShieldByOwnerDefence)
                //|| buffManager.InBuffState(BuffType.ShieldByOnwerNature)
                ;
        }

        public void BroadcastAddBuff(BuffModel model)
        {
            MSG_ZGC_ADD_BUFF msg = new MSG_ZGC_ADD_BUFF()
            {
                InstanceId = instanceId,
                BuffId = model.Id
            };
            BroadCast(msg);
        }

        public void BroadcastRemoveBuff(BuffModel model)
        {
            MSG_ZGC_REMOVE_BUFF msg = new MSG_ZGC_REMOVE_BUFF()
            {
                InstanceId = instanceId,
                BuffId = model.Id
            };
            BroadCast(msg);
        }
    }
}
