using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddFleeBuff : BaseBuff
    {
        public AddFleeBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_FLEE, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_FLEE, (int)c * -1);
        }
    }
}
