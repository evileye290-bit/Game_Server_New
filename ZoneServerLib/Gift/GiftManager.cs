using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class GiftManager
    {
        public PlayerChar Owner { get; private set; }
        private Dictionary<RechargeGiftType, Gift> giftList = new Dictionary<RechargeGiftType, Gift>();
        private Dictionary<int, CultivateGift> cultivateGifts = new Dictionary<int, CultivateGift>();
        private PettyGift pettyGift = new PettyGift((int)RechargeGiftType.PettyMoney);
        private Dictionary<int, DailyRechargeInfo> dailyRechargeList = new Dictionary<int, DailyRechargeInfo>();
        private Dictionary<int, HeroDaysRewardsInfo> heroDaysRewardsList = new Dictionary<int, HeroDaysRewardsInfo>();
        private Dictionary<int, NewServerPromotionInfo> newServerPromotionList = new Dictionary<int, NewServerPromotionInfo>();
        private Dictionary<int, LuckyFlipCardInfo> luckyFlipCardList = new Dictionary<int, LuckyFlipCardInfo>();
        private Dictionary<int, IslandHighGiftInfo> islandGiftList = new Dictionary<int, IslandHighGiftInfo>();
        private Dictionary<int, TreasureFlipCardInfo> treasureFlipCardList = new Dictionary<int, TreasureFlipCardInfo>();

        public int LuckFCPeriod { get; private set; }
        public int TreasureFCPeriod { get; private set; }
        public int FlipCardNum { get; private set; }

        public GiftManager(PlayerChar owner)
        {
            this.Owner = owner;
        }

        public void BindGiftInfo(Dictionary<RechargeGiftType, DbGiftInfo> giftInfo)
        {
            giftInfo.Remove(RechargeGiftType.LimitTime);
            foreach (var kv in giftInfo)
            {
                Gift gift = new Gift();
                gift.BindDbItems(kv.Value.ItemList);
                giftList.Add(kv.Key, gift);
            }
        }

        public void BindLimitTimeGiftInfo(DbGiftInfo limitTimeGifts)
        {
            Gift gift = new Gift();
            gift.BindLimitTimeDbItems(limitTimeGifts.LimitTimeItemList);
            giftList[RechargeGiftType.LimitTime] = gift;
        }

        public MSG_ZGC_GIFT_INFO GenerateGiftInfoMsg()
        {
            MSG_ZGC_GIFT_INFO msg = new MSG_ZGC_GIFT_INFO();
            foreach (var gift in giftList)
            {
                msg.GiftList.Add(GenerateGiftListMsg((int)gift.Key, gift.Value));
            }
            msg.Result = (int)ErrorCode.Success;
            return msg;
        }

        private ZGC_GIFT_LIST GenerateGiftListMsg(int giftType, Gift gift)
        {
            ZGC_GIFT_LIST msg = new ZGC_GIFT_LIST();
            msg.GiftId = giftType;
            foreach (var item in gift.ItemList)
            {
                msg.ItemList.Add(GenerateGiftItemMsg(item.Value));
            }
            return msg;
        }

        private ZGC_GIFT_ITEM GenerateGiftItemMsg(GiftItem item)
        {
            ZGC_GIFT_ITEM msg = new ZGC_GIFT_ITEM();
            msg.GiftItemId = item.Id;
            msg.BuyCount = item.CurBuyCount;
            msg.RewardRatio = item.DiamondRatio;
            msg.Discount = item.GetDiscount();
            return msg;
        }

        public string BuildGiftItemIdString(RechargeGiftType giftType)
        {
            Gift gift;
            giftList.TryGetValue(giftType, out gift);
            if (gift != null)
            {
                return gift.BuildGiftItemIdString();
            }
            return "";
        }

        public string BuildGiftBuyCountString(RechargeGiftType giftType)
        {
            Gift gift;
            giftList.TryGetValue(giftType, out gift);
            if (gift != null)
            {
                return gift.BuildGiftBuyCountString();
            }
            return "";
        }

        public string BuildGiftCurBuyCountString(RechargeGiftType giftType)
        {
            Gift gift;
            giftList.TryGetValue(giftType, out gift);
            if (gift != null)
            {
                return gift.BuildGiftCurBuyCountString();
            }
            return "";
        }

        public string BuildGiftDoubleFlagString(RechargeGiftType giftType)
        {
            Gift gift;
            giftList.TryGetValue(giftType, out gift);
            if (gift != null)
            {
                return gift.BuildGiftDoubleFlagString();
            }
            return "";
        }

        public string BuildGiftDiscountString(RechargeGiftType giftType)
        {
            Gift gift;
            giftList.TryGetValue(giftType, out gift);
            if (gift != null)
            {
                return gift.BuildGiftDiscountString();
            }
            return "";
        }

        public string BuildGiftItemStartTimeString(RechargeGiftType giftType)
        {
            Gift gift;
            giftList.TryGetValue(giftType, out gift);
            if (gift != null)
            {
                return gift.BuildGifItemStartTimeString();
            }
            return "";
        }

        public bool CheckHaveThisTypeGift(RechargeItemModel item)
        {
            Gift gift;
            if (giftList.TryGetValue(item.GiftType, out gift))
            {
                return true;
            }
            return false;
        }

        public bool ClearTypeGift(RechargeGiftType giftType)
        {
            Gift gift;
            if (giftList.TryGetValue(giftType, out gift))
            {
                List<int> needRefreshList = new List<int>();
                foreach (var kv in gift.ItemList)
                {
                    if (kv.Value.CurBuyCount > 0)
                    {
                        gift.ReSetItemCurBuyCount(kv.Key);
                        needRefreshList.Add(kv.Key);
                    }
                }
                //SyncDbUpdateGiftCurBuyCount(giftType);
                SyncGiftRefreshMsg(removeList: needRefreshList);

                giftList.Remove(giftType);
                return true;
            }
            return false;
        }

        public bool CheckGiftItemHaveBuyCount(RechargeItemModel model)
        {
            Gift gift;
            if (giftList.TryGetValue(model.GiftType, out gift))
            {
                if (!gift.CheckHasBuyCount(model))
                {
                    return false;
                }
                else if (model.GiftType == RechargeGiftType.DirectPurchase && !CheckCanBuyDirectPurchaseGift(model))
                {
                    return false;
                }
            }
            else
            {
                if (!CheckDifferentGiftHaveBuyCount(model))
                {
                    return false;
                }
            }
            return true;
        }

        public GiftItem UpdateGiftItem(RechargeItemModel model)
        {
            Gift gift;
            GiftItem giftItem;
            if (giftList.TryGetValue(model.GiftType, out gift))
            {
                if (!gift.CheckHasBuyCount(model))
                {
                    return null;
                }
                giftItem = gift.UpdateGiftItem(model, Owner);
            }
            else
            {
                gift = new Gift();
                giftItem = gift.CreateItem(model, Owner);
                giftList.Add(model.GiftType, gift);
            }
            return giftItem;
        }

        public GiftItem GetGiftItem(RechargeItemModel model)
        {
            Gift gift;
            GiftItem giftItem = null;
            if (giftList.TryGetValue(model.GiftType, out gift))
            {
                giftItem = gift.RandomNotBoughtLimitTimeGift(model.Id);
            }
            return giftItem;
        }

        public void RefreshDailyRechargeGift()
        {
            List<GiftItem> needRefreshList = new List<GiftItem>();
            foreach (var gift in giftList)
            {
                switch (gift.Key)
                {
                    case RechargeGiftType.Common:
                        break;
                    default:
                        continue;
                }
                Dictionary<int, GiftItem> dic;
                gift.Value.SubTypeList.TryGetValue((int)CommonGiftType.Daily, out dic);
                if (dic != null)
                {
                    foreach (var kv in dic)
                    {
                        if (kv.Value.CurBuyCount > 0)
                        {
                            gift.Value.ReSetItemCurBuyCount(kv.Key);
                            needRefreshList.Add(kv.Value);
                        }
                    }
                    SyncDbUpdateGiftCurBuyCount(gift.Key);
                }
            }
            SyncGiftRefreshMsg(needRefreshList);
        }

        public void RefreshWeeklyRechargeGift()
        {
            List<GiftItem> needRefreshList = new List<GiftItem>();
            foreach (var gift in giftList)
            {
                switch (gift.Key)
                {
                    case RechargeGiftType.Common:
                        break;
                    default:
                        continue;
                }
                Dictionary<int, GiftItem> dic;
                gift.Value.SubTypeList.TryGetValue((int)CommonGiftType.Weekly, out dic);
                if (dic != null)
                {
                    foreach (var kv in dic)
                    {
                        if (kv.Value.CurBuyCount > 0)
                        {
                            gift.Value.ReSetItemCurBuyCount(kv.Key);
                            needRefreshList.Add(kv.Value);
                        }
                    }
                    SyncDbUpdateGiftCurBuyCount(gift.Key);
                }
            }
            SyncGiftRefreshMsg(needRefreshList);
        }

        public void RefreshMonthlyRechargeGift()
        {
            List<GiftItem> needRefreshList = new List<GiftItem>();
            foreach (var gift in giftList)
            {
                switch (gift.Key)
                {
                    case RechargeGiftType.Common:
                        break;
                    default:
                        continue;
                }
                Dictionary<int, GiftItem> dic;
                gift.Value.SubTypeList.TryGetValue((int)CommonGiftType.Monthly, out dic);
                if (dic != null)
                {
                    foreach (var kv in dic)
                    {
                        if (kv.Value.CurBuyCount > 0)
                        {
                            gift.Value.ReSetItemCurBuyCount(kv.Key);
                            needRefreshList.Add(kv.Value);
                        }
                    }
                    SyncDbUpdateGiftCurBuyCount(gift.Key);
                }
            }
            SyncGiftRefreshMsg(needRefreshList);
        }

        private void SyncGiftRefreshMsg(List<GiftItem> giftItems = null, List<int> removeList = null)
        {
            MSG_ZGC_GIFT_INFO msg = new MSG_ZGC_GIFT_INFO();
            msg.Result = (int)ErrorCode.Success;
            if (giftItems != null && giftItems.Count > 0)
            {
                ZGC_GIFT_LIST giftList = new ZGC_GIFT_LIST();
                giftList.GiftId = (int)RechargeGiftType.Common;
                foreach (var item in giftItems)
                {
                    giftList.ItemList.Add(GenerateGiftItemMsg(item));
                }
                msg.GiftList.Add(giftList);
            }
            if (removeList != null && removeList.Count > 0)
            {
                msg.RemoveList.AddRange(removeList);
            }
            Owner.Write(msg);
        }

        public ulong RecordActionTriggerGiftTime(int giftItemId, int actionId, bool isSdkGift, string dataBox)
        {
            RechargeItemModel giftItem = RechargeLibrary.GetRechargeItem(giftItemId, isSdkGift);
            if (giftItem == null)
            {
                return 0;
            }
            Gift gift;
            GiftItem item;
            giftList.TryGetValue(giftItem.GiftType, out gift);
            string startTime = ZoneServerApi.nowString;
            ulong uid = Owner.server.UID.NewIuid(Owner.server.MainId, Owner.server.SubId);
            if (gift == null)
            {
                gift = new Gift();
                item = gift.CreateLimitTimeGiftItem(uid, giftItem, startTime, isSdkGift, dataBox);
                giftList.Add(giftItem.GiftType, gift);
                SyncDbInsertGiftStartTime(uid, giftItem, startTime, isSdkGift, dataBox);
            }
            else
            {
                item = gift.CreateLimitTimeGiftItem(uid, giftItem, startTime, isSdkGift, dataBox);
                SyncDbInsertGiftStartTime(uid, giftItem, startTime, isSdkGift, dataBox);
            }
            NotifyClientGiftOpen(giftItem, startTime, uid, isSdkGift);
            return uid;
        }

        private void SyncDbInsertGiftStartTime(ulong uid, RechargeItemModel giftItem, string startTime, bool isSdkGift, string dataBox)
        {
            QueryInsertGiftStartTime query = new QueryInsertGiftStartTime(uid, Owner.Uid, giftItem.Id, 0, startTime, isSdkGift, dataBox);
            Owner.server.GameDBPool.Call(query);
        }

        private void NotifyClientGiftOpen(RechargeItemModel giftItem, string startTime, ulong giftUid, bool isSdkGift)
        {
            MSG_ZGC_GIFT_OPEN notify = new MSG_ZGC_GIFT_OPEN();
            notify.GiftItemId = giftItem.Id;
            notify.SatrtTime = Timestamp.GetUnixTimeStampSeconds(DateTime.Parse(startTime));
            notify.UidHigh = giftUid.GetHigh();
            notify.UidLow = giftUid.GetLow();
            notify.Visible = true;
            notify.IsSdkGift = isSdkGift;
            Owner.Write(notify);
        }

        public int GetRechargeItemRatio(int rechargeItemId)
        {
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItemOrSdkItem(rechargeItemId);
            //if (recharge.GiftType != RechargeGiftType.Common || (CommonGiftType)recharge.SubType != CommonGiftType.Diamond)
            //{
            //    return false;
            //}
            Gift gift;
            giftList.TryGetValue(recharge.GiftType, out gift);
            if (gift == null)
            {
                return 2;
            }
            return gift.GetRechargeItemRatio(rechargeItemId);
        }

        //重置钻石礼包双倍
        public void ResetDiamondGiftDoubleFlag()
        {
            Gift gift;
            giftList.TryGetValue(RechargeGiftType.Common, out gift);
            if (gift == null)
            {
                return;
            }
            Dictionary<int, GiftItem> diamondGiftDic;
            gift.SubTypeList.TryGetValue((int)CommonGiftType.Diamond, out diamondGiftDic);
            if (diamondGiftDic == null)
            {
                return;
            }
            MSG_ZGC_RESET_DOUBLE_FLAG response = new MSG_ZGC_RESET_DOUBLE_FLAG();

            foreach (var item in diamondGiftDic)
            {
                if (item.Value.DoubleFlag == 0)
                {
                    item.Value.ResetDoubleFlag();
                }
                if (item.Value.DiamondRatio < 2)
                {
                    item.Value.ResetDiamondRatio();
                    response.Items.Add(new ZGC_DOUBLE_RECHARGE_ITEM() { GiftItemId = item.Value.Id, RewardRatio = item.Value.DiamondRatio });
                }
            }
            SyncDbUpdateGiftDoubleFlag(RechargeGiftType.Common);

            response.Result = (int)ErrorCode.Success;
            Owner.Write(response);
        }

        public int GetGiftItemCurBuyCount(int itemId)
        {
            foreach (var gift in giftList.Values)
            {
                GiftItem item = gift.GetGiftItem(itemId);
                if (item != null)
                {
                    return item.CurBuyCount;
                }
            }
            return 0;
        }

        public bool CheckHasDiscountPrice(RechargeItemModel recharge, RechargePriceModel discountPrice)
        {
            //RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(giftItemId);
            //RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
            if (recharge == null || discountPrice == null)
            {
                return false;
            }
            if (recharge.DiscountRechargeId == 0 || discountPrice.Price == 0)
            {
                return false;
            }
            Gift gift;
            giftList.TryGetValue(recharge.GiftType, out gift);
            if (gift == null)
            {
                return true;
            }
            return gift.CheckHasDiscountPrice(recharge.Id);
        }

        public void RefreshRechargeDiscount()
        {
            MSG_ZGC_RESET_RECHARGE_DISCOUNT response = new MSG_ZGC_RESET_RECHARGE_DISCOUNT();
            Gift gift;
            foreach (var kv in giftList)
            {
                if (giftList.TryGetValue(kv.Key, out gift))
                {
                    List<GiftItem> resetList = gift.RefreshRechargeDiscount();
                    foreach (var item in resetList)
                    {
                        response.Items.Add(new ZGC_RECHARGE_ITEM() { GiftItemId = item.Id, Discount = item.GetDiscount() });
                    }
                    SyncDbUpdateGiftDiscount(kv.Key);
                }
            }
            Owner.Write(response);
        }

        private void SyncDbUpdateGiftDoubleFlag(RechargeGiftType giftType)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateGiftItemDoubleFlag(Owner.Uid, giftType, BuildGiftDoubleFlagString(giftType), BuildDiamondRatioString(giftType)));
        }

        private void SyncDbUpdateGiftDiscount(RechargeGiftType giftType)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateGiftItemDiscount(Owner.Uid, giftType, BuildGiftDiscountString(giftType)));
        }

        private void SyncDbUpdateGiftCurBuyCount(RechargeGiftType giftType)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateGiftItemCurBuyCount(Owner.Uid, giftType, BuildGiftCurBuyCountString(giftType)));
        }

        public void RefreshPassCardBuyState()
        {
            List<GiftItem> needRefreshList = new List<GiftItem>();
            Gift gift;
            giftList.TryGetValue(RechargeGiftType.PassCard, out gift);
            if (gift != null)
            {
                foreach (var kv in gift.ItemList)
                {
                    if (kv.Value.CurBuyCount > 0)
                    {
                        gift.ReSetItemCurBuyCount(kv.Key);
                        needRefreshList.Add(kv.Value);
                    }
                }
                SyncDbUpdateGiftCurBuyCount(RechargeGiftType.PassCard);
            }
            SyncGiftRefreshMsg(needRefreshList);
        }

        private bool CheckDifferentGiftHaveBuyCount(RechargeItemModel model)
        {
            switch (model.GiftType)
            {
                case RechargeGiftType.PettyMoney:
                    if (!CheckPettyGiftHaveBuyCount(model))
                    {
                        return false;
                    }
                    break;
                case RechargeGiftType.DailyRecharge:
                    if (!CheckDailyRechargeHaveBuyCount(model))
                    {
                        return false;
                    }
                    break;
                case RechargeGiftType.NewServerPromotion:
                    if (!CheckNewServerPromotionHaveBuyCount(model))
                    {
                        return false;
                    }
                    break;
                case RechargeGiftType.LuckyFlipCard:
                    if (!CheckLuckyFlipCardHaveBuyCount(model))
                    {
                        return false;
                    }
                    break;
                case RechargeGiftType.IslandHighGift:
                    if (!CheckIslandHighGiftHaveBuyCount(model))
                    {
                        return false;
                    }
                    break;
                case RechargeGiftType.Trident:
                    if (!Owner.CheckTridentRechargeLegal(model))
                    {
                        return false;
                    }
                    break;
                case RechargeGiftType.DragonBoat:
                    if (!Owner.CheckCanBuyDragonBoatRights(model))
                    {
                        return false;
                    }
                    break;
                case RechargeGiftType.DaysRecharge:
                    return Owner.DaysRechargeManager.CheckDaysRecharge(model);
                case RechargeGiftType.DirectPurchase:
                    return CheckCanBuyDirectPurchaseGift(model);
                default:
                    break;
            }
            return true;
        }

        public MSG_ZGC_LIMIT_TIME_GIFTS GenerateOpenedGiftListMsg()
        {
            MSG_ZGC_LIMIT_TIME_GIFTS msg = new MSG_ZGC_LIMIT_TIME_GIFTS();
            Gift gift;
            giftList.TryGetValue(RechargeGiftType.LimitTime, out gift);
            if (gift != null)
            {
                foreach (var item in gift.LimitTimeItemList)
                {
                    if ((ZoneServerApi.now - DateTime.Parse(item.Value.StartTime)).TotalSeconds < RechargeLibrary.GiftLimitTime * 3600 && item.Value.BuyCount == 0)
                    {
                        msg.GiftItemList.Add(new ZGC_LIMIT_TIME_GIFT() 
                            { 
                                GiftItemId = item.Value.Id, 
                                SatrtTime = Timestamp.GetUnixTimeStampSeconds(DateTime.Parse(item.Value.StartTime)), 
                                UidHigh = item.Value.Uid.GetHigh(), 
                                UidLow = item.Value.Uid.GetLow(), 
                                Visible = true,
                                IsSdkGift = item.Value.IsSdkGift
                            }
                        );
                    }
                }
            }
            return msg;
        }

        public void GenerateGiftInfoListMsg(List<MSG_ZMZ_GIFT_INFO_LIST> msgList)
        {
            foreach (var gift in giftList)
            {
                var list = GenerateGiftInfoMsg((int)gift.Key, gift.Value);
                foreach (var kv in list)
                {
                    MSG_ZMZ_GIFT_INFO_LIST msg = new MSG_ZMZ_GIFT_INFO_LIST();
                    msg.GiftList.Add(kv);
                    msgList.Add(msg);
                }
            }
        }

        private static int transformLimit = 20;
        private List<ZMZ_GIFT_INFO> GenerateGiftInfoMsg(int giftType, Gift gift)
        {
            int num = 0;
            List<ZMZ_GIFT_INFO> infos = new List<ZMZ_GIFT_INFO>();
            ZMZ_GIFT_INFO msg = new ZMZ_GIFT_INFO();
            msg.GiftType = giftType;

            foreach (var item in gift.ItemList)
            {
                if (++num > transformLimit)
                {
                    infos.Add(msg);

                    num = 0;
                    msg = new ZMZ_GIFT_INFO();
                    msg.GiftType = giftType;
                }
                msg.ItemList.Add(GenerateRegularGiftItemTransformMsg(item.Value));
            }

            foreach (var item in gift.LimitTimeItemList)
            {
                if (++num > transformLimit)
                {
                    num = 0;
                    infos.Add(msg);
                    msg = new ZMZ_GIFT_INFO();
                    msg.GiftType = giftType;
                }
                msg.ItemList.Add(GenerateLmitTimeGiftItemTransformMsg(item.Value));
            }

            infos.Add(msg);
            return infos;
        }

        private ZMZ_GIFT_ITEM GenerateRegularGiftItemTransformMsg(GiftItem item)
        {
            ZMZ_GIFT_ITEM msg = new ZMZ_GIFT_ITEM()
            {
                ItemId = item.Id, 
                BuyCount = item.BuyCount, 
                CurBuyCount = item.CurBuyCount, 
                DoubleFlag = item.DoubleFlag, 
                Discount = item.Discount , 
				RewardRatio = item.DiamondRatio,
                IsSdkGift = item.IsSdkGift,
                DataBox = item.DataBox,
            };
            return msg;
        }

        private ZMZ_GIFT_ITEM GenerateLmitTimeGiftItemTransformMsg(GiftItem item)
        {
            ZMZ_GIFT_ITEM msg = new ZMZ_GIFT_ITEM()
            {
                ItemId = item.Id,
                BuyCount = item.BuyCount, 
                Uid = item.Uid, 
                StartTime = item.StartTime ,
                IsSdkGift = item.IsSdkGift,
                DataBox = item.DataBox,
            };
            return msg;
        }

        public void LoadGiftListInfoTransform(RepeatedField<ZMZ_GIFT_INFO> giftListInfo)
        {
            foreach (var info in giftListInfo)
            {
                RechargeGiftType giftType = (RechargeGiftType)info.GiftType;
                Gift gift;
                if (!giftList.TryGetValue(giftType, out gift))
                {
                    gift = new Gift();
                    giftList.Add(giftType, gift);
                }

                foreach (var item in info.ItemList)
                {
                    RechargeItemModel model = RechargeLibrary.GetRechargeItem(item.ItemId);
                    gift.LoadItem(model, Owner, item);
                }
            }
        }

        #region 养成礼包
        public void BindCultivateGiftInfo(Dictionary<int, DbGift2Info> dbGiftList)
        {
            foreach (var kv in dbGiftList)
            {
                CultivateGift gift = new CultivateGift(kv.Key);
                gift.BindDbItems(kv.Value.LimitTimeItemList);
                cultivateGifts.Add(kv.Key, gift);
            }
        }

        public void TriggerCultivateGift(List<GiftItemModel> giftItems, int num)
        {
            List<GiftItemModel> triggerItems = new List<GiftItemModel>();
            foreach (var item in giftItems)
            {
                if (num == item.TrggerParam)
                {
                    triggerItems.Add(item);
                }
            }
            if (triggerItems.Count == 1)
            {
                InsertCultivateGift(triggerItems.FirstOrDefault());
            }
            else if (triggerItems.Count > 1)
            {
                int giftType;
                if (!ChechCultivateGiftIsSameType(triggerItems, out giftType))
                {
                    Logger.Log.Warn($"player {Owner.Uid} trigger cultivate gift failed: trigger different type gift");
                    return;
                }
                BatchInsertCultivateGift(giftType, triggerItems);
            }
        }

        public void TriggerCultivateGift(List<GiftItemModel> giftItems)
        {
            List<GiftItemModel> triggerItems = new List<GiftItemModel>();
            foreach (var item in giftItems)
            {
                switch ((TriggerGiftType)item.TriggerType)
                {
                    case TriggerGiftType.MainTask:
                        if (Owner.MainTaskId >= item.TrggerParam)
                        {
                            triggerItems.Add(item);
                        }
                        break;
                    case TriggerGiftType.BranchTask:
                        if (Owner.BranchTaskIds.Contains(item.TrggerParam))
                        {
                            triggerItems.Add(item);
                        }
                        break;
                    default:
                        break;
                }
            }
            if (triggerItems.Count == 1)
            {
                InsertCultivateGift(triggerItems.FirstOrDefault());
            }
            else if (triggerItems.Count > 1)
            {
                int giftType;
                if (!ChechCultivateGiftIsSameType(triggerItems, out giftType))
                {
                    Logger.Log.Warn($"player {Owner.Uid} trigger cultivate gift failed: trigger different type gift");
                    return;
                }
                BatchInsertCultivateGift(giftType, triggerItems);
            }
        }

        private void InsertCultivateGift(GiftItemModel giftModel)
        {
            CultivateGiftItem giftItem = AddCultivateGiftItem(giftModel);

            if (giftItem != null)
            {
                SyncDbInsertCultivateGift(giftItem);
                List<CultivateGiftItem> list = new List<CultivateGiftItem>() { giftItem };
                NotifyCultivateGiftOpen(giftItem.Type, list);
            }
        }

        private void BatchInsertCultivateGift(int giftType, List<GiftItemModel> giftModelList)
        {
            List<CultivateGiftItem> giftItemList = new List<CultivateGiftItem>();
            foreach (var giftMode in giftModelList)
            {
                CultivateGiftItem giftItem = AddCultivateGiftItem(giftMode);
                if (giftItem != null)
                {
                    giftItemList.Add(giftItem);
                }
            }
            SyncDbBatchInsertCultivateGift(giftItemList);
            NotifyCultivateGiftOpen(giftType, giftItemList);
        }

        private bool ChechCultivateGiftIsSameType(List<GiftItemModel> giftModelList, out int giftType)
        {
            giftType = 0;
            foreach (var item in giftModelList)
            {
                if (giftType != 0 && item.Type != giftType)
                {
                    return false;
                }
                giftType = item.Type;
            }
            return true;
        }

        private CultivateGiftItem AddCultivateGiftItem(GiftItemModel giftModel)
        {
            //后续礼包需要先买前置礼包才能开启
            if (!CheckCanOpenNextCultivateGift(giftModel))
            {
                return null;
            }
            BaseGiftItem baseGiftItem;
            CultivateGift gift;
            cultivateGifts.TryGetValue(giftModel.Type, out gift);
            DateTime createTime = ZoneServerApi.now;
            ulong uid = Owner.server.UID.NewIuid(Owner.server.MainId, Owner.server.SubId);
            if (gift == null)
            {
                gift = new CultivateGift(giftModel.Type);
                baseGiftItem = gift.AddGiftItem(giftModel.Id, giftModel.Type, createTime, uid);
                cultivateGifts.Add(baseGiftItem.Type, gift);
            }
            else
            {
                baseGiftItem = gift.AddGiftItem(giftModel.Id, giftModel.Type, createTime, uid);
            }
            CultivateGiftItem giftItem = baseGiftItem as CultivateGiftItem;
            return giftItem;
        }

        private bool CheckCanOpenNextCultivateGift(GiftItemModel giftModel)
        {
            if (giftModel.Type == 2)
            {
                CultivateGift gift;
                if (!cultivateGifts.TryGetValue(1, out gift))
                {
                    return false;
                }
                BaseGiftItem giftItem = gift.LimitTimeItemList.Values.FirstOrDefault();
                if (giftItem == null || giftItem.BuyState == (int)GiftBuyState.NotBuy)
                {
                    return false;
                }
            }
            return true;
        }

        private void NotifyCultivateGiftOpen(int giftType, List<CultivateGiftItem> giftItems)
        {
            if (giftItems.Count == 0)
            {
                return;
            }
            MSG_ZGC_CULTIVATE_GIFT_OPEN notify = new MSG_ZGC_CULTIVATE_GIFT_OPEN();
            notify.GiftType = giftType;
            foreach (var item in giftItems)
            {
                ZGC_CULTIVATE_SUB_GIFT msg = new ZGC_CULTIVATE_SUB_GIFT()
                {
                    //UidHigh = item.Uid.GetHigh(),
                    //UidLow = item.Uid.GetLow(),
                    GiftId = item.Id,
                    BuyState = item.BuyState
                };
                notify.SubList.Add(msg);
                notify.CreateTime = Timestamp.GetUnixTimeStampSeconds(item.CreateTime);
            }
            Owner.Write(notify);
        }

        public void UpdateCultivateGift(GiftItemModel giftModel, MSG_ZGC_BUY_CULTIVATE_GIFT response)
        {
            CultivateGift gift;
            cultivateGifts.TryGetValue(giftModel.Type, out gift);
            if (gift == null)
            {
                Logger.Log.Warn($"player {Owner.Uid} buy cultivate gift {giftModel.Id} failed: gift type {giftModel.Type} not triggered yet");
                response.Result = (int)ErrorCode.Fail;
                return;
            }
            Dictionary<ulong, BaseGiftItem> giftList;
            gift.SameIdGiftList.TryGetValue(giftModel.Id, out giftList);
            if (giftList == null)
            {
                Logger.Log.Warn($"player {Owner.Uid} buy cultivate gift {giftModel.Id} failed: gift not triggered yet");
                response.Result = (int)ErrorCode.Fail;
                return;
            }
            CultivateGiftItem giftItem = giftList.Values.FirstOrDefault() as CultivateGiftItem;
            if (giftItem == null)
            {
                Logger.Log.Warn($"player {Owner.Uid} buy cultivate gift {giftModel.Id} failed: not find gift");
                response.Result = (int)ErrorCode.Fail;
                return;
            }
            if ((ZoneServerApi.now - giftItem.CreateTime).TotalSeconds > giftModel.Duration * 3600)
            {
                Logger.Log.Warn($"player {Owner.Uid} buy cultivate gift uid {giftItem.Uid} id {giftItem.Id} failed: gift over time");
                response.Result = (int)ErrorCode.Fail;
                return;
            }
            if (giftItem.BuyState == (int)GiftBuyState.Bought)
            {
                Logger.Log.Warn($"player {Owner.Uid} buy cultivate gift uid {giftItem.Uid} id {giftItem.Id} failed: already bought");
                response.Result = (int)ErrorCode.Fail;
                return;
            }
            giftItem.BuyState = (int)GiftBuyState.Bought;
            response.BuyState = giftItem.BuyState;
            response.Result = (int)ErrorCode.Success;

            SyncDbUpdateCultivateGift(giftItem, giftModel.Rewards);
        }

        private void SyncDbInsertCultivateGift(CultivateGiftItem giftItem)
        {
            QueryInsertCultivateGift query = new QueryInsertCultivateGift(giftItem.Uid, Owner.Uid, giftItem.Type, giftItem.Id, giftItem.CreateTime);
            Owner.server.GameDBPool.Call(query);
        }

        private void SyncDbBatchInsertCultivateGift(List<CultivateGiftItem> giftItemList)
        {
            if (giftItemList.Count == 0)
            {
                return;
            }
            List<DbGift2Item> dbGiftItems = new List<DbGift2Item>();
            foreach (var item in giftItemList)
            {
                DbGift2Item dbItem = new DbGift2Item()
                {
                    Uid = item.Uid,
                    Type = item.Type,
                    Id = item.Id,
                    //CreateTime = item.CreateTime;
                };
                dbGiftItems.Add(dbItem);
            }
            QueryBatchInsertCultivateGift query = new QueryBatchInsertCultivateGift(Owner.Uid, dbGiftItems);
            Owner.server.GameDBPool.Call(query);
        }

        private void SyncDbUpdateCultivateGift(CultivateGiftItem giftItem, string rewards)
        {
            QueryUpdateCultivateGift query = new QueryUpdateCultivateGift(giftItem.Uid, giftItem.BuyState, rewards);
            Owner.server.GameDBPool.Call(query);
        }

        public MSG_ZGC_CULTIVATE_GIFT_LIST GenerateCultivateGiftListMsg()
        {
            MSG_ZGC_CULTIVATE_GIFT_LIST msg = new MSG_ZGC_CULTIVATE_GIFT_LIST();
            foreach (var gift in cultivateGifts)
            {
                msg.GiftList.Add(GenerateCultivateGiftMsg(gift.Key, gift.Value));
            }
            return msg;
        }

        private ZGC_CULTIVATE_GIFT GenerateCultivateGiftMsg(int type, CultivateGift gift)
        {
            ZGC_CULTIVATE_GIFT msg = new ZGC_CULTIVATE_GIFT();
            msg.GiftType = type;
            foreach (var item in gift.LimitTimeItemList)
            {
                CultivateGiftItem culGiftItem = item.Value as CultivateGiftItem;
                GiftItemModel giftModel = GiftLibrary.GetGiftItemModel(culGiftItem.Id);
                if ((ZoneServerApi.now - culGiftItem.CreateTime).TotalSeconds < giftModel.Duration * 3600)
                {
                    msg.SubList.Add(GenerateCultivateSubGiftMsg(culGiftItem));
                    msg.CreateTime = Timestamp.GetUnixTimeStampSeconds(culGiftItem.CreateTime);
                }
            }
            return msg;
        }

        private ZGC_CULTIVATE_SUB_GIFT GenerateCultivateSubGiftMsg(CultivateGiftItem item)
        {
            ZGC_CULTIVATE_SUB_GIFT msg = new ZGC_CULTIVATE_SUB_GIFT()
            {
                GiftId = item.Id,
                BuyState = item.BuyState
            };
            return msg;
        }

        public MSG_ZMZ_CULTIVATE_GIFT_LIST GenerateCultivateGiftTransformMsg()
        {
            MSG_ZMZ_CULTIVATE_GIFT_LIST msg = new MSG_ZMZ_CULTIVATE_GIFT_LIST();
            foreach (var gift in cultivateGifts)
            {
                msg.GiftList.Add(GenerateCultivateGiftTransformMsg(gift.Key, gift.Value));
            }
            return msg;
        }

        private ZMZ_CULTIVATE_GIFT GenerateCultivateGiftTransformMsg(int giftType, CultivateGift gift)
        {
            ZMZ_CULTIVATE_GIFT msg = new ZMZ_CULTIVATE_GIFT();
            msg.GiftType = giftType;
            foreach (var item in gift.LimitTimeItemList)
            {
                CultivateGiftItem culGiftItem = item.Value as CultivateGiftItem;
                msg.ItemList.Add(GenerateCultivateGiftItemMsg(culGiftItem));
            }
            return msg;
        }

        private ZMZ_CULTIVATE_GIFT_ITEM GenerateCultivateGiftItemMsg(CultivateGiftItem giftItem)
        {
            ZMZ_CULTIVATE_GIFT_ITEM msg = new ZMZ_CULTIVATE_GIFT_ITEM()
            {
                Uid = giftItem.Uid,
                Id = giftItem.Id,
                Type = giftItem.Type,
                BuyState = giftItem.BuyState,
                CreateTime = Timestamp.GetUnixTimeStamp(giftItem.CreateTime)
            };
            return msg;
        }

        public void LoadCultivateGiftTransform(RepeatedField<ZMZ_CULTIVATE_GIFT> giftListMsg)
        {
            foreach (var giftMsg in giftListMsg)
            {
                CultivateGift gift = new CultivateGift(giftMsg.GiftType);
                foreach (var item in giftMsg.ItemList)
                {
                    gift.LoadItem(item);
                }
                cultivateGifts.Add(giftMsg.GiftType, gift);
            }
        }
        #endregion

        #region 小额礼包
        public void BindPettyGiftInfo(DbRegularGiftInfo dbGift)
        {
            pettyGift.BindDbItems(dbGift.ItemList);
        }

        public MSG_ZGC_PETTY_GIFT_LIST GeneratePettyGiftListMsg()
        {
            MSG_ZGC_PETTY_GIFT_LIST msg = new MSG_ZGC_PETTY_GIFT_LIST();
            //首次开启
            if (pettyGift.ItemList.Count == 0)
            {
                PettyGiftModel giftModel = GiftLibrary.GetPettyGiftModel(GiftLibrary.FirstPettyGift);
                RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(giftModel.Id);
                BaseGiftItem baseItem = pettyGift.AddGiftItem(giftModel.Id, recharge.SubType, ZoneServerApi.now);
                PettyGiftItem giftItem = baseItem as PettyGiftItem;
                SyncDbInsertPettyGiftInfo(giftItem);
                msg.GiftList.Add(GeneratePettyGiftItemMsg(giftItem, giftModel));
                return msg;
            }
            //1元礼包筛选后通知前端
            List<BaseGiftItem> type1List = pettyGift.ItemList.Values.Where(x => x.Type == (int)PettyGiftType.OneRmb).ToList();
            Dictionary<int, PettyGiftItem> notifyList = new Dictionary<int, PettyGiftItem>();
            foreach (var item in type1List)
            {
                PettyGiftItem giftItem = item as PettyGiftItem;
                PettyGiftModel giftModel = GiftLibrary.GetPettyGiftModel(giftItem.Id);
                //if ( giftItem.BuyState == (int)GiftBuyState.Bought)
                //{
                //    continue;
                //}
                notifyList.Add(giftModel.Type, giftItem);
            }
            PettyGiftItem notifyItem = notifyList.Values.Where(x => x.BuyState == (int)GiftBuyState.NotBuy && x.CurFlag == 1).FirstOrDefault();
            if (notifyItem != null)
            {
                if (pettyGift.RefreshGiftItem != null && pettyGift.RefreshGiftItem.BuyState == 0 && pettyGift.RefreshGiftItem.Id != notifyItem.Id)
                {
                    msg.GiftList.Add(GeneratePettyGiftItemMsg(pettyGift.RefreshGiftItem));
                }
                else
                {
                    msg.GiftList.Add(GeneratePettyGiftItemMsg(notifyItem));
                }
            }
            //六元礼包
            List<BaseGiftItem> type2List = pettyGift.ItemList.Values.Where(x => x.Type == (int)PettyGiftType.SixRmb).ToList();
            foreach (var item in type2List)
            {
                PettyGiftItem giftItem = item as PettyGiftItem;
                if (giftItem.CurFlag != 2)
                {
                    PettyGiftModel giftModel = GiftLibrary.GetPettyGiftModel(giftItem.Id);
                    msg.GiftList.Add(GeneratePettyGiftItemMsg(giftItem, giftModel));

                }
            }
            return msg;
        }

        private ZGC_PETTY_GIFT GeneratePettyGiftItemMsg(PettyGiftItem giftItem, PettyGiftModel giftModel = null)
        {
            ZGC_PETTY_GIFT msg = new ZGC_PETTY_GIFT();
            msg.GiftId = giftItem.Id;
            msg.BuyState = giftItem.BuyState;

            if (giftItem.Type == (int)PettyGiftType.OneRmb)
            {
                msg.FreeTime = 0;
            }
            else
            {
                msg.FreeTime = Timestamp.GetUnixTimeStampSeconds(giftItem.CreateTime) + giftModel.Duration * 3600;
            }
            return msg;
        }

        //1元礼包刷新
        public PettyGiftItem RefreshPettyGift(bool reachTime)
        {
            int refreshType = 0;
            PettyGiftItem curGiftItem = null;
            List<BaseGiftItem> oneRmbiftItems = pettyGift.ItemList.Values.Where(x => x.Type == 1).ToList();
            foreach (var item in oneRmbiftItems)
            {
                PettyGiftItem giftItem = item as PettyGiftItem;
                if (giftItem.CurFlag == 1)
                {
                    curGiftItem = giftItem;
                }
            }

            if (curGiftItem != null)
            {
                PettyGiftModel giftModel = GiftLibrary.GetPettyGiftModel(curGiftItem.Id);
                PettyGiftModel nextGiftModel = GiftLibrary.GetPettyGiftModel(giftModel.NextOneRmbGift);
                refreshType = nextGiftModel.Type;
            }

            PettyGiftItem refreshItem = RefreshOneRmbPettyGift(refreshType, reachTime);
            return refreshItem;
        }

        public PettyGiftItem RefreshOneRmbPettyGift(int giftType, bool reachTime)
        {
            PettyGiftItem refreshItem = null;
            PettyGiftItem giftItem = null;
            Dictionary<int, bool> refreshedItems = new Dictionary<int, bool>();
            List<BaseGiftItem> oneRmbList = pettyGift.ItemList.Values.Where(x => x.Type == (int)PettyGiftType.OneRmb).ToList();
            foreach (var item in oneRmbList)
            {
                giftItem = item as PettyGiftItem;
                PettyGiftModel giftModel = GiftLibrary.GetPettyGiftModel(giftItem.Id);
                bool refreshed = CheckPettyGiftRefreshed(giftItem, reachTime);
                if (!refreshed)
                {
                    bool resetRewards = false;
                    if (giftItem.CurFlag == 1)
                    {
                        pettyGift.UpdateCurFlag(giftItem, 0);
                    }
                    if (giftModel.Type == giftType)
                    {
                        pettyGift.ResetPettyGiftBuyState(giftItem);
                        pettyGift.UpdateCurFlag(giftItem, 1);
                        resetRewards = true;
                    }
                    pettyGift.UpdateRefreshTime(giftItem, ZoneServerApi.now);
                    SyncDbUpdatePettyGiftRefreshTime(giftItem, resetRewards);
                    refreshedItems.Add(giftItem.Id, true);
                }
                if (giftModel.Type == giftType)
                {
                    refreshItem = giftItem;
                }
            }
            List<BaseGiftItem> sixRmbList = pettyGift.ItemList.Values.Where(x => x.Type == (int)PettyGiftType.SixRmb).ToList();
            foreach (var item in sixRmbList)
            {
                giftItem = item as PettyGiftItem;
                if (giftItem.BuyState == (int)GiftBuyState.Received)
                {
                    pettyGift.UpdateCurFlag(giftItem, 2);
                    SyncDbUpdatePettyGiftRefreshTime(giftItem, false);
                }
            }

            if (refreshItem == null)
            {
                //没买过情况下才添加
                refreshItem = RefreshNewOneRmbPettyGift(giftType, reachTime);
            }
            else
            {
                bool refrehFlag;
                if (!refreshedItems.TryGetValue(refreshItem.Id, out refrehFlag))
                {
                    refreshItem = null;
                }
            }
            return refreshItem;
        }

        private bool CheckPettyGiftRefreshed(PettyGiftItem giftItem, bool reachTime)
        {
            if ((ZoneServerApi.now - giftItem.RefreshTime).TotalDays < 1 && !reachTime)
            {
                return true;
            }
            return false;
        }

        private PettyGiftItem RefreshNewOneRmbPettyGift(int giftType, bool reachTime)
        {
            int preGiftType = 0;
            if (giftType == 0)
            {
                giftType = 1;
            }
            if (giftType - 1 > 0)
            {
                preGiftType = giftType - 1;
            }
            else
            {
                preGiftType = RechargeLibrary.PettyGiftTypeNum;
            }
            PettyGiftModel preGiftModel = GiftLibrary.GetPettyGiftModelByType(1, preGiftType);
            BaseGiftItem preBaseItem;
            pettyGift.ItemList.TryGetValue(preGiftModel.Id, out preBaseItem);
            PettyGiftItem preGiftItem = preBaseItem as PettyGiftItem;
            if (preGiftItem == null || CheckPettyGiftRefreshed(preGiftItem, reachTime))
            {
                return null;
            }
            PettyGiftModel giftModel = GiftLibrary.GetPettyGiftModelByType(1, giftType);
            BaseGiftItem baseItem = pettyGift.AddGiftItem(giftModel.Id, giftModel.MainType, ZoneServerApi.now);
            PettyGiftItem giftItem = baseItem as PettyGiftItem;
            SyncDbInsertPettyGiftInfo(giftItem);
            return giftItem;
        }

        public void UpdatePettyMoneyGift(RechargeItemModel recharge)
        {
            BaseGiftItem giftItem;
            if (!pettyGift.ItemList.TryGetValue(recharge.Id, out giftItem))
            {
                return;
            }

            PettyGiftModel giftModel = GiftLibrary.GetPettyGiftModel(giftItem.Id);

            if (recharge.SubType == 1)
            {
                giftItem = pettyGift.UpdatePettyMoneyGift(recharge.Id);
                SyncDbUpdatePettyGiftBuyInfo(giftItem, recharge);

                //更新六元礼包   
                PettyGiftItem nextGiftItem;
                PettyGiftModel nextGiftModel = GiftLibrary.GetPettyGiftModel(giftModel.NextGiftId);
                bool isAdd = pettyGift.UpdateNextPettyMoneyGift(nextGiftModel.Id, nextGiftModel.MainType, out nextGiftItem);
                if (isAdd)
                {
                    SyncDbInsertPettyGiftInfo(nextGiftItem);
                }
                else
                {
                    SyncDbUpdateSixRmbPettyGiftInfo(nextGiftItem);
                }
                SendPetteyGiftBuyInfo(giftItem, nextGiftModel.Duration, nextGiftItem);
            }
            else
            {
                giftItem = pettyGift.UpdatePettyMoneyGift(recharge.Id);
                SyncDbUpdatePettyGiftBuyInfo(giftItem, recharge);
                SendPetteyGiftBuyInfo(giftItem, giftModel.Duration);
            }
        }

        private void SendPetteyGiftBuyInfo(BaseGiftItem baseItem, int duration, PettyGiftItem nextGiftItem = null)
        {
            if (baseItem == null)
            {
                return;
            }
            PettyGiftItem giftItem = baseItem as PettyGiftItem;
            MSG_ZGC_BUY_PETTY_GIFT response = new MSG_ZGC_BUY_PETTY_GIFT();
            response.GiftId = giftItem.Id;
            response.BuyState = giftItem.BuyState;
            if (nextGiftItem != null)
            {
                response.FreeTime = Timestamp.GetUnixTimeStampSeconds(nextGiftItem.CreateTime) + duration * 3600;
            }
            else
            {
                response.FreeTime = Timestamp.GetUnixTimeStampSeconds(giftItem.CreateTime) + duration * 3600;
            }

            //发奖
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(giftItem.Id);
            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(recharge.Reward);
            manager.GenerateRewardItemInfo(response.Rewards);

            response.Result = (int)ErrorCode.Success;
            Owner.Write(response);
        }

        //免费领取小额礼包
        public void ReceiveFreePettyGift(int giftId)
        {
            MSG_ZGC_FREE_PETTY_GIFT response = new MSG_ZGC_FREE_PETTY_GIFT();
            response.GiftId = giftId;

            RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(giftId);
            if (recharge == null)
            {
                //没有找到产品ID
                Log.Warn($"player {Owner.Uid} ReceiveFreePettyGift giftId {giftId} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Owner.Write(response);
                return;
            }

            BaseGiftItem baseItem;
            if (!pettyGift.ItemList.TryGetValue(giftId, out baseItem))
            {
                Log.Warn($"player {Owner.Uid} ReceiveFreePettyGift giftId {giftId} failed: not exists in giftList");
                response.Result = (int)ErrorCode.Fail;
                Owner.Write(response);
                return;
            }
            PettyGiftModel giftModel = GiftLibrary.GetPettyGiftModel(giftId);
            PettyGiftItem giftItem = baseItem as PettyGiftItem;
            if (giftItem == null || giftItem.Type != (int)PettyGiftType.SixRmb)
            {
                Log.Warn($"player {Owner.Uid} ReceiveFreePettyGift giftId {giftId} failed: not right gift type");
                response.Result = (int)ErrorCode.Fail;
                Owner.Write(response);
                return;
            }
            if (giftItem.BuyState != (int)GiftBuyState.NotBuy)
            {
                Log.Warn($"player {Owner.Uid} ReceiveFreePettyGift giftId {giftId} failed: already received");
                response.Result = (int)ErrorCode.AlreadyReceived;
                Owner.Write(response);
                return;
            }
            bool isLeft = false;
            if ((ZoneServerApi.now - giftItem.RefreshTime).TotalSeconds > giftModel.Duration * 3600 && (ZoneServerApi.now - giftItem.CreateTime).TotalSeconds < giftModel.Duration * 3600)
            {
                isLeft = true;
            }
            if (isLeft)
            {
                giftItem.RefreshTime = ZoneServerApi.now;
                SyncDbUpdatePettyGiftRefreshTime(giftItem, false);
                response.BuyState = (int)GiftBuyState.Received;
            }
            else
            {
                if ((ZoneServerApi.now - giftItem.CreateTime).TotalSeconds < giftModel.Duration * 3600)
                {
                    Log.Warn($"player {Owner.Uid} ReceiveFreePettyGift giftId {giftId} failed: can not receive for free yet");
                    response.Result = (int)ErrorCode.NotReachTime;
                    Owner.Write(response);
                    return;
                }
                giftItem.BuyState = (int)GiftBuyState.Received;
                response.BuyState = giftItem.BuyState;
                SyncDbUpdatePettyGiftBuyInfo(giftItem, recharge);
            }

            //发奖
            response.Result = (int)ErrorCode.Success;
            RewardManager manager = Owner.GetSimpleReward(recharge.Reward, ObtainWay.ReceiveFreePettyGift);
            manager.GenerateRewardItemInfo(response.Rewards);
            Owner.Write(response);
        }

        public void SetRefreshPettyGift(PettyGiftItem giftItem)
        {
            pettyGift.SetRefreshPettyGift(giftItem);
        }

        private bool CheckPettyGiftHaveBuyCount(RechargeItemModel model)
        {
            BaseGiftItem giftItem;
            if (!pettyGift.ItemList.TryGetValue(model.Id, out giftItem))
            {
                return false;
            }
            if (giftItem.Type == (int)PettyGiftType.OneRmb && giftItem.BuyState == (int)GiftBuyState.Bought)
            {
                return false;
            }
            else if (giftItem.Type == (int)PettyGiftType.SixRmb && giftItem.BuyState == (int)GiftBuyState.Received)
            {
                return false;
            }
            return true;
        }

        public void SyncDbInsertPettyGiftInfo(PettyGiftItem giftItem)
        {
            QueryInsertPettyGift query = new QueryInsertPettyGift(Owner.Uid, giftItem.Id, giftItem.Type, giftItem.CreateTime, giftItem.RefreshTime, giftItem.CurFlag);
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncDbUpdateSixRmbPettyGiftInfo(PettyGiftItem giftItem)
        {
            QueryUpdateSixRmbPettyGiftInfo query = new QueryUpdateSixRmbPettyGiftInfo(Owner.Uid, giftItem.Id, giftItem.BuyState, giftItem.CreateTime, "", giftItem.CurFlag);
            Owner.server.GameDBPool.Call(query);
        }

        private void SyncDbUpdatePettyGiftBuyInfo(BaseGiftItem giftItem, RechargeItemModel recharge = null)
        {
            string rewards = "";
            if (recharge != null)
            {
                rewards = recharge.Reward;
            }
            QueryUpdatePettyGiftBuyInfo query = new QueryUpdatePettyGiftBuyInfo(Owner.Uid, giftItem.Id, giftItem.BuyState, rewards);
            Owner.server.GameDBPool.Call(query);
        }

        private void SyncDbUpdatePettyGiftRefreshTime(PettyGiftItem giftItem, bool resetRewards)
        {
            if (resetRewards)
            {
                QueryUpdateRefreshNewPettyGift query = new QueryUpdateRefreshNewPettyGift(Owner.Uid, giftItem.Id, giftItem.BuyState, giftItem.RefreshTime, "", giftItem.CurFlag);
                Owner.server.GameDBPool.Call(query);
            }
            else
            {
                QueryUpdatePettyGiftRefershTime query = new QueryUpdatePettyGiftRefershTime(Owner.Uid, giftItem.Id, giftItem.BuyState, giftItem.RefreshTime, giftItem.CurFlag);
                Owner.server.GameDBPool.Call(query);
            }
        }

        public MSG_ZMZ_PETTY_GIFT GeneratePettyGiftTransformMsg()
        {
            MSG_ZMZ_PETTY_GIFT msg = new MSG_ZMZ_PETTY_GIFT();
            foreach (var item in pettyGift.ItemList)
            {
                PettyGiftItem giftItem = item.Value as PettyGiftItem;
                msg.ItemList.Add(GeneratePettyGiftItemTransformMsg(giftItem));
            }
            return msg;
        }

        private ZMZ_PETTY_GIFT_ITEM GeneratePettyGiftItemTransformMsg(PettyGiftItem giftItem)
        {
            ZMZ_PETTY_GIFT_ITEM msg = new ZMZ_PETTY_GIFT_ITEM()
            {
                Id = giftItem.Id,
                Type = giftItem.Type,
                BuyState = giftItem.BuyState,
                CreateTime = Timestamp.GetUnixTimeStamp(giftItem.CreateTime),
                RefreshTime = Timestamp.GetUnixTimeStamp(giftItem.RefreshTime),
                CurFlag = giftItem.CurFlag
            };
            return msg;
        }

        public void LoadPettyGiftTransform(RepeatedField<ZMZ_PETTY_GIFT_ITEM> giftItemsMsg)
        {
            pettyGift = new PettyGift((int)RechargeGiftType.PettyMoney);
            foreach (var item in giftItemsMsg)
            {
                pettyGift.LoadItem(item);
            }
        }
        #endregion

        #region 每日充值
        public void BindDailyRechargeInfo(Dictionary<int, DailyRechargeInfo> infoList)
        {
            foreach (var info in infoList)
            {
                dailyRechargeList.Add(info.Key, info.Value);
            }
        }

        private bool CheckDailyRechargeHaveBuyCount(RechargeItemModel model)
        {
            if (!CheckIsRightDay(model.GiftType, model.SubType, model.Day))
            {
                return false;
            }
            DailyRechargeInfo info;
            dailyRechargeList.TryGetValue(model.SubType, out info);
            if (info != null)
            {
                string[] ids = StringSplit.GetArray("|", info.Ids);
                if (ids.Contains(model.Id.ToString()))
                {
                    return false;
                }
                //天数顺序检查
                List<int> idList = new List<int>();
                foreach (string item in ids)
                {
                    idList.Add(item.ToInt());
                }
                if (idList.Count > 0)
                {
                    int maxId = idList.Max();
                    RechargeItemModel lastItem = RechargeLibrary.GetRechargeItem(maxId);
                    if (model.Day != lastItem.Day + 1)
                    {
                        return false;
                    }
                }
                else if (model.Day != 1)
                {
                    return false;
                }
                return true;
            }
            else
            {
                if (model.Day == 1)
                {
                    return true;
                }
                return false;
            }
        }

        private bool CheckIsRightDay(RechargeGiftType giftType, int period, int day = 0)
        {
            RechargeGiftModel periodGift;
            if (!RechargeLibrary.CheckInRechargeActivityTime(giftType, ZoneServerApi.now, out periodGift))
            {
                return false;
            }
            if (periodGift.StartWeekTime != DateTime.MinValue && periodGift.EndWeekTime != DateTime.MinValue && !RechargeLibrary.IgnoreNewServerActivity)
            {
                if (day != 0 && (ZoneServerApi.now.Date - periodGift.StartWeekTime.Date).Days + 1 < day)
                {
                    return false;
                }
            }
            else
            {
                if (day != 0 && (ZoneServerApi.now.Date - periodGift.StartTime.Date).Days + 1 < day)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 购买每日充值
        /// </summary>      
        public void UpdateDailyRecharge(RechargeItemModel model)
        {
            DailyRechargeInfo info;
            if (dailyRechargeList.TryGetValue(model.SubType, out info))
            {
                info.Ids += "|" + model.Id;
                SyncDbUpdateDailyRechargeBuyInfo(info);
            }
            else
            {
                info = new DailyRechargeInfo()
                {
                    Period = model.SubType,
                    Ids = model.Id.ToString()
                };
                dailyRechargeList.Add(info.Period, info);
                SyncDbInsertDailyRechargeInfo(info);
            }
            //通知前端
            SendDailyRechargeMsg(info);
        }

        private void SyncDbInsertDailyRechargeInfo(DailyRechargeInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryInsertDailyRechargeInfo(Owner.Uid, info.Period, info.Ids));
        }

        private void SyncDbUpdateDailyRechargeBuyInfo(DailyRechargeInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateDailyRechargeBuyInfo(Owner.Uid, info.Period, info.Ids));
        }

        public MSG_ZGC_DAILY_RECHARGE_INFO GenerateDailyRechargeMsg()
        {
            MSG_ZGC_DAILY_RECHARGE_INFO msg = new MSG_ZGC_DAILY_RECHARGE_INFO();
            RechargeGiftModel activityModel;
            if (RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.DailyRecharge, ZoneServerApi.now, out activityModel))
            {
                DailyRechargeInfo info;
                dailyRechargeList.TryGetValue(activityModel.SubType, out info);
                if (info != null)
                {
                    string[] ids = StringSplit.GetArray("|", info.Ids);
                    ids.ForEach(x => msg.IdList.Add(x.ToInt()));
                    string[] states = StringSplit.GetArray("|", info.GetStates);
                    states.ForEach(x => msg.GetStateList.Add(x.ToInt()));
                }
            }
            return msg;
        }

        private void SendDailyRechargeMsg(DailyRechargeInfo info)
        {
            MSG_ZGC_DAILY_RECHARGE_INFO msg = new MSG_ZGC_DAILY_RECHARGE_INFO();

            string[] ids = StringSplit.GetArray("|", info.Ids);
            ids.ForEach(x => msg.IdList.Add(x.ToInt()));
            string[] states = StringSplit.GetArray("|", info.GetStates);
            states.ForEach(x => msg.GetStateList.Add(x.ToInt()));

            Owner.Write(msg);
        }

        public DailyRechargeInfo GetDailyRechargeByPeriod(int period)
        {
            DailyRechargeInfo info;
            dailyRechargeList.TryGetValue(period, out info);
            return info;
        }

        public void UpdateDailyRechargeGetState(DailyRechargeInfo info, int rewardId)
        {
            info.GetStates += rewardId + "|";
        }

        public RepeatedField<ZMZ_DAILY_RECHARGE> GenerateDailyRechargeTransformMsg()
        {
            RepeatedField<ZMZ_DAILY_RECHARGE> infoList = new RepeatedField<ZMZ_DAILY_RECHARGE>();
            foreach (var item in dailyRechargeList)
            {
                ZMZ_DAILY_RECHARGE info = new ZMZ_DAILY_RECHARGE();
                info.Period = item.Value.Period;
                info.Ids = item.Value.Ids;
                info.GetStates = item.Value.GetStates;
                infoList.Add(info);
            }
            return infoList;
        }

        public void LoadDailyRechargeTransform(RepeatedField<ZMZ_DAILY_RECHARGE> infoList)
        {
            foreach (var info in infoList)
            {
                DailyRechargeInfo rechargeInfo = new DailyRechargeInfo()
                {
                    Period = info.Period,
                    Ids = info.Ids,
                    GetStates = info.GetStates
                };
                dailyRechargeList.Add(rechargeInfo.Period, rechargeInfo);
            }
        }
        #endregion

        #region 角色七日奖励
        public void BindHeroDaysRewardsInfo(Dictionary<int, HeroDaysRewardsInfo> infoList)
        {
            foreach (var info in infoList)
            {
                heroDaysRewardsList.Add(info.Key, info.Value);
            }
        }

        public void AddHeroDaysRewardsInfo(int period)
        {
            HeroDaysRewardsInfo info;
            if (!heroDaysRewardsList.TryGetValue(period, out info))
            {
                info = new HeroDaysRewardsInfo()
                {
                    Period = period,
                    HeroGetTime = ZoneServerApi.now
                };
                heroDaysRewardsList.Add(period, info);
                SyncDbInsertHeroDaysRewardsInfo(info);
                SendHeroDaysRewardsMsg(period);
            }
        }

        private void SyncDbInsertHeroDaysRewardsInfo(HeroDaysRewardsInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryInsertHeroDaysRewardsInfo(Owner.Uid, info.Period, info.HeroGetTime));
        }

        public MSG_ZGC_HERO_DAYS_REWARDS_INFO GenerateHeroDaysRewardsMsg()
        {
            MSG_ZGC_HERO_DAYS_REWARDS_INFO msg = new MSG_ZGC_HERO_DAYS_REWARDS_INFO();
            RechargeGiftModel activityModel;
            if (RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.HeroDaysRewards, ZoneServerApi.now, out activityModel))
            {
                int period = activityModel.SubType;
                HeroDaysRewardsInfo info;
                heroDaysRewardsList.TryGetValue(period, out info);
                if (info == null)
                {
                    int curPeriodHero = GiftLibrary.GetHeroIdByPeriod(period);
                    if (Owner.HeroMng.GetHeroInfo(curPeriodHero) != null)
                    {
                        info = new HeroDaysRewardsInfo()
                        {
                            Period = period,
                            HeroGetTime = ZoneServerApi.now
                        };
                        heroDaysRewardsList.Add(period, info);
                        SyncDbInsertHeroDaysRewardsInfo(info);
                    }
                }
                if (info != null)
                {
                    string[] rewards = StringSplit.GetArray("|", info.Rewards);
                    rewards.ForEach(x => msg.RewardList.Add(x.ToInt()));
                    msg.HeroGetTime = Timestamp.GetUnixTimeStampSeconds(info.HeroGetTime);
                }
            }
            return msg;
        }

        public HeroDaysRewardsInfo GetHeroDaysRewardsInfoByPeriod(int period)
        {
            HeroDaysRewardsInfo info;
            heroDaysRewardsList.TryGetValue(period, out info);
            return info;
        }

        public void UpdateHeroDaysRewardsGotRewardsInfo(HeroDaysRewardsInfo info, int rewardId)
        {
            info.Rewards += rewardId + "|";
        }

        private void SendHeroDaysRewardsMsg(int period)
        {
            MSG_ZGC_HERO_DAYS_REWARDS_INFO msg = new MSG_ZGC_HERO_DAYS_REWARDS_INFO();
            HeroDaysRewardsInfo info;
            heroDaysRewardsList.TryGetValue(period, out info);
            if (info == null)
            {
                return;
            }
            string[] rewards = StringSplit.GetArray("|", info.Rewards);
            rewards.ForEach(x => msg.RewardList.Add(x.ToInt()));
            msg.HeroGetTime = Timestamp.GetUnixTimeStampSeconds(info.HeroGetTime);
            Owner.Write(msg);
        }

        public void NotifyClientNewHeroDaysRewardStart()
        {
            //通知前端空流即可
            MSG_ZGC_HERO_DAYS_REWARDS_INFO msg = new MSG_ZGC_HERO_DAYS_REWARDS_INFO();
            Owner.Write(msg);
        }

        public RepeatedField<ZMZ_HERO_DAYS_REWARDS> GenerateHeroDaysRewardsTransformMsg()
        {
            RepeatedField<ZMZ_HERO_DAYS_REWARDS> infoList = new RepeatedField<ZMZ_HERO_DAYS_REWARDS>();
            foreach (var item in heroDaysRewardsList)
            {
                ZMZ_HERO_DAYS_REWARDS info = new ZMZ_HERO_DAYS_REWARDS();
                info.Period = item.Value.Period;
                info.Rewards = item.Value.Rewards;
                info.HeroGetTime = Timestamp.GetUnixTimeStampSeconds(item.Value.HeroGetTime);
                infoList.Add(info);
            }
            return infoList;
        }

        public void LoadHeroDaysRewardsTransform(RepeatedField<ZMZ_HERO_DAYS_REWARDS> infoList)
        {
            foreach (var info in infoList)
            {
                HeroDaysRewardsInfo daysRewardsInfo = new HeroDaysRewardsInfo()
                {
                    Period = info.Period,
                    Rewards = info.Rewards,
                    HeroGetTime = Timestamp.TimeStampToDateTime(info.HeroGetTime)
                };
                heroDaysRewardsList.Add(daysRewardsInfo.Period, daysRewardsInfo);
            }
        }
        #endregion

        #region 新服促销
        public void BindNewServerPromotionInfo(Dictionary<int, NewServerPromotionInfo> infoList)
        {
            foreach (var info in infoList)
            {
                newServerPromotionList.Add(info.Key, info.Value);
            }
        }

        private bool CheckNewServerPromotionHaveBuyCount(RechargeItemModel model)
        {
            if (!CheckIsRightDayByOpenTimeType(model.GiftType, model.SubType, RechargeOpenTimeType.OpenServerDay, model.Day))
            {
                return false;
            }
            NewServerPromotionInfo info;
            newServerPromotionList.TryGetValue(model.SubType, out info);
            if (info != null)
            {
                string[] ids = StringSplit.GetArray("|", info.Ids);
                if (ids.Contains(model.Id.ToString()))
                {
                    return false;
                }
                //天数顺序检查
                List<int> idList = new List<int>();
                foreach (string item in ids)
                {
                    idList.Add(item.ToInt());
                }
                if (idList.Count > 0)
                {
                    int maxId = idList.Max();
                    RechargeItemModel lastItem = RechargeLibrary.GetRechargeItem(maxId);
                    if (model.Day != lastItem.Day + 1)
                    {
                        return false;
                    }
                }
                else if (model.Day != 1)
                {
                    return false;
                }
                return true;
            }
            else
            {
                if (model.Day == 1)
                {
                    return true;
                }
                return false;
            }
        }

        private bool CheckIsRightDayByOpenTimeType(RechargeGiftType giftType, int period, RechargeOpenTimeType openTimeType, int day = 0)
        {
            Dictionary<int, RechargeGiftModel> rechargeGiftDic = RechargeLibrary.GetRechargeGiftModelByGiftType(giftType);
            RechargeGiftModel periodGift;
            rechargeGiftDic.TryGetValue(period, out periodGift);
            if (periodGift == null)
            {
                return false;
            }
            switch (openTimeType)
            {
                case RechargeOpenTimeType.NormalTime:
                    if (periodGift.StartTime != DateTime.MinValue && ZoneServerApi.now < periodGift.StartTime)
                    {
                        return false;
                    }
                    if (periodGift.EndTime != DateTime.MinValue && ZoneServerApi.now > periodGift.EndTime)
                    {
                        return false;
                    }
                    if (day != 0 && (ZoneServerApi.now.Date - periodGift.StartTime.Date).Days + 1 < day)
                    {
                        return false;
                    }
                    break;
                case RechargeOpenTimeType.OpenServerDay:
                    int serverDay = (int)(Owner.server.Now().Date - Owner.server.OpenServerDate).Days;
                    if (serverDay < periodGift.ServerOpenDayStart)
                    {
                        return false;
                    }
                    if (periodGift.ServerOpenDayEnd != 0 && serverDay >= periodGift.ServerOpenDayEnd)
                    {
                        return false;
                    }
                    if (day != 0 && serverDay + 1 < day)
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// 购买新服大促销
        /// </summary>      
        public void UpdateNewServerPromotion(RechargeItemModel model)
        {
            NewServerPromotionInfo info;
            if (newServerPromotionList.TryGetValue(model.SubType, out info))
            {
                info.Ids += "|" + model.Id;
                SyncDbUpdateNewServerPromotionBuyInfo(info);
            }
            else
            {
                info = new NewServerPromotionInfo()
                {
                    Period = model.SubType,
                    Ids = model.Id.ToString()
                };
                newServerPromotionList.Add(info.Period, info);
                SyncDbInsertNewServerPromotionInfo(info);
            }
            //通知前端
            SendNewServerPromotionMsg(info);
        }

        private void SyncDbInsertNewServerPromotionInfo(NewServerPromotionInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryInsertNewServerPromotionInfo(Owner.Uid, info.Period, info.Ids));
        }

        private void SyncDbUpdateNewServerPromotionBuyInfo(NewServerPromotionInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateNewServerPromotionBuyInfo(Owner.Uid, info.Period, info.Ids));
        }

        private void SendNewServerPromotionMsg(NewServerPromotionInfo info)
        {
            MSG_ZGC_NEWSERVER_PROMOTION_INFO msg = new MSG_ZGC_NEWSERVER_PROMOTION_INFO();

            string[] ids = StringSplit.GetArray("|", info.Ids);
            ids.ForEach(x => msg.IdList.Add(x.ToInt()));
            string[] states = StringSplit.GetArray("|", info.GetStates);
            states.ForEach(x => msg.GetStateList.Add(x.ToInt()));

            Owner.Write(msg);
        }

        public MSG_ZGC_NEWSERVER_PROMOTION_INFO GenerateNewServerPromotionMsg()
        {
            MSG_ZGC_NEWSERVER_PROMOTION_INFO msg = new MSG_ZGC_NEWSERVER_PROMOTION_INFO();
            Dictionary<int, RechargeGiftModel> newServerPromotionDic = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.NewServerPromotion);
            foreach (var item in newServerPromotionList)
            {
                //发送当前期数的
                RechargeGiftModel newServerPromotion;
                newServerPromotionDic.TryGetValue(item.Value.Period, out newServerPromotion);
                if (newServerPromotion == null)
                {
                    continue;
                }
                int serverDay = (int)(Owner.server.Now().Date - Owner.server.OpenServerDate).Days;
                if (serverDay >= newServerPromotion.ServerOpenDayStart && serverDay < newServerPromotion.ServerOpenDayEnd)
                {
                    string[] ids = StringSplit.GetArray("|", item.Value.Ids);
                    ids.ForEach(x => msg.IdList.Add(x.ToInt()));
                    string[] states = StringSplit.GetArray("|", item.Value.GetStates);
                    states.ForEach(x => msg.GetStateList.Add(x.ToInt()));
                    break;
                }
            }
            return msg;
        }

        public NewServerPromotionInfo GetNewServerPromotionByPeriod(int period)
        {
            NewServerPromotionInfo info;
            newServerPromotionList.TryGetValue(period, out info);
            return info;
        }

        public void UpdateNewServerPromotionGetState(NewServerPromotionInfo info, int rewardId)
        {
            info.GetStates += rewardId + "|";
        }

        public RepeatedField<ZMZ_DAILY_RECHARGE> GenerateNewServerPromotionTransformMsg()
        {
            RepeatedField<ZMZ_DAILY_RECHARGE> infoList = new RepeatedField<ZMZ_DAILY_RECHARGE>();
            foreach (var item in newServerPromotionList)
            {
                ZMZ_DAILY_RECHARGE info = new ZMZ_DAILY_RECHARGE();
                info.Period = item.Value.Period;
                info.Ids = item.Value.Ids;
                info.GetStates = item.Value.GetStates;
                infoList.Add(info);
            }
            return infoList;
        }

        public void LoadNewServerPromotionTransform(RepeatedField<ZMZ_DAILY_RECHARGE> infoList)
        {
            foreach (var info in infoList)
            {
                NewServerPromotionInfo promotionInfo = new NewServerPromotionInfo()
                {
                    Period = info.Period,
                    Ids = info.Ids,
                    GetStates = info.GetStates
                };
                newServerPromotionList.Add(promotionInfo.Period, promotionInfo);
            }
        }
        #endregion

        #region 幸运翻翻乐
        public void BindLuckyFlipCardInfo(Dictionary<int, LuckyFlipCardInfo> list)
        {
            foreach (var item in list)
            {
                luckyFlipCardList.Add(item.Key, item.Value);
            }
        }

        public void SendLuckyFlipCardMsg()
        {
            RechargeGiftModel activityModel;
            if (RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.LuckyFlipCard, ZoneServerApi.now, out activityModel))
            {
                LuckFCPeriod = activityModel.SubType;
                SendLuckFlipCardInfo();
            }
        }

        public void SendLuckFlipCardInfo()
        {
            MSG_ZGC_LUCKY_FLIP_CARD_INFO msg = new MSG_ZGC_LUCKY_FLIP_CARD_INFO();
            msg.Period = LuckFCPeriod;

            LuckyFlipCardInfo info;
            luckyFlipCardList.TryGetValue(LuckFCPeriod, out info);
            if (info != null)
            {
                int buyCount = info.RechargeIdList.Count;
                if (info.RandRewardList.Count == buyCount)
                {
                    msg.OpenState = 0;
                }
                else
                {
                    msg.OpenState = 1;
                }
                if (buyCount > 0)
                {
                    if (msg.OpenState > 0)
                    {
                        msg.RechargeId = info.RechargeIdList.LastOrDefault();
                    }
                    else
                    {
                        msg.RechargeId = GiftLibrary.GetLuckyFlipCardNextRechargeId(LuckFCPeriod, info.RechargeIdList.LastOrDefault());
                    }
                }
                else
                {
                    msg.RechargeId = GiftLibrary.GetFirstLuckFlipRechargeId(LuckFCPeriod);
                }
                msg.FlipCount = GiftLibrary.GetLuckFlipCardRewardMaxSubType(info.Period) * info.Round + info.RandRewardList.Count;
                msg.CumulateRewardList.AddRange(info.CumulateRewardList);
            }
            else
            {
                msg.OpenState = 0;
                msg.RechargeId = GiftLibrary.GetFirstLuckFlipRechargeId(LuckFCPeriod);
            }
            Owner.Write(msg);
        }

        public bool CheckLuckyFlipCardHaveBuyCount(RechargeItemModel model)
        {
            bool inTime = false;
            RechargeGiftModel activityModel;
            if (RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.LuckyFlipCard, ZoneServerApi.now, out activityModel))
            {
                LuckFCPeriod = activityModel.SubType;
                if (model.SubType == LuckFCPeriod)
                {
                    inTime = true;
                }
            }
            if (!inTime)
            {
                return false;
            }
            LuckyFlipCardInfo info;
            if (!luckyFlipCardList.TryGetValue(LuckFCPeriod, out info))
            {
                return true;
            }
            if (info.RechargeIdList.Count > 0)
            {
                int lastRechargeId = info.RechargeIdList.LastOrDefault();
                int curRechrageId = GiftLibrary.GetLuckyFlipCardNextRechargeId(LuckFCPeriod, lastRechargeId);
                if (curRechrageId != model.Id)
                {
                    return false;
                }
            }
            else if (model.Id != GiftLibrary.GetFirstLuckFlipRechargeId(LuckFCPeriod))
            {
                return false;
            }
            return true;
        }

        public void UpdateLuckyFlipCardInfo(RechargeItemModel recharge)
        {
            LuckyFlipCardInfo info;
            luckyFlipCardList.TryGetValue(LuckFCPeriod, out info);
            if (info == null)
            {
                info = new LuckyFlipCardInfo();
                info.Period = LuckFCPeriod;
                info.RechargeIdList.Add(recharge.Id);
                luckyFlipCardList.Add(info.Period, info);
                SyncDbInsertLuckyFlipCardInfo(info);
            }
            else
            {
                info.RechargeIdList.Add(recharge.Id);
                SyncDbUpdateLuckyFlipCardRechargeInfo(info);
            }
            SendLuckFlipCardInfo();
        }

        public LuckyFlipCardInfo GetLuckyFlipCardInfoByPeriod()
        {
            LuckyFlipCardInfo info;
            luckyFlipCardList.TryGetValue(LuckFCPeriod, out info);
            return info;
        }

        private void SyncDbInsertLuckyFlipCardInfo(LuckyFlipCardInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryInsertLuckyFlipCardInfo(Owner.Uid, info.Period, info.RechargeIdList));
        }

        private void SyncDbUpdateLuckyFlipCardRechargeInfo(LuckyFlipCardInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryUpdatLuckyFlipCardRechargeInfo(Owner.Uid, info.Period, info.RechargeIdList));
        }

        public MSG_ZMZ_LUCKY_FLIP_CARD GenerateLuckyFlipCardTransformMsg()
        {
            MSG_ZMZ_LUCKY_FLIP_CARD msg = new MSG_ZMZ_LUCKY_FLIP_CARD();
            msg.CurPeriod = LuckFCPeriod;
            foreach (var item in luckyFlipCardList)
            {
                ZMZ_LUCKY_FLIP_CARD info = new ZMZ_LUCKY_FLIP_CARD();
                info.Period = item.Value.Period;
                info.Round = item.Value.Round;
                info.RechargeIds.AddRange(item.Value.RechargeIdList);
                info.RandRewards.AddRange(item.Value.RandRewardList);
                info.CumulateRewards.AddRange(item.Value.CumulateRewardList);
                msg.List.Add(info);
            }
            return msg;
        }

        public void LoadLuckyFlipCardTransform(MSG_ZMZ_LUCKY_FLIP_CARD msg)
        {
            foreach (var item in msg.List)
            {
                LuckyFlipCardInfo info = new LuckyFlipCardInfo();
                info.Period = item.Period;
                info.Round = item.Round;
                info.RechargeIdList.AddRange(item.RechargeIds);
                info.RandRewardList.AddRange(item.RandRewards);
                info.CumulateRewardList.AddRange(item.CumulateRewards);
                luckyFlipCardList.Add(info.Period, info);
            }
            LuckFCPeriod = msg.CurPeriod;
        }
        #endregion

        #region 海岛登高礼包
        public void BindIslandHighGiftInfo(List<IslandHighGiftInfo> infoList)
        {
            foreach (var info in infoList)
            {
                islandGiftList.Add(info.SubType, info);
            }
        }

        //public void SendIslandHighGiftMsg()
        //{
        //    if (RechargeLibrary.CheckInSpecialRechargeGiftTime(RechargeGiftType.IslandHighGift, ZoneServerApi.now))
        //    {
        //        SendIslandHighGiftInfo();
        //    }
        //}

        public void SendIslandHighGiftInfo()
        {
            List<int> subList;
            if (RechargeLibrary.CheckInSpecialRechargeActivityTime(RechargeGiftType.IslandHighGift, ZoneServerApi.now, out subList))
            {
                IslandHighGiftInfo islandGift;
                MSG_ZGC_ISLAND_HIGH_GIFT_INFO msg = new MSG_ZGC_ISLAND_HIGH_GIFT_INFO();
                foreach (var subType in subList)
                {
                    ISLAND_HIGH_GIFT_INFO info = new ISLAND_HIGH_GIFT_INFO();
                    info.SubType = subType;
                    if (islandGiftList.TryGetValue(subType, out islandGift))
                    {
                        info.RechargeIdList.AddRange(islandGift.RechargeIdList);
                    }
                    msg.List.Add(info);
                }
                Owner.Write(msg);
            }
        }

        public void ResetIslandHighGiftInfo(int subType)
        {
            IslandHighGiftInfo islandGift;
            if (islandGiftList.TryGetValue(subType, out islandGift))
            {
                islandGift.RechargeIdList.Clear();
                SyncDbUpdateIslandHighGiftInfo(islandGift);
            }
        }

        public void ClearIslandHighGiftMemory(IslandHighGiftSubType subType)
        {
            IslandHighGiftInfo islandGift;
            if (islandGiftList.TryGetValue((int)subType, out islandGift))
            {
                islandGift.RechargeIdList.Clear();
            }
            SendIslandHighGiftInfo();
        }

        public bool CheckIslandHighGiftHaveBuyCount(RechargeItemModel rechargeItem)
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInSpecialRechargeActivitySubTypeTime(RechargeGiftType.IslandHighGift, rechargeItem.SubType, ZoneServerApi.now, out model))
            {
                return false;
            }

            //if (rechargeItem.SubType != model.SubType)
            //{
            //    return false;
            //}
            IslandHighGiftInfo islandGift;
            islandGiftList.TryGetValue(model.SubType, out islandGift);
            if (islandGift == null)
            {
                if (rechargeItem.Day != 1)
                {
                    return false;
                }
            }
            else
            {
                if (islandGift.RechargeIdList.Count > 0)
                {
                    if (islandGift.RechargeIdList.Contains(rechargeItem.Id))
                    {
                        //最后一档可以无限买
                        //if (rechargeItem.Day == RechargeLibrary.IslandHighGiftMaxLevel)
                        //{
                        //    return true;
                        //}
                        return false;
                    }
                    int lastRechargeId = islandGift.RechargeIdList.LastOrDefault();
                    RechargeItemModel lastRechargeItem = RechargeLibrary.GetRechargeItem(lastRechargeId);
                    if (lastRechargeItem == null || rechargeItem.Day != lastRechargeItem.Day + 1)
                    {
                        return false;
                    }
                }
                else if (rechargeItem.Day != 1)
                {
                    return false;
                }
                SpecialGiftConfig config = GiftLibrary.GetSpecialGiftConfigByType((int)RechargeGiftType.IslandHighGift, model.SubType);
                if (config == null)
                {
                    return false;
                }
            }
            return true;
        }

        public void UpdateIslandHighGiftInfo(RechargeItemModel rechargeItem)
        {
            //if (!islandGift.RechargeIdList.Contains(rechargeItem.Id))
            //{
            //    islandGift.RechargeIdList.Add(rechargeItem.Id);
            //    SyncDbUpdateIslandHighGiftInfo();
            //}
            IslandHighGiftInfo islandGift;
            islandGiftList.TryGetValue(rechargeItem.SubType, out islandGift);
            if (islandGift == null)
            {
                islandGift = new IslandHighGiftInfo();
                islandGift.SubType = rechargeItem.SubType;
                islandGift.RechargeIdList.Add(rechargeItem.Id);
                islandGiftList.Add(islandGift.SubType, islandGift);
                SyncDbInsertIslandHighGiftInfo(islandGift);
            }
            else
            {
                SpecialGiftConfig config = GiftLibrary.GetSpecialGiftConfigByType((int)RechargeGiftType.IslandHighGift, rechargeItem.SubType);
                if (rechargeItem.Day >= config.GiftMaxLevel)
                {
                    islandGift.RechargeIdList.Clear();
                    islandGift.RechargeIdList.Add(config.FreeGiftId);
                }
                else
                {
                    islandGift.RechargeIdList.Add(rechargeItem.Id);
                }
                SyncDbUpdateIslandHighGiftInfo(islandGift);
            }
            SendIslandHighGiftInfo();
        }

        private void SyncDbInsertIslandHighGiftInfo(IslandHighGiftInfo islandGift)
        {
            Owner.server.GameDBPool.Call(new QueryInsertIslandHighGiftInfo(Owner.Uid, islandGift.SubType, islandGift.RechargeIdList));
        }

        private void SyncDbUpdateIslandHighGiftInfo(IslandHighGiftInfo islandGift)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateIslandHighGiftInfo(Owner.Uid, islandGift.SubType, islandGift.RechargeIdList));
        }

        public MSG_ZMZ_ISLAND_HIGH_GIFT_INFO GenerateIslandHighGiftTransformMsg()
        {
            MSG_ZMZ_ISLAND_HIGH_GIFT_INFO msg = new MSG_ZMZ_ISLAND_HIGH_GIFT_INFO();
            foreach (var kv in islandGiftList)
            {
                ZMZ_ISLAND_HIGH_GIFT_INFO info = new ZMZ_ISLAND_HIGH_GIFT_INFO();
                info.SubType = kv.Key;
                info.RechargeIds.AddRange(kv.Value.RechargeIdList);
                msg.List.Add(info);
            }
            return msg;
        }

        public void LoadIslandHighGiftTransform(MSG_ZMZ_ISLAND_HIGH_GIFT_INFO msg)
        {
            foreach (var item in msg.List)
            {
                IslandHighGiftInfo info = new IslandHighGiftInfo();
                info.SubType = item.SubType;
                info.RechargeIdList.AddRange(item.RechargeIds);
                islandGiftList.Add(info.SubType, info);
            }
        }
        #endregion

        #region 夺宝翻翻乐
        public void BindTreasureFlipCardInfo(Dictionary<int, TreasureFlipCardInfo> list)
        {
            foreach (var item in list)
            {
                treasureFlipCardList.Add(item.Key, item.Value);
            }
        }

        public void SendTreasureFlipCardMsg()
        {
            RechargeGiftModel activityModel;
            if (RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.TreasureFlipCard, ZoneServerApi.now, out activityModel))
            {
                TreasureFCPeriod = activityModel.SubType;
                SendTreasureFlipCardInfo();
            }
        }

        public void SendTreasureFlipCardInfo()
        {
            MSG_ZGC_TREASURE_FLIP_CARD_INFO msg = new MSG_ZGC_TREASURE_FLIP_CARD_INFO();
            msg.Period = TreasureFCPeriod;

            TreasureFlipCardInfo info;
            treasureFlipCardList.TryGetValue(TreasureFCPeriod, out info);
            if (info != null)
            {
                int buyCount = info.RechargeIdList.Count;
                if (info.RandRewardList.Count == buyCount)
                {
                    msg.OpenState = 0;
                }
                else
                {
                    msg.OpenState = 1;
                }
                if (buyCount > 0)
                {
                    if (msg.OpenState > 0)
                    {
                        msg.RechargeId = info.RechargeIdList.LastOrDefault();
                    }
                    else
                    {
                        msg.RechargeId = GiftLibrary.GetTreasureFlipCardNextRechargeId(TreasureFCPeriod, info.RechargeIdList.LastOrDefault());
                    }
                }
                else
                {
                    msg.RechargeId = GiftLibrary.GetFirstTreasureFlipRechargeId(TreasureFCPeriod);
                }
                msg.FlipCount = GiftLibrary.GetTreasureFlipCardRewardMaxSubType(info.Period) * (info.Round - 1) + info.RandRewardList.Count;
                msg.CumulateRewardList.AddRange(info.CumulateRewardList);
                msg.FlipCardNum = info.FlipCardNum;
                if (info.Round < RechargeLibrary.TreasureFlipCardMaxRound)
                {
                    msg.Round = info.Round;
                }
                else
                {
                    msg.Round = RechargeLibrary.TreasureFlipCardMaxRound;
                }
            }
            else
            {
                msg.OpenState = 0;
                msg.RechargeId = GiftLibrary.GetFirstTreasureFlipRechargeId(TreasureFCPeriod);
                msg.Round = 1;
            }
            Owner.Write(msg);
        }

        public bool CheckTreasureFlipCardHaveBuyCount(RechargeItemModel model)
        {
            bool inTime = false;
            RechargeGiftModel activityModel;
            if (RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.TreasureFlipCard, ZoneServerApi.now, out activityModel))
            {
                TreasureFCPeriod = activityModel.SubType;
                if (model.SubType == TreasureFCPeriod)
                {
                    inTime = true;
                }
            }
            if (!inTime)
            {
                return false;
            }
            TreasureFlipCardInfo info;
            if (!treasureFlipCardList.TryGetValue(TreasureFCPeriod, out info))
            {
                return true;
            }
            if (info.RechargeIdList.Count > 0)
            {
                int lastRechargeId = info.RechargeIdList.LastOrDefault();
                int curRechrageId = GiftLibrary.GetTreasureFlipCardNextRechargeId(TreasureFCPeriod, lastRechargeId);
                if (curRechrageId != model.Id)
                {
                    return false;
                }
            }
            else if (model.Id != GiftLibrary.GetFirstTreasureFlipRechargeId(TreasureFCPeriod))
            {
                return false;
            }
            return true;
        }

        public void UpdateTreasureFlipCardInfo(RechargeItemModel recharge)
        {
            TreasureFlipCardInfo info;
            treasureFlipCardList.TryGetValue(TreasureFCPeriod, out info);
            if (info == null)
            {
                info = new TreasureFlipCardInfo();
                info.Period = TreasureFCPeriod;
                info.RechargeIdList.Add(recharge.Id);
                treasureFlipCardList.Add(info.Period, info);
                info.Round = 1;
                SyncDbInsertTreasureFlipCardInfo(info);
            }
            else if (!info.RechargeIdList.Contains(recharge.Id))
            {
                info.RechargeIdList.Add(recharge.Id);
                SyncDbUpdateTreasureFlipCardRechargeInfo(info);
            }

            SendTreasureFlipCardInfo();
        }

        public TreasureFlipCardInfo GetTreasureFlipCardInfoByPeriod()
        {
            TreasureFlipCardInfo info;
            treasureFlipCardList.TryGetValue(TreasureFCPeriod, out info);
            return info;
        }

        private void SyncDbInsertTreasureFlipCardInfo(TreasureFlipCardInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryInsertTreasureFlipCardInfo(Owner.Uid, info.Period, info.RechargeIdList, info.Round));
        }

        private void SyncDbUpdateTreasureFlipCardRechargeInfo(TreasureFlipCardInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateTreasureFlipCardRechargeInfo(Owner.Uid, info.Period, info.RechargeIdList));
        }

        public MSG_ZMZ_TREASURE_FLIP_CARD GenerateTreasureFlipCardTransformMsg()
        {
            MSG_ZMZ_TREASURE_FLIP_CARD msg = new MSG_ZMZ_TREASURE_FLIP_CARD();
            msg.CurPeriod = TreasureFCPeriod;
            foreach (var item in treasureFlipCardList)
            {
                ZMZ_TREASURE_FLIP_CARD info = new ZMZ_TREASURE_FLIP_CARD();
                info.Period = item.Value.Period;
                info.Round = item.Value.Round;
                info.RechargeIds.AddRange(item.Value.RechargeIdList);
                info.RandRewards.AddRange(item.Value.RandRewardList);
                info.CumulateRewards.AddRange(item.Value.CumulateRewardList);
                info.FlipCardNum = item.Value.FlipCardNum;
                msg.List.Add(info);
            }
            return msg;
        }

        public void LoadTreasureFlipCardTransform(MSG_ZMZ_TREASURE_FLIP_CARD msg)
        {
            foreach (var item in msg.List)
            {
                TreasureFlipCardInfo info = new TreasureFlipCardInfo();
                info.Period = item.Period;
                info.Round = item.Round;
                info.RechargeIdList.AddRange(item.RechargeIds);
                info.RandRewardList.AddRange(item.RandRewards);
                info.CumulateRewardList.AddRange(item.CumulateRewards);
                info.FlipCardNum = item.FlipCardNum;
                treasureFlipCardList.Add(info.Period, info);
            }
            TreasureFCPeriod = msg.CurPeriod;

        }
        #endregion

        #region 直购礼包
        private bool CheckCanBuyDirectPurchaseGift(RechargeItemModel model)
        {
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInDirectPurchaseTime(ZoneServerApi.now, out activityModel) || model.SubType != activityModel.SubType)
            {
                return false;
            }
            return true;
        }

        public void UpdateDirectPurchaseInfo(RechargeItemModel recharge)
        {
            GiftItem item;
            Gift gift;
            if (giftList.TryGetValue(recharge.GiftType, out gift))
            {
                if (gift.ItemList.Count > 0)
                {
                    int maxGiftId = gift.ItemList.Keys.Max();
                    RechargeItemModel oldRechargeItem = RechargeLibrary.GetRechargeItem(maxGiftId);
                    //只要之前记录的充值项期数和这期不一样就说明数据需要清理
                    if (oldRechargeItem == null || recharge.SubType != oldRechargeItem.SubType)
                    {
                        gift.ItemList.Clear();
                    }
                }
                item = gift.CreateItem(recharge, Owner);
                Owner.server.GameDBPool.Call(new QueryUpdateGiftItemBuyCount(Owner.Uid, recharge.GiftType, BuildGiftItemIdString(recharge.GiftType), BuildGiftBuyCountString(recharge.GiftType), BuildGiftCurBuyCountString(recharge.GiftType), BuildGiftDoubleFlagString(recharge.GiftType), BuildGiftDiscountString(recharge.GiftType), BuildDiamondRatioString(recharge.GiftType)));
            }
            else
            {
                gift = new Gift();
                item = gift.CreateItem(recharge, Owner);
                giftList.Add(recharge.GiftType, gift);
                Owner.server.GameDBPool.Call(new QueryInsertGiftItem(Owner.Uid, recharge.GiftType, recharge.Id.ToString(), "1", "1", "0", "0", "1"));
            }
        }
        #endregion
        
        public GiftItem GetOrAddGiftItem(RechargeItemModel model)
        {
            Gift gift;
            GiftItem giftItem;
            if (giftList.TryGetValue(model.GiftType, out gift))
            {
                if (!gift.ItemList.TryGetValue(model.Id, out giftItem))
                {
                    giftItem = gift.CreateItem(model, Owner);
                }
            }
            else
            {
                gift = new Gift();
                giftItem = gift.CreateItem(model, Owner);
                giftList.Add(model.GiftType, gift);
            }
            return giftItem;
        }
        
        public bool ChangeDiamondGiftRatio(int ratio, GiftItem giftItem)
        {
            if (giftItem.DiamondRatio < ratio)
            {
                giftItem.DiamondRatio = ratio;
                return true;
            }
            return false;
        }
        
        public bool ChangeDiamondGiftRatio(RechargeItemModel recharge, int ratio)
        {
            GiftItem item = null;
            if (CheckHaveThisTypeGift(recharge))
            {
                //更新礼包信息
                item = GetOrAddGiftItem(recharge);
                if (item == null) return false;
                if (!ChangeDiamondGiftRatio(ratio, item))return false;
                Owner.server.GameDBPool.Call(new QueryUpdateGiftItemBuyCount(Owner.Uid, recharge.GiftType, BuildGiftItemIdString(recharge.GiftType), BuildGiftBuyCountString(recharge.GiftType), BuildGiftCurBuyCountString(recharge.GiftType), BuildGiftDoubleFlagString(recharge.GiftType), BuildGiftDiscountString(recharge.GiftType), BuildDiamondRatioString(recharge.GiftType)));
            }
            else
            {
                //更新礼包信息
                item = GetOrAddGiftItem(recharge);
                if (item == null) return false;
                if (!ChangeDiamondGiftRatio(ratio, item))return false;
                Owner.server.GameDBPool.Call(new QueryInsertGiftItem(Owner.Uid, recharge.GiftType, BuildGiftItemIdString(recharge.GiftType), BuildGiftBuyCountString(recharge.GiftType), BuildGiftCurBuyCountString(recharge.GiftType), BuildGiftDoubleFlagString(recharge.GiftType), BuildGiftDiscountString(recharge.GiftType), BuildDiamondRatioString(recharge.GiftType)));
            }

            return true;
        }
        
        public string BuildDiamondRatioString(RechargeGiftType giftType)
        {
            Gift gift;
            giftList.TryGetValue(giftType, out gift);
            if (gift != null)
            {
                return gift.BuildDiamondRatioString();
            }
            return "";
        }
    }
}
