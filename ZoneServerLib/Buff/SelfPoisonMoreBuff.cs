using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class SelfPoisionMoreBuff : BaseBuff
    {
        public SelfPoisionMoreBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_POISON_ADD_MORE, (int)c * pileNum, buffModel.Notify);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureAddedValue(NatureType.PRO_POISON_ADD_MORE, (int)(c * addNum), buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_POISON_ADD_MORE, pileNum * (int)c * -1);
        }
    }
}
