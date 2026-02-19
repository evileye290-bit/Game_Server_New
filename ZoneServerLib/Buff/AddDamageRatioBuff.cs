using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddDamageRatioBuff : BaseBuff
    {
        public AddDamageRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ADD_DMG_RATIO, (int)c * pileNum, Model.Notify);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureRatio(NatureType.PRO_ADD_DMG_RATIO, (int)(c * addNum), buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ADD_DMG_RATIO, (int)c * pileNum * -1);
        }
    }
}
