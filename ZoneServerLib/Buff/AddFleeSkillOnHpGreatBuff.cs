using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;

namespace ZoneServerLib
{
    // 血量高于百分比c时 将概率 x 赋值给 技能闪避属性 PRO_FLEE_SKL
    public class AddFleeSkillOnHpGreatBuff : BaseBuff
    {
        private bool effected = false;
        public AddFleeSkillOnHpGreatBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.DamageOnce, OnDamage);
            AddListener(TriggerMessageType.AddHp, OnAddHp);
            if(owner.HpGreaterThanRate((int)c))
            {
                Effect();
            }
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
            owner.AddNatureAddedValue(NatureType.PRO_FLEE_SKL, x, Model.Notify);
        }

        private void UnEffect()
        {
            effected = false; ;
            owner.AddNatureAddedValue(NatureType.PRO_FLEE_SKL, -1 * x);
        }

        private void OnDamage(object param)
        {
            // 已起效 且血量低于要求值 则取消加成
            if(effected && !owner.HpGreaterThanRate((int)c))
            {
                UnEffect();
            }
        }

        // 受控buff结束 如果没有其他受控buff起效，则应将减伤属性还原
        private void OnAddHp(object param)
        {
            // 未起效 且满足起效条件 则起效
            if(!effected && owner.HpGreaterThanRate((int)c))
            {
                Effect();
            }
        }
    }
}
