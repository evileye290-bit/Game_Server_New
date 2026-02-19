using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddAttackBuff : BaseBuff
    {
        public AddAttackBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ATK, (int)c, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ATK, (int)c * -1);
        }
    }
}
