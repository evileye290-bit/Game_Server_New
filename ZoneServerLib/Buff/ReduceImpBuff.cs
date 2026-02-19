using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class ReduceImpBuff : BaseBuff
    {
        public ReduceImpBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_IMP, (int)c * -1, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_IMP, (int)c);
        }
    }
}
