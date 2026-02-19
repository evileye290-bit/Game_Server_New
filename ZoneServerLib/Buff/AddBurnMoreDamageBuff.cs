using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddBurnMoreDamageBuff : BaseBuff
    {
        public AddBurnMoreDamageBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_BURN_MORE, (int)c, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_BURN_MORE, (int)c * -1);
        }
    }
}
