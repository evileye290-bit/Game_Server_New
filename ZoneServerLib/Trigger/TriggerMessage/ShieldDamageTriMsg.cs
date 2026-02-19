using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ShieldDamageTriMsg
    {
        public readonly FieldObject Caster;
        public long Damage;
        public ShieldDamageTriMsg(FieldObject caster, long damage)
        {
            Caster = caster;
            Damage = damage;
        }
    }
}
