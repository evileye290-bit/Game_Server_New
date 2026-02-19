using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public class ShieldByNatureBuff : ShieldBuff
    {
        NatureType natureType;

        public ShieldByNatureBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            natureType = (NatureType)x;

            //该buff 使用x参数作为属性类型 m作为百分比
        }

        protected override void Start()
        {
            int shieldValue = (int)(owner.GetNatureValue(natureType) * (0.0001f * m));
            owner.AddNatureAddedValue(NatureType.PRO_SHIELD_MAX_HP, shieldValue, Model.Notify);
            owner.AddNatureAddedValue(NatureType.PRO_SHIELD_HP, shieldValue, Model.Notify);
            owner.BroadCastHp();
        }
    }
}

