using DataProperty;
using EnumerateUtility;
using Logger;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class CampGatherLibrary
    {
        //key:id
        private static Dictionary<int, CampGatherModel> gatherList = new Dictionary<int, CampGatherModel>();
        //dungeonId
        private static Dictionary<int, CampGatherModel> gatherDic = new Dictionary<int, CampGatherModel>();

        private static Dictionary<int, CampScoreRuleModel> scoreRuleList = new Dictionary<int, CampScoreRuleModel>();

        private static Dictionary<int, CampBattleExpendModel> expendList = new Dictionary<int, CampBattleExpendModel>();

        public static void Init()
        {        
            //gatherList.Clear();
            //gatherDic.Clear();
            //scoreRuleList.Clear();
            //expendList.Clear();
     
            DataList campGather = DataListManager.inst.GetDataList("CampGather");
            DataList campScoreRule = DataListManager.inst.GetDataList("CampScoreRule");
            DataList campExpend = DataListManager.inst.GetDataList("CampBattleConfig");

            InitGather(campGather);
            InitCampScoreRule(campScoreRule);
            InitCampExpend(campExpend);
        }

        private static void InitGather(DataList dataList)
        {
            Dictionary<int, CampGatherModel> gatherList = new Dictionary<int, CampGatherModel>();
            Dictionary<int, CampGatherModel> gatherDic = new Dictionary<int, CampGatherModel>();
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampGatherModel model = new CampGatherModel(data);
                gatherList.Add(model.Id, model);

                if (model.DungeonId != 0 && !gatherDic.ContainsKey(model.DungeonId))
                {
                    gatherDic.Add(model.DungeonId, model);
                }
            }
            CampGatherLibrary.gatherList = gatherList;
            CampGatherLibrary.gatherDic = gatherDic;
        }

        private static void InitCampScoreRule(DataList dataList)
        {
            Dictionary<int, CampScoreRuleModel> scoreRuleList = new Dictionary<int, CampScoreRuleModel>();
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampScoreRuleModel model = new CampScoreRuleModel(data);
                scoreRuleList.Add(model.ID, model);
            }
            CampGatherLibrary.scoreRuleList = scoreRuleList;
        }

        private static void InitCampExpend(DataList dataList)
        {
            Dictionary<int, CampBattleExpendModel> expendList = new Dictionary<int, CampBattleExpendModel>();
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampBattleExpendModel model = new CampBattleExpendModel(data);
                expendList.Add(model.Id, model);
            }
            CampGatherLibrary.expendList = expendList;
        }

        private static Dictionary<int, CampGatherModel> GetCampGatherWeightDic()
        {
            int weight;
            int weights = 0;
            int totalWeight = 0;
            List<int> list = new List<int>();
            foreach (var item in gatherList)
            {
                weights += item.Value.Weight;
                list.Add(item.Key);
            }

            Dictionary<int, CampGatherModel> weightDic = new Dictionary<int, CampGatherModel>();
            for (int i = 0; i < gatherList.Count; i++)
            {
                if (i == gatherList.Count - 1)
                {
                    weight = 10000 - totalWeight;
                }
                else
                {
                    weight = gatherList[list[i]].Weight * 10000 / weights;
                }
                totalWeight += weight;
                weightDic.Add(totalWeight, gatherList[list[i]]);
            }
            return weightDic;
        }

        public static CampGatherModel GetCampGatherRandomEvent(int rand)
        {
            Dictionary<int, CampGatherModel> weightDic = GetCampGatherWeightDic();
            Dictionary<int, CampGatherModel> decWeightDic = weightDic.OrderByDescending(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
            int weight = 0;
            foreach (var item in decWeightDic)
            {
                if (rand < item.Key)
                {
                    weight = item.Key;
                }
            }
            CampGatherModel model;
            weightDic.TryGetValue(weight, out model);
            return model;
        }

        public static CampGatherModel GetCampGatherByDungeonId(int dungeonId)
        {
            CampGatherModel model;
            gatherDic.TryGetValue(dungeonId, out model);
            return model;
        }

        public static CampGatherModel GetCampGatherById(int id)
        {
            CampGatherModel model;
            gatherList.TryGetValue(id, out model);
            return model;
        }

        public static CampScoreRuleModel GetBattleStepScore(int id)
        {
            CampScoreRuleModel model;
            scoreRuleList.TryGetValue(id, out model);
            return model;
        }

        public static CampBattleExpendModel GetCampBattleExpend(int id = 1)
        {
            CampBattleExpendModel model;
            expendList.TryGetValue(id, out model);
            return model;
        }

        public static bool CheckCampGatherHasDungeon(int dungeonId)
        {
            if (gatherDic.ContainsKey(dungeonId))
            {
                return true;
            }
            return false;
        }
    }
}
