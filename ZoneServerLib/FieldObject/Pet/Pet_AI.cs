using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    partial class Pet
    {
        private float hateRange { get; set; }
        private float hateRefreshTime { get; set; }
        private float forgiveTime = float.MaxValue;

        public override void InitAI()
        {
            autoAI = true;
            skillEngine = new SkillEngine(this);
            hateManager = new HateManager(this, hateRange, hateRefreshTime, forgiveTime);
            BindTriggers();
            PassiveSkillEffect();
        }
    }
}
