using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class AddMaxHpBuff : BaseBuff
    {
        public AddMaxHpBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            long oldMaxHp = owner.GetNatureValue(NatureType.PRO_MAX_HP);
            owner.AddNatureAddedValue(NatureType.PRO_MAX_HP, (int)c, true);
            long newMaxHp = owner.GetNatureValue(NatureType.PRO_MAX_HP);
            long addValue = newMaxHp - oldMaxHp;
            owner.AddNatureAddedValue(NatureType.PRO_HP, addValue, Model.Notify);
            //owner.CurrentMap.RecordBattleDataCure(caster, owner, BattleDataType.Cure, (int)c);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_MAX_HP, (int)c * -1);
        }
    }
}
