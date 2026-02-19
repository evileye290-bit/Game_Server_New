using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public class InvincibleBuff : BaseBuff
    {
        public InvincibleBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

    }
}
