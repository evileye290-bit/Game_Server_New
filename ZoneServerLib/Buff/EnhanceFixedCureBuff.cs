using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class EnhanceFixedCureBuff : BaseBuff
    {
        public EnhanceFixedCureBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRP_CURE_ENHANCE_FIXED, (int)c, Model.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRP_CURE_ENHANCE_FIXED, (int)c * -1);
        }
    }
}
