using ServerModels.Monster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class UtilMonsterLessGen : BaseMonsterGen
    {
        // key other monster gen id, value less condition
        Dictionary<int, int> monsterLessList = new Dictionary<int, int>();
        private bool operated = false;
        public UtilMonsterLessGen(FieldMap map, MonsterGenModel model):base(map, model)
        {
            // 101:2|102:1
            string[] strArr = model.GenParam.Split('|');
            foreach(var str in strArr)
            {
                if(string.IsNullOrEmpty(str))
                {
                    continue;
                }
                string[] kv = str.Split(':');
                if (!string.IsNullOrEmpty(kv[0]) && kv.Length == 2)
                {
                    monsterLessList.Add(int.Parse(kv[0]), int.Parse(kv[1]));
                }
            }
        }

        public override void CheckGen()
        {
            if (operated) return;
            foreach(var condition in monsterLessList)
            {
                // 该区域还没有种过怪
                BaseMonsterGen gen = curMap.GetMonsterGen(condition.Key);
                if(gen == null || !gen.Generated)
                {
                    return;
                }

                int aliveCount = curMap.GetAliveMonsterCountByGenId(condition.Key);
                if(aliveCount >= condition.Value)
                {
                    return;
                }
            }

            // 检查通过
            GenerateMonstersDelay(Model.GenCount, Model.GenDelay);
            operated = true;
        }
    }
}
