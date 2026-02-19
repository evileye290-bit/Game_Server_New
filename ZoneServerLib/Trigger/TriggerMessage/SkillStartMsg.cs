using ServerModels;

namespace ZoneServerLib
{
    public class SkillStartMsg
    {
        public readonly SkillModel Model;
        public readonly FieldObject Caster;

        public SkillStartMsg(SkillModel model, FieldObject caster)
        {
            Model = model;
            Caster = caster;
        }
    }
}
