using ServerShared;
using System.Collections.Generic;
using DataProperty;
using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public partial class DungeonMap : FieldMap
    {
        //所有参与副本角色通用奖励
        protected RewardManager shardRewads = new RewardManager();

        //个人差异奖励
        protected Dictionary<int, RewardManager> differentRewards = new Dictionary<int, RewardManager>();

        //不同副本更具需求重载
        public virtual void OnMonsterDropItems(int killerId, int dropDataId)
        {
            Data data = DataListManager.inst.GetData("Monster_Drop", dropDataId);
            if (data == null)
            {
                return;
            }

            //符合要求的都掉落
            string shareRS = data.GetString("DropItem");
            if (!string.IsNullOrEmpty(shareRS))
            {
                AddAllPlayeRewards(1, shareRS);
            }

            //只掉落一项
            string randomRS = data.GetString("DropItemRandom");
            if (!string.IsNullOrEmpty(randomRS))
            {
                AddAllPlayeRewards(2, randomRS);
            }

            List<int> rwardDropIds = data.GetIntList("RewardDropId", "|");
            if (rwardDropIds.Count > 0)
            {
                AddAllPlayeRewards(rwardDropIds);
            }
        }

        protected virtual void DoReward()
        {
            //string rewardStr = DungeonModel.Data.GetString("GeneralReward");
            //if (string.IsNullOrEmpty(rewardStr))
            //{
            //    return;
            //}
            //AddShardRewards(rewardStr);

            AddAllPlayeRewards(DungeonModel.Data.GetIntList("GeneralRewardId", "|"));
        }

        protected void InitRewards(string rewardStr)
        {
            shardRewads.InitSimpleReward(rewardStr, false);
        }

        //添加共享奖励
        protected void AddShardRewards(string rewardStr)
        {
            shardRewads.AddSimpleReward(rewardStr);
        }

        protected void AddAllPlayeRewards(List<int> rwardDropIds)
        {
            foreach (var kv in PcList)
            {
                List<ItemBasicInfo> items = kv.Value.AddRewardDrop(rwardDropIds);
                AddDiffentRewards(kv.Value.Uid, items);
            }
        }

        protected void AddAllPlayeRewards(int action, string rewardStr)
        {
            List<ItemBasicInfo> items = null;
            foreach (var kv in PcList)
            {
                items = RewardDropLibrary.GetProbability(action, rewardStr);
                if (items.Count == 0)
                {
                    continue;
                }
                AddDiffentRewards(kv.Value.Uid, items);
            }
        }

        //添加不同玩家奖励
        protected void AddDiffentRewards(int uid, List<ItemBasicInfo> items)
        {
            RewardManager mng = null;
            if (!this.differentRewards.TryGetValue(uid, out mng))
            {
                mng = new RewardManager();
                this.differentRewards[uid] = mng;
            }
            mng.AddReward(items);
        }

        protected RewardManager GetFinalReward(int uid)
        {
            RewardManager reward;
            if (differentRewards.TryGetValue(uid, out reward))
            {
                reward.AddReward(shardRewads.AllRewards);
                return reward;
            }
            else
            {
                reward = new RewardManager();
                reward.AddReward(shardRewads.AllRewards);
            }

            return reward;
        }

        protected void ResetReward()
        {
            shardRewads = null;
            differentRewards.Clear();
        }
    }
}
