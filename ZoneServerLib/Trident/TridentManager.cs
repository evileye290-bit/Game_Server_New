using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class TridentManager
    {
        private PlayerChar owner;
        private TridentDbInfo dbInfo;
        public TridentDbInfo TridentDbInfo => dbInfo;
        public int Period { get; private set; }

        public TridentManager(PlayerChar player)
        {
            owner = player;
        }

        public void Init(TridentDbInfo info)
        {
            dbInfo = info;
            if (info.Tier == 0)
            {
                info.Tier = 1;
            }

            InitPeriodInfo();
        }

        public void SetPeriod(int period)
        {
            Period = period;
        }

        private void InitPeriodInfo()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.Trident, ZoneServerApi.now, out model))
            {
                Period = model.SubType;
            }
        }

        public void GetTridentShovel()
        {
            if (TridentLibrary.GetShovelProbability(Period, dbInfo.ShovelNum, dbInfo.PullTotalNum))
            {
                dbInfo.ShovelNum++;
            }
        }

        public void UpdateRechargeInfo(RechargeItemModel model)
        {
            TridentRechargeModel rechargeModel = TridentLibrary.GetTridentRechargeModel(Period, model.Id);
            if (rechargeModel == null)
            {
                Log.Error($"trident have not this recharge item model Id {model.Id}");
                return;
            }

            TridentTierRewardModel tierRewardModel = TridentLibrary.GetTridentTierRewardModel(Period, dbInfo.Tier);
            if (tierRewardModel == null)
            {
                Log.Error($"trident have not this tierRewardModel period {Period} tier {dbInfo.Tier}");
                return;
            }

            int nextGiftId = TridentLibrary.GetNextRechargeId(Period, dbInfo.PullTotalNum);
            if (nextGiftId != model.Id)
            {
                Log.Error($"trident recharge gift id {model.Id} not equal trident next rechargeId {nextGiftId}, period {Period} tier {dbInfo.Tier} total recharge count {dbInfo.PullTotalNum}");
                return;
            }

            if (dbInfo.GotRewardList.Count >= 4)
            {
                if (dbInfo.IsTierReward)
                {
                    GotoNextTier();
                }
                else
                {
                    Log.Error($"trident have had got all Reward");
                    return;
                }
            }

            TridentTierRewardRandomModel rewardRandomModel = null;
            List<TridentTierRewardRandomModel> rewardBasicInfos = tierRewardModel.RewardList.Where(x => !dbInfo.GotRewardList.Contains(x.Index)).ToList();
            if (rewardBasicInfos.Count > 0)
            {
                int totalWeight = rewardBasicInfos.Sum(x => x.Weight);
                int random = RAND.Range(0, totalWeight);
                foreach (TridentTierRewardRandomModel tridentTierRewardRandomModel in rewardBasicInfos)
                {
                    if (random <= tridentTierRewardRandomModel.Weight)
                    {
                        rewardRandomModel = tridentTierRewardRandomModel;
                        break;
                    }
                    else
                    {
                        random -= tridentTierRewardRandomModel.Weight;
                    }
                }
            }

            if (rewardRandomModel == null)
            {
                Log.Error($"reward TridentTierRewardRandomModel error ");
                return;
            }

            RewardManager rewardManager = new RewardManager();
            rewardManager.AddReward(rewardRandomModel.RewardInfo);
            rewardManager.BreakupRewards();
            owner.AddRewards(rewardManager, ObtainWay.Trident);

            dbInfo.GotRewardList.Add(rewardRandomModel.Index);
            dbInfo.PullTotalNum += 1;
            SyncTrident2Db();

            owner.SendTridentInfo();
        }

        public void GotUnlockReward()
        {
            GotoNextTier();
        }

        private void GotoNextTier()
        {
            dbInfo.Tier += 1;
            dbInfo.GotRewardList.Clear();
            dbInfo.IsTierReward = false;
            SyncTrident2Db();
        }

        public void GotTotalReward(int rewardId)
        {
            if (!dbInfo.TotalGotRewardList.Contains(rewardId))
            {
                dbInfo.TotalGotRewardList.Add(rewardId);
                SyncTrident2Db();
            }
        }

        public void Clear()
        {
            dbInfo.Reset();
            SyncTrident2Db();
            InitPeriodInfo();
            owner.SendTridentInfo();
        }

        public MSG_ZGC_TRIDENT_INFO GenerateTridentInfo()
        {
            MSG_ZGC_TRIDENT_INFO msg = new MSG_ZGC_TRIDENT_INFO(){IsTireReward = dbInfo.IsTierReward, Tier = dbInfo.Tier, PullTotalNum = dbInfo.PullTotalNum};
            msg.GotRewardList.Add(dbInfo.GotRewardList);
            msg.TotalGotRewardList.Add(dbInfo.TotalGotRewardList);
            msg.ShovelNum = dbInfo.ShovelNum;

            return msg;
        }

        public MSG_ZMZ_TRIDENT_INFO GenerateTransformInfo()
        {
            MSG_ZMZ_TRIDENT_INFO msg = new MSG_ZMZ_TRIDENT_INFO() { IsTireReward = dbInfo.IsTierReward, Tier = dbInfo.Tier, PullTotalNum = dbInfo.PullTotalNum };
            msg.GotRewardList.Add(dbInfo.GotRewardList);
            msg.TotalGotRewardList.Add(dbInfo.TotalGotRewardList);
            return msg;
        }

        public void LoadFromTransform(MSG_ZMZ_TRIDENT_INFO msg)
        {
            TridentDbInfo dbInfo = new TridentDbInfo()
            {
                Tier = msg.Tier,
                PullTotalNum = msg.PullTotalNum,
                IsTierReward = msg.IsTireReward,
            };
            dbInfo.GotRewardList = new List<int>(msg.GotRewardList);
            dbInfo.TotalGotRewardList = new List<int>(msg.TotalGotRewardList);
            Init(dbInfo);
        }

        public void SyncTrident2Db()
        {
            QueryUpdateTrident query = new QueryUpdateTrident(owner.Uid, dbInfo);
            owner.server.GameDBPool.Call(query);
        }
    }
}
