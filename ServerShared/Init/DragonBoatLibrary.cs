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
    public static class DragonBoatLibrary
    {
        //key1: period, key2:rewardId
        private static Dictionary<int, SortedDictionary<int, DragonBoatRewardModel>> rewardList = new Dictionary<int, SortedDictionary<int, DragonBoatRewardModel>>();
        private static Dictionary<int, List<int>> directionDic = new Dictionary<int, List<int>>();
        private static Dictionary<int, DragonBoatTicketModel> ticketDic = new Dictionary<int, DragonBoatTicketModel>();
        public static int MaxDistance { get; private set; }
        public static int MaxOperateCount { get; private set; }
        public static int BasicDistance { get; private set; }
        public static int SpeedUpDistance { get; private set; }
        public static int OnceTicketMaxBuyCount { get; private set; }

        public static void Init()
        {
            BindDragonBoatConfig();
            BindDragonBoatReward();
            BindDragonBoatDirection();
            BindDragonBoatTicket();
        }

        private static void BindDragonBoatConfig()
        {
            Data data = DataListManager.inst.GetData("DragonBoatConfig", 1);
            MaxDistance = data.GetInt("MaxDistance");
            MaxOperateCount = data.GetInt("MaxOperateCount");
            BasicDistance = data.GetInt("BasicDistance");
            SpeedUpDistance = data.GetInt("SpeedUpDistance");
            OnceTicketMaxBuyCount = data.GetInt("OnceTicketMaxBuyCount");
        }

        private static void BindDragonBoatReward()
        {
            Dictionary<int, SortedDictionary<int, DragonBoatRewardModel>> rewardList = new Dictionary<int, SortedDictionary<int, DragonBoatRewardModel>>();
            int period = 1;
            while (true)
            {
                DataList dataList = DataListManager.inst.GetDataList("DragonBoatReward_" + period);
                if (dataList != null)
                {
                    InitPeriodDragonBoatReward(dataList, period, rewardList);
                }
                else
                {
                    Logger.Log.Info($"DragonBoatReward inited with max period {period - 1}");
                    break;
                }
                period++;
            }
            DragonBoatLibrary.rewardList = rewardList;
        }

        private static void InitPeriodDragonBoatReward(DataList dataList, int period, Dictionary<int, SortedDictionary<int, DragonBoatRewardModel>> rewardList)
        {
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                DragonBoatRewardModel item = new DragonBoatRewardModel(data, period);

                //if (item.IsLoop > 0 && item.AvailableLevel < dailyRewardLoopBegin)
                //{
                //    dailyRewardLoopBegin = item.AvailableLevel;
                //    dailyRewardLoopGap = item.LoopGap;

                //}
                SortedDictionary<int, DragonBoatRewardModel> dic;
                if (!rewardList.TryGetValue(item.Period, out dic))
                {
                    dic = new SortedDictionary<int, DragonBoatRewardModel>();
                    rewardList.Add(item.Period, dic);
                }
                dic.Add(item.Id, item);
            }
        }

        private static void BindDragonBoatDirection()
        {
            Dictionary<int, List<int>> directionDic = new Dictionary<int, List<int>>();

            DataList dataList = DataListManager.inst.GetDataList("DragonBoatDirection");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int index = data.GetInt("Index");
                List<int> list;
                if (!directionDic.TryGetValue(index, out list))
                {
                    list = new List<int>();
                    directionDic.Add(index, list);
                }
                list.Add(data.ID);
            }
            DragonBoatLibrary.directionDic = directionDic;
        }

        private static void BindDragonBoatTicket()
        {
            Dictionary<int, DragonBoatTicketModel> ticketDic = new Dictionary<int, DragonBoatTicketModel>();
            DataList dataList = DataListManager.inst.GetDataList("DragonBoatTicket");
            foreach (var item in dataList)
            {
                DragonBoatTicketModel model = new DragonBoatTicketModel(item.Value);
                ticketDic.Add(model.Id, model);
            }

            DragonBoatLibrary.ticketDic = ticketDic;
        }

        public static DragonBoatTicketModel GetTicketModelByPeriod(int period)
        {
            DragonBoatTicketModel model;
            ticketDic.TryGetValue(period, out model);
            return model;
        }

        public static List<int> RandDirections()
        {
            List<int> randList = new List<int>();
            List<int> indexList;
            for (int i = 1; i <= MaxOperateCount; i++)
            {
                if (directionDic.TryGetValue(i, out indexList))
                {
                    int index = NewRAND.Next(0, indexList.Count-1);
                    randList.Add(indexList[index]);
                }
            }
            return randList;
        }

        public static List<string> GetCurDistanceLeftRewards(int period, int curDistance, int lastDistance, int bought)
        {
            List<string> rewards = new List<string>();          
            SortedDictionary<int, DragonBoatRewardModel> dic;
            rewardList.TryGetValue(period, out dic);
            if (dic == null)
            {
                return rewards;
            }
            foreach (var item in dic)
            {
                if (item.Value.AvailableDistance <= curDistance && item.Value.AvailableDistance > lastDistance)
                {
                    if (item.Value.RewardType == 2 && bought != 1)
                    {
                        continue;
                    }
                    rewards.Add(item.Value.Reward);
                }
            }           
            return rewards;
        }

        public static List<string> GetAvailableRightsRewards(int period, int curDistance)
        {
            List<string> rewards = new List<string>();        
            SortedDictionary<int, DragonBoatRewardModel> dic;
            rewardList.TryGetValue(period, out dic);
            if (dic == null)
            {
                return rewards;
            }
            foreach (var item in dic)
            {
                if (item.Value.AvailableDistance <= curDistance && item.Value.RewardType == 2)
                {
                    rewards.Add(item.Value.Reward);
                }
            }          
            return rewards;
        }
    }
}
