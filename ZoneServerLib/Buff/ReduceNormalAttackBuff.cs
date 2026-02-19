using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class ReduceNormalAttackBuff : BaseBuff
    {
        public ReduceNormalAttackBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
        //protected override void Start()
        //{
        //    owner.AddNatureAddedValue(NatureType.PRO_NORMAL_ATK, (int)c * -1, Model.Notify);
        //}

        //protected override void End()
        //{
        //    owner.AddNatureAddedValue(NatureType.PRO_NORMAL_ATK, (int)c * 1);
        //}

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_NORMAL_ATK, (int)c * pileNum * -1, buffModel.Notify);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureAddedValue(NatureType.PRO_NORMAL_ATK, (int)(c * addNum * -1), buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_NORMAL_ATK, pileNum * (int)c);
        }
    }
}
