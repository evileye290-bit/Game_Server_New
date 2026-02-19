using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class SelfPositionReduceBuff : BaseBuff
    {
        public SelfPositionReduceBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_POISON_REDUCE, (int)c, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_POISON_REDUCE, (int)c * -1);
        }
    }
}
