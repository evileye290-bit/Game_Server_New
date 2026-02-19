using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class ReduceDamageBuff : BaseBuff
    {
        public ReduceDamageBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG, (int)m, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG, (int)m * -1);
        }
    }
}
