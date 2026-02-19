using CommonUtility;
using DBUtility;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ShreklandManager
    {
        private PlayerChar owner;

        private int rewardLevel;
        public int RewardLevel { get { return rewardLevel; } }
        
        private int stepIndex;
        public int StepIndex { get { return stepIndex; } }

        private int score;
        public int Score { get { return score; } }

        private List<int> gridRewards;
        private List<int> scoreRewards;

        public ShreklandManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(ShreklandInfo info)
        {
            rewardLevel = info.RewardLevel;
            stepIndex = info.StepIndex;
            score = info.Score;
            gridRewards = info.GridRewards;
            scoreRewards = info.ScoreRewards;
            InitRandomRewards();
        }

        public List<int> GetGridRewards()
        {
            return gridRewards;
        }

        public List<int> GetScoreRewards()
        {
            return scoreRewards;
        }

        public void InitRandomRewards()
        {
            if (gridRewards.Count != 0) return;            
            int rewardType = ShreklandLibrary.GetRewardTypeByRewardLevel(RewardLevel);
            List<ShreklandRandReward> rewardList = ShreklandLibrary.GetRandomRewardList(rewardType);
            if (rewardList == null) return;
            gridRewards.AddRange(GenerateRandomReward(rewardList, ShreklandLibrary.GridNum));
            SyncDbUpdateGridRewards();
        }

        public List<int> GenerateRandomReward(List<ShreklandRandReward> rewardList, int num)
        {
            List<int> rewards = new List<int>();
            int rewardId;
            if (rewardList == null)
            {
                return rewards;
            }
            int totalWeight;
            SortedDictionary<int, ShreklandRandReward> weightDic = GetRewardListWeight(rewardList, out totalWeight);
            for (int i = 0; i < num; i++)
            {
                rewardId = GetRandomRewardByWeight(weightDic, totalWeight);
                if (rewardId != 0)
                {
                    rewards.Add(rewardId);
                }
            }
            return rewards;
        }

        private SortedDictionary<int, ShreklandRandReward> GetRewardListWeight(List<ShreklandRandReward> rewardList, out int totalWeight)
        {
            totalWeight = 0;
            SortedDictionary<int, ShreklandRandReward> weightDic = new SortedDictionary<int, ShreklandRandReward>();
            foreach (var rewardModel in rewardList)
            {
                totalWeight += rewardModel.Weight;
                weightDic.Add(totalWeight, rewardModel);
            }
            return weightDic;
        }

        private int GetRandomRewardByWeight(SortedDictionary<int, ShreklandRandReward> weightDic, int totalWeight)
        {
            int rewardId = 0;
            int rand = NewRAND.Next(1, totalWeight);
            int cur = 0;
            foreach (var kv in weightDic)
            {
                if (rand > cur && rand <= kv.Key)
                {
                    rewardId = kv.Value.Id;
                    break;
                }
                cur = kv.Key;
            }

            return rewardId;
        }

        public Tuple<int, int> GetNextStepIndex()
        {
            List<ShreklandRandReward> neighborRewards = new List<ShreklandRandReward>();
            for (int i = 1; i <= ShreklandLibrary.StepNumMax; i++)//需考虑超出终点情况
            {
                int rewardIndex = GetNextStepIndexByStepNum(i);
                int rewardId = gridRewards[rewardIndex];
                ShreklandRandReward rewardModel = ShreklandLibrary.GetRandomReward(rewardId);
                if (rewardModel != null)
                {
                    neighborRewards.Add(rewardModel);
                }
            }
            if (neighborRewards.Count == 0)
            {
                Logger.Log.Warn($"player {owner.Uid} UseShreklandRoulette step forward num 0");
            }
            int totalWeight;
            SortedDictionary<int, ShreklandRandReward> rewardsByWeight = GetRewardListWeight(neighborRewards, out totalWeight);
            int finalRewardId = GetRandomRewardByWeight(rewardsByWeight, totalWeight);
            int stepNum = neighborRewards.FindIndex(x => x.Id == finalRewardId) + 1;
            int nextStepIndex = GetNextStepIndexByStepNum(stepNum);
            return new Tuple<int, int>(stepNum, nextStepIndex);
        }

        public int GetNextStepIndexByStepNum(int stepNum)
        {
            int nextIndex = StepIndex + stepNum <= ShreklandLibrary.GridNum - 1 ? StepIndex + stepNum : StepIndex + stepNum - ShreklandLibrary.GridNum;
            return nextIndex;
        }

        public void UseRoulette(int newStepIndex, int factor)
        {
            AddScore(factor);
            //到达或超过终点
            if (newStepIndex < StepIndex)
            {
                int oldRewardType = ShreklandLibrary.GetRewardTypeByRewardLevel(RewardLevel);
                RewardLevelUp();
                //刷新奖池
                RewardsPoolLevelUp(oldRewardType);
            }
            SetStepIndex(newStepIndex);
            SyncDbUpdateShreklandInfo();
        }

        private void SetStepIndex(int stepIndex)
        {
            this.stepIndex = stepIndex;
        }

        private void AddScore(int factor)
        {
            int onceScore = ShreklandLibrary.GetOnceAddScoreByRewardLevel(RewardLevel);
            score += onceScore * factor;
        }

        private void RewardLevelUp()
        {
            if (RewardLevel < ShreklandLibrary.MaxRewardLevel)
            {
                rewardLevel++;
            }
        }

        private void RewardsPoolLevelUp(int oldRewardType)
        {
            int newRewardType = ShreklandLibrary.GetRewardTypeByRewardLevel(RewardLevel);
            if (newRewardType == oldRewardType)
            {
                List<ShreklandRandReward> rewardList = ShreklandLibrary.GetRandomRewardList(newRewardType);
                if (rewardList == null) return;
                RefreshGridRewards(GenerateRandomReward(rewardList, ShreklandLibrary.GridNum));
                return;
            }
            List<int> upRewards = new List<int>();
            foreach (int rewardId in gridRewards)
            {
                ShreklandRandReward rewardModel = ShreklandLibrary.GetRandomReward(rewardId);
                if (rewardModel == null) continue;
                ShreklandRandReward upReward = ShreklandLibrary.GetRandomReward(rewardModel.LevelUpRewardId);
                if (upReward != null)
                {
                    upRewards.Add(upReward.Id);
                }
                else if(rewardModel.RewardType == ShreklandLibrary.MaxRewardType)
                {
                    upRewards.Add(rewardModel.Id);
                }
                else
                {
                    upRewards.Add(0);
                    Logger.Log.Warn($"player {owner.Uid} UseRoulette rewardPool levelUp param error");
                }
            }
            RefreshGridRewards(upRewards);
        }

        public bool RefreshRandomRewards()
        {
            int rewardType = ShreklandLibrary.GetRewardTypeByRewardLevel(RewardLevel);
            List<ShreklandRandReward> rewardList = ShreklandLibrary.GetRandomRewardList(rewardType);
            if (rewardList == null) return false;
            RefreshGridRewards(GenerateRandomReward(rewardList, ShreklandLibrary.GridNum));
            SyncDbUpdateGridRewards();
            return true;
        }

        private void RefreshGridRewards(List<int> newRewards)
        {
            gridRewards.Clear();
            gridRewards.AddRange(newRewards);
        }

        public void AddScoreReward(int rewardId)
        {
            scoreRewards.Add(rewardId);
            SyncDbUpdateScoreRewards();
        }

        private void SyncDbUpdateShreklandInfo()
        {
            owner.server.GameDBPool.Call(new QueryUpdateShreklandInfo(owner.Uid, RewardLevel, StepIndex, Score, gridRewards.ToString("|"), scoreRewards.ToString("|")));
        }

        private void SyncDbUpdateGridRewards()
        {
            owner.server.GameDBPool.Call(new QueryUpdateShreklandGridRewards(owner.Uid, gridRewards.ToString("|")));
        }

        private void SyncDbUpdateScoreRewards()
        {
            owner.server.GameDBPool.Call(new QueryUpdateShreklandScoreRewards(owner.Uid, scoreRewards.ToString("|")));
        }

        public void Clear()
        {
            rewardLevel = 1;
            stepIndex = 0;
            score = 0;
            gridRewards.Clear();
            scoreRewards.Clear();
        }

        public MSG_ZMZ_SHREKLAND_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_SHREKLAND_INFO msg = new MSG_ZMZ_SHREKLAND_INFO();
            msg.RewardLevel = RewardLevel;
            msg.StepIndex = StepIndex;
            msg.Score = Score;
            msg.GridRewards.AddRange(GetGridRewards());
            msg.ScoreRewards.AddRange(GetScoreRewards());
            return msg;
        }

        public void LoadTransformMsg(MSG_ZMZ_SHREKLAND_INFO msg)
        {
            rewardLevel = msg.RewardLevel;
            stepIndex = msg.StepIndex;
            score = msg.Score;
            gridRewards = new List<int>();
            gridRewards.AddRange(msg.GridRewards);
            scoreRewards = new List<int>();
            scoreRewards.AddRange(msg.ScoreRewards);
        }        
    }
}
