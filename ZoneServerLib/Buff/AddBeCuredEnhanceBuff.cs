using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class AddBeCuredEnhanceBuff : BaseBuff
    {
        public AddBeCuredEnhanceBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_BECURED_ENHANCE, (int)c, buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_BECURED_ENHANCE, (int)c * -1);
        }
    }
}
