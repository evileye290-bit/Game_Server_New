using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class ReduceArmourBuff : BaseBuff
    {
        public ReduceArmourBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ARM, (int)c * -1, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ARM, (int)c);
        }
    }
}
