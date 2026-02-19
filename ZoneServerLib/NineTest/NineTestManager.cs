using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerModels;
using ServerShared;
using EnumerateUtility;
using DBUtility;
using Message.Zone.Protocol.ZM;

namespace ZoneServerLib
{
    public class NineTestManager
    {
        private PlayerChar owner;
        private NineTestInfo info;
        public NineTestInfo Info { get { return info; } }

        public NineTestManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(NineTestInfo info)
        {
            this.info = info;
            
            RechargeGiftModel model;
            if (RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.NineTest, ZoneServerApi.now, out model))
            {
                int period = model.SubType;
                NineTestConfig config = NineTestLibrary.GetConfig(period);
                if (config != null)
                {
                    if (info.CurRewards.Count > 0)
                    {
                        RecordCurRewardsInfo(config.RewardTypeWeight);
                    }
                }
            }
        }

        public void CheckInit(int period)
        {
            //没玩过需初始化当前奖励
            if (info.CurRewards.Count == 0)
            {
                NineTestConfig config = NineTestLibrary.GetConfig(period);
                if (config != null)
                {
                    InitNewRoundRewards(config);
                }
            }
        }

        /// <summary>
        /// 初始化新一局的奖励
        /// </summary>
        /// <param name="config"></param>
        private void InitNewRoundRewards(NineTestConfig config)
        {
            Dictionary<int, int> rewardCounts = config.RandomRewardTypeCount();

            List<string> rewardList = new List<string>();
            List<int> itemIdList = new List<int>();
            foreach (var kv in rewardCounts)
            {
                List<RandomRewardModel> randomRewards = NineTestLibrary.GetRandomRewardList(config.Id, kv.Key);
                owner.GenerateNoneRepeateRandomReward(rewardList, itemIdList, randomRewards, kv.Value, info.CurRewards);
            }

            RecordCurRewardsInfo(config.RewardTypeWeight);

            SyncDbUpdateRewardInfo();
        }

        private void RecordCurRewardsInfo(Dictionary<int, int> rewardTypeWeight)
        {
            info.CurRewardsWeight.Clear();
            RandomRewardModel rewardModel;
            int totalWeight = 0;
            foreach (int rewardId in info.CurRewards)
            {
                rewardModel = NineTestLibrary.GetRandomReward(rewardId);
                if (rewardModel == null || info.IndexRewards.Values.Contains(rewardModel.Id))
                {
                    continue;
                }
                int weight;
                rewardTypeWeight.TryGetValue(rewardModel.RewardType, out weight);
                totalWeight += weight;
                info.CurRewardsWeight.Add(totalWeight, rewardModel);
            }
        }

        public RandomRewardModel GetRandomReward(Dictionary<int, int> rewardTypeWeight)
        {
            return info.GetRandomReward(rewardTypeWeight);
        }

        public void UpdateIndexRewardInfo(int index, int rewardId, int score)
        {
            info.IndexRewards.Add(index, rewardId);
            info.Score += score;

            SyncDbUpdateRewardInfo();
        }
        
        public void UpdateScoreRewards(int rewardId)
        {
            info.ScoreRewards.Add(rewardId);
            SyncDbUpdateScoreRewards();
        }

        public void Reset(NineTestConfig config)
        {
            info.CurRewards.Clear();
            info.IndexRewards.Clear();
            InitNewRoundRewards(config);
        }

        private void SyncDbUpdateRewardInfo()
        {
            owner.server.GameDBPool.Call(new QueryUpdateNineTestRewardInfo(owner.Uid, info.Score, info.CurRewards.ToString("|"), info.IndexRewards.ToString("|", ":")));
        }

        private void SyncDbUpdateScoreRewards()
        {
            owner.server.GameDBPool.Call(new QueryUpdateNineTestScoreRewards(owner.Uid, string.Join("|", info.ScoreRewards)));
        }

        public void Clear()
        {
            info.Clear();
            RechargeGiftModel model;
            if (RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.NineTest, ZoneServerApi.now, out model))
            {
                int period = model.SubType;
                NineTestConfig config = NineTestLibrary.GetConfig(period);
                if (config != null)
                {
                    InitNewRoundRewards(config);
                }
            }
        }

        public MSG_ZMZ_NINE_TEST GenerateTransformMsg()
        {
            MSG_ZMZ_NINE_TEST msg = new MSG_ZMZ_NINE_TEST();
            msg.Score = info.Score;
            msg.CurRewards.AddRange(info.CurRewards);
            info.IndexRewards.ForEach(x=> msg.IndexRewards.Add(x.Key, x.Value));
            msg.ScoreRewards.AddRange(info.ScoreRewards);
            info.CurRewardsWeight.ForEach(x=>msg.CurRewardsWeight.Add(x.Key, x.Value.Id));
            return msg;
        }

        public void LoadTransformMsg(MSG_ZMZ_NINE_TEST msg)
        {
            info = new NineTestInfo();
            info.Score = msg.Score;
            info.CurRewards.AddRange(msg.CurRewards);
            msg.IndexRewards.ForEach(x=>info.IndexRewards.Add(x.Key, x.Value));
            info.ScoreRewards.AddRange(msg.ScoreRewards);
            RandomRewardModel rewardModel;
            foreach (var rewardWeight in msg.CurRewardsWeight)
            {
                rewardModel = NineTestLibrary.GetRandomReward(rewardWeight.Value);
                info.CurRewardsWeight.Add(rewardWeight.Key, rewardModel);
            }
        }
    }
}
