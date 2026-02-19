using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 只收到普攻伤害技能buff
    /// </summary>
    public class NormalAttackDamageBuff : BaseBuff
    {
        public NormalAttackDamageBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
        }
    }
}

