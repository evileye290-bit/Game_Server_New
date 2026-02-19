using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class ReduceArmourRatioBuff : BaseBuff
    {
        private long changeValue = 0;
        public ReduceArmourRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            changeValue = (long)((c * 0.0001f) * owner.GetNatureValue(NatureType.PRO_ARM));
            owner.AddNatureAddedValue(NatureType.PRO_ARM, changeValue * -1, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ARM, changeValue);
        }
    }
}
