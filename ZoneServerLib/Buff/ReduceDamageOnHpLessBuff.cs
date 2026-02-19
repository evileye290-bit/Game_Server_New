using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ReduceDamageOnHpLessBuff : BaseBuff
    {
        private bool effected = false;
        public ReduceDamageOnHpLessBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.DamageOnce, OnDamage);
            AddListener(TriggerMessageType.AddHp, OnAddHp);
            if (owner.HpLessThanRate((int)c))
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
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, n, Model.Notify);
        }

        private void UnEffect()
        {
            effected = false; ;
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, -1 * n);
        }

        private void OnDamage(object param)
        {
            // 未起效 且满足起效条件 则起效
            if (!effected && owner.HpLessThanRate((int)c))
            {
                Effect();
            }
        }

        // 受控buff结束 如果没有其他受控buff起效，则应将减伤属性还原
        private void OnAddHp(object param)
        {      
            // 已起效 且血量高于要求值 则取消加成
            if (effected && !owner.HpLessThanRate((int)c))
            {
                UnEffect();
            }
        }
    }
}
