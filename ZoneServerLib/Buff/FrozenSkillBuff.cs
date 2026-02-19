using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class FrozenSkillBuff : BaseBuff
    {
        public FrozenSkillBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (owner.SkillManager != null)
            {
                owner.SkillManager.FrozenSkill((SkillType)c);
            }
        }

        protected override void End()
        {
            if (owner.SkillManager != null)
            {
                owner.SkillManager.UnFrozenSkill((SkillType)c);
            }
        }
    }
}
