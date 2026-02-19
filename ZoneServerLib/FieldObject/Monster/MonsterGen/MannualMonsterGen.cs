using ServerModels.Monster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class MannualMonsterGen : BaseMonsterGen
    {
        // 什么都不用做，等待需要种怪的逻辑调用GenerateMonsters即可
        public MannualMonsterGen(FieldMap map, MonsterGenModel model) : base(map, model)
        {
        }

        public override void CheckGen()
        {
            
        }
    }
}
