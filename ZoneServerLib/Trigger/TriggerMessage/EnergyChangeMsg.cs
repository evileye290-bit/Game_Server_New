using ServerModels;

namespace ZoneServerLib
{
    class EnergyChangeMsg
    {
        public readonly SkillModel Model;
        public readonly FieldObject Target;

        public EnergyChangeMsg(SkillModel model, FieldObject target)
        {
            Model = model;
            Target = target;
        }
    }
}