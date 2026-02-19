using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class ReduceHitRatioBuff : BaseBuff
    {
        private long changeValue = 0;
        public ReduceHitRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            changeValue = (long)((c * 0.0001f) * owner.GetNatureValue(NatureType.PRO_HIT));
            owner.AddNatureAddedValue(NatureType.PRO_HIT, changeValue * -1, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_HIT, changeValue);
        }
    }
}
