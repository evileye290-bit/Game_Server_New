using DBUtility;
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
        public DivineLoveManager DivineLoveMng { get; set; }

        public void InitDivineLoveManager()
        {
            DivineLoveMng = new DivineLoveManager(this);
        }

        public void SendDivineLoveInfo()
        {
            if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DivineLove, ZoneServerApi.now))
            {
                MSG_ZGC_DIVINE_LOVE_INFO_LIST msg = new MSG_ZGC_DIVINE_LOVE_INFO_LIST();
                foreach (var kv in DivineLoveMng.DivineLoveList)
                {
                    DivineLoveInfo info = kv.Value;

                    DIVINE_LOVE_INFO msgInfo = new DIVINE_LOVE_INFO();

                    msgInfo.Id = info.Type;
                    msgInfo.RoundState = info.RoundState;
                    int gotCount = Math.Min(info.GotIndexList.Count, info.GotRewardList.Count);

                    for (int i = 0; i < gotCount; i++)
                    {
                        DIVINE_LOVE_GOT_REWARD_INFO gotInfo = new DIVINE_LOVE_GOT_REWARD_INFO();
                        gotInfo.Index = info.GotIndexList[i];
                        int rewardId = info.GotRewardList[i];
                        DivineLoveRewardModel rewardModel = DivineLoveLibrary.GetDivineLoveReward(rewardId);
                        gotInfo.RewardId = rewardModel.RewardId;
                        msgInfo.GotList.Add(gotInfo);
                    }

                    msg.List.Add(msgInfo);
                }
                Write(msg);
            }
        }

        public void GetDivineLoveInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_DIVINE_LOVE_VALUE msg = new MSG_ZR_GET_DIVINE_LOVE_VALUE();
            server.SendToRelation(msg, Uid);
        }

        public void GetDivineLoveInfoByLoading()
        {
            if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DivineLove, ZoneServerApi.now))
            {
                GetDivineLoveInfo();
            }
        }

        public void GetDivineLoveReward(int type, bool useDiamond, int index)
        {
            DivineLoveConfig config = DivineLoveLibrary.GetDivineLoveConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} GetDivineLoveReward failed: not type {type}");
                return;
            }

            MSG_ZGC_GET_DIVINE_LOVE_REWARD msg = new MSG_ZGC_GET_DIVINE_LOVE_REWARD();
            msg.Id = type;
            msg.Index = index;

            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DivineLove, ZoneServerApi.now))
            {
                Log.Warn($"player {Uid} GetDivineLoveReward failed: time is error");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            int costDiamondNum = 0;
            DivineLoveInfo info = DivineLoveMng.GetDivineLoveInfo(type);
            if (info == null)
            {
                Log.Warn($"player {Uid} GetDivineLoveReward failed: not open divine love yet");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }
            int divineCount = info.GotRewardList.Count;
            int costItemNum = config.GetCostCount(divineCount);
            int heartNum = costItemNum;

            BaseItem item = BagManager.GetItem(MainType.Consumable, config.CostItem);
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
                                Log.Warn($"player {Uid} GetDivineLoveReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                                msg.Result = (int)ErrorCode.DiamondNotEnough;
                                Write(msg);
                                return;
                            }
                        }
                        else
                        {
                            //不是用钻石
                            Log.Warn($"player {Uid} GetDivineLoveReward failed: item count {item.PileNum} not enough");
                            msg.Result = (int)ErrorCode.ItemNotEnough;
                            Write(msg);
                            return;
                        }
                    }
                    else
                    {
                        //不是用钻石
                        Log.Warn($"player {Uid} GetDivineLoveReward failed: item count {item.PileNum} not enough");
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
                            Log.Warn($"player {Uid} GetDivineLoveReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                            msg.Result = (int)ErrorCode.DiamondNotEnough;
                            Write(msg);
                            return;
                        }
                    }
                    else
                    {
                        //不是用钻石
                        Log.Warn($"player {Uid} GetDivineLoveReward failed: item count 0 not enough");
                        msg.Result = (int)ErrorCode.ItemNotEnough;
                        Write(msg);
                        return;
                    }
                }
                else
                {
                    //不是用钻石
                    Log.Warn($"player {Uid} GetDivineLoveReward failed: item count 0 not enough");
                    msg.Result = (int)ErrorCode.ItemNotEnough;
                    Write(msg);
                    return;
                }
            }

            if (costItemNum > 0)
            {
                BaseItem it;
                if ((ActivityPlayType)type == ActivityPlayType.Normal)
                {
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.DivineLove);
                }
                else
                {
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.DivineLoveHigh);
                }
                if (it != null)
                {
                    SyncClientItemInfo(it);
                }
            }

            if (costDiamondNum > 0)
            {
                if ((ActivityPlayType)type == ActivityPlayType.Normal)
                {
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.DivineLove, divineCount.ToString());
                }
                else
                {
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.DivineLoveHigh, divineCount.ToString());
                }
            }

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            string reward = string.Empty;        
            int rewardId = config.GeRewardId(info.GotRewardList);
            int cardId = 0;

            DivineLoveRewardModel rewardModel = DivineLoveLibrary.GetDivineLoveReward(rewardId);
            if (rewardModel != null)
            {
                reward = rewardModel.Param;
                cardId = rewardModel.CardId;
            }

            if (!CheckDivineLoveInfo(info, index, rewardId))
            {
                //已领取或未开启一局
                Log.Warn($"player {Uid} GetDivineLoveReward failed: already got or round not open");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if ((ActivityPlayType)type == ActivityPlayType.High)
            {              
                info.HeartNum += heartNum;
                           
                DivineLoveFetterRewardModel fetterRewardModel = DivineLoveLibrary.GetDivineLoveFetterReward(cardId);
                if (fetterRewardModel != null)
                {
                    DivineLoveFetterRewardModel fetterReward = DivineLoveLibrary.GetDivineLoveFetterReward(fetterRewardModel.FetterId);       
                    if (fetterReward != null)
                    {
                        DivineLoveRewardModel tempReward = DivineLoveLibrary.GetDivineLoveByCardId(fetterReward.Id);
                        if (tempReward != null && info.GotRewardList.FirstOrDefault() == tempReward.Id)
                        {
                            reward += "|" + fetterReward.Reward;
                        }
                    }
                }
            }

            info.GotIndexList.Add(index);
            info.GotRewardList.Add(rewardId);
            SyncDbUpdateDivineLoveGetInfo(info);

            if (!string.IsNullOrEmpty(reward))
            {
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
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
            if ((ActivityPlayType)type == ActivityPlayType.Normal)
            {
                AddRewards(manager, ObtainWay.DivineLove, type.ToString());
            }
            else
            {
                AddRewards(manager, ObtainWay.DivineLoveHigh, type.ToString());
            }
            //通知前端奖励
            //manager.GenerateRewardMsg(msg.Rewards);
            msg.HeartNum = info.HeartNum;
            int gotCount = Math.Min(info.GotIndexList.Count, info.GotRewardList.Count);

            for (int i = 0; i < gotCount; i++)
            {
                DIVINE_LOVE_GOT_REWARD_INFO gotInfo = new DIVINE_LOVE_GOT_REWARD_INFO();
                gotInfo.Index = info.GotIndexList[i];
                int subRewardId = info.GotRewardList[i];
                DivineLoveRewardModel subRewardModel = DivineLoveLibrary.GetDivineLoveReward(subRewardId);
                gotInfo.RewardId = subRewardModel.RewardId;
                msg.GotList.Add(gotInfo);
            }
            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            MSG_ZR_CROSS_NOTES_LIST motesMsg = new MSG_ZR_CROSS_NOTES_LIST();
            motesMsg.Type = (int)NotesType.DivineLove;
            foreach (var rewardItem in manager.AllRewards)
            {
                if (config.AnnounceList.Contains(rewardItem.Id))
                {
                    CrossBroadcastAnnouncement(ANNOUNCEMENT_TYPE.DIVINE_LOVE_ITEM,
                        new List<string>() { server.MainId.ToString(), Name, rewardItem.Id.ToString() });
                }

                if (config.NotesList.Contains(rewardItem.Id))
                {
                    ZR_CROSS_NOTES notesItemMsg = GetCrossNotes(NotesType.DivineLove, server.MainId, Name, rewardItem.Id);
                    motesMsg.List.Add(notesItemMsg);
                }
            }
            SendCrossNotes(motesMsg);

            if ((ActivityPlayType)type == ActivityPlayType.High)
            {
                MSG_ZR_UPDATE_DIVINE_LOVE_VALUE updateMsg = new MSG_ZR_UPDATE_DIVINE_LOVE_VALUE();
                updateMsg.HeartNum = heartNum;
                server.SendToRelation(updateMsg, Uid);
            }
        }

        private bool CheckDivineLoveInfo(DivineLoveInfo info, int index, int rewardId)
        {
            if (info.GotIndexList.Contains(index) || info.GotRewardList.Contains(rewardId) || info.RoundState != 1)
            {
                return false;
            }
            return true;
        }

        public void GetDivineLoveCumulateReward(int type, int num)
        {
            DivineLoveConfig config = DivineLoveLibrary.GetDivineLoveConfig(type);
            if (config == null || (ActivityPlayType)type != ActivityPlayType.High)
            {
                Log.Warn($"player {Uid} GetDivineLoveCumulateReward failed: not type {type}");
                return;
            }

            MSG_ZGC_GET_DIVINE_LOVE_CUMULATE_REWARD msg = new MSG_ZGC_GET_DIVINE_LOVE_CUMULATE_REWARD();
            msg.Id = type;           

            bool inTime = false;
            Dictionary<int, RechargeGiftModel> gifts = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.DivineLove);
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
                Log.Warn($"player {Uid} GetDivineLoveCumulateReward failed: time is error");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }
            DivineLoveInfo info;
            DivineLoveMng.DivineLoveList.TryGetValue(type, out info);
            if (info == null)
            {
                Log.Warn($"player {Uid} GetDivineLoveCumulateReward failed: not open divine love yet");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (info.CumulateRewardList.Contains(num))
            {
                Log.Warn($"player {Uid} GetDivineLoveCumulateReward failed: has got {num} reward");
                msg.Result = (int)ErrorCode.AlreadyGot;
                Write(msg);
                return;
            }

            if (info.HeartNum < num)
            {
                Log.Warn($"player {Uid} GetDivineLoveCumulateReward failed: cur is {info.HeartNum } not {num}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            int rewardId = config.GetCumulateRewardId(num);
            if (rewardId <= 0)
            {
                Log.Warn($"player {Uid} GetDivineLoveCumulateReward failed: not num {num} reward");
                msg.Result = (int)ErrorCode.NotFindModel;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            DivineLoveRewardModel rewardModel = DivineLoveLibrary.GetDivineLoveReward(rewardId);
            if (rewardModel != null)
            {
                string reward = rewardModel.Param;
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
            }

            info.CumulateRewardList.Add(num);
            SyncDbUpdateDivineLoveCumulateRewards(info.Type, info.CumulateRewardList);

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
            AddRewards(manager, ObtainWay.DivineLoveHigh, type.ToString());
            //通知前端奖励
            //manager.GenerateRewardMsg(msg.Rewards);
            msg.HeartNum = info.HeartNum;
            msg.CumulateRewards.AddRange(info.CumulateRewardList);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }
     
        public void BuyDivineLoveItem(int type, int num)
        {
            DivineLoveConfig config = DivineLoveLibrary.GetDivineLoveConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} BuyDivineLoveItem failed: not type {type}");
                return;
            }

            if (config.CostItem > 0 && config.CostDiamond > 0)
            {
                if (num > 0 && config.BuyMaxCount >= num)
                {
                    MSG_ZGC_BUY_DIVINE_LOVE_ITEM msg = new MSG_ZGC_BUY_DIVINE_LOVE_ITEM();
                    msg.Id = type;
                    msg.Num = num;

                    int costDiamondNum = num * config.CostDiamond;
                    //使用钻石
                    if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                    {
                        Log.Warn($"player {Uid} BuyDivineLoveItem failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                        msg.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(msg);
                        return;
                    }

                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.BuyDivineLoveItem, type.ToString());

                    RewardManager manager = new RewardManager();
                    manager.AddReward(new ItemBasicInfo((int)RewardType.NormalItem, config.CostItem, num));
                    manager.BreakupRewards();
                    // 发放奖励
                    AddRewards(manager, ObtainWay.BuyDivineLoveItem, type.ToString());
                    //通知前端奖励
                    manager.GenerateRewardMsg(msg.Rewards);
                    msg.Result = (int)ErrorCode.Success;
                    Write(msg);

                }
                else
                {
                    Log.Warn($"player {Uid} BuyDivineLoveItem failed: num {num} max {config.BuyMaxCount}");
                    return;
                }
            }
            else
            {
                Log.Warn($"player {Uid} BuyDivineLoveItem failed: no CostItem {config.CostItem} CostDiamond {config.CostDiamond}");
                return;
            }
        }

        public void OpenDivineLoveRound(int type)
        {
            DivineLoveConfig config = DivineLoveLibrary.GetDivineLoveConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} OpenDivineLoveRound failed: not type {type}");
                return;
            }

            MSG_ZGC_OPEN_DIVINE_LOVE_ROUND msg = new MSG_ZGC_OPEN_DIVINE_LOVE_ROUND();
            msg.Id = type;

            bool inTime = false;
            Dictionary<int, RechargeGiftModel> gifts = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.DivineLove);
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
                Log.Warn($"player {Uid} OpenDivineLoveRound failed: time is error");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }
            DivineLoveInfo info;
            DivineLoveMng.DivineLoveList.TryGetValue(type, out info);
            if (info == null)
            {
                info = new DivineLoveInfo()
                {
                    Type = type,
                    RoundState = 1,
                };
                DivineLoveMng.AddInfo(info);
                SyncDbInsertDivineLoveInfo(info.Type, info.RoundState);
            }
            else
            {
                if (info.RoundState == 1)
                {
                    Log.Warn($"player {Uid} OpenDivineLoveRound failed: already open");
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
                info.RoundState = 1;
                SyncDbUpdateDivineLoveRoundState(info.Type, info.RoundState);
            }
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void CloseDivineLoveRound(int type)
        {
            DivineLoveConfig config = DivineLoveLibrary.GetDivineLoveConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} CloseDivineLoveRound failed: not type {type}");
                return;
            }

            MSG_ZGC_CLOSE_DIVINE_LOVE_ROUND msg = new MSG_ZGC_CLOSE_DIVINE_LOVE_ROUND();
            msg.Id = type;

            bool inTime = false;
            Dictionary<int, RechargeGiftModel> gifts = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.DivineLove);
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
                Log.Warn($"player {Uid} CloseDivineLoveRound failed: time is error");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }
            DivineLoveInfo info;
            DivineLoveMng.DivineLoveList.TryGetValue(type, out info);
            if (info == null)
            {
                Log.Warn($"player {Uid} CloseDivineLoveRound failed: not open divine love yet");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }
            else
            {
                if (info.RoundState == 0)
                {
                    Log.Warn($"player {Uid} OpenDivineLoveRound failed: already close");
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
                info.CloseRound();
                SyncDbUpdateDivineLoveRoundInfo(info);
            }
            SendDivineLoveInfo();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);          
        }

        private void SyncDbUpdateDivineLoveGetInfo(DivineLoveInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateDivineLoveGetInfo(Uid, info.Type, info.HeartNum, info.GotIndexList, info.GotRewardList));
        }

        private void SyncDbUpdateDivineLoveCumulateRewards(int type, List<int> cumulateRewardList)
        {
            server.GameDBPool.Call(new QueryUpdateDivineLoveCumulateRewards(Uid, type, cumulateRewardList));
        }

        private void SyncDbUpdateDivineLoveRoundInfo(DivineLoveInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateDivineLoveRoundInfo(Uid, info.Type, info.GotIndexList, info.GotRewardList, info.RoundState));
        }

        private void SyncDbInsertDivineLoveInfo(int type, int roundState)
        {
            server.GameDBPool.Call(new QueryInsertDivineLove(Uid, type, roundState));
        }

        private void SyncDbUpdateDivineLoveRoundState(int type, int roundState)
        {
            server.GameDBPool.Call(new QueryUpdateDivineLoveRoundState(Uid, type, roundState));
        }

        public MSG_ZMZ_DIVINE_LOVE GenerateDivineLoveTransformMsg()
        {
            MSG_ZMZ_DIVINE_LOVE msg = new MSG_ZMZ_DIVINE_LOVE();
            foreach (var kv in DivineLoveMng.DivineLoveList)
            {
                ZMZ_DIVINE_LOVE info = new ZMZ_DIVINE_LOVE();
                info.Type = kv.Value.Type;
                info.HeartNum = kv.Value.HeartNum;
                info.RoundState = kv.Value.RoundState;
                info.IndexList.AddRange(kv.Value.GotIndexList);
                info.RewardList.AddRange(kv.Value.GotRewardList);
                info.CumulateRewards.AddRange(kv.Value.CumulateRewardList);
                msg.List.Add(info);
            }
            return msg;
        }

        public void LoadDivineLoveTransform(MSG_ZMZ_DIVINE_LOVE msg)
        {
            foreach (var item in msg.List)
            {
                DivineLoveInfo info = new DivineLoveInfo();
                info.Type = item.Type;
                info.HeartNum = item.HeartNum;
                info.RoundState = item.RoundState;
                info.GotIndexList.AddRange(item.IndexList);
                info.GotRewardList.AddRange(item.RewardList);
                info.CumulateRewardList.AddRange(item.CumulateRewards);
                DivineLoveMng.AddInfo(info);
            }
        }
    }
}
