using System;
using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class AddHpTriHdl : BaseTriHdl
    {
        private readonly int hp = 0;
        public AddHpTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out hp))
            {
                Log.Warn("in add hp tri hdl: invalid param {0}", handlerParam);
                return;
            }
            hp = trigger.CalcParam(handlerType, hp);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            long maxHP = Owner.GetNatureValue(NatureType.PRO_MAX_HP);
            long currHP = Owner.GetNatureValue(NatureType.PRO_HP);
            long addhp = hp;
            if (addhp + currHP > maxHP)
            {
                addhp = maxHP - currHP;
            }
            // 不能调用UpdateHp 消息会循环发送
            Owner.AddNatureBaseValue(NatureType.PRO_HP, addhp);
            Owner.BroadCastHp();

            Owner.CurrentMap.RecordBattleDataHurt(trigger.Caster, Owner, BattleDataType.Cure, addhp);

            MSG_ZGC_DAMAGE damageMsg = Owner.GenerateDamageMsg(Owner.InstanceId, DamageType.Cure, hp * -1);
            Owner.BroadCast(damageMsg);
        }
    }
}
