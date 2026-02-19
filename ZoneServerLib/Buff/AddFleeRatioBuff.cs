using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddFleeRatioBuff : BaseBuff
    {
        public AddFleeRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureRatio(NatureType.PRO_FLEE, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureRatio(NatureType.PRO_FLEE, (int)c * -1);
        }
    }
}
