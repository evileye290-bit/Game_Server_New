using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddHitBuff : BaseBuff
    {
        public AddHitBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_HIT, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_HIT, (int)c * -1);
        }
    }
}
