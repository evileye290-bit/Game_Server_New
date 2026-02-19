using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class MixSkillLibrary
    {
        // key heroId_1 << 32 + heroId_2, value data id,用于客户端反查显示融合技名字
        private static Dictionary<long, int> heroCompose = new Dictionary<long, int>();
        private static Dictionary<int, MixSkillModel> policyList = new Dictionary<int, MixSkillModel>();
        public static void Init()
        {
            //heroCompose.Clear();
            //policyList.Clear();
            Dictionary<long, int> heroCompose = new Dictionary<long, int>();
            Dictionary<int, MixSkillModel> policyList = new Dictionary<int, MixSkillModel>();

            DataList dataList = DataListManager.inst.GetDataList("MixSkillHero");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                long heroId_1 = (long)data.GetInt("Hero_1");
                long heroId_2 = (long)data.GetInt("Hero_2");

                long key; // 小id在前
                if(heroId_1 <=  heroId_2)
                {
                    key = (heroId_1 << 32) + heroId_2;
                }
                else
                {
                    key = (heroId_2 << 32) + heroId_1;
                }

                if (heroCompose.ContainsKey(key))
                {
                    Logger.Log.Warn($"hero {heroId_1} and {heroId_2} already exist!");
                }
                else
                {
                    heroCompose.Add(key, data.ID);
                }
            }

            dataList = DataListManager.inst.GetDataList("MixSkillPolicy");
            foreach (var item in dataList)
            {
                policyList.Add(item.Key, new MixSkillModel(item.Value));
            }

            MixSkillLibrary.heroCompose = heroCompose;
            MixSkillLibrary.policyList = policyList;
        }

        // 返回对应的compose id
        public static bool TryGetHeroComposeId(long heroId_1, long heroId_2, out int id)
        {
            long key;
            if (heroId_1 <= heroId_2)
            {
                key = (heroId_1 << 32) + heroId_2;
            }
            else
            {
                key = (heroId_2 << 32) + heroId_1;
            }
            return heroCompose.TryGetValue(key, out id);
        }


        public static MixSkillModel GetMixSkillModel(int id)
        {
            MixSkillModel model = null;
            policyList.TryGetValue(id, out model);
            return model;
        }

    }
}
