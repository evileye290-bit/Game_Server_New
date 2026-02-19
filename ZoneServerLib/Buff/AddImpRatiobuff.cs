using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddImpRatioBuff : BaseBuff
    {
        public AddImpRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureRatio(NatureType.PRO_IMP, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureRatio(NatureType.PRO_IMP, (int)c * -1);
        }
    }
}
