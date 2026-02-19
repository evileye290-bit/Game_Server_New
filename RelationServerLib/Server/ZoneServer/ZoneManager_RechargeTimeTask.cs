using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBUtility.Sql;

namespace RelationServerLib
{
    public partial class ZoneServerManager
    {
        public void InitRechargeTimerManager(DateTime time, int addDay)
        {
            time = time.AddDays(addDay);
            //获取刷新任务
            Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic = RechargeLibrary.GetRechargeTimingLists(time);
            if (taskDic.Count > 0)
            {
                var kv = taskDic.First();
                AddTaskTimer(taskDic, kv.Key);
            }
            else
            {
                if (addDay > 0)
                {
                    DateTime nextTime = time.Date.AddDays(0.5);
                    taskDic.Add(nextTime, new List<RechargeGiftTimeType>());
                    //说明已经增加过1天
                    AddTaskTimer(taskDic, nextTime);
                }
                else
                {
                    //当天没有了，下一天
                    InitRechargeTimerManager(time.Date, 1);
                }
            }
        }

        private void AddTaskTimer(Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic, DateTime time)
        {
            double interval = (time - DateTime.Now).TotalMilliseconds;
            Log.Info($"InitRechargeTimer add task timer {time} ：after {interval}");
            RechargeTimerQuery rechargeTimer = new RechargeTimerQuery(interval, taskDic);
            Api.TaskTimerMng.Call(rechargeTimer, (ret) =>
            {
                RechargeTimingRefresh(rechargeTimer.TaskDic);
            });
        }

        private void CallBackNextTask(DateTime time, Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic)
        {
            if (taskDic.Count > 0)
            {
                var firstTask = taskDic.First();
                AddTaskTimer(taskDic, firstTask.Key);
            }
            else
            {
                InitRechargeTimerManager(time.AddSeconds(1), 0);
            }
        }

        private void RechargeTimingRefresh(Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic)
        {
            var firstTask = taskDic.First();
            taskDic.Remove(firstTask.Key);
            CallBackNextTask(firstTask.Key, taskDic);

            //有刷新任务
            foreach (var giftType in firstTask.Value)
            {
                Api.TrackingLoggerMng.TrackRechargeTimerLog(Api.MainId, "relation", giftType.ToString(), Api.Now());
                switch (giftType)
                {
                    case RechargeGiftTimeType.ThemeBossStart:
                        Api.ThemeBossMng.Clear();
                        break;
                    case RechargeGiftTimeType.ThemeBossEnd:
                        Api.ThemeBossMng.ActivityEnd();
                        break;
                    case RechargeGiftTimeType.IslandHighGiftStart:
                        Api.GameDBPool.Call(new QueryClearIslandHighGiftInfo((int)IslandHighGiftSubType.IslandHigh));
                        break;
                    case RechargeGiftTimeType.Trident:
                    case RechargeGiftTimeType.NewServerTridentEnd:
                        Api.GameDBPool.Call(new QueryClearTrident());
                        break;
                    case RechargeGiftTimeType.DragonBoatStart:
                        Api.GameDBPool.Call(new QueryClearDragonBoatInfo());
                        break;
                    case RechargeGiftTimeType.CarnivalRechargeStart:
                        Api.GameDBPool.Call(new QueryClearCarnivalRechargeInfo());
                        break;
                    case RechargeGiftTimeType.CarnivalMallStart:
                        Api.GameDBPool.Call(new QueryClearCarnivalMallInfo());
                        break;
                    case RechargeGiftTimeType.ShrekInvitaionStart:
                        Api.GameDBPool.Call(new QueryClearShrekInvitationInfo());
                        break;
                    case RechargeGiftTimeType.CanoeGiftStart:
                        Api.GameDBPool.Call(new QueryClearIslandHighGiftInfo((int)IslandHighGiftSubType.Canoe));
                        break;
                    case RechargeGiftTimeType.IslandGiftThreeStart:
                        Api.GameDBPool.Call(new QueryClearIslandHighGiftInfo((int)IslandHighGiftSubType.Three));
                        break;
                    case RechargeGiftTimeType.MidAutumnStart:
                        Api.GameDBPool.Call(new QueryClearMidAutumn());
                        break;
                    case RechargeGiftTimeType.ThemeFireworkStart:
                        Api.GameDBPool.Call(new QueryClearThemeFirework());
                        break;
                    case RechargeGiftTimeType.ActivityShopEnd:
                        if (CommonShopLibrary.EndActivityShop > 0)
                        {
                            Api.GameDBPool.Call(new QueryUpdateActivityShopReflashFlag(CommonShopLibrary.EndActivityShop));
                        }
                        break;
                    case RechargeGiftTimeType.NineTestStart:
                        Api.GameDBPool.Call(new QueryClearNineTest());
                        break;
                    case RechargeGiftTimeType.DiamondRebateStart:
                        Api.GameDBPool.Call(new QueryClearDiamondRebate());
                        break;
                    case RechargeGiftTimeType.XuanBoxStart:
                        Api.GameDBPool.Call(new QueryClearXuanBox());
                        break;
                    case RechargeGiftTimeType.WishLanternStart:
                        Api.GameDBPool.Call(new QueryClearWishLantern());
                        break;
                    case RechargeGiftTimeType.NewRechargeGiftStart:
                        Api.GameDBPool.Call(new QueryClearNewRechargeGift());
                        Api.GameDBPool.Call(new QueryClearGiftItem(RechargeGiftType.NewRechargeGift));
                        break;
                    case RechargeGiftTimeType.DaysRechargeStart:
                        Api.GameDBPool.Call(new QueryClearDaysRecharge());
                        break;
                    case RechargeGiftTimeType.ShreklandStart:
                        Api.GameDBPool.Call(new QueryClearShreklandInfo());
                        break;
                    //case RechargeGiftTimeType.DevilTrainingStart:
                    //    Api.GameDBPool.Call(new QueryClearDevilTrainingInfo());
                    //    break;
                    case RechargeGiftTimeType.DomainBenedictionStart:
                        Api.GameDBPool.Call(new QueryClearDomainBenedictionInfo());
                        break;
                    default:
                        break;
                }
                MSG_RZ_UPDATE_RECHARGE_ACTIVITY_VALUE msg = new MSG_RZ_UPDATE_RECHARGE_ACTIVITY_VALUE();
                msg.GiftType = (int)giftType;
                Api.ZoneManager.Broadcast(msg);
            }
        }
    }
}
