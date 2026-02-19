using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddDefSDamBuff : BaseBuff
    {
        public AddDefSDamBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_DEF_SDAM, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_DEF_SDAM, (int)c * -1);
        }
    }
}
