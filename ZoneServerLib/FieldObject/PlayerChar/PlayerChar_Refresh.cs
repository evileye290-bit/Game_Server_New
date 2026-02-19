using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Timing;
using Logger;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //刷新 
        public DateTime LastRefreshTime { get; set; }
        public DateTime LastOfflineTime { get; set; }

        //public DateTime LastPhyRecoveryTime { get; set; }
        /// <summary>
        /// 检测是否需要刷新
        /// </summary>
        public void CheckRefresh()
        {
            //// 商店
            //CheckShopRefresh();
            DateTime now = DateTime.Now;
            List<TimingType> tasks = TimingLibrary.GetTimingListToRefresh(LastRefreshTime, now);
            if (tasks.Count > 0)
            {
                foreach (var task in tasks)
                {
                    TimingRefresh(task);

                    BIRecordRefreshLog(LastRefreshTime.ToString(), task.ToString(), (int)task, "longin");
                }
                //可以保存DB
                LastRefreshTime = now;
                UpdateLastRefresh();
            }

            List<int> shopList = CommonShopLibrary.GetActivityStartNeedRefreshShop(ZoneServerApi.now, ShopManager.GetCommonShop().ActivityRefreshDic);
            if (shopList.Count > 0)
            {
                ActivityShopRefresh(true, shopList);
            }
        }

        public void TimingRefresh(TimingType task)
        {
            switch (task)
            {
                #region 每日刷新
                case TimingType.EmailTask:
                    RefreshEmailTask();
                    break;
                case TimingType.CounterDailyRefresh_1:
                    ResetTaskDailyFinishState();
                    CheckCounterRefresh(task);
                    CheckTaskPeriod();
                    break;
                case TimingType.CounterDailyRefresh_2:
                    CheckCounterRefresh(task);
                    break;
                case TimingType.CounterDailyRefresh_3:
                    CheckCounterRefresh1(task);
                    break;
                case TimingType.CounterDailyRefresh_4:
                    CheckCounterRefresh(task);
                    break;
                case TimingType.CounterDailyRefresh_5:
                    CheckCounterRefresh1(task);
                    break;
                case TimingType.CounterDailyRefresh_6:
                    CheckCounterRefresh1(task);
                    break;
                case TimingType.CounterDailyRefresh_7:
                    CheckCounterRefresh1(task);
                    break;
                case TimingType.DailRefreshShop:
                    DailyRefrshShopAutoRefresh();
                    break;
                case TimingType.FriendlyHeartTiming:
                    FriendlyHeartCountRefresh(task);
                    break;
                case TimingType.DelegationDataRefresh:
                    RefreshDelegationData();
                    break;
                //case TimingType.HuntingTiming:
                //    RefreshHunting();
                //    break;
                case TimingType.DailyActivityRefresh:
                    DailyActivityRefresh();
                    DailySpecialActivityRefresh();
                    break;
                //case TimingType.Clear7DayStatData:
                //    Clear7DayStatData();
                //    break;
                //case TimingType.ClearDailyRecharge:
                //    ClearDailyRecharge();
                //    break;
                case TimingType.DailyPassCard:
                    RefreshDailyPassCard();
                    break;
                case TimingType.WishPool:
                    RefreshDailyWishPool();
                    break;
                case TimingType.GodPath:
                    RefreshDailyGodPath();
                    break;
                case TimingType.CrossActiveReward:
                    RefreshCrossDailyActiveReward();
                    break;
                case TimingType.Action:
                    RefreshAction();
                    break;
                case TimingType.DailyRecharge:
                    RefreshDailyRechargeGift();
                    break;
                case TimingType.MonthCardReceiveState:
                    RefreshMonthCardReceiveState();
                    break;
                case TimingType.DailyRechargeAmount:
                    ClearDailyRecharge();
                    break;
                case TimingType.RefreshPassCardPeriod:
                    if (PassCardLibrary.CheckPeriodUpdate(server.OpenServerTime, ZoneServerApi.now))
                    {
                        RefreshPassCardPeriod();
                    }
                    break;
                case TimingType.CheckUpdateThemePass:
                    CheckUpdateThemePass();
                    break;
                case TimingType.RefreshPettyGift:
                    SendPettyGiftRefreshMsg();
                    break;
                case TimingType.RefreshDailyRecharge:
                    NotifyClientRefreshDailyRecharge();
                    break;
                //case TimingType.CheckOpenNewThemeBoss:
                //    CheckOpenNewThemeBoss();
                //    break;
                case TimingType.RefreshNewServerPromotion:
                    NotifyClientRefreshNewServerPromotion();
                    break;
                case TimingType.RefreshLuckFlipCard:
                    NotifyClientRefreshLuckyFlipCard();
                    break;
                case TimingType.IslandHighGift:
                    RefreshIslandHighGift();
                    break;
                case TimingType.DragonBoat:
                    RefreshDragonBoatFreeTicketState();
                    break;
                case TimingType.CrossChallengeActiveReward:
                    RefreshCrossChallengeDailyActiveReward();
                    break;
                case TimingType.MidAutumn:
                    RefreshMidAutumnFreeFlag();
                    break;
                case TimingType.RefreshDailySchoolTask:
                    RefreshDailySchoolTaskInfo();
                    break;
                case TimingType.AnswerQuestion:
                    RefreshAnswerQuestionInfo();
                    break;
                case TimingType.RefreshTreasureFlipCard:
                    NotifyClientRefreshTreasureFlipCard();
                    break;
                case TimingType.DomainBenedictionNum:
                {
                    RefreshDomainBenedictionNum();
                    break;
                }
                #endregion
                #region 每周
                case TimingType.WeeklyCampBuildRefresh:
                    CheckCounterRefresh(task);
                    break;
                case TimingType.WeeklyPassCard:
                    RefreshWeeklyPassCard();
                    break;
                case TimingType.WeeklyDrawBlessing:
                    RefreshDrawBlessing((int)task);
                    break;
                case TimingType.WeeklyRefreshShop:
                    WeeklyRefreshShopAutoRefresh();
                    break;
                case TimingType.WeeklyRecharge:
                    RefreshWeeklyRechargeGift();
                    break;
                case TimingType.WeeklyCampBattleRefresh:
                    CheckCounterRefresh(task);
                    break;
                case TimingType.WeeklyActivityFinish:
                    ResetWeeklyTaskFinishState();
                    break;
                case TimingType.RefreshWeeklySchoolTask:
                    RefreshWeeklySchoolTaskInfo();
                    break;
                case TimingType.SpaceTimeTowerReset:
                    SpaceTimeTowerMng.ActivityOpen();
                    break;
                case TimingType.RefreshWeeklyDriftExplore:
                    RefreshWeeklyDriftExplore();
                    break;
                //case TimingType.CrossBattleResetRank:
                //    RefreshCrossRank();
                //    break;
                #endregion
                #region 固定日期
                case TimingType.CounterDateRefresh_1:
                    CheckCounterRefresh(task);
                    break;
                case TimingType.DateDrawBlessing:
                    RefreshDrawBlessing((int)task);
                    break;
                //case TimingType.CrossSeason:
                //    RefreshCrossSeason();
                //    break;
                case TimingType.FirstRechargeDiamond:
                    ResetDiamondGiftDoubleFlag();
                    break;
                case TimingType.RechargeDiscount:
                    RefreshRechargeDiscount();
                    break;          
                #endregion
                #region 每月
                case TimingType.MonthlyRefreshShop:
                    MonthlyRefreshShopAutoRefresh();
                    break;
                case TimingType.MonthlyRecharge:
                    RefreshMonthlyRechargeGift();
                    break;
                #endregion
                default:
                    break;
            }
        }


        private void RefreshBuyCampNatureCount()
        {
            throw new NotImplementedException();
        }

        private void RefreshBuyCampActionCount()
        {
            throw new NotImplementedException();
        }

        //private void CheckShopRefresh()
        //{
        //    List<ShopType> shopList = ShopLibrary.CheckRefreshShopList(LastRefreshTime, ZoneServerApi.now);
        //    RefreshShop(shopList);
        //}

        private void RefreshEmailTask()
        {
            //检查邮箱是否已经有邮件任务
            if (!EmailMng.CheckHasEmailItemByType(EmailType.Daily))
            {
                //任务是否已经有邮件任务
                Dictionary<int, int> list = TaskMng.GetEmailTaskIds((int)TaskMainType.Hide);
                if (list.Count < 2)
                {
                    //获取email ID
                    int emailId = GetTaskEmailId();
                    if (emailId > 0)
                    {
                        //发送邮件
                        SendPersonEmail(emailId);
                        //MSG_ZR_SEND_SINGLE_EMAI msg = new MSG_ZR_SEND_SINGLE_EMAI();
                        //msg.Uid = Uid;
                        //msg.EmailId = emailId;
                        //server.SendToRelation(msg);
                    }
                    else
                    {
                        Log.Warn("player {0} refresh email task not get task email id {1}", Uid, emailId);
                    }
                }
                else
                {
                    //已经含有任务，不继续随机
                    Log.Warn("player {0} refresh email task get email task ids count {1}", Uid, list.Count);
                }
            }
            else
            {
                //已经含有任务，不继续随机
            }
        }


        public void ClearDailyRecharge()
        {
            if (RechargeMng.ClientDailyRecharge())
            {
                //string tableName = "recharge";
                server.GameDBPool.Call(new QueryUpdateRechargeProducts(Uid, RechargeMng.AccumulateTotal, RechargeMng.AccumulateCurrent, RechargeMng.AccumulateDaily, RechargeMng.AccumulateOnceMaxMoney, RechargeMng.AccumulatePrice, RechargeMng.AccumulateMoney, RechargeMng.PayCount, RechargeMng.NewRechargeGiftScore));
                SendRechargeManger();
            }
        }

        public void UpdateLastRefresh()
        {
            server.GameDBPool.Call(new QueryUpdateLastRefresh(Uid, LastRefreshTime));
        }

        public void RechargeActivityRefresh(RechargeGiftTimeType giftType)
        {
            switch (giftType)
            {
                case RechargeGiftTimeType.ThemeBossStart:
                    CheckOpenNewThemeBoss();
                    break;
                case RechargeGiftTimeType.HeroDaysRewardsStart:
                    GiftManager.NotifyClientNewHeroDaysRewardStart();
                    break;
                case RechargeGiftTimeType.ActivityShopStart:
                    if (CommonShopLibrary.StartActivityShop > 0)
                    {
                        ActivityShopRefresh(false, new List<int>() { CommonShopLibrary.StartActivityShop });
                    }
                    break;
                //case RechargeGiftTimeType.XuanBoxStart:
                //    XuanBoxManager.Clear();
                //    break;
                case RechargeGiftTimeType.WishLanternStart:
                    WishLanternManager.Clear();
                    break;
                case RechargeGiftTimeType.DaysRechargeStart:
                    DaysRechargeManager.Clear();
                    break;
                default:
                    break;
            }
        }
    }
}
