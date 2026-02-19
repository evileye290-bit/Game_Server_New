using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class EnchanceCureBuff : BaseBuff
    {
        public EnchanceCureBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_CURE, (int)c * pileNum, buffModel.Notify);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureAddedValue(NatureType.PRO_CURE, (int)(c * addNum), buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_CURE, pileNum * (int)c * -1);
        }
    }
}
