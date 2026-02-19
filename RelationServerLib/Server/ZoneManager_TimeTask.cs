using CommonUtility;
using EnumerateUtility.Timing;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationServerLib
{
    public partial class ZoneServerManager
    {
        /// <summary>
        /// 每天刷新触发器
        /// </summary>
        //double updatedTime = 0.0;
        //double updateTickTime = 5000.0;
        //private DateTime lastRefresh = RelationServerApi.now;

        //private void UpdateDalyRefresh(double dt)
        //{
        //    //控制检查刷新帧数
        //    if (CheckUpdateTickTime(dt))
        //    {
        //        //获取刷新任务
        //        List<TimingType> tasks = TimingLibrary.GetRelationTimingListToRefresh(lastRefresh, RelationServerApi.now);
        //        if (tasks.Count > 0)
        //        {
        //            foreach (var task in tasks)
        //            {
        //                TimingRefresh(task);
        //            }
        //        }
        //        //刷新最后刷新时间
        //        lastRefresh = RelationServerApi.now;
        //    }
        //}

        public void InitTimerManager(DateTime time)
        {
            //获取刷新任务
            Dictionary<DateTime, List<TimingType>> taskDic = TimingLibrary.GetRelationTimingLists(time);
            if (taskDic.Count > 0)
            {
                var kv = taskDic.First();
                double interval = (kv.Key - DateTime.Now).TotalMilliseconds;
                CounterTimerQuery counterTimer = new CounterTimerQuery(interval, taskDic);
                Log.Info($"InitTimerManager call task {kv.Key} and after {interval}");
                Api.TaskTimerMng.Call(counterTimer, (ret) =>
                {
                    TimingRefreshByPlayers(counterTimer.TaskDic);
                });
            }
            else
            {
                //当天没有了，下一天
                InitTimerManager(time.Date.AddDays(1));
            }
        }

        private void CallBackNextTask(DateTime time, Dictionary<DateTime, List<TimingType>> taskDic)
        {
            if (taskDic.Count > 0)
            {
                var firstTask = taskDic.First();
                double interval = (firstTask.Key - DateTime.Now).TotalMilliseconds;
                CounterTimerQuery counterTimer = new CounterTimerQuery(interval, taskDic);
                Api.TaskTimerMng.Call(counterTimer, (ret) =>
                {
                    TimingRefreshByPlayers(counterTimer.TaskDic);
                });
            }
            else
            {
                InitTimerManager(time.Date.AddDays(1));
            }
        }
        private void TimingRefreshByPlayers(Dictionary<DateTime, List<TimingType>> taskDic)
        {
            var firstTask = taskDic.First();
            taskDic.Remove(firstTask.Key);
            CallBackNextTask(firstTask.Key, taskDic);

            if (firstTask.Value.Contains(TimingType.DailyActivityRefresh))
            {
                //包含刷新活动，先更新当天的活动列表
                ActivityLibrary.RefreshTodayActivityList(RelationServerApi.now);
            }
            //有刷新任务
            foreach (var timingType in firstTask.Value)
            {
                TimingRefresh(timingType);

                Api.TrackingLoggerMng.TrackTimerLog(Api.MainId, "relation", timingType.ToString(), Api.Now());
            }

        }

        public void TimingRefresh(TimingType task)
        {
            switch (task)
            {
                #region 每日刷新
                case TimingType.ArenaDailyReward:
                    RefreshArenaDailyReward();
                    break;
                case TimingType.CampBattlePowerRank:
                    Api.CampRankMng.ReStartRank();
                    break;
                #endregion
                #region 每周刷新
                case TimingType.CampBattleReward:
                    Api.CampRewardMng.InitList();
                    break;
                case TimingType.CampBattleResetRank:
                    Api.CampRewardMng.ClearRewardList();
                    Api.CampActivityMng.ResetRankList();
                    break;
                case TimingType.WeeklyCampBuildRefresh:
                    Api.CampActivityMng.ResetCampBuildRankList();
                    Api.CampActivityMng.ResetCampBuildCounter();
                    break;
                case TimingType.CampBuildRankReward:
                    Api.CampActivityMng.InitRankReward();
                    break;
                case TimingType.SpaceTimeMonsterPeriod:
                    Api.SpaceTimeTowerManager.Refresh();
                    break;
                //case TimingType.CrossBattleResetRank:
                //    //清空排行榜
                //    Api.RankMng.CrossBattleRank.ResetRankList();
                //    break;
                #endregion
                default:
                    break;
            }
        }

        //private bool CheckUpdateTickTime(double deltaTime)
        //{
        //    updatedTime += (float)deltaTime;
        //    if (updatedTime < updateTickTime)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        updatedTime = 0;
        //        return true;
        //    }
        //}


        //private List<TimingType> CheckDailyTiming(DateTime LastRefresh, DateTime now)
        //{
        //    List<TimingType> tasks = new List<TimingType>();
        //    //检查是否是隔天了
        //    if (LastRefresh.Date < now.Date)
        //    {
        //        //TimeSpan span = now - LastRefresh;
        //        ////查看时间间隔
        //        //if (span.TotalDays >= 1)
        //        //{
        //        //    //如果大于1，说明超过了24小时，每日刷新都应该执行
        //        //    foreach (var dailyTasks in DailyTimings)
        //        //    {
        //        //        foreach (var dailyTask in dailyTasks.Value)
        //        //        {
        //        //            if (!tasks.Contains(dailyTask))
        //        //            {
        //        //                tasks.Add(dailyTask);
        //        //            }
        //        //        }
        //        //    }
        //        //}
        //        //else
        //        {
        //            //如果没大于1，说明0点前登录或者刷新过，
        //            foreach (var dailyTasks in DailyTimings)
        //            {
        //                //刷新时间只要大于上次刷新时间或者小于当前时间就可以刷新
        //                if (LastRefresh.TimeOfDay < dailyTasks.Key || dailyTasks.Key <= now.TimeOfDay)
        //                {
        //                    foreach (var dailyTask in dailyTasks.Value)
        //                    {
        //                        if (!tasks.Contains(dailyTask))
        //                        {
        //                            tasks.Add(dailyTask);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else if (LastRefresh.Date == now.Date)
        //    {
        //        //日期相同，上次是同一天刷新时间
        //        foreach (var dailyTasks in DailyTimings)
        //        {
        //            //刷新时间只要大于上次刷新时间并且小于当前时间就可以刷新
        //            if (LastRefresh.TimeOfDay < dailyTasks.Key && dailyTasks.Key <= now.TimeOfDay)
        //            {
        //                foreach (var dailyTask in dailyTasks.Value)
        //                {
        //                    if (!tasks.Contains(dailyTask))
        //                    {
        //                        tasks.Add(dailyTask);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return tasks;
        //}



        //private bool CheckRefreshTime(TimeSpan refreshTime)
        //{
        //    if (lastRefresh.Date < RelationServerApi.now.Date)
        //    {
        //        if (lastRefresh.TimeOfDay < refreshTime || refreshTime <= RelationServerApi.now.TimeOfDay)
        //        {
        //            return true;
        //        }
        //    }
        //    else if (lastRefresh.Date == RelationServerApi.now.Date)
        //    {
        //        if (lastRefresh.TimeOfDay < refreshTime && refreshTime <= RelationServerApi.now.TimeOfDay)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}


    }
}
