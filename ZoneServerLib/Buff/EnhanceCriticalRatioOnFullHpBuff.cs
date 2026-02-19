using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class EnhanceCriticalRatioOnFullHpBuff : BaseBuff
    {
        private bool effected = false;
        public EnhanceCriticalRatioOnFullHpBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.DamageOnce, OnDamage);
            AddListener(TriggerMessageType.AddHp, OnAddHp);
            if (owner.FullHp())
            {
                Effect();
            }
        }

        protected override void Update(float dt)
        {
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.DamageOnce, OnDamage);
            RemoveListener(TriggerMessageType.AddHp, OnAddHp);
            // 属性还原
            if (effected)
            {
                UnEffect();
            }
        }

        private void Effect()
        {
            effected = true;
            owner.AddNatureAddedValue(NatureType.PRO_CRI_RATE, (int)c, Model.Notify);
        }

        private void UnEffect()
        {
            effected = false;
            owner.AddNatureAddedValue(NatureType.PRO_CRI_RATE, (int)c * -1);
        }

        private void OnDamage(object param)
        {
            DamageTriMsg msg = param as DamageTriMsg;
            if (msg == null)
            {
                return;
            }
            if (effected && msg.Damage > 0)
            {
                UnEffect();
            }
        }

        private void OnAddHp(object param)
        {
            long hp;
            if (!long.TryParse(param.ToString(), out hp))
            {
                return;
            }
            if (!effected && Owner.GetHp() + hp >= Owner.GetMaxHp())
            {
                Effect();
            }
        }
    }
}
