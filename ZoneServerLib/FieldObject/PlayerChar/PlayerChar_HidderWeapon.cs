using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerModels.HidderWeapon;
using ServerShared;
using System.Collections.Generic;
using static DBUtility.QuerySetCrossBossRankReward;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {

        public void GetHidderWeaponInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_HIDDER_WEAPON_VALUE msg = new MSG_ZR_GET_HIDDER_WEAPON_VALUE();
            server.SendToRelation(msg, Uid);
        }

        public void GetHidderWeaponInfoByLoading()
        {
            if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.HiddenWeapon, ZoneServerApi.now))
            {
                GetHidderWeaponInfo();
            }
        }

        public void GetHidderWeaponReward(int type, bool isConsecutive, bool useDiamond, int ringNum)
        {
            HidderWeaponConfig comfig = HidderWeaponLibrary.GetHidderWeaponConfig(type);
            if (comfig == null)
            {
                Log.Warn($"player {Uid} GetHidderWeaponReward failed: not type {type}");
                return;
            }

            HidderWeaponRingRewardModel rewardModel = comfig.GetRingReward(ringNum);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} GetHidderWeaponReward failed: not ring {ringNum} reward");
                return;
            }


            MSG_ZGC_GET_HIDDER_WEAPON_REWARD msg = new MSG_ZGC_GET_HIDDER_WEAPON_REWARD();
            msg.Id = type;
            msg.RingNum = ringNum;

            RechargeGiftModel activity;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.HiddenWeapon, ZoneServerApi.now, out activity))
            {
                Log.Warn($"player {Uid} GetHidderWeaponReward failed: time is error");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            int costItemNum = 1;
            int costDiamondNum = 0;
            if (isConsecutive)
            {
                costItemNum = comfig.ConsecutiveCount;
                ringNum *= comfig.ConsecutiveCount;
            }

            BaseItem item = BagManager.GetItem(MainType.Consumable, comfig.CostItem);
            if (item != null)
            {
                //飞镖不足
                if (item.PileNum < costItemNum)
                {
                    if (useDiamond)
                    {
                        if (comfig.CostDiamond > 0)
                        {
                            //使用钻石
                            costDiamondNum = (costItemNum - item.PileNum) * comfig.CostDiamond;
                            costItemNum = item.PileNum;
                            //使用钻石
                            if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                            {
                                Log.Warn($"player {Uid}GetHidderWeaponReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                                msg.Result = (int)ErrorCode.DiamondNotEnough;
                                Write(msg);
                                return;
                            }
                        }
                        else
                        {
                            //不是用钻石
                            Log.Warn($"player {Uid}GetHidderWeaponReward failed: item count {item.PileNum} not enough");
                            msg.Result = (int)ErrorCode.ItemNotEnough;
                            Write(msg);
                            return;
                        }
                    }
                    else
                    {
                        //不是用钻石
                        Log.Warn($"player {Uid}GetHidderWeaponReward failed: item count {item.PileNum} not enough");
                        msg.Result = (int)ErrorCode.ItemNotEnough;
                        Write(msg);
                        return;
                    }
                }
            }
            else
            {
                //飞镖不足
                if (useDiamond)
                {
                    if (comfig.CostDiamond > 0)
                    {
                        costDiamondNum = costItemNum * comfig.CostDiamond;
                        costItemNum = 0;
                        //使用钻石
                        if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                        {
                            Log.Warn($"player {Uid}GetHidderWeaponReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                            msg.Result = (int)ErrorCode.DiamondNotEnough;
                            Write(msg);
                            return;
                        }
                    }
                    else
                    {
                        //不是用钻石
                        Log.Warn($"player {Uid}GetHidderWeaponReward failed: item count 0 not enough");
                        msg.Result = (int)ErrorCode.ItemNotEnough;
                        Write(msg);
                        return;
                    }
                }
                else
                {
                    //不是用钻石
                    Log.Warn($"player {Uid}GetHidderWeaponReward failed: item count 0 not enough");
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
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.HidderWeapon);
                }
                else
                {
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.HidderWeaponHigh);
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
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.HidderWeapon, ringNum.ToString());
                }
                else
                {
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.HidderWeaponHigh, ringNum.ToString());
                }
            }

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            int rewardId = 0;
            int rewardNum = 1;
            string reward = string.Empty;
            if (isConsecutive)
            {
                rewardNum = comfig.ConsecutiveCount;
            }
            //bool updateDb = false;
            for (int i = 0; i < rewardNum; i++)
            {
                if (CrossBossInfoMng.CounterInfo.HiddenWeaponGet == 0 && msg.RingNum == 10)
                {
                    rewardId = rewardModel.GeRewardId(CrossBossInfoMng.CounterInfo.HiddenWeaponNum);

                    if (rewardModel.AddRatios.ContainsKey(rewardId))
                    {
                        //说明抽中
                        CrossBossInfoMng.CounterInfo.HiddenWeaponGet = 1;
                    }
                    //updateDb = true;
                }
                else
                {
                    rewardId = rewardModel.GeRewardId();
                }



                HiddenWeaponRewardModel subRewardIdModel = HidderWeaponLibrary.GetHiddenWeaponReward(rewardId);
                if (subRewardIdModel != null)
                {
                    reward = subRewardIdModel.Param;

                    RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                    List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                    rewardItems.AddRange(items);
                }
            }
            if (type == 2)
            {
                CrossBossInfoMng.CounterInfo.HiddenWeaponNum += rewardNum;
                CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum += ringNum;
                SyncDbUpdateHiddenWeapon();
                BIRecordPointGameLog(ringNum,  CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum, "hidden_weapon", activity.SubType);
            }


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
                AddRewards(manager, ObtainWay.HidderWeapon, type.ToString());
            }
            else
            {
                AddRewards(manager, ObtainWay.HidderWeaponHigh, type.ToString());
            }
            //通知前端奖励
            //manager.GenerateRewardMsg(msg.Rewards);
            msg.HiddenWeaponNum = CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            MSG_ZR_CROSS_NOTES_LIST motesMsg = new MSG_ZR_CROSS_NOTES_LIST();
            motesMsg.Type = (int)NotesType.HidderWeaponItem;
            foreach (var rewardItem in manager.AllRewards)
            {
                if (comfig.AnnounceList.Contains(rewardItem.Id))
                {
                    CrossBroadcastAnnouncement(ANNOUNCEMENT_TYPE.HIDDER_WEAPON_ITEM,
                        new List<string>() { server.MainId.ToString(), Name, rewardItem.Id.ToString() });
                }

                if (comfig.NotesList.Contains(rewardItem.Id))
                {
                    ZR_CROSS_NOTES notesItemMsg = GetCrossNotes(NotesType.HidderWeaponItem, server.MainId, Name, rewardItem.Id);
                    motesMsg.List.Add(notesItemMsg);
                }
            }
            SendCrossNotes(motesMsg);

            if (type == 2)
            {
                MSG_ZR_UPDATE_HIDDER_WEAPON_VALUE updateMsg = new MSG_ZR_UPDATE_HIDDER_WEAPON_VALUE();
                updateMsg.RingNum = ringNum;
                server.SendToRelation(updateMsg, Uid);
            }
        }

        public void GetHidderWeaponNumReward(int type, int num)
        {
            HidderWeaponConfig comfig = HidderWeaponLibrary.GetHidderWeaponConfig(type);
            if (comfig == null)
            {
                Log.Warn($"player {Uid} GetHidderWeaponNumReward failed: not type {type}");
                return;
            }

            MSG_ZGC_GET_HIDDER_WEAPON_NUM_REWARD msg = new MSG_ZGC_GET_HIDDER_WEAPON_NUM_REWARD();
            msg.Id = type;
            msg.RewardNum = num;

            bool inTime = false;
            Dictionary<int, RechargeGiftModel> gifts = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.HiddenWeapon);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= ZoneServerApi.now && ZoneServerApi.now <= gift.Value.EndTime)
                    {
                        inTime = true;
                        break;
                    }
                }
            }
            if (!inTime)
            {
                Log.Warn($"player {Uid} GetHidderWeaponNumReward failed: time is error");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            if (CrossBossInfoMng.CounterInfo.HiddenWeaponNumRewards.Contains(num))
            {
                Log.Warn($"player {Uid} GetHidderWeaponNumReward failed: has got {num} reward");
                msg.Result = (int)ErrorCode.AlreadyGot;
                Write(msg);
                return;
            }

            if (CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum < num)
            {
                Log.Warn($"player {Uid} GetHidderWeaponNumReward failed: cur is {CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum } not {num}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            int rewardId = comfig.GetNumRewardId(num);
            if (rewardId <= 0)
            {
                Log.Warn($"player {Uid} GetHidderWeaponNumReward failed: not ring {num} reward");
                msg.Result = (int)ErrorCode.NotFindModel;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            HiddenWeaponRewardModel subRewardIdModel = HidderWeaponLibrary.GetHiddenWeaponReward(rewardId);
            if (subRewardIdModel != null)
            {
                string reward = subRewardIdModel.Param;
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
            }

            CrossBossInfoMng.CounterInfo.HiddenWeaponNumRewards.Add(num);
            SyncDbUpdateHiddenWeapon();


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
            AddRewards(manager, ObtainWay.HidderWeaponHigh, type.ToString());
            //通知前端奖励
            //manager.GenerateRewardMsg(msg.Rewards);
            msg.HiddenWeaponNumRewards.AddRange(CrossBossInfoMng.CounterInfo.HiddenWeaponNumRewards);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void BuyHidderWeaponItem(int type, int num)
        {
            HidderWeaponConfig comfig = HidderWeaponLibrary.GetHidderWeaponConfig(type);
            if (comfig == null)
            {
                Log.Warn($"player {Uid} BuyHidderWeaponItem failed: not type {type}");
                return;
            }

            if (comfig.CostItem > 0 && comfig.CostDiamond > 0)
            {
                if (num > 0 && comfig.BuyMaxCount >= num)
                {
                    MSG_ZGC_BUY_HIDDER_WEAPON_ITEM msg = new MSG_ZGC_BUY_HIDDER_WEAPON_ITEM();
                    msg.Id = type;
                    msg.Num = num;

                    int costDiamondNum = num * comfig.CostDiamond;
                    //使用钻石
                    if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                    {
                        Log.Warn($"player {Uid} BuyHidderWeaponItem failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                        msg.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(msg);
                        return;
                    }

                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.BuyHidderWeaponItem, type.ToString());

                    RewardManager manager = new RewardManager();
                    manager.AddReward(new ItemBasicInfo((int)RewardType.NormalItem, comfig.CostItem, num));
                    manager.BreakupRewards();
                    // 发放奖励
                    AddRewards(manager, ObtainWay.BuyHidderWeaponItem, type.ToString());
                    //通知前端奖励
                    manager.GenerateRewardMsg(msg.Rewards);
                    msg.Result = (int)ErrorCode.Success;
                    Write(msg);

                }
                else
                {
                    Log.Warn($"player {Uid} BuyHidderWeaponItem failed: num {num} max {comfig.BuyMaxCount}");
                    return;
                }
            }
            else
            {
                Log.Warn($"player {Uid} BuyHidderWeaponItem failed: no CostItem {comfig.CostItem} CostDiamond {comfig.CostDiamond}");
                return;
            }
        }



        public void GetSeaTreasureInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_SEA_TREASURE_VALUE msg = new MSG_ZR_GET_SEA_TREASURE_VALUE();
            server.SendToRelation(msg, Uid);
        }

        public void GetSeaTreasureReward(int type, bool isConsecutive, bool useDiamond)
        {
            SeaTreasureConfig config = HidderWeaponLibrary.GetSeaTreasureConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} GetSeaTreasureReward failed: not type {type}");
                return;
            }
            MSG_ZGC_GET_SEA_TREASURE_REWARD msg = new MSG_ZGC_GET_SEA_TREASURE_REWARD();
            msg.Id = type;

            int period = 0;
            bool inTime = false;
            Dictionary<int, RechargeGiftModel> gifts = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.SeaTreasure);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= ZoneServerApi.now && ZoneServerApi.now <= gift.Value.EndTime)
                    {
                        inTime = true;
                        period = gift.Value.SubType;
                        break;
                    }
                }
            }
            if (!inTime)
            {
                Log.Warn($"player {Uid} GetSeaTreasureReward failed: time is error");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            int AddNum = 1;
            if (isConsecutive)
            {
                AddNum = config.ConsecutiveCount;
            }
            //if (config.CostCount.Count > 0)
            //{
            //    AddNum = config.GetCostCount(CrossBossInfoMng.CounterInfo.BlessingNum);
            //    costItemNum = AddNum;
            //}
            int costItemNum = AddNum;

            int costDiamondNum = 0;
            BaseItem item = null;
            if (config.CostItem > 0)
            {
                item = BagManager.GetItem(MainType.Consumable, config.CostItem);
                if (item != null)
                {
                    //飞镖不足
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
                                    Log.Warn($"player {Uid} GetSeaTreasureReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                                    msg.Result = (int)ErrorCode.DiamondNotEnough;
                                    Write(msg);
                                    return;
                                }
                            }
                            else
                            {
                                //不是用钻石
                                Log.Warn($"player {Uid} GetSeaTreasureReward failed: item count {item.PileNum} not enough");
                                msg.Result = (int)ErrorCode.ItemNotEnough;
                                Write(msg);
                                return;
                            }
                        }
                        else
                        {
                            //不是用钻石
                            Log.Warn($"player {Uid} GetSeaTreasureReward failed: item count {item.PileNum} not enough");
                            msg.Result = (int)ErrorCode.ItemNotEnough;
                            Write(msg);
                            return;
                        }
                    }
                }
                else
                {
                    //飞镖不足
                    if (useDiamond)
                    {
                        if (config.CostDiamond > 0)
                        {
                            costDiamondNum = costItemNum * config.CostDiamond;
                            costItemNum = 0;
                            //使用钻石
                            if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                            {
                                Log.Warn($"player {Uid} GetSeaTreasureReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                                msg.Result = (int)ErrorCode.DiamondNotEnough;
                                Write(msg);
                                return;
                            }
                        }
                        else
                        {
                            //不是用钻石
                            Log.Warn($"player {Uid} GetSeaTreasureReward failed: item count 0 not enough");
                            msg.Result = (int)ErrorCode.ItemNotEnough;
                            Write(msg);
                            return;
                        }
                    }
                    else
                    {
                        //不是用钻石
                        Log.Warn($"player {Uid} GetHidderWeaponReward failed: item count 0 not enough");
                        msg.Result = (int)ErrorCode.ItemNotEnough;
                        Write(msg);
                        return;
                    }
                }
            }
            else
            {
                if (config.CostDiamond > 0)
                {
                    costDiamondNum = costItemNum * config.CostDiamond;
                    costItemNum = 0;
                    //使用钻石
                    if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                    {
                        Log.Warn($"player {Uid} GetSeaTreasureReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                        msg.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(msg);
                        return;
                    }
                }
                else
                {
                    //不是用钻石
                    Log.Warn($"player {Uid} GetSeaTreasureReward failed: item count 0 not enough");
                    msg.Result = (int)ErrorCode.ItemNotEnough;
                    Write(msg);
                    return;
                }
            }

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();
            string reward = string.Empty;

            for (int i = 0; i < AddNum; i++)
            {
                int rewardId = config.GeRewardId();
                SeaTreasureRewardModel rewardModel = HidderWeaponLibrary.GetSeaTreasureReward(rewardId);
                if (rewardModel == null)
                {
                    Log.Warn($"player {Uid} GetSeaTreasureReward failed:not find {rewardId} model");
                    msg.Result = (int)ErrorCode.NotFindModel;
                    Write(msg);
                    return;
                }
                while (msg.GetRewardIndexList.Contains(rewardModel.Index))
                {
                    rewardId = config.GeRewardId();
                    rewardModel = HidderWeaponLibrary.GetSeaTreasureReward(rewardId);
                    if (rewardModel == null)
                    {
                        Log.Warn($"player {Uid} GetSeaTreasureReward failed:not find {rewardId} model");
                        msg.Result = (int)ErrorCode.NotFindModel;
                        Write(msg);
                        return;
                    }
                }
                switch (type)
                {
                    case 1:
                        {
                            reward = rewardModel.Param;
                        }
                        break;
                    case 2:
                        {
                            reward = rewardModel.Param;
                            if (rewardModel.Ratios.Count > 0)
                            {
                                int subRewardId = rewardModel.GeSubRewardId();
                                if (CrossBossInfoMng.CounterInfo.ItemList.Count >= config.SubRewardCount)
                                {
                                    //随机替换一个
                                    int index = RAND.Range(0, CrossBossInfoMng.CounterInfo.ItemList.Count - 1);
                                    int oldRewardId = CrossBossInfoMng.CounterInfo.ItemList[index];
                                    while (oldRewardId == subRewardId)
                                    {
                                        subRewardId = rewardModel.GeSubRewardId();
                                    }
                                    CrossBossInfoMng.CounterInfo.ItemList[index] = subRewardId;

                                    msg.ChangeIndexList.Add(index);
                                }
                                else
                                {
                                    CrossBossInfoMng.CounterInfo.ItemList.Add(subRewardId);

                                    msg.ChangeIndexList.Add(CrossBossInfoMng.CounterInfo.ItemList.Count - 1);
                                    //msg.ChangeIndex = CrossBossInfoMng.CounterInfo.ItemList.Count - 1;
                                }

                                CrossBossInfoMng.CounterInfo.BlessingNum = 0;
                                CrossBossInfoMng.CounterInfo.BlessingMultiple = 1;
                            }
                        }
                        break;
                    case 3:
                        {
                            if (CrossBossInfoMng.CounterInfo.BlessingNum > 0)
                            {
                                Log.Warn($"player {Uid} GetSeaTreasureReward failed:not find {rewardId} model");
                                msg.Result = (int)ErrorCode.Fail;
                                Write(msg);
                                //CloseSeaTreasureBlessing();
                                //return;
                            }

                            switch (rewardModel.Type)
                            {
                                case 2:
                                    CrossBossInfoMng.CounterInfo.BlessingMultiple *= int.Parse(rewardModel.Param);
                                    foreach (var subRewardId in CrossBossInfoMng.CounterInfo.ItemList)
                                    {
                                        SeaTreasureRewardModel subRewardIdModel = HidderWeaponLibrary.GetSeaTreasureReward(subRewardId);
                                        if (subRewardIdModel != null)
                                        {
                                            reward += "|" + subRewardIdModel.Param;
                                        }
                                    }

                                    break;
                                case 3:
                                    CrossBossInfoMng.CounterInfo.BlessingMultiple += int.Parse(rewardModel.Param);
                                    foreach (var subRewardId in CrossBossInfoMng.CounterInfo.ItemList)
                                    {
                                        SeaTreasureRewardModel subRewardIdModel = HidderWeaponLibrary.GetSeaTreasureReward(subRewardId);
                                        if (subRewardIdModel != null)
                                        {
                                            reward += "|" + subRewardIdModel.Param;
                                        }
                                    }
                                    break;
                                case 4:
                                    {
                                        int index = RAND.Range(0, CrossBossInfoMng.CounterInfo.ItemList.Count - 1);
                                        int subRewardId = CrossBossInfoMng.CounterInfo.ItemList[index];
                                        SeaTreasureRewardModel subRewardIdModel = HidderWeaponLibrary.GetSeaTreasureReward(subRewardId);
                                        if (subRewardIdModel != null)
                                        {
                                            reward = subRewardIdModel.Param;
                                        }
                                    }

                                    break;
                                default:
                                    break;
                            }

                        }
                        break;
                    default:
                        Log.Warn($"player {Uid} GetSeaTreasureReward failed: get reward no type {type}");
                        break;
                }

                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                rewardItems.AddRange(RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job));
                msg.GetRewardIndexList.Add(rewardModel.Index);
            }


            if (costItemNum > 0 && item != null)
            {
                BaseItem it;
                if (type == 1)
                {
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.SeaTreasure);
                }
                else
                {
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.SeaTreasureHigh);
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
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.SeaTreasure, type.ToString());
                }
                else
                {
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.SeaTreasureHigh, type.ToString());
                }
            }

            //rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
            if (type == 3)
            {
                foreach (var rewardItem in rewardItems)
                {
                    rewardItem.Num *= CrossBossInfoMng.CounterInfo.BlessingMultiple;
                }

                CrossBossInfoMng.CounterInfo.BlessingNum += AddNum;
            }
            manager.AddReward(rewardItems);
            manager.BreakupRewards(true);

            //msg.GetRewardIndex = rewardModel.Index;

            // 发放奖励
            if (type == 1)
            {
                AddRewards(manager, ObtainWay.SeaTreasure, type.ToString());
            }
            else
            {
                AddRewards(manager, ObtainWay.SeaTreasureHigh, type.ToString());
            }
            //通知前端奖励
            manager.GenerateRewardMsg(msg.Rewards);
            msg.Result = (int)ErrorCode.Success;
            msg.BlessingNum = CrossBossInfoMng.CounterInfo.BlessingNum;
            msg.BlessingMultiple = CrossBossInfoMng.CounterInfo.BlessingMultiple;
            msg.ItemList.AddRange(CrossBossInfoMng.CounterInfo.ItemList);

            Write(msg);

            MSG_ZR_CROSS_NOTES_LIST motesMsg = new MSG_ZR_CROSS_NOTES_LIST();
            motesMsg.Type = (int)NotesType.SeaTreasure;
            foreach (var rewardItem in manager.AllRewards)
            {
                if (config.AnnounceList.Contains(rewardItem.Id))
                {
                    switch (type)
                    {
                        case 3:
                            CrossBroadcastAnnouncement(ANNOUNCEMENT_TYPE.SEA_TREASURE_ITEM_3,
                                                new List<string>() { server.MainId.ToString(), Name, rewardItem.Id.ToString() });
                            break;
                        default:
                            CrossBroadcastAnnouncement(ANNOUNCEMENT_TYPE.SEA_TREASURE_ITEM_2,
                                                new List<string>() { server.MainId.ToString(), Name, rewardItem.Id.ToString() });
                            break;
                    }

                }

                if (config.NotesList.Contains(rewardItem.Id))
                {
                    ZR_CROSS_NOTES notesItemMsg = GetCrossNotes(NotesType.SeaTreasure, server.MainId, Name, rewardItem.Id);
                    motesMsg.List.Add(notesItemMsg);
                }
            }
            SendCrossNotes(motesMsg);

            switch (type)
            {
                case 1:
                    break;
                default:
                    MSG_ZR_UPDATE_SEA_TREASURE_VALUE updateMsg = new MSG_ZR_UPDATE_SEA_TREASURE_VALUE();
                    updateMsg.RingNum = AddNum * config.AddValue;
                    server.SendToRelation(updateMsg, Uid);
                    if (type == 2 || type == 3)
                    {
                        CrossBossInfoMng.CounterInfo.SeaTreasureNum += AddNum * config.AddValue;
                        BIRecordPointGameLog(updateMsg.RingNum,  CrossBossInfoMng.CounterInfo.SeaTreasureNum, "sea_treasure", period);
                    }
                    SyncDbUpdateSeaTreasure();
                    break;
            }
        }

        public void BuySeaTreasureItem(int type, int num)
        {
            SeaTreasureConfig comfig = HidderWeaponLibrary.GetSeaTreasureConfig(type);
            if (comfig == null)
            {
                Log.Warn($"player {Uid} BuySeaTreasureItem failed: not type {type}");
                return;
            }

            if (comfig.CostItem > 0 && comfig.CostDiamond > 0)
            {
                if (num > 0 && comfig.BuyMaxCount >= num)
                {
                    MSG_ZGC_BUY_SEA_TREASURE_ITEM msg = new MSG_ZGC_BUY_SEA_TREASURE_ITEM();
                    msg.Id = type;
                    msg.Num = num;

                    int costDiamondNum = num * comfig.CostDiamond;
                    //使用钻石
                    if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                    {
                        Log.Warn($"player {Uid}BuySeaTreasureItem failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                        msg.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(msg);
                        return;
                    }

                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.BuySeaTreasureItem, type.ToString());

                    RewardManager manager = new RewardManager();
                    manager.AddReward(new ItemBasicInfo((int)RewardType.NormalItem, comfig.CostItem, num));
                    manager.BreakupRewards();
                    // 发放奖励
                    AddRewards(manager, ObtainWay.BuySeaTreasureItem, type.ToString());
                    //通知前端奖励
                    manager.GenerateRewardMsg(msg.Rewards);
                    msg.Result = (int)ErrorCode.Success;
                    Write(msg);

                }
                else
                {
                    Log.Warn($"player {Uid} BuySeaTreasureItem failed: num {num} max {comfig.BuyMaxCount}");
                    return;
                }
            }
            else
            {
                Log.Warn($"player {Uid} BuySeaTreasureItem failed: no CostItem {comfig.CostItem} CostDiamond {comfig.CostDiamond}");
                return;
            }
        }

        public void CloseSeaTreasureBlessing()
        {
            if (CrossBossInfoMng.CounterInfo.BlessingNum != 0)
            {
                CrossBossInfoMng.CounterInfo.BlessingNum = 0;
                CrossBossInfoMng.CounterInfo.BlessingMultiple = 1;
                CrossBossInfoMng.CounterInfo.ItemList.Clear();

                MSG_ZGC_CLOSE_SEA_TREASURE_BLESSING msg = new MSG_ZGC_CLOSE_SEA_TREASURE_BLESSING();
                msg.BlessingNum = CrossBossInfoMng.CounterInfo.BlessingNum;
                msg.BlessingMultiple = CrossBossInfoMng.CounterInfo.BlessingMultiple;
                Write(msg);

                //保存DB
                SyncDbUpdateSeaTreasure();

                GetSeaTreasureInfo();
            }
        }

        public void GetSeaTreasureNumReward(int type, int num)
        {
            SeaTreasureConfig config = HidderWeaponLibrary.GetSeaTreasureConfig(type);
            if (config == null || type != 2)
            {
                Log.Warn($"player {Uid} GetSeaTreasureNumReward failed: not type {type}");
                return;
            }

            MSG_ZGC_GET_SEA_TREASURE_NUM_REWARD msg = new MSG_ZGC_GET_SEA_TREASURE_NUM_REWARD();
            msg.Id = type;

            bool inTime = false;
            Dictionary<int, RechargeGiftModel> gifts = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.SeaTreasure);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= ZoneServerApi.now && ZoneServerApi.now <= gift.Value.EndTime)
                    {
                        inTime = true;
                        break;
                    }
                }
            }
            if (!inTime)
            {
                Log.Warn($"player {Uid} GetSeaTreasureNumReward failed: time is error");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            if (CrossBossInfoMng.CounterInfo.SeaTreasureNumRewards.Contains(num))
            {
                Log.Warn($"player {Uid} GetSeaTreasureNumReward failed: has got {num} reward");
                msg.Result = (int)ErrorCode.AlreadyGot;
                Write(msg);
                return;
            }

            if (CrossBossInfoMng.CounterInfo.SeaTreasureNum < num)
            {
                Log.Warn($"player {Uid} GetSeaTreasureNumReward failed: cur is {CrossBossInfoMng.CounterInfo.SeaTreasureNum} not {num}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            int rewardId = config.GetNumRewardId(num);
            if (rewardId <= 0)
            {
                Log.Warn($"player {Uid} GetSeaTreasureNumReward failed: not num {num} reward");
                msg.Result = (int)ErrorCode.NotFindModel;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            SeaTreasureRewardModel rewardModel = HidderWeaponLibrary.GetSeaTreasureReward(rewardId);
            if (rewardModel != null)
            {
                string reward = rewardModel.Param;
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
            }

            CrossBossInfoMng.CounterInfo.SeaTreasureNumRewards.Add(num);
            SyncDbUpdateSeaTreasureNumRewards();

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
            AddRewards(manager, ObtainWay.SeaTreasureHigh, type.ToString());
            //msg.RewardNum = CrossBossInfoMng.CounterInfo.SeaTreasureNum;
            msg.SeaTreasureNumRewards.AddRange(CrossBossInfoMng.CounterInfo.SeaTreasureNumRewards);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        private void SyncDbUpdateSeaTreasure()
        {
            server.GameDBPool.Call(new QueryUpdateSeaTreasureBlessing(Uid,
                CrossBossInfoMng.CounterInfo.BlessingNum, CrossBossInfoMng.CounterInfo.BlessingMultiple, CrossBossInfoMng.CounterInfo.ItemList, CrossBossInfoMng.CounterInfo.SeaTreasureNum));
        }

        private void SyncDbUpdateSeaTreasureNumRewards()
        {
            server.GameDBPool.Call(new QueryUpdateSeaTreasureNumRewards(Uid, CrossBossInfoMng.CounterInfo.SeaTreasureNumRewards));
        }

        private void SyncDbUpdateHiddenWeapon()
        {
            server.GameDBPool.Call(new QueryUpdateHiddenWeapon(Uid, CrossBossInfoMng.CounterInfo.HiddenWeaponNum, CrossBossInfoMng.CounterInfo.HiddenWeaponGet, CrossBossInfoMng.CounterInfo.HiddenWeaponNumRewards, CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum));
        }

        public void ClearCrossActivityValue(RechargeGiftType type)
        {
            switch (type)
            {
                case RechargeGiftType.HiddenWeapon:
                    //if (CrossBossInfoMng.CounterInfo.HiddenWeaponNum != 0)
                    {
                        CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum = 0;
                        CrossBossInfoMng.CounterInfo.HiddenWeaponNum = 0;
                        CrossBossInfoMng.CounterInfo.HiddenWeaponGet = 0;
                        CrossBossInfoMng.CounterInfo.HiddenWeaponNumRewards.Clear();
                    }
                    break;
                case RechargeGiftType.SeaTreasure:
                    //if (CrossBossInfoMng.CounterInfo.BlessingNum != 0)
                    {
                        CrossBossInfoMng.CounterInfo.BlessingNum = 0;
                        CrossBossInfoMng.CounterInfo.BlessingMultiple = 1;
                        CrossBossInfoMng.CounterInfo.ItemList.Clear();
                        CrossBossInfoMng.CounterInfo.SeaTreasureNum = 0;
                        CrossBossInfoMng.CounterInfo.SeaTreasureNumRewards.Clear();
                    }
                    break;
                case RechargeGiftType.Garden:
                    GardenManager.Clear();
                    break;
                case RechargeGiftType.DivineLove:
                    DivineLoveMng.Clear();
                    break;
                case RechargeGiftType.IslandHigh:
                    IslandHighManager.Clear();
                    break;
                case RechargeGiftType.StoneWall:
                    StoneWallMng.Clear();
                    break;
                case RechargeGiftType.CarnivalBoss:
                    ClearCarnivalBossInfo();
                    break;
                case RechargeGiftType.Roulette:
                    RouletteManager.Clear();
                    break;
                case RechargeGiftType.Canoe:
                    ClearCanoeInfo();
                    break;
                case RechargeGiftType.XuanBox:
                    XuanBoxManager.Clear();
                    break;
                case RechargeGiftType.WishLantern:
                    WishLanternManager.Clear();
                    break;
                case RechargeGiftType.DaysRecharge:
                    DaysRechargeManager.Clear();
                    break;
                default:
                    break;
            }

        }
    }
}
