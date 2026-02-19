using CommonUtility;
using ServerModels;

namespace ZoneServerLib.Buff
{
    public class EnhanceCureEffectBuff : BaseBuff
    {
        public EnhanceCureEffectBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            caster.AddNatureAddedValue(NatureType.PRO_CURE_ENHANCE, (long)c, Model.Notify);
        }

        protected override void End()
        {
            caster.AddNatureAddedValue(NatureType.PRO_CURE_ENHANCE, (long)c * -1);
        }
    }
}
