using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class ReduceDefenceRatioBuff : BaseBuff
    {
        private long changeValue = 0;
        public ReduceDefenceRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            changeValue = (long)((c *pileNum * 0.0001f) * owner.GetNatureValue(NatureType.PRO_DEF));
            owner.AddNatureAddedValue(NatureType.PRO_DEF, changeValue * -1, Model.Notify);
        }

        protected override void Pile(int addNum)
        {
            long value = (long)((c *addNum * 0.0001f) * owner.GetNatureValue(NatureType.PRO_DEF));
            changeValue += value;

            owner.AddNatureAddedValue(NatureType.PRO_DEF, value * -1, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_DEF, changeValue);
        }
    }
}
