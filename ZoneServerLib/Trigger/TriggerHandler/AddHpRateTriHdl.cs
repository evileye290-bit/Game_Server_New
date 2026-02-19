using System;
using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class AddHpRateTriHdl : BaseTriHdl
    {
        private readonly int rate = 0;
        public AddHpRateTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out rate))
            {
                Log.Warn("in add hp rate tri hdl: invalid param {0}", handlerParam);
                return;
            }
            rate = trigger.CalcParam(handlerType, rate);
        }

        public override void Handle()
        {
            // 不能调用UpdateHp 消息会循环发送
            long cure = (long)(Owner.GetNatureValue(NatureType.PRO_MAX_HP) * (rate * 0.0001f));
            long maxHP = Owner.GetNatureValue(NatureType.PRO_MAX_HP);
            long currHP = Owner.GetNatureValue(NatureType.PRO_HP);
            long addhp = cure;
            if (addhp + currHP > maxHP)
            {
                addhp = maxHP - currHP;
            }

            Owner.AddNatureBaseValue(NatureType.PRO_HP, addhp);
            Owner.CheckDead();
            Owner.BroadCastHp();

            if (addhp > 0)
            {
                Owner.CurrentMap.RecordBattleDataHurt(trigger.Caster, Owner, BattleDataType.Cure, addhp);
            }
            else if(trigger.Caster != Owner)
            {
                addhp = -addhp;
                Owner.CurrentMap.RecordBattleDataHurt(trigger.Caster, Owner, BattleDataType.Cure, addhp);
            }

            MSG_ZGC_DAMAGE damageMsg = Owner.GenerateDamageMsg(Owner.InstanceId, DamageType.Cure, cure * -1);
            Owner.BroadCast(damageMsg);

            if (Owner.IsDead)
            {
                Owner.FsmManager.SetNextFsmStateType(FsmStateType.DEAD);
            }

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id{Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
