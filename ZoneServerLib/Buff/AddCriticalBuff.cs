using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddCriticalBuff : BaseBuff
    {
        public AddCriticalBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_CRI, (int)c, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_CRI, (int)c * -1);
        }
    }
}
