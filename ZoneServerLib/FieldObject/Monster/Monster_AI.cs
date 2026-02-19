using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class Monster
    {
        public override void InitAI()
        {
            InitFSM();

            hateManager = new HateManager(this, monsterModel.HateRange, monsterModel.HateRefreshTime, monsterModel.ForgiveTime);

            BindTriggers();
            PassiveSkillEffect();
        }
    }
}
