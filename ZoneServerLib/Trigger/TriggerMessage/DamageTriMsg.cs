using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class DamageTriMsg
    {
        public readonly FieldObject Caster,Target;
        public readonly DamageType DamageType;
        public readonly long Damage;

        public DamageTriMsg(FieldObject caster, DamageType damageType, long damage, FieldObject target)
        {
            Caster = caster;
            Target = target;
            DamageType = damageType;
            Damage = damage;
        }
    }
}
