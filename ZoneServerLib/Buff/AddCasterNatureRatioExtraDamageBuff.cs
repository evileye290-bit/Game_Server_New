using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddCasterNatureRatioExtraDamageBuff : BaseBuff
    {
        public AddCasterNatureRatioExtraDamageBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_EXTRA_DAMAGE, (int)c, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_EXTRA_DAMAGE, (int)c * -1);
        }
    }
}


