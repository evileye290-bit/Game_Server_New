using EnumerateUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class FixedBuff : BaseBuff
    {
        public FixedBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if(owner.IsMoving)
            {
                owner.OnMoveStop();
                owner.BroadCastStop();
            }
        }
    }
}
