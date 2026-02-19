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
        public StoneWallManager StoneWallMng { get; set; }

        public void InitStoneWallManager()
        {
            StoneWallMng = new StoneWallManager(this);
        }

        public void SendStoneWallInfo()
        {
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.StoneWall, ZoneServerApi.now))
            {
                return;
            }
            MSG_ZGC_STONE_WALL_INFO_LIST msg = new MSG_ZGC_STONE_WALL_INFO_LIST();
            foreach (var kv in StoneWallMng.InfoList)
            {               
                STONE_WALL_INFO msgInfo = new STONE_WALL_INFO();
                StoneWallConfig config = StoneWallLibrary.GetStoneWallConfig(kv.Key);
                msgInfo.Id = kv.Key;
                StoneWallInfo info = kv.Value;
                int rewardId = 0;
                StoneWallRewardModel rewardModel = null;
                for (int i = 0; i < info.IndexInfos.Length; i++)
                {
                    for (int j = 0; j < info.IndexInfos[i].Length; j++)
                    {
                        rewardId = info.IndexInfos[i][j];
                        if (rewardId > 0)
                        {
                            rewardModel = StoneWallLibrary.GetStoneWallReward(rewardId);
                            if (rewardModel != null)
                            {
                                msgInfo.IndexList.Add(new STONE_WALL_INDEX_INFO() { Line = i, Column = j, RewardId = rewardModel.RewardId });
                            }
                        }
                    }
                }
                int boxCount;
                foreach (var item in config.BoxesTotalCount)
                {
                    rewardModel = StoneWallLibrary.GetStoneWallReward(item.Key);
                    if (rewardModel != null)
                    {
                        info.BoxesNumDic.TryGetValue(item.Key, out boxCount);
                        msgInfo.RestBoxNumList.Add(rewardModel.RewardId, item.Value - boxCount);
                    }
                }
                msgInfo.UnlockRewardList.AddRange(info.UnlockRewards);
                msg.List.Add(msgInfo);
            }
            Write(msg);
        }

        public void GetStoneWallInfoByLoading()
        {
            if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.StoneWall, ZoneServerApi.now))
            {
                GetStoneWallInfo();
            }
        }

        public void GetStoneWallInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_STONE_WALL_VALUE msg = new MSG_ZR_GET_STONE_WALL_VALUE();
            server.SendToRelation(msg, Uid);
        }

        public void GetStoneWallReward(int type, int line, int column, bool useDiamond)
        {
            StoneWallConfig config = StoneWallLibrary.GetStoneWallConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} GetStoneWallReward failed: not type {type}");
                return;
            }
            if (line < 0 || column < 0)
            {
                Log.Warn($"player {Uid} GetStoneWallReward failed: line or column param error");
                return;
            }

            MSG_ZGC_GET_STONE_WALL_REWARD msg = new MSG_ZGC_GET_STONE_WALL_REWARD();
            
            STONE_WALL_INDEX_INFO indexInfo = new STONE_WALL_INDEX_INFO()
            {
                Line = line,
                Column = column
            };
            msg.Index = indexInfo;

            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.StoneWall, ZoneServerApi.now))
            {
                Log.Warn($"player {Uid} GetStoneWallReward failed: time is error");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            int costDiamondNum = 0;
            int costItemNum = config.CostCount;

            BaseItem item = BagManager.GetItem(MainType.Consumable, config.CostItem);
            if (item != null)
            {
                //飞镖不足
                if (item.PileNum < costItemNum)
                {
                    if (!useDiamond || config.CostDiamond <= 0)
                    {
                        //不是用钻石
                        Log.Warn($"player {Uid} GetStoneWallReward failed: item count {item.PileNum} not enough");
                        msg.Result = (int)ErrorCode.ItemNotEnough;
                        Write(msg);
                        return;
                    }
                    //使用钻石
                    costDiamondNum = (costItemNum - item.PileNum) * config.CostDiamond;
                    costItemNum = item.PileNum;
                    //使用钻石
                    if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                    {
                        Log.Warn($"player {Uid} GetStoneWallReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                        msg.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(msg);
                        return;
                    }
                }
            }
            else
            {
                //飞镖不足
                if (!useDiamond || config.CostDiamond <= 0)
                {
                    //不是用钻石
                    Log.Warn($"player {Uid} GetStoneWallReward failed: item count 0 not enough");
                    msg.Result = (int)ErrorCode.ItemNotEnough;
                    Write(msg);
                    return;
                }
                costDiamondNum = costItemNum * config.CostDiamond;
                costItemNum = 0;
                //使用钻石
                if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                {
                    Log.Warn($"player {Uid} GetStoneWallReward failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                    msg.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(msg);
                    return;
                }
            }

            if (costItemNum > 0)
            {
                BaseItem it;
                if ((ActivityPlayType)type == ActivityPlayType.Normal)
                {
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.StoneWall);
                }
                else
                {
                    it = DelItem2Bag(item, RewardType.NormalItem, costItemNum, ConsumeWay.StoneWallHigh);
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
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.StoneWall, type.ToString());
                }
                else
                {
                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.StoneWallHigh, type.ToString());
                }
            }

            bool insert = false;
            StoneWallInfo info = StoneWallMng.GetStoneWallInfo(type);
            if (info == null)
            {
                info = CreateStoneWallInfo(type);
                StoneWallMng.AddStoneWallInfo(info);
                insert = true;
            }

            if (!CheckStoneWallInfo(info, line, column))
            {
                //已领取
                Log.Warn($"player {Uid} GetStoneWallReward failed: already got");
                msg.Result = (int)ErrorCode.AlreadyGot;
                Write(msg);
                return;
            }

            string reward = string.Empty;
            int rewardId = config.GeRewardId(info.BoxesNumDic);
            StoneWallRewardModel rewardModel = StoneWallLibrary.GetStoneWallReward(rewardId);
            if (rewardModel != null)
            {
                reward = rewardModel.Param;
            }

            STONE_WALL_INFO msgInfo = new STONE_WALL_INFO();
            msgInfo.Id = type;
            //锤子值
            if ((ActivityPlayType)type == ActivityPlayType.High)
            {
                info.HammerNum += config.CostCount;
            }
            msg.HammerNum = info.HammerNum;
            //索引奖励信息
            info.IndexInfos[line][column] = rewardId;
            //领取宝箱信息
            int boxNum;
            if (info.BoxesNumDic.TryGetValue(rewardId, out boxNum))
            {
                info.BoxesNumDic[rewardId] = ++boxNum;
            }
            else
            {
                info.BoxesNumDic.Add(rewardId, 1);
            }
          
            foreach (var kv in config.BoxesTotalCount)
            {
                rewardModel = StoneWallLibrary.GetStoneWallReward(kv.Key);
                if (rewardModel != null)
                {
                    info.BoxesNumDic.TryGetValue(kv.Key, out boxNum);
                    msgInfo.RestBoxNumList.Add(rewardModel.RewardId, kv.Value - boxNum);
                }
            }
            //解锁奖励信息
            int lineUnlockNum = 0;
            int columnUnlockNum = 0;
            int rewardCount = 0;
            for (int i = 0; i < info.IndexInfos.Length; i++)
            {
                for (int j = 0; j < info.IndexInfos[i].Length; j++)
                {
                    if (i == line && info.IndexInfos[i][j] > 0)
                    {
                        lineUnlockNum++;
                    }
                    if (j == column && info.IndexInfos[i][j] > 0)
                    {
                        columnUnlockNum++;
                    }
                    if (info.IndexInfos[i][j] > 0)
                    {
                        rewardCount++;
                        //索引位奖励信息
                        rewardModel = StoneWallLibrary.GetStoneWallReward(info.IndexInfos[i][j]);
                        if (rewardModel != null)
                        {
                            msgInfo.IndexList.Add(new STONE_WALL_INDEX_INFO() { Line = i, Column = j, RewardId = rewardModel.RewardId });
                        }
                    }
                }
            }
            List<StoneWallUnlockRewardModel> unlockRewards = StoneWallLibrary.GetStoneWallUnlockReward(type, line, lineUnlockNum, column, columnUnlockNum, rewardCount);
            foreach (var unlockReward in unlockRewards)
            {
                info.UnlockRewards.Add(unlockReward.Id);
                reward += "|" + unlockReward.Reward;
            }
            msgInfo.UnlockRewardList.AddRange(info.UnlockRewards);
            msg.Info = msgInfo;

            if (insert)
            {
                SyncDbInsertStoneWallInfo(info);
            }
            else
            {
                SyncDbUpdateStoneWallGetInfo(info);
            }

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();           

            if (!string.IsNullOrEmpty(reward))
            {
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
            }
            manager.AddReward(rewardItems);
            manager.BreakupRewards(true);
            // 发放奖励
            if ((ActivityPlayType)type == ActivityPlayType.Normal)
            {
                AddRewards(manager, ObtainWay.StoneWall, type.ToString());
            }
            else
            {
                AddRewards(manager, ObtainWay.StoneWallHigh, type.ToString());
            }
            manager.GenerateRewardMsg(msg.Rewards);

            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            SendStoneWallCrossNoteMsg(manager, config);           

            if ((ActivityPlayType)type == ActivityPlayType.High)
            {
                MSG_ZR_UPDATE_STONE_WALL_VALUE updateMsg = new MSG_ZR_UPDATE_STONE_WALL_VALUE();
                updateMsg.HammerNum = config.CostCount;
                server.SendToRelation(updateMsg, Uid);
            }
        }

        private bool CheckStoneWallInfo(StoneWallInfo info, int line, int column)
        {
            if (info.IndexInfos[line][column] > 0)
            {
                return false;
            }
            return true;
        }

        public void BuyStoneWallItem(int type, int num)
        {
            StoneWallConfig config = StoneWallLibrary.GetStoneWallConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} BuyStoneWallItem failed: not type {type}");
                return;
            }

            if (config.CostItem > 0 && config.CostDiamond > 0)
            {
                if (num > 0 && config.BuyMaxCount >= num)
                {
                    MSG_ZGC_BUY_STONE_WALL_ITEM msg = new MSG_ZGC_BUY_STONE_WALL_ITEM();
                    msg.Id = type;
                    msg.Num = num;

                    int costDiamondNum = num * config.CostDiamond;
                    //使用钻石
                    if (GetCoins(CurrenciesType.diamond) < costDiamondNum)
                    {
                        Log.Warn($"player {Uid} BuyStoneWallItem failed: diamond count {GetCoins(CurrenciesType.diamond)} not enough");
                        msg.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(msg);
                        return;
                    }

                    DelCoins(CurrenciesType.diamond, costDiamondNum, ConsumeWay.BuyStoneWallItem, type.ToString());

                    RewardManager manager = new RewardManager();
                    manager.AddReward(new ItemBasicInfo((int)RewardType.NormalItem, config.CostItem, num));
                    manager.BreakupRewards();
                    // 发放奖励
                    AddRewards(manager, ObtainWay.BuyStoneWallItem, type.ToString());
                    //通知前端奖励
                    manager.GenerateRewardMsg(msg.Rewards);
                    msg.Result = (int)ErrorCode.Success;
                    Write(msg);

                }
                else
                {
                    Log.Warn($"player {Uid} BuyStoneWallItem failed: num {num} max {config.BuyMaxCount}");
                    return;
                }
            }
            else
            {
                Log.Warn($"player {Uid} BuyStoneWallItem failed: no CostItem {config.CostItem} CostDiamond {config.CostDiamond}");
                return;
            }
        }

        public void ResetStoneWall(int type)
        {
            MSG_ZGC_RESET_STONE_WALL msg = new MSG_ZGC_RESET_STONE_WALL();
            msg.Id = type;

            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.StoneWall, ZoneServerApi.now))
            {
                Log.Warn($"player {Uid} ResetStoneWall failed: not in activity time");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            StoneWallInfo info = StoneWallMng.GetStoneWallInfo(type);
            if (info == null)
            {
                Log.Warn($"player {Uid} ResetStoneWall failed: type {type} not right");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            info.Reset();
            SyncDbUpdateStoneWallGetInfo(info);
            SendStoneWallInfo();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        private StoneWallInfo CreateStoneWallInfo(int type)
        {
            StoneWallInfo info = new StoneWallInfo();           
            info.Type = type;
            info.IndexInfos = new int[StoneWallLibrary.MaxLine][];
            for (int i = 0; i < StoneWallLibrary.MaxLine; i++)
            {
                for (int j = 0; j < StoneWallLibrary.MaxColumn; j++)
                {
                    if (j == 0)
                    {
                        info.IndexInfos[i] = new int[StoneWallLibrary.MaxColumn];
                    }
                    info.IndexInfos[i][j] = 0;
                }
            }
            return info;
        }

        private void SendStoneWallCrossNoteMsg(RewardManager manager, StoneWallConfig config)
        {
            MSG_ZR_CROSS_NOTES_LIST motesMsg = new MSG_ZR_CROSS_NOTES_LIST();
            motesMsg.Type = (int)NotesType.StoneWall;
            foreach (var rewardItem in manager.AllRewards)
            {
                if (config.AnnounceList.Contains(rewardItem.Id))
                {
                    CrossBroadcastAnnouncement(ANNOUNCEMENT_TYPE.STONE_WALL_ITEM,
                        new List<string>() { server.MainId.ToString(), Name, rewardItem.Id.ToString() });
                }

                if (config.NotesList.Contains(rewardItem.Id))
                {
                    ZR_CROSS_NOTES notesItemMsg = GetCrossNotes(NotesType.StoneWall, server.MainId, Name, rewardItem.Id);
                    motesMsg.List.Add(notesItemMsg);
                }
            }
            SendCrossNotes(motesMsg);
        }

        private void SyncDbInsertStoneWallInfo(StoneWallInfo info)
        {
            server.GameDBPool.Call(new QueryInsertStoneWallInfo(Uid, info.Type, info.HammerNum, info.IndexInfos, info.BoxesNumDic));
        }

        private void SyncDbUpdateStoneWallGetInfo(StoneWallInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateStoneWallGetInfo(Uid, info.Type, info.HammerNum, info.IndexInfos, info.BoxesNumDic, info.UnlockRewards));
        }

        public MSG_ZMZ_STONE_WALL_INFO GenerateStoneWallTransformMsg()
        {
            MSG_ZMZ_STONE_WALL_INFO msg = new MSG_ZMZ_STONE_WALL_INFO();
            foreach (var info in StoneWallMng.InfoList)
            {
                msg.List.Add(GenerateStoneWallInfo(info.Value));
            }
            return msg;
        }

        private ZMZ_STONE_WALL_INFO GenerateStoneWallInfo(StoneWallInfo info)
        {
            ZMZ_STONE_WALL_INFO msg = new ZMZ_STONE_WALL_INFO();
            msg.Type = info.Type;
            msg.HammerNum = info.HammerNum;
            for (int i = 0; i < info.IndexInfos.Length; i++)
            {
                ZMZ_STONE_WALL_INDEX indexInfo = new ZMZ_STONE_WALL_INDEX();
                for (int j = 0; j < info.IndexInfos[i].Length; j++)
                {
                    indexInfo.RewardIds.Add(info.IndexInfos[i][j]);
                }
                msg.IndexInfos.Add(indexInfo);
            }
            foreach (var item in info.BoxesNumDic)
            {
                msg.BoxNums.Add(item.Key, item.Value);
            }
            msg.UnlockRewards.AddRange(info.UnlockRewards);
            return msg;
        }

        public void LoadStoneWallTransform(MSG_ZMZ_STONE_WALL_INFO msg)
        {
            foreach (var item in msg.List)
            {
                StoneWallInfo info = new StoneWallInfo();
                info.Type = item.Type;
                info.HammerNum = item.HammerNum;
                info.IndexInfos = new int[item.IndexInfos.Count][];
                for (int i = 0; i < item.IndexInfos.Count; i++)
                {
                    for (int j = 0; j < item.IndexInfos[i].RewardIds.Count; j++)
                    {
                        if (j == 0)
                        {
                            info.IndexInfos[i] = new int[item.IndexInfos[i].RewardIds.Count];
                        }
                        info.IndexInfos[i][j] = item.IndexInfos[i].RewardIds[j];
                    }
                }
                foreach (var kv in item.BoxNums)
                {
                    info.BoxesNumDic.Add(kv.Key, kv.Value);
                }
                info.UnlockRewards.AddRange(item.UnlockRewards);

                StoneWallMng.AddStoneWallInfo(info);
            }
        }
    }
}
