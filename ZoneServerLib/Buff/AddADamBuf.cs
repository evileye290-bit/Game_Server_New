using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddADamBuf : BaseBuff
    {
        public AddADamBuf(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ADAM, (int)c, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ADAM, (int)c * -1);
        }
    }
}


