using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class AddNormalSkill1EnergyBuff : BaseBuff
    {
        public AddNormalSkill1EnergyBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (happened) return;
            if (owner.SkillManager.HasActiveSkill && !owner.IsDead)
            {
                owner.SkillManager.AddEnergy(SkillType.Normal_Skill_1, (int)c, true, true, true);
            }
            happened = true;
            isEnd = true;
        }
    }
}
