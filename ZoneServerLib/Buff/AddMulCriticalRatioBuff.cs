using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddMulCriticalRatioBuff : BaseBuff
    {
        public AddMulCriticalRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_MUL_CRI, (int)c * pileNum, Model.Notify);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureAddedValue(NatureType.PRO_MUL_CRI, (int)(c * addNum), buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_MUL_CRI, pileNum * (int)c * -1);
        }
    }
}
