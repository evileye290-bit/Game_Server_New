using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddHitRatioBuff : BaseBuff
    {
        public AddHitRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureRatio(NatureType.PRO_HIT, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureRatio(NatureType.PRO_HIT, (int)c * -1);
        }
    }
}
