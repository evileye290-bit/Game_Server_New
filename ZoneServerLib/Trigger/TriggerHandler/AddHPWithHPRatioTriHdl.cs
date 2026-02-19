using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class AddHPWithHPRatioTriHdl : BaseTriHdl
    {
        private readonly float ratio;
        public AddHPWithHPRatioTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int value;
            if (!int.TryParse(handlerParam, out value))
            {
                Log.Warn("in AddHPWithHPRatioTriHdl: invalid param {0}", handlerParam);
                return;
            }
            ratio = value * 0.0001f;
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            long maxHP = Owner.GetNatureValue(NatureType.PRO_MAX_HP);
            long currHP = Owner.GetNatureValue(NatureType.PRO_HP);
            long hp = (long) ((maxHP - currHP) * ratio);

            if (hp + currHP > maxHP)
            {
                hp = maxHP - currHP;
            }

            if(hp<=0) return;

            // 不能调用UpdateHp 消息会循环发送
            Owner.AddNatureBaseValue(NatureType.PRO_HP, hp);
            Owner.CheckDead();
            Owner.BroadCastHp();

            Owner.CurrentMap.RecordBattleDataHurt(trigger.Caster, Owner, BattleDataType.Cure, hp);

            MSG_ZGC_DAMAGE damageMsg = Owner.GenerateDamageMsg(Owner.InstanceId, DamageType.Cure, hp * -1);
            Owner.BroadCast(damageMsg);
        }
    }
}
