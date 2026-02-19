using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddMaxHPRatioBuff : BaseBuff
    {
        public AddMaxHPRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            ChangeHp(pileNum);
        }

        protected override void Pile(int addNum)
        {
            ChangeHp(addNum);
        }

        protected override void End()
        {
            owner.AddNatureRatio(NatureType.PRO_MAX_HP, (int)c * pileNum* -1);
        }

        private void ChangeHp(int num)
        {
            long oldMaxHp = owner.GetNatureValue(NatureType.PRO_MAX_HP);
            owner.AddNatureRatio(NatureType.PRO_MAX_HP, (int)c * num, Model.Notify);

            long newMaxHp = owner.GetNatureValue(NatureType.PRO_MAX_HP);
            long addValue = newMaxHp - oldMaxHp;
            owner.AddNatureAddedValue(NatureType.PRO_HP, addValue);

            //魂师传记buff给怪加减血上限百分比buff
            if (caster == owner && addValue < 0) return;

            owner.CurrentMap.RecordBattleDataCure(caster, owner, BattleDataType.Cure, addValue);
        }
    }
}
