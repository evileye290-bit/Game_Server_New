using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddDefADamBuff : BaseBuff
    {
        public AddDefADamBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_DEF_ADAM, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_DEF_ADAM, (int)c * -1);
        }
    }
}
