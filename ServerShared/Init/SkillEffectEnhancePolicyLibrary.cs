using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class SkillEffectEnhancePolicyLibrary
    {
        private static Dictionary<int, SkillEffectEnhancePolicy> enhancePolicyList = new Dictionary<int, SkillEffectEnhancePolicy>();
        public static void Init()
        {
            //enhancePolicyList.Clear();
            Dictionary<int, SkillEffectEnhancePolicy> enhancePolicyList = new Dictionary<int, SkillEffectEnhancePolicy>();

            DataList dataList = DataListManager.inst.GetDataList("SkillEffectEnhancePolicy");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!enhancePolicyList.ContainsKey(item.Key))
                {
                    enhancePolicyList.Add(item.Key, new SkillEffectEnhancePolicy(data));
                }
            }
            SkillEffectEnhancePolicyLibrary.enhancePolicyList = enhancePolicyList;
        }

        public static SkillEffectEnhancePolicy GetSkillEnhanceEffectPolicy(int id)
        {
            SkillEffectEnhancePolicy policy = null;
            enhancePolicyList.TryGetValue(id, out policy);
            return policy;
        }
    }
}
