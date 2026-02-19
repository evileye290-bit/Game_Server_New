using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class DelayBuff
    {
        public readonly FieldObject Caster;
        public readonly int BuffId;
        public readonly int SkillLevel;
        public readonly float DelayTime;
        private float elapsedTime;

        public DelayBuff (FieldObject caster, int buffId, int skillLevel, float delayTime)
        {
            Caster = caster;
            BuffId = buffId;
            SkillLevel = skillLevel;
            DelayTime = delayTime;
            elapsedTime = 0f;
        }

        public bool Check(float dt)
        {
            elapsedTime += dt;
            return elapsedTime >= DelayTime;
        }
    }
}
