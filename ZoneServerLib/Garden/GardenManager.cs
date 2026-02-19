using CommonUtility;
using DBUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using System;
using System.Linq;
using EnumerateUtility;
using ServerShared;

namespace ZoneServerLib
{
    public class GardenManager : BasePeriodActivity
    {
        private PlayerChar owner;
        private GardenInfo gardenInfo;

        public GardenInfo GardenInfo => gardenInfo;

        public GardenManager(PlayerChar player) : base(RechargeGiftType.Garden)
        {
            owner = player;
        }

        public void Init(GardenInfo gardenInfo)
        {
            this.gardenInfo = gardenInfo;
            InitPeriodInfo();
        }

        public override void Clear()
        {
            gardenInfo.Reset();
            SyncUpdateToDB();
            InitPeriodInfo();
        }

        public SeedInfo GetSeedInfo(int pit)
        {
            SeedInfo seedInfo;
            gardenInfo.SeedEndTime.TryGetValue(pit, out seedInfo);
            return seedInfo;
        }

        public SeedInfo PlantSeed(int pit, int seedId, int time)
        {
            if (gardenInfo.SeedEndTime.ContainsKey(pit)) return null;

            DateTime endTime = owner.server.Now().AddSeconds(time);

            SeedInfo seedInfo = new SeedInfo() { Pit = pit, SeedId = seedId, EndTime = Timestamp.GetUnixTimeStampSeconds(endTime) };

            gardenInfo.SeedEndTime.Add(pit, seedInfo);

            SyncUpdateToDB();

            return seedInfo;
        }

        public void HarvestPitSeed(int pit, int score)
        {
            if (gardenInfo.SeedEndTime.ContainsKey(pit))
            {
                gardenInfo.SeedEndTime.Remove(pit);

                RechargeGiftModel model;
                if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.Garden, ZoneServerApi.now, out model))
                {
                    gardenInfo.Score += score;
                }

                UpdateRank(); 
                SyncUpdateToDB();
            }
        }

        public int MaxScordRewardId()
        {
            return gardenInfo.ScoreRewardList.Count > 0 ? gardenInfo.ScoreRewardList.Max() : 0;
        }

        public bool AddRewardId(int id)
        {
            if (!gardenInfo.ScoreRewardList.Contains(id))
            { 
                gardenInfo.ScoreRewardList.Add(id);
                SyncUpdateToDB();
                return true;
            }
            return false;
        }

        public MSG_ZGC_GARDEN_INFO GenerateGardenInfo()
        {
            MSG_ZGC_GARDEN_INFO msg = new MSG_ZGC_GARDEN_INFO();
            msg.Score = gardenInfo.Score;
            gardenInfo.SeedEndTime.ForEach(x => msg.SeedInfos.Add(new MSG_SEED_INFO() { Pit = x.Key, FinishTime = x.Value.EndTime, SeedId = x.Value.SeedId }));
            msg.RewardId = MaxScordRewardId();
            return msg;
        }

        public MSG_ZMZ_GARDEN_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_GARDEN_INFO msg = new MSG_ZMZ_GARDEN_INFO();
            msg.Score = gardenInfo.Score;
            gardenInfo.SeedEndTime.ForEach(x => msg.SeedInfos.Add(new ZMZ_SEED_INFO() { Pit = x.Key, FinishTime = x.Value.EndTime, SeedId = x.Value.SeedId }));
            msg.RewardId.AddRange(gardenInfo.ScoreRewardList);
            return msg;
        }

        public void LoadFromTransform(MSG_ZMZ_GARDEN_INFO info)
        {
            GardenInfo gardenInfo = new GardenInfo() { Score = info.Score };
            gardenInfo.ScoreRewardList.AddRange(info.RewardId);

            info.SeedInfos.ForEach(x =>
            {
                gardenInfo.SeedEndTime.Add(x.Pit, new SeedInfo() { Pit = x.Pit, SeedId = x.SeedId, EndTime = x.FinishTime });
            });

            this.gardenInfo = gardenInfo;
            InitPeriodInfo();
        }

        public void UpdateRank()
        {
            owner.SerndUpdateRankValue(EnumerateUtility.RankType.Garden, gardenInfo.Score);
        }

        public void SyncUpdateToDB()
        {
            QueryUpdateGardenInfo query = new QueryUpdateGardenInfo(owner.Uid, gardenInfo);
            owner.server.GameDBPool.Call(query);
        }
    }
}
