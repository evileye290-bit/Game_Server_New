using DBUtility;
using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CanoeManager
    {
        private PlayerChar owner { get; set; }
        private CanoeInfo info = new CanoeInfo();
        public CanoeInfo Info { get { return info; } }

        public CanoeManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(CanoeInfo info)
        {
            this.info = info;
        }

        public void UpdateNpcDistance(int distance)
        {
            info.NpcDistance = distance;
        }

        public void UpdateCurDistance(int addDistance)
        {
            info.CurDistance += addDistance;           
        }

        public void UpdateTrainCount()
        {
            info.TrainCount++;
            SyncDbUpdateCanoeTrainCount();
        }

        public void UpdateScore(int score)
        {
            info.Score = score;
        }

        public void UpdateRank(int score)
        {
            owner.SerndUpdateRankValue(RankType.Canoe, score);
        }

        public void UpdateGetMatchRewards(int rewardId)
        {
            info.MatchRewards.Add(rewardId);
            SyncDbUpdateCanoeMatchRewards();
        }

        public void UpdateCostState(bool hasCost)
        {
            info.HasCost = hasCost;
        }

        public void Clear()
        {
            info.CurDistance = 0;
            info.Score = 0;
            info.TrainCount = 0;
            info.NpcDistance = 0;
            info.MatchRewards.Clear();
        }

        public void SyncDbUpdateCanoeInfo()
        {
            owner.server.GameDBPool.Call(new QueryUpdateCanoeInfo(owner.Uid, info.CurDistance, info.Score));
        }

        private void SyncDbUpdateCanoeTrainCount()
        {
            owner.server.GameDBPool.Call(new QueryUpdateCanoeTrainCount(owner.Uid, info.TrainCount));
        }

        private void SyncDbUpdateCanoeMatchRewards()
        {
            owner.server.GameDBPool.Call(new QueryUpdateCanoeMatchRewards(owner.Uid, string.Join("|", info.MatchRewards)));
        }
    }
}
