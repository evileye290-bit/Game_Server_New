using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerModels.HidderWeapon;
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
        public DevilTrainingManager DevilTrainingMng { get; private set; }

        public void InitDevilTrainingManager()
        {
            DevilTrainingMng = new DevilTrainingManager(this);
        }
        public void SendDevilTrainingInfoByLoading()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DevilTraining, ZoneServerApi.now, out model))
            {
                SendDevilTrainingInfo();
            }
        }


        public void GetDevilTrainingReward(int type, bool isConsecutive, bool useDiamond)
        {
            DevilTrainingConfig config = DevilTrainingLibrary.GetDevilTrainingConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} DevilTrainingConifg failed: not type {type}");
                return;
            }
            DevilTrainingInfo info = DevilTrainingMng.GetDevilTrainingInfo(type);
            if (info == null)
            {
                Log.Warn($"player {Uid} DevilTrainingInfo failed: not type {type}");
                return;
            }
            MSG_ZGC_GET_DEVIL_TRAINING_REWARD msg = new MSG_ZGC_GET_DEVIL_TRAINING_REWARD();
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.DevilTraining, ZoneServerApi.now, out activityModel))
            {
                msg.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get deviltraining reward failed: not open");
                Write(msg);
                return;
            }

            int costItemNum = 1;
            int costDiamondNum = 0;
            if (isConsecutive)
            {
                costItemNum = config.ConsecutiveCount;
            }

            BaseItem item = BagManager.GetItem(MainType.Consumable, config.CostItem);

            if (item != null)
            {
                if (item.PileNum < costItemNum)
                {
                    if (useDiamond)
                    {
                        if (config.CostDiamond > 0)
                        {
                            //使用钻石
                            costDiamondNum = (costItemNum - item.PileNum) * config.CostDiamond;
                            costItemNum = item.PileNum;
                            //使用钻石
                            if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                            {
                                Log.Warn($"player {Uid}GetDevilTrainingReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                                msg.Result = (int)ErrorCode.DiamondNotEnough;
                                Write(msg);
                                return;
                            }
                        }
                        else
                        {
                            //不是用钻石
                            Log.Warn($"player {Uid}GetDevilTrainingReward failed: item count {item.PileNum} not enough");
                            msg.Result = (int)ErrorCode.ItemNotEnough;
                            Write(msg);
                            return;
                        }
                    }
                    else
                    {
                        //不是用钻石
                        Log.Warn($"player {Uid}GetDevilTrainingReward failed: item count {item.PileNum} not enough");
                        msg.Result = (int)ErrorCode.ItemNotEnough;
                        Write(msg);
                        return;
                    }
                }
            }
            else
            {
                //道具不足
                if (useDiamond)
                {
                    if (config.CostDiamond > 0)
                    {
                        costDiamondNum = costItemNum * config.CostDiamond;
                        costItemNum = 0;
                        //使用钻石
                        if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                        {
                            Log.Warn($"player {Uid}GetDevilTrainingReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                            msg.Result = (int)ErrorCode.DiamondNotEnough;
                            Write(msg);
                            return;
                        }
                    }
                    else
                    {
                        //不是用钻石
                        Log.Warn($"player {Uid}GetDevilTrainingReward failed: item count 0 not enough");
                        msg.Result = (int)ErrorCode.ItemNotEnough;
                        Write(msg);
                        return;
                    }
                }
                else
                {
                    //不是用钻石
                    Log.Warn($"player {Uid}GetDevilTrainingReward failed: item count 0 not enough");
                    msg.Result = (int)ErrorCode.ItemNotEnough;
                    Write(msg);
                    return;
                }
            }

            if (costItemNum > 0)
            {
                BaseItem it;
                if (type == 1)
                {
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.DevilTraining);
                }
                else
                {
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.DevilTrainingHigh);
                }

                if (it != null)
                {
                    SyncClientItemInfo(it);
                }
            }

            if (costDiamondNum > 0)
            {
                if (type == 1)
                {
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.DevilTraining, type.ToString());
                }
                else
                {
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.DevilTrainingHigh, type.ToString());
                }
            }
            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            int rewardNum = 1;
            string reward = string.Empty;
            if (isConsecutive)
            {
                rewardNum = config.ConsecutiveCount;
            }

            List<int> baseRemoveIndexList = new List<int>();
            List<int> buffRemoveIndexList = new List<int>();
            int period = activityModel.SubType;
            Dictionary<int, int> rewardsIndexDic = DevilTrainingMng.GetAllRewards(type, period);
            Dictionary<int, int> indexRatioDic = new Dictionary<int, int>();
            int sumRatio = 0;
            //通过存的奖励取权重Dic
            foreach (var kvpair in rewardsIndexDic)
            {
                DevilTrainingRewardModel rewardModel = DevilTrainingLibrary.GetDevilTrainingRewardModel(kvpair.Value);
                if (rewardModel == null)
                {
                    msg.Result = (int)ErrorCode.Fail;
                    Log.Warn($"player {Uid} get deviltraining reward failed: rewardModel is null. id:{kvpair.Value}");
                    Write(msg);
                    return;
                }
                indexRatioDic.Add(kvpair.Key, rewardModel.Ratio);
                sumRatio += rewardModel.Ratio;
            }
            
           
            for (int i = 0; i < rewardNum; i++)
            {
                //计算结果
                int rand = RandomRatio(indexRatioDic, sumRatio);
                int id = rewardsIndexDic[rand]; //取不到
                DevilTrainingRewardModel tempReward = DevilTrainingLibrary.GetDevilTrainingRewardModel(id);
                sumRatio -= indexRatioDic[rand];
                indexRatioDic.Remove(rand);

                reward = tempReward.Param;

                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
                if (!baseRemoveIndexList.Contains(rand))
                {
                    baseRemoveIndexList.Add(rand);
                }
                bool useBuff = false;
                int buffRand = RAND.Range(0, 10000);
                DevilTrainingBuffModel buffModel = DevilTrainingLibrary.GetDevilTrainingBuff(info.BuffId);
                if (buffModel.Probability > buffRand)
                {
                    useBuff = true;
                }
                if (type == 2 && useBuff)
                {
                    if (buffModel.Before != 0)
                    {
                        for (int index = rand - 1; index >= rand - buffModel.Before; index--)
                        {
                            int temp = index;
                            if (index <= 0)
                            {
                                temp = temp + 20;
                            }
                            id = rewardsIndexDic[temp];
                            DevilTrainingRewardModel buffReward = DevilTrainingLibrary.GetDevilTrainingRewardModel(id);
                            reward = buffReward.Param;
                            rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                            items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                            rewardItems.AddRange(items);
                            if (!buffRemoveIndexList.Contains(temp))
                            {
                                buffRemoveIndexList.Add(temp);
                            }
                        }
                    }
                    if (buffModel.After != 0)
                    {
                        for (int index = rand + 1; index <= rand + buffModel.After; index++)
                        {
                            int temp = index;
                            if (index > 20)
                            {
                                temp = temp - 20;
                            }
                            id = rewardsIndexDic[temp];
                            DevilTrainingRewardModel buffReward = DevilTrainingLibrary.GetDevilTrainingRewardModel(id);
                            reward = buffReward.Param;
                            rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                            items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                            rewardItems.AddRange(items);
                            if (!buffRemoveIndexList.Contains(temp))
                            {
                                buffRemoveIndexList.Add(temp);
                            }
                        }
                    }
                    if (buffModel.CurrentTimes != 1)
                    {
                        DevilTrainingRewardModel buffReward = DevilTrainingLibrary.GetDevilTrainingRewardModel(id);
                        reward = buffReward.Param;
                        rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                        items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        for (int j = 0; j < items.Count; j++)
                        {
                            items[j].Num = items[j].Num * (buffModel.CurrentTimes - 1);
                        }
                        rewardItems.AddRange(items);
                    }

                }
                
            }

            for (int baseIndex = 0; baseIndex < baseRemoveIndexList.Count; baseIndex++)
            {
                rewardsIndexDic.Remove(baseRemoveIndexList[baseIndex]);
            }
            Dictionary<int, int> buffCountDic;
            Dictionary<int, int> baseNewRewards = DevilTrainingMng.GetNewRewards(baseRemoveIndexList, type, out buffCountDic);

            for (int buffIndex = 0; buffIndex < buffRemoveIndexList.Count; buffIndex++)
            {
                int id;
                if (rewardsIndexDic.TryGetValue(buffRemoveIndexList[buffIndex], out id))
                {
                    rewardsIndexDic.Remove(buffRemoveIndexList[buffIndex]);
                }
            }
            Dictionary<int, int> buffNewRewards = DevilTrainingMng.GetNewRewards(buffRemoveIndexList, type, out buffCountDic);
            foreach (var kvpair in baseNewRewards)
            {
                DEVIL_TRAINING_REWARD_INFO rewardInfo = new DEVIL_TRAINING_REWARD_INFO();
                rewardInfo.Index = kvpair.Key;
                rewardInfo.Id = kvpair.Value;
                DevilTrainingBuffModel buffModel = DevilTrainingLibrary.GetDevilTrainingBuff(info.BuffId);
                rewardInfo.Count = buffModel.CurrentTimes;
                if (buffNewRewards.ContainsKey(kvpair.Key))
                {
                    if (buffCountDic.ContainsKey(kvpair.Key))
                    {
                        rewardInfo.Count += buffCountDic[kvpair.Key];
                        msg.BaseReward.Add(rewardInfo);
                        continue;
                    }
                    rewardInfo.Count += 1;
                }
                
                msg.BaseReward.Add(rewardInfo);
            }
            foreach (var kvpair in buffNewRewards)
            {
                DEVIL_TRAINING_REWARD_INFO rewardInfo = new DEVIL_TRAINING_REWARD_INFO();
                if (baseNewRewards.ContainsKey(kvpair.Key))
                {
                    continue;
                }
                rewardInfo.Index = kvpair.Key;
                rewardInfo.Id = kvpair.Value;
                if (buffCountDic.ContainsKey(kvpair.Key))
                {
                    rewardInfo.Count = buffCountDic[kvpair.Key] + 1;
                }
                else
                {
                    rewardInfo.Count = 1;
                }
                msg.BuffReward.Add(rewardInfo);
            }
            DevilTrainingMng.SetNewRewards(baseNewRewards, buffNewRewards);
            if (type == 2)
            {
                info.Point += rewardNum;
                BIRecordPointGameLog(rewardNum, info.Point, "devil_training", activityModel.SubType);
            }
            DevilTrainingMng.SyncDbUpdateGetReward(info);


            foreach (var rewardItem in rewardItems)
            {
                REWARD_ITEM_INFO rewardMsg = new REWARD_ITEM_INFO();
                rewardMsg.MainType = rewardItem.RewardType;
                rewardMsg.TypeId = rewardItem.Id;
                rewardMsg.Num = rewardItem.Num;
                if (rewardItem.Attrs != null)
                {
                    foreach (var attr in rewardItem.Attrs)
                    {
                        rewardMsg.Param.Add(attr);
                    }
                }
                msg.Rewards.Add(rewardMsg);
            }

            manager.AddReward(rewardItems);
            manager.BreakupRewards(true);
            // 发放奖励
            if (type == 1)
            {
                AddRewards(manager, ObtainWay.DevilTraining, type.ToString());
            }
            else
            {
                AddRewards(manager, ObtainWay.DevilTrainingHigh, type.ToString());
            }
            msg.Point = info.Point;
            msg.Type = type;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public int RandomRatio(Dictionary<int, int> ratioDic, int sumRatio)
        {
            int rand = RAND.RandValue(ratioDic, sumRatio);
            return rand;
        }

       

        public void SendDevilTrainingInfo()
        {
            MSG_ZGC_DEVIL_TRAINING_INFO msg = DevilTrainingMng.InitDevilTrainingInfoMsg();
            Write(msg);
        }


        public void GetDevilTrainingPointReward(int rewardId)
        {
            MSG_ZGC_GET_DEVIL_TRAINING_POINT_REWARD response = new MSG_ZGC_GET_DEVIL_TRAINING_POINT_REWARD();
            response.Id = rewardId;

            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.DevilTraining, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get deviltraining point reward failed: not open");
                Write(response);
                return;
            }
            int period = activityModel.SubType;

            DevilTrainingCumulativeRewardModel cumulativeModel = DevilTrainingLibrary.GetDevilTrainingCumulativeRewardModel(rewardId);
            if (cumulativeModel == null)
            {
                Log.Warn($"player {Uid} get deviltraining point reward failed: not find rewardId {rewardId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (cumulativeModel.Period != period)
            {
                Log.Warn($"player {Uid} get deviltraining point reward failed: rewardId {rewardId} not cur period {period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            List<int> pointRewards = DevilTrainingMng.GetReceiveList();
            if (pointRewards.Contains(rewardId))
            {
                Log.Warn($"player {Uid} get deviltraining point reward failed: rewardId {rewardId} alrady got");
                response.Result = (int)ErrorCode.AlreadyGot;
                Write(response);
                return;
            }
            if (DevilTrainingMng.GetPoint() < cumulativeModel.Point)
            {
                Log.Warn($"player {Uid} get deviltraining point reward {rewardId} failed: score {cumulativeModel.Point} not enough");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            DevilTrainingMng.AddPointReward(rewardId);

            if (!string.IsNullOrEmpty(cumulativeModel.Param))
            {
                //按有装备和魂骨生成奖励
                RewardManager manager = new RewardManager();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, cumulativeModel.Param);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);

                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.DevilTrainingHigh);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void ChangeDevilTrainingBuff(int buffId)
        {
            MSG_ZGC_CHANGE_DEVIL_TRAINING_BUFF response = new MSG_ZGC_CHANGE_DEVIL_TRAINING_BUFF();
            response.Id = buffId;
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.DevilTraining, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get deviltraining change buff failed: not open");
                Write(response);
                return;
            }
            DevilTrainingBuffModel buffModel = DevilTrainingLibrary.GetDevilTrainingBuff(buffId);
            if (buffModel == null)
            {
                Log.Warn($"player {Uid} get deviltraining change buff failed: not find buffId {buffId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (DevilTrainingMng.GetPoint() < buffModel.Point)
            {
                Log.Warn($"player {Uid} get deviltraining change buff {buffId} failed: score {buffModel.Point} not enough");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (DevilTrainingMng.GetBuffId() == buffId)
            {
                Log.Warn($"player {Uid} get deviltraining change buff {buffId} failed: buffId is equiped");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            DevilTrainingMng.ChangeBuffId(buffId);
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void BuyDevilTrainingItem(int type, int num)
        {
            DevilTrainingConfig config = DevilTrainingLibrary.GetDevilTrainingConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} buy deviltraining item failed: not type {type}");
                return;
            }
            if (config.CostItem > 0 && config.CostDiamond > 0)
            {
                if (num > 0 && config.BuyMaxCount >= num)
                {
                    MSG_ZGC_BUY_DEVIL_TRAINING_ITEM msg = new MSG_ZGC_BUY_DEVIL_TRAINING_ITEM();
                    msg.Id = type;
                    msg.Num = num;

                    int costDiamondNum = num * config.CostDiamond;
                    //使用钻石
                    if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                    {
                        Log.Warn($"player {Uid}buy deviltraining item failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                        msg.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(msg);
                        return;
                    }
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.DevilTrainingHigh, type.ToString());

                    RewardManager manager = new RewardManager();
                    manager.AddReward(new ItemBasicInfo((int)RewardType.NormalItem, config.CostItem, num));
                    manager.BreakupRewards();
                    // 发放奖励
                    AddRewards(manager, ObtainWay.DevilTraining, type.ToString());
                    //通知前端奖励
                    manager.GenerateRewardMsg(msg.Rewards);
                    msg.Result = (int)ErrorCode.Success;
                    Write(msg);

                }
                else
                {
                    Log.Warn($"player {Uid} buy deviltraining item failed: num {num} max {config.BuyMaxCount}");
                    return;
                }
            }
            else
            {
                Log.Warn($"player {Uid} buy deviltraining item failed: no CostItem {config.CostItem} CostDiamond {config.CostDiamond}");
                return;
            }
        }

        
    }
}
