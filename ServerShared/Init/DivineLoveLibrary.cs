using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class DivineLoveLibrary
    {
        private static Dictionary<int, DivineLoveConfig> divineLoveConfigs = new Dictionary<int, DivineLoveConfig>();
        private static Dictionary<int, DivineLoveRewardModel> divineLoveRewards = new Dictionary<int, DivineLoveRewardModel>();
        private static Dictionary<int, DivineLoveFetterRewardModel> divineLoveFetterRewards = new Dictionary<int, DivineLoveFetterRewardModel>();

        public static void Init()
        {
            InitDivineLoveConfig();
            InitDivineLoveRewards();
            InitDivineLoveFetterRewards();
        }

        private static void InitDivineLoveConfig()
        {
            Dictionary<int, DivineLoveConfig> divineLoveConfigs = new Dictionary<int, DivineLoveConfig>();

            DataList dataList = DataListManager.inst.GetDataList("FlipCards");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                DivineLoveConfig info = new DivineLoveConfig(data);
                divineLoveConfigs.Add(info.Type, info);
            }
            DivineLoveLibrary.divineLoveConfigs = divineLoveConfigs;
        }

        private static void InitDivineLoveRewards()
        {
            Dictionary<int, DivineLoveRewardModel> divineLoveRewards = new Dictionary<int, DivineLoveRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("FlipCardsReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                DivineLoveRewardModel info = new DivineLoveRewardModel(data);
                divineLoveRewards.Add(info.Id, info);
            }
            DivineLoveLibrary.divineLoveRewards = divineLoveRewards;
        }

        private static void InitDivineLoveFetterRewards()
        {
            Dictionary<int, DivineLoveFetterRewardModel> divineLoveFetterRewards = new Dictionary<int, DivineLoveFetterRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("FlipCardsFettersReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                DivineLoveFetterRewardModel info = new DivineLoveFetterRewardModel(data);
                divineLoveFetterRewards.Add(info.Id, info);
            }
            DivineLoveLibrary.divineLoveFetterRewards = divineLoveFetterRewards;
        }

        public static DivineLoveRewardModel GetDivineLoveReward(int id)
        {
            DivineLoveRewardModel info;
            divineLoveRewards.TryGetValue(id, out info);
            return info;
        }

        public static DivineLoveFetterRewardModel GetDivineLoveFetterReward(int id)
        {
            DivineLoveFetterRewardModel info;
            divineLoveFetterRewards.TryGetValue(id, out info);
            return info;
        }

        public static DivineLoveRewardModel GetDivineLoveByCardId(int cardId)
        {
            DivineLoveRewardModel info = null;
            foreach (var item in divineLoveRewards)
            {
                if (item.Value.CardId == cardId)
                {
                    return item.Value;
                }
            }
            return info;
        }

        public static DivineLoveConfig GetDivineLoveConfig(int type)
        {
            DivineLoveConfig info;
            divineLoveConfigs.TryGetValue(type, out info);
            return info;
        }
    }
}
