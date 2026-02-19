using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Zone.Protocol.ZA;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class ActionManager
    {
        private List<int> finishedActions = new List<int>();
        private Dictionary<int, BaseAction> actions = new Dictionary<int, BaseAction>();
        private ListMap<ActionType, int> actionsType = new ListMap<ActionType, int>();

        private Dictionary<ulong, TimingGiftInfo> timingGiftInfos = new Dictionary<ulong, TimingGiftInfo>();

        public int CurTimingGiftCount => timingGiftInfos.Count;

        public PlayerChar Owner { get; private set; }

        public ActionManager(PlayerChar player)
        {
            Owner = player;
        }

        public void BindActionInfo(List<ActionInfo> actionInfos, List<TimingGiftInfo> timingGiftInfos)
        {
            LoadActions(actionInfos);

            LoadTimingGiftRecommendInfo(timingGiftInfos);

            InitTodayActions();
        }

        public void RecordActionAndCheck(ActionType action, Object param, string dataBox="")
        {
            List<int> actionIds;
            actionsType.TryGetValue(action, out actionIds);

            if (actionIds == null) return;

            BaseAction baseAction;
            foreach (var id in actionIds.ToList())
            {
                baseAction = GetAction(id);
                if (baseAction == null) continue;

                if (baseAction.Check(param))
                {
                    RecommondRechargeGift(baseAction, dataBox);
                }
            }
        }

        public void InvokeBySdk(int actionId, int sdkGiftId, int actionParam, int sdkActionType, string dataBox)
        {
            BaseAction action = GetAction(actionId);
            if (action == null)
            {
                Log.Warn($"player {Owner.Uid} action InvokeBySdk action id {actionId} gift type {sdkGiftId} current param {actionParam} action type {sdkActionType} error : had not find action {actionId}");
                return;
            }

            if (finishedActions.Contains(actionId))
            {
                Log.Warn($"player {Owner.Uid} action InvokeBySdk action id {actionId} gift type {sdkGiftId} current param {actionParam} action type {sdkActionType} error : action {actionId} had finished");
                return;
            }

            //sdk 没有推荐礼包类型，则走自己推荐系统
            if (sdkGiftId == 0)
            {
                RecommondRechargeGift(action, dataBox);
                action.SetFinishedBySdk(actionParam);
            }
            else
            {
                RechargeItemModel model = RechargeLibrary.GetRechargeItem(sdkGiftId, true);
                if (model == null)
                {
                    Log.Warn($"player {Owner.Uid} action InvokeBySdk action id {actionId} gift type {sdkGiftId} current param {actionParam} action type {sdkActionType} error : had not find model {sdkGiftId}");
                    return;
                }
                action.SetFinishedBySdk(actionParam);

                //读取sdk推荐礼包
                RecommendTimingGiftResult(model, sdkGiftId, actionId, true, dataBox);
            }
        }

        public void RecommendTimingGiftResult(RechargeItemModel model, int productId, int actionId, bool isSdkGift, string dataBox)
        {
            Log.Debug($"player {Owner.Uid} action InvokeBySdk action id {actionId} product id {productId} is sdk gift {isSdkGift}");
            ulong id = Owner.RecordActionTriggerGiftTime(productId, actionId, isSdkGift, dataBox);

            TimingGiftInfo info = new TimingGiftInfo()
            {
                Id = id,
                ProductId = productId,
                ActionId = actionId,
                TimingGiftType = 0,
                Buyed = false,
                CreateTime = Owner.server.Now(),
            };

            timingGiftInfos.Add(id, info);

            SyncInsertTimingGift(info);

            RechargePriceModel price = RechargeLibrary.GetRechargePrice(model.RechargeId);
            if (price != null)
            {
                Owner.BIRecordLimitPackLog(price.Price, "CNY", 1, 0, actionId, info.ProductId, info.Id);
                Owner.KomoeEventLogGiftPush(0, productId, (TimingGiftType.None).ToString(), price.Price, actionId, RewardManager.GetRewardDic(model.Reward), id, dataBox);
            }
        }

        public void Refresh()
        {
            //主要用于日、周、月重置
            actions.ForEach(x=>x.Value.Refresh());

            InitTodayActions();
        }

        private void InitTodayActions()
        {
            InitActions(ActionLibrary.TodayActions);
        }

        public void ActionFinished(BaseAction action)
        {
            RemoveActionFromCache(action);

            finishedActions.Add(action.Id);

            //添加序列的下一个
            ActionModel model = ActionLibrary.GetNextActionModel(action.Id);
            if (model != null)
            {
                InitActions(new List<int>() { model.Id });
            }
        }

        public void RemoveActionFromCache(BaseAction action)
        {
            //需要重置的不用移除
            if(action.Model.IsResetAble())return;
            
            actions.Remove(action.Id);
            actionsType.Remove(action.Model.ActionType, action.Id);
        }

        private void LoadActions(List<ActionInfo> actionInfos)
        {
            List<int> initIds = new List<int>();
            initIds.AddRange(ActionLibrary.InitActions);
            finishedActions.ForEach(x => initIds.Remove(x));
            

            foreach (var kv in actionInfos)
            {
                AddAction(kv);

                initIds.Remove(kv.Id);
            }

            //添加遗漏的
            if (initIds.Count > 0)
            {
                InitActions(initIds);
            }
        }

        private void InitActions(List<int> ids)
        {
            DateTime now = Owner.server.Now();
            int timestemp = Timestamp.GetUnixTimeStampSeconds(now);
            foreach (var kv in ids)
            {
                //已经存在不需要重复添加
                if (finishedActions.Contains(kv) || actions.ContainsKey(kv)) continue;

                ActionModel model = ActionLibrary.GetActionModel(kv);
                if (model == null) continue;

                BaseAction action = ActionFactory.CreateAction(model, this, new ActionInfo() { Id = kv, Time = timestemp });

                AddNewAction(action);
            }
        }

        private void AddAction(ActionInfo actionInfo)
        {
            ActionModel model = ActionLibrary.GetActionModel(actionInfo.Id);
            if (model == null)
            {
                //表中已经删除的action
                return;
            }

            //完成了就不在需要添加了
            if (!model.IsResetAble() && actionInfo.State == 1)
            {
                finishedActions.Add(actionInfo.Id);
                return;
            }

            BaseAction action = ActionFactory.CreateAction(model, this, actionInfo);
            AddAction(action);
        }

        private void AddNewAction(BaseAction action)
        {
            if (!actions.ContainsKey(action.Id))
            {
                AddAction(action);

                SyncInsertAction(action.ActionInfo);
            }
        }

        public void AddAction(BaseAction action)
        {
            if (!actions.ContainsKey(action.Id))
            { 
                actions.Add(action.Id, action);
                actionsType.Add(action.Model.ActionType, action.Id);
            }
        }

        public BaseAction GetAction(int id)
        {
            BaseAction action;
            actions.TryGetValue(id, out action);
            return action;
        }

        public void SyncInsertAction(ActionInfo actionInfo)
        {
            QueryInsertAction query = new QueryInsertAction(Owner.Uid, actionInfo);
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncUpdateAction(ActionInfo actionInfo)
        {
            QueryUpdateAction query = new QueryUpdateAction(Owner.Uid, actionInfo);
            Owner.server.GameDBPool.Call(query);
        }

        #region 限时礼包

        private void LoadTimingGiftRecommendInfo(List<TimingGiftInfo> timingGiftInfos)
        {
            TimingGiftInfo latest = null;
            timingGiftInfos.ForEach(x => this.timingGiftInfos.Add(x.Id, x));
        }


        private bool IsPlus(int productId)
        {
            TimingGiftStepInfo timingGiftStepInfo = ActionLibrary.GetGiftStepInfoByItemId(productId);
            return timingGiftStepInfo?.IsPlus() == true;
        }

        public void RecommondRechargeGift(BaseAction action, string dataBox)
        {
            try
            {
                Dictionary<TimingGiftType, float> weight = ActionLibrary.GetActionWeight(action.Id);
                if (weight == null)
                {
                    Logger.Log.Warn($"RecommondRechargeGift error, info have not action {action.Id} weight, check it !");
                    return;
                }

                int days = (int)(Owner.server.Now() - Owner.TimeCreated).TotalDays + 1;

                //推荐礼包逻辑
                MSG_ZA_GET_TIMING_GIFT msg = GenerateTimingGiftInfo((int)action.Model.ActionType, dataBox);
                msg.ActionId = action.Id;
                msg.CreateDays = days;

                //当前限制不出的礼包类型
                ActionLibrary.TimingGiftLimit.ForEach(kv =>
                {
                    if (!Owner.CheckLimitOpen(kv.Value))
                    {
                        msg.LimitedGiftTypes.Add((int)kv.Key);
                    }
                });

                int breakType = (int)TimingGiftType.Break;
                if (!msg.LimitedGiftTypes.Contains(breakType))
                {
                    if (!Owner.CheckResonanceLevel())
                    {
                        msg.LimitedGiftTypes.Add(breakType);
                    }
                }

                //7连抽道具
                BaseItem item = Owner.BagManager.NormalBag.GetItem((int)ConsumableType.DrawCard7);
                msg.DrawHero7 = item == null ? 0 : item.PileNum;

                //装备强化道具
                List<BaseItem> baseItems = new List<BaseItem>()
                {
                    Owner.BagManager.NormalBag.GetItem((int)ConsumableType.EquipmentGhost3),
                    Owner.BagManager.NormalBag.GetItem((int)ConsumableType.EquipmentSpar),
                };
                msg.EquipentUpgrade = baseItems.Sum(x => { return x == null ? 0 : x.PileNum; });

                //货币
                Owner.Currencies.ForEach(x => msg.Currencies.Add((int)x.Key, x.Value));



                //modify by joker so that when AnalysisServer is Null .can run normal
                if(Owner.server.AnalysisServer != null)
                {
                    Owner.server.AnalysisServer.Write(msg, Owner.Uid);
                }
                //Owner.server.AnalysisServer.Write(msg, Owner.Uid);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private bool HadRecommendSameIdPlusLevel(int productId)
        {
            if (!IsPlus(productId)) return false;
            TimingGiftInfo info = timingGiftInfos.Values.FirstOrDefault(x => x.ProductId == productId && IsPlus(x.ProductId));
            return info != null;
        }

        public void RecommendTimingGiftResult(int productId, int actionId, int timeGiftType, int level, string dataBox, bool resetRecentMaxMoney = false, bool isSdkGift = false)
        {
            RechargeItemModel model = RechargeLibrary.GetRechargeItem(productId, isSdkGift);
            if (model == null)
            {
                Log.Warn($"RecommendTimingGiftResult error gift id {productId}, error info not find RechargeItemModel {productId} is sdk gift {isSdkGift}");
                return;
            }

            Log.Warn($"player {Owner.Uid} action {actionId} recommend gift type {(TimingGiftType)timeGiftType} level {level} id {productId}  is sdk gift {isSdkGift}");

            if (HadRecommendSameIdPlusLevel(productId))
            {
                level = ActionLibrary.GetSmallerStep(level);
                productId = ActionLibrary.GetGiftItemId(level, (TimingGiftType)timeGiftType);
                Log.Warn($"player {Owner.Uid} action {actionId} recommend gift type {(TimingGiftType)timeGiftType} last recommend is plus down to common level {level} id {productId}");
            }

            ulong id = Owner.RecordActionTriggerGiftTime(productId, actionId, isSdkGift, dataBox);

            TimingGiftInfo info = new TimingGiftInfo()
            {
                Id = id,
                ProductId = productId,
                ActionId = actionId,
                TimingGiftType = timeGiftType,
                Buyed = false,
                CreateTime = Owner.server.Now(),
            };

            timingGiftInfos.Add(id, info);

            SyncInsertTimingGift(info);

            RechargePriceModel price = RechargeLibrary.GetRechargePrice(model.RechargeId);
            if (price != null)
            {
                Owner.BIRecordLimitPackLog(price.Price, "CNY", 1, 0, actionId, info.ProductId, info.Id);
                Owner.KomoeEventLogGiftPush(timeGiftType, productId, ((TimingGiftType)timeGiftType).ToString(), price.Price, actionId, RewardManager.GetRewardDic(model.Reward), id, dataBox);
            }

            if (resetRecentMaxMoney)
            {
                Owner.SetRecentAccumulateOnceMaxMoney(0);
            }
        }

        public bool CheckHaveNeedBuyTimingGift()
        {
            return timingGiftInfos.Values.Where(x => !x.Buyed && (Owner.server.Now() - x.CreateTime).TotalHours < 2).FirstOrDefault() != null;
        }

        public bool OnBuyedTimeGift(GiftItem item)
        {
            TimingGiftInfo info;
            if (timingGiftInfos.TryGetValue(item.Uid, out info))
            {
                info.Buyed = true;
                info.BuyedTime = Owner.server.Now();

                SyncUpdateTimingOnBuyed(item.Uid);
                TimerManager.Instance.NewOnceTimer(Timestamp.GetUnixTimeStamp(Owner.server.Now().AddSeconds(3)), CheckBuyLastTimingLimitGiftDelay, item.Id);
            }

            var model = RechargeLibrary.GetRechargeItemOrSdkItem(item.Id);
            if (model == null)
            {
                Log.Error($"OnBuyedTimeGift action error: info have not rechargeitem id {item.Id}");
                return false;
            }

            if (model.GiftType == RechargeGiftType.Common)
            {
                switch ((CommonGiftType)model.SubType)
                {
                    case CommonGiftType.Daily:
                        RecordActionAndCheck(ActionType.BuyAllDailyGiftBag, item.Id);
                        return true;
                    case CommonGiftType.Weekly:
                        RecordActionAndCheck(ActionType.BuyAllWeeklyGiftBag, item.Id);
                        return true;
                    case CommonGiftType.Monthly:
                        RecordActionAndCheck(ActionType.BuyAllMonthlyGiftBag, item.Id);
                        return true;
                }
            }
            return false;
        }

        private void CheckBuyLastTimingLimitGiftDelay(object param)
        {
            RecordActionAndCheck(ActionType.BuyTheLastTimingLimitGift, (int)param);
        }

        public MSG_ZA_GET_TIMING_GIFT GenerateTimingGiftInfo(int actionType, string dataBox)
        {
            MSG_ZA_GET_TIMING_GIFT msg = new MSG_ZA_GET_TIMING_GIFT()
            {
                Uid = Owner.Uid,
                ActionType = actionType,
                MaxProductMoney = Owner.RechargeManager.AccumulateOnceMaxMoney,
                LastCommonRechargeTime = Owner.RechargeManager.LastCommonRechargeTime,
                DataBox = dataBox
            };

            timingGiftInfos.ForEach(x=>msg.TimingGiftInfo.Add(GenerateGiftInfo(x.Value)));

            return msg;
        }

        private MSG_ZA_TIMING_GIFT_INFO GenerateGiftInfo(TimingGiftInfo giftInfo)
        {
            MSG_ZA_TIMING_GIFT_INFO msg = new MSG_ZA_TIMING_GIFT_INFO()
            {
                Id = giftInfo.Id,
                ProduceId = giftInfo.ProductId,
                ActionId = giftInfo.ActionId,
                TimingGiftType = giftInfo.TimingGiftType,
                Buyed = giftInfo.Buyed,
                CreateTime = Timestamp.GetUnixTimeStampSeconds(giftInfo.CreateTime),
                BuyedTime = Timestamp.GetUnixTimeStampSeconds(giftInfo.BuyedTime),
            };

            ActionModel actionModel = ActionLibrary.GetActionModel(giftInfo.ActionId);
            if (actionModel != null)
            {
                msg.ActionFrequence = (int)actionModel.ActionFrequence;
            }

            TimingGiftStepInfo stepInfo = ActionLibrary.GetGiftStepInfoByItemId(giftInfo.ProductId);
            if (stepInfo != null)
            {
                msg.Step = stepInfo.Step;
            }

            RechargeItemModel model = RechargeLibrary.GetRechargeItemOrSdkItem(giftInfo.ProductId);
            if (model != null)
            {
                RechargePriceModel priceModel = RechargeLibrary.GetRechargePrice(model.RechargeId);            
                if (priceModel != null)
                { 
                    msg.ProductMoney = priceModel.Money;
                }
            }

            return msg;
        }

        public MSG_ZMZ_GET_TIMING_GIFT GenerateTransformMsg()
        {
            MSG_ZMZ_GET_TIMING_GIFT msg = new MSG_ZMZ_GET_TIMING_GIFT();

            msg.FinishedIds.Add(finishedActions);

            actions.ForEach(x => msg.ActionInfoes.Add(new MSG_ZMZ_ACTION_INFO()
            {
                Id = x.Value.Id,
                Num = x.Value.CurNum,
                State = x.Value.State,
                Time = x.Value.ActionInfo.Time,
                Infos = x.Value.BuildActionInfo()
            }));

            timingGiftInfos.ForEach(x => msg.TimingGiftInfo.Add(new MSG_ZMZ_TIMING_GIFT_INFO()
            {
                Id = x.Value.Id,
                ProduceId = x.Value.ProductId,
                ActionId = x.Value.ActionId,
                Buyed = x.Value.Buyed,
                TimingGiftType = x.Value.TimingGiftType,
                CreateTime = Timestamp.GetUnixTimeStampSeconds(x.Value.CreateTime),
                BuyedTime = Timestamp.GetUnixTimeStampSeconds(x.Value.BuyedTime),
            }));

            return msg;
        }

        public void LoadFromTransformMsg(MSG_ZMZ_GET_TIMING_GIFT msg)
        {
            List<ActionInfo> actionInfos = new List<ActionInfo>();
            List<TimingGiftInfo> timingGiftInfos = new List<TimingGiftInfo>();

            finishedActions.AddRange(msg.FinishedIds);

            msg.ActionInfoes.ForEach(x => actionInfos.Add(new ActionInfo()
            {
                Id = x.Id,
                Num = x.Num,
                State = x.State,
                Time = x.Time,
                Infos = x.Infos
            }));

            msg.TimingGiftInfo.ForEach(x => timingGiftInfos.Add(new TimingGiftInfo()
            {
                Id = x.Id,
                Buyed = x.Buyed,
                ProductId = x.ProduceId,
                ActionId = x.ActionId,
                TimingGiftType = x.TimingGiftType,
                BuyedTime = Timestamp.TimeStampToDateTime(x.BuyedTime),
                CreateTime = Timestamp.TimeStampToDateTime(x.CreateTime),
            }));

            BindActionInfo(actionInfos, timingGiftInfos);
        }

        public void SyncInsertTimingGift(TimingGiftInfo info)
        {
            QueryInsertTimingGift query = new QueryInsertTimingGift(Owner.Uid, info);
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncUpdateTimingOnBuyed(ulong id)
        {
            QueryUpdateTimingGiftOnBuy query = new QueryUpdateTimingGiftOnBuy(Owner.Uid, id);
            Owner.server.GameDBPool.Call(query);
        }

        public void BIRecordLimitTimePackLog(RechargeItemModel recharge, GiftItem giftItem, EnumerateUtility.RechargeWay way)
        {
            RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
            TimingGiftInfo info = GetTimingGiftInfo(giftItem.Uid);
            if (info != null)
            {
                int operationTime = (int)(info.BuyedTime - info.CreateTime).TotalSeconds;
                if (way == EnumerateUtility.RechargeWay.Token)
                {
                    Owner.BIRecordLimitPackLog(price.Price, "CNY", 5, operationTime, info.ActionId, giftItem.Id, giftItem.Uid);
                }
                else
                {
                    Owner.BIRecordLimitPackLog(price.Price, "CNY", 3, operationTime, info.ActionId, giftItem.Id, giftItem.Uid);
                }
                Owner.KomoeEventLogGiftPushBuy((int)recharge.GiftType, giftItem.Id, recharge.GiftType.ToString(), price.Price, info.ActionId, RewardManager.GetRewardDic(recharge.Reward), giftItem.Uid, giftItem.DataBox);
            }
        }

        public TimingGiftInfo GetTimingGiftInfo(ulong giftUid)
        {
            TimingGiftInfo info;
            timingGiftInfos.TryGetValue(giftUid, out info);
            return info;
        }
        #endregion
    }
}
