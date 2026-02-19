using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public CanoeManager CanoeManager { get; private set; }
        public void InitCanoeManager()
        {
            CanoeManager = new CanoeManager(this);
        }

        public void GetCanoeInfoByLoading()
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.Canoe, ZoneServerApi.now, out model))
            {
                return;
            }
            GetCanoeInfo();
        }

        public void GetCanoeInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_RANK_FIRST_INFO msg = new MSG_ZR_GET_RANK_FIRST_INFO() { RankType = (int)RankType.Canoe };
            server.SendToRelation(msg, Uid);
        }

        public MSG_ZGC_CANOE_INFO GenerateCanoeInfo()
        {
            MSG_ZGC_CANOE_INFO msg = new MSG_ZGC_CANOE_INFO();
            msg.CurDistance = CanoeManager.Info.CurDistance;
            msg.MatchRewards.AddRange(CanoeManager.Info.MatchRewards);
            msg.TrainCount = CanoeManager.Info.TrainCount;
            return msg;
        }

        public void CanoeGameStart(int type)
        {          
            MSG_ZGC_CANOE_GAME_START response = new MSG_ZGC_CANOE_GAME_START();
            response.Type = type;

            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.Canoe, ZoneServerApi.now))
            {
                Log.Warn($"player {Uid} CanoeGameStart failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Write(response);
                return;
            }

            CanoeConfig config = CanoeLibrary.GetCanoeConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} CanoeGameStart failed: not find type {type} in config");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }
          
            bool hasCost = false;
            if ((ActivityPlayType)type == ActivityPlayType.Normal)
            {
                int needCost = config.GetCostDiamond(CanoeManager.Info.TrainCount + 1);
                if (GetCoins(CurrenciesType.diamond) < needCost)
                {
                    Log.Warn($"player {Uid} CanoeGameStart failed: diamond {GetCoins(CurrenciesType.diamond)} not enough, need num {needCost}");
                    response.Result = (int)ErrorCode.DiamondNotEnough;
                    Log.Write(response);
                    return;
                }
                DelCoins(CurrenciesType.diamond, needCost, ConsumeWay.CanoeTrain, type.ToString());

                CanoeManager.UpdateTrainCount();

                hasCost = true;
            }
            else
            {
                BaseItem item = BagManager.GetItem(MainType.Consumable, config.TicketId);
                if (item == null || item.PileNum < config.CostTicket)
                {
                    Log.Warn($"player {Uid} CanoeGameStart failed: ticket not enough");
                    response.Result = (int)ErrorCode.ItemNotEnough;
                    Log.Write(response);
                    return;
                }

                BaseItem it = DelItem2Bag(item, RewardType.NormalItem, config.CostTicket, ConsumeWay.CanoeMatch);
                if (it != null)
                {
                    SyncClientItemInfo(it);
                }

                hasCost = true;
            }

            CanoeManager.UpdateCostState(hasCost);

            int npcDistance = config.RandNpcDistance();
            CanoeManager.UpdateNpcDistance(npcDistance);
            response.NpcDistance = npcDistance;

            List<int> directionList = CanoeLibrary.RandDirections(config.MaxOperateCount);
            response.Directions.AddRange(directionList);
            response.Result = (int)ErrorCode.Success;

            Write(response);
        }

        public void CanoeGameEnd(int type, int index)
        {
            MSG_ZGC_CANOE_GAME_END response = new MSG_ZGC_CANOE_GAME_END();
            response.Type = type;

            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.Canoe, ZoneServerApi.now))
            {
                Log.Warn($"player {Uid} CanoeGameEnd failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Write(response);
                return;
            }

            CanoeConfig config = CanoeLibrary.GetCanoeConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} CanoeGameEnd failed: not find type {type} in config");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }

            if (index > config.MaxOperateCount)
            {
                Log.Warn($"player {Uid} CanoeGameEnd failed: illegal operate");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }

            if (CanoeManager.Info.NpcDistance == 0)
            {
                Log.Warn($"player {Uid} CanoeGameEnd failed: npcDistance error");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }

            if (!CanoeManager.Info.HasCost)
            {
                Log.Warn($"player {Uid} CanoeGameEnd failed: not cost diamond or item");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }
            //重置消费状态
            CanoeManager.UpdateCostState(false);

            int addDistance = config.BasicDistance + index * config.SpeedUpDistance;
            addDistance = Math.Min(addDistance, config.OnceMaxDistance);

            if ((ActivityPlayType)type == ActivityPlayType.High)
            {
                CanoeManager.UpdateCurDistance(addDistance);
            }

            List<string> rewards = CanoeLibrary.GetOnceDistanceRewardByType(type, addDistance);
            if (rewards.Count > 0)
            {
                RewardManager manager = new RewardManager();
                List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();
                //按有装备和魂骨生成奖励
                foreach (var reward in rewards)
                {
                    if (!string.IsNullOrEmpty(reward))
                    {
                        RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                        List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        rewardItems.AddRange(items);
                    }
                }
                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                if ((ActivityPlayType)type == ActivityPlayType.Normal)
                {
                    AddRewards(manager, ObtainWay.CanoeTrain);
                }
                else
                {
                    AddRewards(manager, ObtainWay.CanoeMatch);
                }
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.CurDistance = CanoeManager.Info.CurDistance;
            response.TrainCount = CanoeManager.Info.TrainCount;
            response.Result = (int)ErrorCode.Success;
            Write(response);

            if ((ActivityPlayType)type == ActivityPlayType.High)
            {
                int score = addDistance - CanoeManager.Info.NpcDistance;
                if (score > 0 && CanoeManager.Info.Score < score)
                {
                    CanoeManager.UpdateScore(score);
                    CanoeManager.UpdateRank(score);
                }
                CanoeManager.SyncDbUpdateCanoeInfo();
            }
        }

        public void CanoeGetReward(int rewardId)
        {
            MSG_ZGC_CANOE_GET_REWARD response = new MSG_ZGC_CANOE_GET_REWARD();
            response.RewardId = rewardId;

            RechargeGiftModel model;
            if (!RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.Canoe, ZoneServerApi.now, out model))
            {
                Log.Warn($"player {Uid} CanoeGetReward failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Write(response);
                return;
            }

            CanoeMathchReward rewardModel = CanoeLibrary.GetCanoeMatchReward(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} CanoeGetReward failed: not find reward {rewardId} in xml");
                response.Result = (int)ErrorCode.Fail;
                Log.Write(response);
                return;
            }

            if (CanoeManager.Info.MatchRewards.Contains(rewardId))
            {
                Log.Warn($"player {Uid} CanoeGetReward failed: already got reward {rewardId}");
                response.Result = (int)ErrorCode.AlreadyGot;
                Log.Write(response);
                return;
            }

            if (CanoeManager.Info.CurDistance < rewardModel.AvailableDistance)
            {
                Log.Warn($"player {Uid} CanoeGetReward {rewardId} failed: availableDistance {rewardModel.AvailableDistance} curDistance {CanoeManager.Info.CurDistance}");
                response.Result = (int)ErrorCode.NotReachGetCondition;
                Log.Write(response);
                return;
            }

            CanoeManager.UpdateGetMatchRewards(rewardId);

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();
            if (!string.IsNullOrEmpty(rewardModel.Reward))
            {
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
            }
            manager.AddReward(rewardItems);
            manager.BreakupRewards(true);
            AddRewards(manager, ObtainWay.CanoeMatch);
            manager.GenerateRewardMsg(response.Rewards);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void ClearCanoeInfo()
        {
            CanoeManager.Clear();
            //GetCanoeInfo();
        }

        public MSG_ZMZ_CANOE_INFO GenerateCanoeTransformMsg()
        {
            MSG_ZMZ_CANOE_INFO msg = new MSG_ZMZ_CANOE_INFO()
            {
                CurDistance = CanoeManager.Info.CurDistance,
                Score = CanoeManager.Info.Score,
                TrainCount = CanoeManager.Info.TrainCount,
                NpcDistance = CanoeManager.Info.NpcDistance,
            };
            msg.MatchRewards.AddRange(CanoeManager.Info.MatchRewards);
            return msg;
        }

        public void LoadCanoeTransform(MSG_ZMZ_CANOE_INFO msg)
        {
            CanoeInfo info = new CanoeInfo()
            {
                CurDistance = msg.CurDistance,
                Score = msg.Score,
                TrainCount = msg.TrainCount,
                NpcDistance = msg.NpcDistance
            };
            info.MatchRewards.AddRange(msg.MatchRewards);
            CanoeManager.Init(info);
        }
    }
}
