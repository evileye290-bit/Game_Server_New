using CommonUtility;
using ServerModels.Monster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CommonMonsterGen : BaseMonsterGen
    {
        private bool operated = false;
        public CommonMonsterGen(FieldMap map, MonsterGenModel model) : base(map, model)
        {
        }

        public override void Update(float dt)
        {
            if(curDungeon == null || curDungeon.State <= DungeonState.Open)
            {
                return;
            }

            base.Update(dt);

            if(!operated)
            {
                // 种怪
                GenerateMonstersDelay(Model.GenCount, Model.GenDelay);
                operated = true;
            }
        }

    }
}
