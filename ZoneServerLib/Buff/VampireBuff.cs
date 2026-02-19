using ServerModels;

namespace ZoneServerLib
{
    public class VampireBuff : BaseBuff
    {
        private float value;
        public VampireBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            value = c;
        }
        
        protected override void Pile(int addNum)
        {
            value = c * pileNum;
        }


        public override void SpecLogic(object param)
        {
            int damage = 0;
            if (!int.TryParse(param.ToString(), out damage))
            {
                return;
            }
            long addHp = (long)(damage * value * 0.0001f);
            if (addHp > 0)
            {
                owner.AddHp(owner, addHp);
            }
        }
    }
}
