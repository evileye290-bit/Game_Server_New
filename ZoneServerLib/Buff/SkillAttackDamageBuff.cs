using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 只受到技能伤害技能buff
    /// </summary>
    public class SkillAttackDamageBuff : BaseBuff
    {
        public SkillAttackDamageBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
