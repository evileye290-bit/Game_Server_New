using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddResistanceBuff : BaseBuff
    {
        public AddResistanceBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_RES, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_RES, (int)c * -1);
        }
    }
}
