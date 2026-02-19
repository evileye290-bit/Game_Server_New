using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class DamageMoreToControlledBuff : BaseBuff
    {
        public DamageMoreToControlledBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_DAM_TO_CTR, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_DAM_TO_CTR, (int)c * -1);
        }
    }
}
