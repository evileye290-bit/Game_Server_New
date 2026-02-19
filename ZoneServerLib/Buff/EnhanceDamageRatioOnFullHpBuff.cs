using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class EnhanceDamageRatioOnFullHpBuff : BaseBuff
    {
        private bool effected = false;
        public EnhanceDamageRatioOnFullHpBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
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
            owner.AddNatureAddedValue(NatureType.PRO_DAM, (int)c, Model.Notify);
        }

        private void UnEffect()
        {
            effected = false; ;
            owner.AddNatureAddedValue(NatureType.PRO_DAM, (int)c * -1);
        }

        private void OnDamage(object param)
        {
            if (effected && !owner.FullHp())
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
            if (!effected && IsFullHp(hp))
            {
                Effect();
            }
        }

        private bool IsFullHp(long addHp)
        {
            long hp = Owner.GetHp() + addHp;
            long maxHp = Owner.GetNatureValue(NatureType.PRO_MAX_HP);
            return hp >= maxHp;
        } 
    }
}
