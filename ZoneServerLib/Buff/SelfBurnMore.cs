using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class SelfBurnMoreBuff : BaseBuff
    {
        public SelfBurnMoreBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_BURN_ADD_MORE, (int)c, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_BURN_ADD_MORE, (int)c * -1);
        }
    }
}
