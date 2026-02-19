using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class ReduceDamageRatioBuff : BaseBuff
    {
        public ReduceDamageRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, m * pileNum, buffModel.Notify);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, m * addNum, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, pileNum * m * -1);
        }
    }
}

