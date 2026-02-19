using Logger;
using ServerModels.Monster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class MonsterGenFactory
    {
        public static BaseMonsterGen CreateMonsterGen(FieldMap map, MonsterGenModel model)
        {
            switch(model.GenType)
            {
                case EnumerateUtility.MonsterGenType.Common:
                    return new CommonMonsterGen(map, model);
                case EnumerateUtility.MonsterGenType.Mannual:
                    return new MannualMonsterGen(map, model);
                case EnumerateUtility.MonsterGenType.UntilMonsterLess:
                    return new UtilMonsterLessGen(map, model);
                case EnumerateUtility.MonsterGenType.GodHeroCount:
                    return new GodHeroCountGen(map, model);
                default:
                    Log.Warn($"create monster regen {model.GenType} failed: not supported yet");
                    return new BaseMonsterGen(map, model);
            }
        }
    }
}
