using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddFinalFleeRatioBuff : BaseBuff
    {
        public AddFinalFleeRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_FLEE_FIN_RATIO, (int)c, Model.Notify);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureAddedValue(NatureType.PRO_FLEE_FIN_RATIO, (int)(c * addNum), buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_FLEE_FIN_RATIO, (int)c * pileNum * -1);
        }
    }
}
