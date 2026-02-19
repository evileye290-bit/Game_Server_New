using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddAttackRatioBuff : BaseBuff
    {
        public  AddAttackRatioBuff (FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureRatio(NatureType.PRO_ATK, (int)c * pileNum, buffModel.Notify);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureRatio(NatureType.PRO_ATK, (int)(c * addNum), buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureRatio(NatureType.PRO_ATK, pileNum * (int)c * -1);
        }
    }
}
