using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    class CriticalTriMsg
    {
        public readonly SkillModel Model;
        public readonly FieldObject Target;
        public readonly int Damage;

        public CriticalTriMsg(SkillModel model, FieldObject target, int damage)
        {
            Model = model;
            Target = target;
            Damage = damage;
        }
    }
}
