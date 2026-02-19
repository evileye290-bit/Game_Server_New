using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddDefenceRatioBuff : BaseBuff
    {
        public AddDefenceRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureRatio(NatureType.PRO_DEF, (int)c * pileNum, buffModel.Notify);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureRatio(NatureType.PRO_DEF, (int)(c * addNum), buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureRatio(NatureType.PRO_DEF, pileNum * (int)c * -1);
        }
    }
}
