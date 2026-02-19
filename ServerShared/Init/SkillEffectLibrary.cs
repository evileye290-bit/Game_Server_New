using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class SkillEffectLibrary
    {
        private static Dictionary<int, SkillEffectModel> skillEffectList = new Dictionary<int, SkillEffectModel>();

        public static void Init()
        {
            Dictionary<int, SkillEffectModel> skillEffectList = new Dictionary<int, SkillEffectModel>();
            //skillEffectList.Clear();
            DataList dataList = DataListManager.inst.GetDataList("SkillEffect");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!skillEffectList.ContainsKey(item.Key))
                {
                    skillEffectList.Add(item.Key, new SkillEffectModel(data));
                }
            }
            SkillEffectLibrary.skillEffectList = skillEffectList;
        }

        public static SkillEffectModel GetSkillEffectModel(int skillEffId)
        {
            SkillEffectModel skillEffectModel = null;
            skillEffectList.TryGetValue(skillEffId, out skillEffectModel);
            return skillEffectModel;
        }
    }
}
