using CommonUtility;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Global.Protocol.GZ;
using Message.Zone.Protocol.ZG;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerFrame;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //运营活动
        public ActivityManager ActivityMng = new ActivityManager();

        public void LoadActivityList(List<ActivityItem> list)
        {
            List<int> removeList = new List<int>();
            foreach (var item in list)
            {
                ActivityInfo info = ActivityLibrary.GetActivityInfoById(item.Id);
                if (info == null)
                {
                    //说明今天没有这个活动，可以删除了
                    //Log.Warn("player {0} LoadActivityList not find activity info: {1}", Uid, item.Id);
                    removeList.Add(item.Id);
                    continue;
                }
                if (ActivityLibrary.CheckTodayActivity(info.Type, item.Id))
                {
                    ActivityMng.AddActivityItem(info, item);
                }
                else
                {
                    removeList.Add(item.Id);
                }
            }

            ActivityLibrary.RefreshTodayActivityList(server.Now(), true);

            List<ActivityItem> addList = new List<ActivityItem>();
            //List<ActivityItem> updateList = new List<ActivityItem>();
            Dictionary<ActivityAction, List<int>> todayList = ActivityLibrary.GetTodayActivityList();
            foreach (var kv in todayList)
            {
                foreach (var activityId in kv.Value)
                {
                    //查看今天是否有这个活动
                    ActivityInfo info = ActivityLibrary.GetActivityInfoById(activityId);
                    if (info == null)
                    {
                        //说明今天没有这个活动
                        Log.Warn("player {0} LoadActivityList {1} not find activity info.", Uid, activityId);
                        continue;
                    }
                    ActivityItem item = ActivityMng.GetActivityItemForId(activityId);
                    if (item == null)
                    {
                        //switch (info.Type)
                        //{
                        //    case ActivityAction.BattlePower:
                        //    case ActivityAction.PlayerLevel:
                        //        break;
                        //    default:
                        //        //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                        AddNewActivityItem(addList, info);
                        //        break;
                        //}
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            if (addList.Count > 0)
            {
                SyncDbInsertActivityItem(addList);
            }
            if (removeList.Count > 0)
            {
                SyncDbDeleteActivityItem(removeList);
            }
        }

        private void AddNewActivityItem(List<ActivityItem> addList, ActivityInfo info)
        {
            ActivityItem item = CreatActivityItem(info.Id);
            ActivityInitCurNum(info, item);
            ActivityMng.AddActivityItem(info, item);
            //保存数据
            //SyncDbInsertActivityItem(item);
            addList.Add(item);
        }

        private void ActivityOpen()
        {
            //List<ActivityItem> updateList = new List<ActivityItem>();
            List<ActivityItem> addList = new List<ActivityItem>();
            //List<int> removeOnlineList = new List<int>();
            //bool allGet = true;
            //double passTime = (ZoneServerApi.now - TimeCreated).TotalSeconds;
            foreach (var activityId in ActivityLibrary.OnlineRewardOnceList)
            {
                ActivityInfo info = ActivityLibrary.GetActivityInfoById(activityId);
                if (info == null)
                {
                    //说明今天没有这个活动
                    Log.Warn("player {0} LoadActivityList {1} not find activity info.", Uid, activityId);
                    continue;
                }
                ActivityItem item = ActivityMng.GetActivityItemForId(activityId);
                if (item == null)
                {
                    //if (passTime  > info.Num)
                    //{
                    //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                    //switch (info.Type)
                    //{
                    //    case ActivityAction.BattlePower:
                    //    case ActivityAction.PlayerLevel:
                    //        break;
                    //    default:
                    //        //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                    AddNewActivityItem(addList, info);
                    //        break;
                    //}
                    //item = CreatActivityItem(activityId);
                    //ActivityInitCurNum(info, item);
                    //ActivityMng.AddActivityItem(info, item);
                    ////保存数据
                    ////SyncDbInsertActivityItem(item);
                    //addList.Add(item);
                    //updateList.Add(item);
                    //}
                    //else
                    //{
                    //    //说明已经删除
                    //    break;
                    //}
                }
                //else
                //{
                //    if (item.State != (int)ActivityState.Get)
                //    {
                //        allGet = false;
                //    }
                //    else
                //    {
                //        removeOnlineList.Add(activityId);
                //    }
                //}
            }
            //if (allGet)
            //{
            //    removeList.AddRange(removeOnlineList);
            //}
            if (addList.Count > 0)
            {
                SyncDbInsertActivityItem(addList);
                //TODO 发消息给前台
                SyncActivityChangeMessage(addList, null);
            }
            //if (updateList.Count > 0)
            //{
            //    //TODO 发消息给前台
            //    SyncActivityChangeMessage(updateList, null);
            //}
            OnlineRewardTime = ZoneServerApi.now;
        }

        public void LoadActivityTransform(RepeatedField<MSG_ZMZ_ACTIVITY_INFO> activitys)
        {
            List<int> removeList = new List<int>();
            foreach (var item in activitys)
            {
                ActivityInfo info = ActivityLibrary.GetActivityInfoById(item.Id);
                if (info == null)
                {
                    //说明今天没有这个活动，可以删除了
                    //Log.Warn("player {0} LoadActivityList not find activity info: {1}", Uid, item.Id);
                    removeList.Add(item.Id);
                    continue;
                }
                ActivityItem activity = CreatActivityItem(item.Id);
                activity.CurNum = item.CurNum;
                activity.State = item.State;
                activity.Param = item.Param;

                ActivityMng.AddActivityItem(info, activity);
            }

            if (removeList.Count > 0)
            {
                SyncDbDeleteActivityItem(removeList);

                //TODO 发消息给前台
                SyncActivityChangeMessage(null, removeList);
            }
        }

        public void AddActivityNumForType(ActivityAction type, int num = 1)
        {
            List<ActivityItem> updateList = new List<ActivityItem>();
            List<ActivityItem> addList = new List<ActivityItem>();
            switch (type)
            {
                case ActivityAction.OnlineReward:
                    {
                        //特殊处理在线奖励
                        ActivityItem activity = GetActivityCurOnlineRewardItem();
                        if (activity != null)
                        {
                            AddActivityNumForId(activity.Id, num, updateList, addList);
                        }
                    }
                    break;
                case ActivityAction.OnlineRewardOnce:
                    {
                        //特殊处理在线奖励
                        foreach (var activityId in ActivityLibrary.OnlineRewardOnceList)
                        {
                            AddActivityNumForId(activityId, num, updateList, addList);
                        }
                    }
                    break;
                default:
                    {
                        //获取当前类型所有活动
                        List<int> list = ActivityLibrary.GetTodayActivityItemForType(type);
                        if (list != null && list.Count > 0)
                        {
                            foreach (var activityId in list)
                            {
                                AddActivityNumForId(activityId, num, updateList, addList);
                            }
                        }
                    }
                    break;
            }

            if (updateList.Count > 0 || addList.Count > 0)
            {
                //TODO 记录数据库
                //foreach (var item in updateList)
                //{
                //    SyncDbUpdateActivityItem(item);
                //}
                if (addList.Count > 0)
                {
                    SyncDbInsertActivityItem(addList);
                }
                if (updateList.Count > 0)
                {
                    SyncDbUpdateActivityItem(updateList);
                }
                updateList.AddRange(addList);
                //TODO 发消息给前台
                SyncActivityChangeMessage(updateList, null);
            }
        }

        private void AddActivityNumForId(int activityId, int num, List<ActivityItem> updateList, List<ActivityItem> addList)
        {
            bool isInsert = false;

            //查看今天是否有这个活动
            ActivityInfo info = ActivityLibrary.GetActivityInfoById(activityId);
            if (info == null)
            {
                //说明今天没有这个活动
                Log.Warn("player {0} add activity {1} num not find activity info.", Uid, activityId);
                return;
            }

            if (!info.IsEveryDate)
            {
                if (info.StartDate > ZoneServerApi.now || ZoneServerApi.now > info.EndDate)
                {
                    //说明时间不正确
                    Log.Warn("player {0} add activity {1} num data error: start {2} end {3}.", Uid, activityId, info.StartDate, info.EndDate);
                    return;
                }
            }

            if (!info.IsEveryTime)
            {
                if (info.StartTime > ZoneServerApi.now.TimeOfDay || ZoneServerApi.now.TimeOfDay > info.EndTime)
                {
                    //说明时间不正确
                    Log.Warn("player {0} add activity {1} num time error: start {2} end {3}.", Uid, activityId, info.StartTime, info.EndTime);
                    return;
                }
            }


            ActivityItem item = ActivityMng.GetActivityItemForId(activityId);
            if (item == null)
            {
                //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                item = CreatActivityItem(activityId);
                isInsert = true;
            }
            else
            {
                if (item.State == (int)ActivityState.Get)
                {
                    //状态1表示已经领取
                    return;
                }
            }

            if (ActivityAddCurNum(num, info, item))
            {
                if (isInsert)
                {
                    ActivityMng.AddActivityItem(info, item);

                    addList.Add(item);
                    //SyncDbInsertActivityItem(item);
                }
                else
                {
                    updateList.Add(item);
                }
            }
        }


        private static ActivityItem CreatActivityItem(int activityId)
        {
            ActivityItem item = new ActivityItem();
            item.Id = activityId;
            return item;
        }

        public void ActivityComplete(int activityId)
        {
            bool isInsert = false;

            //领取成功通知客户端
            MSG_ZGC_ACTIVITY_COMPLETE msg = new MSG_ZGC_ACTIVITY_COMPLETE();
            msg.ActivityId = activityId;

            //查看今天是否有这个活动
            ActivityInfo info = ActivityLibrary.GetActivityInfoById(activityId);
            if (info == null)
            {
                //说明今天没有这个活动
                Log.Warn("player {0} ActivityComplete not find activity info: {1}", Uid, activityId);
                msg.Result = (int)ErrorCode.NotExist;
                Write(msg);
                return;
            }

            //查看身上是否有这个活动
            ActivityItem item = ActivityMng.GetActivityItemForId(activityId);

            if (item == null)
            {
                //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                item = CreatActivityItem(activityId);
                ActivityInitCurNum(info, item);
                isInsert = true;
            }


            if (item.State == (int)ActivityState.Get)
            {
                //状态1表示已经领取
                Log.Warn("player {0} ActivityComplete id {1} error: type {2} state is {3}", Uid, info.Id, info.Type, item.State);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!info.IsEveryDate)
            {
                if (info.StartDate > ZoneServerApi.now || ZoneServerApi.now > info.EndDate)
                {
                    //说明时间不正确
                    Log.Warn("player {0} ActivityComplete time id {1} error: start {2} end {3}", Uid, info.Id, info.StartDate, info.EndDate);
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
            }

            if (!info.IsEveryTime)
            {
                if (info.StartTime > ZoneServerApi.now.TimeOfDay || ZoneServerApi.now.TimeOfDay > info.EndTime)
                {
                    //说明时间不正确
                    Log.Warn("player {0} ActivityComplete activity {1} num time error: start {2} end {3}.", Uid, info.Id, info.StartTime, info.EndTime);
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
            }

            //检查任务完成条件
            if (!CheckActivityComplete(item, info))
            {
                Log.Warn("player {0} ActivityComplete activity id {1} error ", Uid, item.Id);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //完成领取状态
            CompleteActivityState(item, info);


            string rewardString = GetActivityRewardString(info, item);

            if (!string.IsNullOrEmpty(rewardString))
            {
                //获取任务奖励
                RewardManager manager = GetSimpleReward(rewardString, ObtainWay.Activity);
                manager.GenerateRewardItemInfo(msg.Rewards);
            }


            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            //TODO 更新数据库
            if (isInsert)
            {
                //添加到内存中
                ActivityMng.AddActivityItem(info, item);
                //同步数据库
                SyncDbInsertActivityItem(item);
            }
            else
            {
                SyncDbUpdateActivityItem(item);
            }

            //TODO 发消息给前台
            SyncActivityChangeMessage(new List<ActivityItem>() { item }, null);

            BIRecordActivityLog(info.Type, activityId);
            //BI 活动
            KomoeEventLogOperationalActivity(activityId, "福利活动", info.Type.ToString(), (int)info.Type, 3, item.CurNum, item.CurNum, "", "", null, "");
        }

        public void ActivityTypeComplete(ActivityAction type)
        {
            MSG_ZGC_ACTIVITY_TYPE_COMPLETE msg = new MSG_ZGC_ACTIVITY_TYPE_COMPLETE();
            List<ActivityItem> updateList = new List<ActivityItem>();
            string rewardString = string.Empty;
            //获取当前类型所有活动
            List<int> list = ActivityLibrary.GetTodayActivityItemForType(type);
            if (list != null && list.Count > 0)
            {
                foreach (var activityId in list)
                {
                    //领取成功通知客户端
                    bool isInsert = false;

                    //查看今天是否有这个活动
                    ActivityInfo info = ActivityLibrary.GetActivityInfoById(activityId);
                    if (info == null)
                    {
                        //说明今天没有这个活动
                        Log.Warn("player {0} ActivityTypeComplete not find activity info: {1}", Uid, activityId);
                        msg.Result = (int)ErrorCode.NotExist;
                        Write(msg);
                        return;
                    }

                    //查看身上是否有这个活动
                    ActivityItem item = ActivityMng.GetActivityItemForId(activityId);

                    if (item == null)
                    {
                        //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                        item = CreatActivityItem(activityId);
                        isInsert = true;
                    }
                    if (!info.IsEveryDate)
                    {
                        if (info.StartDate > ZoneServerApi.now || ZoneServerApi.now > info.EndDate)
                        {
                            //说明时间不正确
                            Log.Warn("player {0} ActivityTypeComplete time id {1} error: start {2} end {3}", Uid, info.Id, info.StartDate, info.EndDate);
                            msg.Result = (int)ErrorCode.Fail;
                            Write(msg);
                            return;
                        }
                    }
                    if (!info.IsEveryTime)
                    {
                        if (info.StartTime > ZoneServerApi.now.TimeOfDay || ZoneServerApi.now.TimeOfDay > info.EndTime)
                        {
                            //说明时间不正确
                            Log.Warn("player {0} ActivityTypeComplete activity {1} num time error: start {2} end {3}.", Uid, info.Id, info.StartTime, info.EndTime);
                            msg.Result = (int)ErrorCode.Fail;
                            Write(msg);
                            return;
                        }
                    }
                    if (item.State == (int)ActivityState.Get)
                    {
                        //状态1表示已经领取
                        Log.Warn("player {0} ActivityTypeComplete id {1} error: type {2} state is {3}", Uid, info.Id, info.Type, item.State);
                        msg.Result = (int)ErrorCode.Fail;
                        Write(msg);
                        return;
                    }
                    //检查任务完成条件
                    if (!CheckActivityComplete(item, info))
                    {
                        Log.Warn("player {0} ActivityTypeComplete id {1} error ", Uid, item.Id);
                        msg.Result = (int)ErrorCode.Fail;
                        Write(msg);
                        return;
                    }

                    //完成领取状态
                    CompleteActivityState(item, info);

                    rewardString += "|" + GetActivityRewardString(info, item);
                    msg.ActivityIds.Add(activityId);

                    //TODO 更新数据库
                    if (isInsert)
                    {
                        //添加到内存中
                        ActivityMng.AddActivityItem(info, item);
                        //同步数据库
                        SyncDbInsertActivityItem(item);
                    }
                    else
                    {
                        SyncDbUpdateActivityItem(item);
                    }
                    updateList.Add(item);


                    BIRecordActivityLog(info.Type, activityId);
                    //BI 活动
                    KomoeEventLogOperationalActivity(activityId, "福利活动", info.Type.ToString(), (int)info.Type, 3, item.CurNum, item.CurNum, "", "", null, "");
                }
            }

            if (!string.IsNullOrEmpty(rewardString))
            {
                //获取任务奖励
                RewardManager manager = GetSimpleReward(rewardString, ObtainWay.Activity);
                manager.GenerateRewardItemInfo(msg.Rewards);
            }

            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            if (updateList.Count > 0)
            {
                //TODO 发消息给前台
                SyncActivityChangeMessage(updateList, null);
            }
        }

        public void ActivityRelatedComplete(int activityId)
        {
            MSG_ZGC_ACTIVITY_RELATED_COMPLETE msg = new MSG_ZGC_ACTIVITY_RELATED_COMPLETE();
            List<ActivityItem> updateList = new List<ActivityItem>();
            string rewardString = string.Empty;

            ActivityInfo info = ActivityLibrary.GetActivityInfoById(activityId);
            //领取成功通知客户端
            bool isInsert = false;

            //查看今天是否有这个活动
            //ActivityInfo info = ActivityLibrary.GetActivityInfoById(activityId);
            if (info == null)
            {
                //说明今天没有这个活动
                Log.Warn("player {0} ActivityRelatedComplete not find activity info: {1}", Uid, activityId);
                msg.Result = (int)ErrorCode.NotExist;
                Write(msg);
                return;
            }

            //查看身上是否有这个活动
            ActivityItem item = ActivityMng.GetActivityItemForId(activityId);

            if (item == null)
            {
                //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                item = CreatActivityItem(activityId);
                isInsert = true;
            }
            if (!info.IsEveryDate)
            {
                if (info.StartDate > ZoneServerApi.now || ZoneServerApi.now > info.EndDate)
                {
                    //说明时间不正确
                    Log.Warn("player {0} ActivityRelatedComplete time id {1} error: start {2} end {3}", Uid, info.Id, info.StartDate, info.EndDate);
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
            }
            if (!info.IsEveryTime)
            {
                if (info.StartTime > ZoneServerApi.now.TimeOfDay || ZoneServerApi.now.TimeOfDay > info.EndTime)
                {
                    //说明时间不正确
                    Log.Warn("player {0} ActivityRelatedComplete activity {1} num time error: start {2} end {3}.", Uid, info.Id, info.StartTime, info.EndTime);
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
            }
            if (item.State == (int)ActivityState.Get)
            {
                //状态1表示已经领取
                Log.Warn("player {0} ActivityRelatedComplete id {1} error: type {2} state is {3}", Uid, info.Id, info.Type, item.State);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }
            //检查任务完成条件
            if (!CheckActivityComplete(item, info))
            {
                Log.Warn("player {0} ActivityRelatedComplete id {1} error ", Uid, item.Id);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //完成领取状态
            CompleteActivityState(item, info);

            rewardString += "|" + GetActivityRewardString(info, item);
            msg.ActivityIds.Add(activityId);

            //TODO 更新数据库
            if (isInsert)
            {
                //添加到内存中
                ActivityMng.AddActivityItem(info, item);
                //同步数据库
                SyncDbInsertActivityItem(item);
            }
            else
            {
                SyncDbUpdateActivityItem(item);
            }
            updateList.Add(item);

            /////////////
            //开始检查所有关联的活动领取没有，如果没有就强行完成领取
            ActivityInfo relatedInfo = ActivityLibrary.GetRelatedActivityInfoById(activityId);
            if (relatedInfo != null)
            {
                ActivityItem relatedItem = ActivityMng.GetActivityItemForId(relatedInfo.Id);
                bool relatedInsert = false;
                bool relatedUpdate = true;
                if (relatedItem == null)
                {
                    //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                    relatedItem = CreatActivityItem(activityId);
                    relatedInsert = true;
                }
                if (!relatedInfo.IsEveryDate)
                {
                    if (relatedInfo.StartDate > ZoneServerApi.now || ZoneServerApi.now > relatedInfo.EndDate)
                    {
                        //说明时间不正确
                        Log.Warn("player {0} ActivityRelatedComplete related time id {1} error: start {2} end {3}", Uid, relatedInfo.Id, relatedInfo.StartDate, relatedInfo.EndDate);
                        relatedUpdate = false;
                    }
                }
                if (!relatedInfo.IsEveryTime)
                {
                    if (relatedInfo.StartTime > ZoneServerApi.now.TimeOfDay || ZoneServerApi.now.TimeOfDay > relatedInfo.EndTime)
                    {
                        //说明时间不正确
                        Log.Warn("player {0} ActivityRelatedComplete related activity {1} num time error: start {2} end {3}.", Uid, relatedInfo.Id, relatedInfo.StartTime, relatedInfo.EndTime);
                        relatedUpdate = false;
                    }
                }
                if (relatedItem.State == (int)ActivityState.Get)
                {
                    //状态1表示已经领取
                    Log.Warn("player {0} ActivityRelatedComplete related id {1} error: type {2} state is {3}", Uid, relatedInfo.Id, relatedInfo.Type, item.State);
                    relatedUpdate = false;
                }
                //检查任务完成条件
                if (!CheckActivityComplete(relatedItem, relatedInfo))
                {
                    Log.Warn("player {0} ActivityRelatedComplete related id {1} error ", Uid, relatedItem.Id);
                    relatedUpdate = false;
                }

                //完成领取状态
                CompleteActivityState(relatedItem, relatedInfo);

                //TODO 更新数据库
                if (relatedUpdate)
                {
                    rewardString += "|" + GetActivityRewardString(relatedInfo, relatedItem);
                    msg.ActivityIds.Add(relatedInfo.Id);

                    if (relatedInsert)
                    {
                        //添加到内存中
                        ActivityMng.AddActivityItem(relatedInfo, relatedItem);
                        //同步数据库
                        SyncDbInsertActivityItem(relatedItem);
                    }
                    else
                    {
                        SyncDbUpdateActivityItem(relatedItem);
                    }
                    updateList.Add(item);
                }
            }
            //////////////


            if (!string.IsNullOrEmpty(rewardString))
            {
                //获取任务奖励
                RewardManager manager = GetSimpleReward(rewardString, ObtainWay.Activity);
                manager.GenerateRewardItemInfo(msg.Rewards);
            }

            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            if (updateList.Count > 0)
            {
                //TODO 发消息给前台
                SyncActivityChangeMessage(updateList, null);
            }
        }

        public void SyncActivityChangeMessage(List<ActivityItem> updateList, List<int> removeList)
        {
            MSG_ZGC_ACTIVITY_CHANGE msg = new MSG_ZGC_ACTIVITY_CHANGE();
            if (updateList != null)
            {
                //List<ActivityItem> realUpdateList = InterceptByQuestionnaireRule(updateList);
                foreach (var item in updateList)
                {
                    msg.UpdateList.Add(GetActivityInfo(item));
                }
            }
            if (removeList != null)
            {
                foreach (var task in removeList)
                {
                    msg.RemoveList.Add(task);
                }
            }
            Write(msg);
        }

        public void DailyActivityRefresh()
        {
            List<ActivityItem> updateList = new List<ActivityItem>();
            List<ActivityItem> addList = new List<ActivityItem>();
            List<int> removeList = new List<int>();

            Dictionary<int, ActivityItem> list = ActivityMng.GetActivityList();
            foreach (var kv in list)
            {
                if (!ActivityLibrary.OnlineRewardOnceList.Contains(kv.Value.Id))
                {
                    ActivityInfo info = ActivityLibrary.GetActivityInfoById(kv.Value.Id);
                    if (info == null)
                    {
                        //说明今天没有这个活动，可以删除了
                        //Log.Warn("player {0} LoadActivityList not find activity info: {1}", Uid, item.Id);
                        removeList.Add(kv.Value.Id);
                    }
                    if (!ActivityLibrary.CheckTodayActivity(info.Type, kv.Value.Id))
                    {
                        removeList.Add(kv.Value.Id);
                    }
                }
            }

            Dictionary<ActivityAction, List<int>> todayList = ActivityLibrary.GetTodayActivityList();
            foreach (var kv in todayList)
            {
                foreach (var activityId in kv.Value)
                {
                    //bool isInsert = false;
                    //查看今天是否有这个活动
                    ActivityInfo info = ActivityLibrary.GetActivityInfoById(activityId);
                    if (info == null)
                    {
                        //说明今天没有这个活动
                        Log.Warn("player {0} DailyActivityRefresh {1} not find activity info.", Uid, activityId);
                        continue;
                    }
                    ActivityItem item = ActivityMng.GetActivityItemForId(activityId);
                    if (item == null)
                    {
                        //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                        //switch (info.Type)
                        //{
                        //    case ActivityAction.BattlePower:
                        //    case ActivityAction.PlayerLevel:
                        //        break;
                        //    default:
                        //        //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                        AddNewActivityItem(addList, info);
                        //        break;
                        //}
                        //item = CreatActivityItem(activityId);
                        //ActivityInitCurNum(info, item);
                        //isInsert = true;
                    }
                    else if (info.IsDailyRefresh == 1)
                    {
                        bool isChange = false;
                        if (item.State != (int)ActivityState.None)
                        {
                            item.SetState(ActivityState.None);
                            isChange = true;
                        }
                        if (item.CurNum > 0)
                        {
                            ActivityInitCurNum(info, item);
                            isChange = true;
                        }
                        if (isChange)
                        {
                            updateList.Add(item);
                        }
                    }
                    else
                    {
                        continue;
                    }

                    //if (isInsert)
                    //{
                    //    ActivityMng.AddActivityItem(info, item);
                    //    addList.Add(item);
                    //    //SyncDbInsertActivityItem(item);
                    //}
                    //else
                    //{
                    //    updateList.Add(item);
                    //}
                }
            }
            //List<int> removeOnlineList = new List<int>();
            //foreach (var activityId in ActivityLibrary.OnlineRewardOnceList)
            //{
            //    ActivityItem item = ActivityMng.GetActivityItemForId(activityId);
            //    if (item != null)
            //    {
            //        if (item.State != (int)ActivityState.Get)
            //        {
            //            removeOnlineList.Clear();
            //            break;
            //        }
            //        else
            //        {
            //            removeOnlineList.Add(activityId);
            //        }
            //    }
            //}
            //removeList.AddRange(removeOnlineList);
            if (updateList.Count > 0 || addList.Count > 0 || removeList.Count > 0)
            {
                if (removeList.Count > 0)
                {
                    foreach (var id in removeList)
                    {
                        ActivityMng.RemoveActivityItem(id);
                    }

                    SyncDbDeleteActivityItem(removeList);
                }

                if (addList.Count > 0)
                {
                    SyncDbInsertActivityItem(addList);
                }
                if (updateList.Count > 0)
                {
                    SyncDbUpdateActivityItem(updateList);
                }
                updateList.AddRange(addList);

                //if (updateList.Count > 0)
                //{
                //    //TODO 记录数据库
                //    //foreach (var item in updateList)
                //    //{
                //    //    SyncDbUpdateActivityItem(item);
                //    //}
                //    SyncDbUpdateActivityItem(updateList);
                //}
                //TODO 发消息给前台
                SyncActivityChangeMessage(updateList, removeList);
            }

            AddActivityNumForType(ActivityAction.ClockIn);
        }

        private MSG_ZGC_ACTIVITY_INFO GetActivityInfo(ActivityItem item)
        {
            MSG_ZGC_ACTIVITY_INFO info = new MSG_ZGC_ACTIVITY_INFO();
            info.Id = item.Id;
            info.CurNum = item.CurNum;
            info.State = item.State;
            info.Param = item.Param;
            return info;
        }

        public List<MSG_ZGC_ACTIVITY_INFO> GetActivityListMessage()
        {
            List<MSG_ZGC_ACTIVITY_INFO> list = new List<MSG_ZGC_ACTIVITY_INFO>();
            Dictionary<int, ActivityItem> activityList = ActivityMng.GetActivityList();
            foreach (var activity in activityList)
            {
                list.Add(GetActivityInfo(activity.Value));
            }
            return list;
        }

        public List<int> GetActivityTypeComplete()
        {
            return ActivityMng.GetActivityCompleteTypes();

        }

        private void ActivityInitCurNum(ActivityInfo info, ActivityItem item)
        {
            switch (info.Type)
            {
                case ActivityAction.BattlePower:
                    {
                        item.CurNum = HeroMng.CalcBattlePower();
                        break;
                    }
                case ActivityAction.PlayerLevel:
                    {
                        item.CurNum = Level;
                        break;
                    }
                case ActivityAction.DailyReward:
                    {
                        int accumulateDay = GetActivityAccumulateSignInDay();
                        DailyRewardInfo DailyRewardInfo = ActivityLibrary.GetDailyRewardInfoByDay(accumulateDay);
                        if (DailyRewardInfo != null)
                        {
                            item.CurNum = DailyRewardInfo.Id;
                        }
                        break;
                    }
                default:
                    {
                        item.CurNum = 0;
                    }
                    break;
            }
        }


        private bool ActivityAddCurNum(int num, ActivityInfo info, ActivityItem item)
        {
            switch (info.Type)
            {
                case ActivityAction.AccumulateSignIn:
                    {
                        bool hasGet = CheckActivityDailyRewardGet();
                        if (!hasGet)
                        {
                            item.CurNum += num;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case ActivityAction.OnlineRewardOnce:
                case ActivityAction.OnlineReward:
                case ActivityAction.ClockIn:
                case ActivityAction.AccumulateGet:
                    {
                        if (item.CurNum < info.Num)
                        {
                            item.CurNum = Math.Min(item.CurNum + num, info.Num);
                            //Log.Warn("AddActivityNumForType id {0} add time {1}", info.Id, num);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case ActivityAction.GrowthFund:
                case ActivityAction.GrowthFundEx:
                case ActivityAction.BattlePower:
                case ActivityAction.PlayerLevel:
                    if (item.CurNum < info.Num)
                    {
                        item.CurNum = Math.Min(num, info.Num);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                //case ActivityAction.Questionnaire:
                //    item.CurNum = num;
                //    break;
                case ActivityAction.DailyReward:
                case ActivityAction.SignIn:
                default:
                    return false;
            }
        }

        public bool CheckActivityComplete(ActivityItem activity, ActivityInfo info)
        {
            switch (info.Type)
            {
                case ActivityAction.SignIn:
                case ActivityAction.Physical:
                    {
                        //状态0表示没有领取，不是0说明错误不对
                        return true;
                    }
                case ActivityAction.OnlineRewardOnce:
                case ActivityAction.OnlineReward:
                    {
                        int passTime = (int)(ZoneServerApi.now - OnlineRewardTime).TotalSeconds + activity.CurNum;
                        if (info.Num <= passTime)
                        {
                            return true;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.Type, info.Num, activity.CurNum);
                            return false;
                        }
                    }
                case ActivityAction.PlayerLevel:
                    {
                        activity.CurNum = Level;
                        if (info.Num > 0 && info.Num <= activity.CurNum)
                        {
                            return true;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.Type, info.Num, activity.CurNum);
                            return false;
                        }
                    }
                case ActivityAction.BattlePower:
                    {
                        activity.CurNum = ShopManager.BattlePower;
                        if (info.Num > 0 && info.Num <= activity.CurNum)
                        {
                            return true;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.Type, info.Num, activity.CurNum);
                            return false;
                        }
                    }
                case ActivityAction.ClockIn:
                    if (info.Num > 0 && info.Num <= activity.CurNum)
                    {
                        return true;
                    }
                    else
                    {
                        Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.Type, info.Num, activity.CurNum);
                        return false;
                    }
                case ActivityAction.MonthCard:
                    {
                        if (RechargeMng.MonthCardTime > Timestamp.GetUnixTimeStampSeconds(server.Now()) && Timestamp.TimeStampToDateTime(activity.Param).Date < server.Now().Date)
                        {
                            return true;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4} MonthCard time error", Uid, info.Id, info.Type, info.Num, activity.CurNum);
                        }
                        return false;
                    }
                case ActivityAction.GrowthFund://level 购买了卡
                    {
                        if (!RechargeMng.GrowthFund1())
                        {
                            return false;
                        }
                        if (activity.CurNum >= info.Num)
                        {
                            return true;
                        }
                        return false;
                    }
                case ActivityAction.GrowthFundEx:
                    {
                        if (!RechargeMng.GrowthFund2())
                        {
                            return false;
                        }
                        if (activity.CurNum >= info.Num)
                        {
                            return true;
                        }
                        return false;
                    }
                case ActivityAction.SeasonCard:
                    {
                        if (RechargeMng.SeasonCardTime > Timestamp.GetUnixTimeStampSeconds(server.Now()) && Timestamp.TimeStampToDateTime(activity.Param).Date < server.Now().Date)
                        {
                            return true;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4} SeasonCard time error", Uid, info.Id, info.Type, info.Num, activity.CurNum);
                        }
                        return false;
                    }
                case ActivityAction.WeekCard:
                    {
                        //判断在期限内
                        int tempTimestamp = Timestamp.GetUnixTimeStampSeconds(server.Now());
                        if (RechargeMng.WeekCardStart > tempTimestamp || RechargeMng.WeekCardEnd < tempTimestamp)
                        {
                            return false;
                        }
                        //判断%7后大小 以及 param作为时间戳来比对
                        DateTime tempStart = Timestamp.TimeStampToDateTime(RechargeMng.WeekCardStart);


                        if ((server.Now().Date - tempStart.Date).TotalDays % 7 < info.Num)
                        {
                            return false;
                        }
                        DateTime lastUpdate = Timestamp.TimeStampToDateTime(activity.Param);
                        DateTime tempEnd = Timestamp.TimeStampToDateTime(RechargeMng.WeekCardEnd);

                        //根据上次更新是否差一周判断
                        if (lastUpdate < tempStart || (server.Now().Date - lastUpdate.Date).TotalDays >= 7)
                        {
                            int curNum = (int)((server.Now().Date - tempStart.Date).TotalDays % 7);
                            if (curNum >= info.Num)
                            {
                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            int curWeek = (int)((server.Now().Date - tempStart.Date).TotalDays / 7);
                            int curNum = (int)((server.Now().Date - tempStart.Date).TotalDays % 7);
                            int lastWeek = (int)((lastUpdate.Date - tempStart.Date).TotalDays / 7);
                            int deltaWeek = curWeek - lastWeek;
                            //判断上次使用时间在该位置前,当前天数在该位置后
                            if ((deltaWeek == 0 && (lastUpdate.Date - tempStart.Date).TotalDays % 7 < info.Num) && curNum >= info.Num || (deltaWeek != 0 && curNum >= info.Num))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    return false;
                case ActivityAction.DailyReward:
                    if (CheckAllMonthCard())
                    {
                        //是月卡可以直接完成
                        return true;
                    }
                    else
                    {
                        bool hasGet = CheckActivityDailyRewardGet();
                        if (hasGet)
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.Type, info.Num, activity.CurNum);
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                case ActivityAction.AccumulateGet:
                    {
                        int dailyId = activity.CurNum + 1;
                        DailyAccumulateGetInfo getInfo = ActivityLibrary.GetDailyAccumulateGetInfo(dailyId);
                        if (getInfo != null)
                        {
                            int accumulateDay = GetActivityAccumulateSignInDay();
                            if (accumulateDay >= getInfo.Day)
                            {
                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.Type, info.Num, activity.CurNum);
                            return false;
                        }
                    }
                case ActivityAction.AccumulateSignIn:
                case ActivityAction.None:
                default:
                    {
                        Log.Warn("player {0} CheckActivityComplete id {1} error: not find type {2}", Uid, info.Id, info.Type);
                        return false;
                    }
            }
        }

        private string GetActivityRewardString(ActivityInfo info, ActivityItem item)
        {
            string rewardString = string.Empty;
            switch (info.Type)
            {
                case ActivityAction.DailyReward:
                    {
                        DailyRewardInfo getInfo = ActivityLibrary.GetDailyRewardInfo(item.CurNum);
                        if (getInfo != null)
                        {
                            rewardString = getInfo.GetReward(info.Num);
                        }
                        else
                        {
                            Log.Warn("player {0} GetActivityRewardString id {1} error: not find DailyRewardInfo type {2} num is {3} cur is {4}",
                                Uid, info.Id, info.Type, info.Num, item.CurNum);
                        }
                    }
                    break;
                case ActivityAction.AccumulateGet:
                    {
                        DailyAccumulateGetInfo getInfo = ActivityLibrary.GetDailyAccumulateGetInfo(item.CurNum);
                        if (getInfo != null)
                        {
                            rewardString = getInfo.Reward;
                        }
                        else
                        {
                            Log.Warn("player {0} GetActivityRewardString id {1} error: not find DailyAccumulateGetInfo type {2} num is {3} cur is {4}",
                                Uid, info.Id, info.Type, info.Num, item.CurNum);
                        }
                    }
                    break;
                case ActivityAction.AccumulateSignIn:
                    break;
                default:
                    {
                        rewardString = info.Reward;
                    }
                    break;
            }

            return rewardString;
        }

        public void CompleteActivityState(ActivityItem item, ActivityInfo info)
        {
            switch (info.Type)
            {
                case ActivityAction.OnlineRewardOnce:
                case ActivityAction.OnlineReward:
                    item.CurNum = info.Num;
                    item.SetState(ActivityState.Get);
                    break;
                case ActivityAction.DailyReward:
                    //签到计数
                    AddActivityNumForType(ActivityAction.AccumulateSignIn);

                    item.SetState(ActivityState.Get);

                    SetActivityDailyRewardGet();
                    break;
                case ActivityAction.AccumulateGet:
                    item.CurNum++;
                    break;
                case ActivityAction.MonthCard:
                    item.Param = Timestamp.GetUnixTimeStampSeconds(server.Now());
                    item.SetState(ActivityState.Continuous);
                    break;
                case ActivityAction.SeasonCard:
                    item.Param = Timestamp.GetUnixTimeStampSeconds(server.Now());
                    item.SetState(ActivityState.Continuous);
                    break;
                case ActivityAction.WeekCard:
                    item.Param = Timestamp.GetUnixTimeStampSeconds(server.Now());
                    item.SetState(ActivityState.Continuous);
                    break;
                default:
                    item.SetState(ActivityState.Get);
                    break;
            }
        }

        public ActivityItem GetActivityCurOnlineRewardItem()
        {
            //获取当前类型所有活动
            List<int> list = ActivityLibrary.GetTodayActivityItemForType(ActivityAction.OnlineReward);
            if (list != null && list.Count > 0)
            {
                foreach (var activityId in list)
                {
                    ActivityItem activity = ActivityMng.GetActivityItemForId(activityId);
                    if (activity != null && activity.State != (int)ActivityState.Get)
                    {
                        return activity;
                    }
                }
            }
            return null;
        }

        public void SetActivityDailyRewardGet()
        {
            List<ActivityItem> updateList = new List<ActivityItem>();

            //获取当前类型所有活动
            List<int> list = ActivityLibrary.GetTodayActivityItemForType(ActivityAction.DailyReward);
            if (list != null && list.Count > 0)
            {
                foreach (var activityId in list)
                {
                    ActivityItem activity = ActivityMng.GetActivityItemForId(activityId);
                    if (activity != null && activity.State != (int)ActivityState.Get && activity.State != (int)ActivityState.Continuous)
                    {
                        activity.SetState(ActivityState.Continuous);
                        updateList.Add(activity);
                    }
                }
            }

            if (updateList.Count > 0)
            {
                //TODO 记录数据库
                //foreach (var item in updateList)
                //{
                //    SyncDbUpdateActivityItem(item);
                //}
                SyncDbUpdateActivityItem(updateList);
                //TODO 发消息给前台
                SyncActivityChangeMessage(updateList, null);
            }
        }

        public bool CheckActivityDailyRewardGet()
        {
            //获取当前类型所有活动
            List<int> list = ActivityLibrary.GetTodayActivityItemForType(ActivityAction.DailyReward);
            if (list != null && list.Count > 0)
            {
                foreach (var activityId in list)
                {
                    ActivityItem activity = ActivityMng.GetActivityItemForId(activityId);
                    if (activity != null && activity.State == (int)ActivityState.Get)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public int GetActivityAccumulateSignInDay()
        {
            int accumulateDay = 0;
            //获取当前类型所有活动
            List<int> list = ActivityLibrary.GetTodayActivityItemForType(ActivityAction.AccumulateSignIn);
            if (list != null && list.Count > 0)
            {
                foreach (var activityId in list)
                {
                    ActivityItem item = ActivityMng.GetActivityItemForId(activityId);
                    if (item != null)
                    {
                        accumulateDay = item.CurNum;
                    }
                }
            }
            return accumulateDay;
        }

        #region 充值返利

        public void SendRechargeRebateInfo()
        {
            if (!IsNeedShowRechargeRebate())
            {
                Write(new MSG_ZGC_RECHARGE_REBATE_INFO());
                return;
            }

            RechargeRebateModel model = RechargeRebateLibrary.GetRechargeRebateModel(AccountName);
            if (model == null)
            {
                Log.Warn($"had not find uid {uid} channel {ChannelName} rebate info!");
                return;
            }

            DateTime date = TimeCreated.Date.AddDays(RechargeRebateLibrary.RewardStartDays);
            MSG_ZGC_RECHARGE_REBATE_INFO msg = new MSG_ZGC_RECHARGE_REBATE_INFO()
            {
                HaveRebate = true,
                TotalMoney = model.Money,
                RebateStartTime = Timestamp.GetUnixTimeStampSeconds(date),
            };

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(model.Data.GetString("Reward"));
            manager.GenerateRewardItemInfo(msg.Rewards);

            Write(msg);
        }

        public bool IsRebateStarted()
        {
            DateTime date = TimeCreated.Date.AddDays(RechargeRebateLibrary.RewardStartDays);
            return server.Now() >= date;
        }

        public bool IsRebateEnd()
        {
            DateTime date = TimeCreated.Date.AddDays(RechargeRebateLibrary.RewardEndDays);
            return server.Now() > date;
        }

        public bool IsNeedShowRechargeRebate()
        {
            return GetRebateErrorCode() == ErrorCode.Success;
        }

        private ErrorCode GetRebateErrorCode()
        {
            if (IsRebated) return ErrorCode.RebateHadReward;

            if (!RechargeRebateLibrary.IsNeedRebate(AccountName)) return ErrorCode.HaveNoRebate;

            //限定只能在前n个服务器登录，才能进行返利
            if (!RechargeRebateLibrary.IsCurrServerRebateAvailable(server.MainId)) return ErrorCode.RebateServerLimited;

            //超过领取时间不能继续领取了
            if (IsRebateEnd()) return ErrorCode.RebateEnd;

            return ErrorCode.Success;
        }

        internal void RechargeRebateReward()
        {
            MSG_ZGC_RECHARGE_REBATE_GET_REWARD msg = new MSG_ZGC_RECHARGE_REBATE_GET_REWARD();

            //未开始
            if (!IsRebateStarted())
            {
                msg.Result = (int)ErrorCode.RebateNotStart;
                Write(msg);
                return;
            }

            ErrorCode errorCode = GetRebateErrorCode();
            if (errorCode != ErrorCode.Success)
            {
                msg.Result = (int)errorCode;
                Write(msg);
                return;
            }

            RechargeRebateModel model = RechargeRebateLibrary.GetRechargeRebateModel(AccountName);
            if (model == null)
            {
                Log.Warn($"had not find uid {uid} channel {ChannelName} rebate info!");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            MSG_ZG_REBATE_UPDATE request = new MSG_ZG_REBATE_UPDATE()
            {
                Account = AccountName,
                Uid = uid,
                ModelId = model.Id
            };
            server.GlobalServer.Write(request, uid);
        }

        internal void RechargeRebateRewardFromGlobal(MSG_GZ_REBATE_UPDATE info)
        {
            MSG_ZGC_RECHARGE_REBATE_GET_REWARD msg = new MSG_ZGC_RECHARGE_REBATE_GET_REWARD();

            RechargeRebateModel model = RechargeRebateLibrary.GetRechargeRebateModel(AccountName);
            if (model == null)
            {
                Log.Warn($"had not find uid {uid} channel {ChannelName} rebate info!");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (info.Result != (int)ErrorCode.Success)
            {
                msg.Result = info.Result;
                Write(msg);
                return;
            }

            IsRebated = true;
            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(model.Data.GetString("Reward"));
            manager.GenerateRewardItemInfo(msg.Rewards);
            msg.Result = (int)ErrorCode.Success;

            AddRewards(manager, ObtainWay.Recharge);
            Write(msg);

            SendRechargeRebateInfo();
        }
        #endregion

        public void SyncDbUpdateActivityItem(ActivityItem item)
        {
            //string tableName = "activity_current";
            server.GameDBPool.Call(new QueryUpdateActivityInfo(Uid, item.Id, item.CurNum, item.State, item.Param));
        }

        public void SyncDbUpdateActivityItem(List<ActivityItem> items)
        {
            List<AbstractDBQuery> querys = new List<AbstractDBQuery>();
            foreach (var item in items)
            {
                server.GameDBPool.Call(new QueryUpdateActivityInfo(Uid, item.Id, item.CurNum, item.State, item.Param));
            }
            //DBQueryTransaction dBQueryTransaction = new DBQueryTransaction(querys);
            ////string tableName = "activity_current";
            //server.GameDBPool.Call(dBQueryTransaction);
        }

        public void SyncDbInsertActivityItem(ActivityItem item)
        {
            //string tableName = "activity_current";
            server.GameDBPool.Call(new QueryInsertActivityInfo(Uid, item.Id, item.CurNum, item.State, item.Param));
        }

        public void SyncDbInsertActivityItem(List<ActivityItem> items)
        {
            List<AbstractDBQuery> querys = new List<AbstractDBQuery>();
            foreach (var item in items)
            {
                server.GameDBPool.Call(new QueryInsertActivityInfo(Uid, item.Id, item.CurNum, item.State, item.Param));
            }
            //DBQueryTransaction dBQueryTransaction = new DBQueryTransaction(querys);
            //server.GameDBPool.Call(dBQueryTransaction);
        }

        public void SyncDbDeleteActivityItem(List<int> ids)
        {
            //string tableName = "activity_current";
            server.GameDBPool.Call(new QueryDeleteActivityInfo(Uid, ids));
        }

        private MSG_ZMZ_ACTIVITY_MANAGER GetActivityTransform()
        {
            MSG_ZMZ_ACTIVITY_MANAGER info = new MSG_ZMZ_ACTIVITY_MANAGER();
            Dictionary<int, ActivityItem> activityList = ActivityMng.GetActivityList();
            foreach (var activity in activityList)
            {
                MSG_ZMZ_ACTIVITY_INFO item = new MSG_ZMZ_ACTIVITY_INFO();
                item.Id = activity.Value.Id;
                item.CurNum = activity.Value.CurNum;
                item.State = activity.Value.State;
                item.Param = activity.Value.Param;
                info.ActivityList.Add(item);
            }

            Dictionary<int, SpecialActivityItem> specialList = ActivityMng.GetSpecialActivityList();
            foreach (var activity in specialList)
            {
                MSG_ZMZ_SPECIAL_ACTIVITY_INFO item = new MSG_ZMZ_SPECIAL_ACTIVITY_INFO();
                item.Id = activity.Value.Id;
                item.CurNum = activity.Value.CurNum;
                item.State = activity.Value.State;
                info.SpecialList.Add(item);
            }

            Dictionary<int, RunawayActivityItem> runawayList = ActivityMng.GetRunawayActivityList();
            foreach (var activity in runawayList)
            {
                MSG_ZMZ_RUNAWAY_ACTIVITY_INFO item = new MSG_ZMZ_RUNAWAY_ACTIVITY_INFO();
                item.Id = activity.Value.Id;
                item.CurNum = activity.Value.CurNum;
                item.State = activity.Value.State;
                info.RunawayList.Add(item);
            }

            info.OpenType = ActivityMng.RunawayType;
            info.OpenTime = ActivityMng.RunawayTime;
            info.DataBox = ActivityMng.DataBox;

            Dictionary<int, float> rechargeMoney = ActivityMng.GetWebPayRebateRechargeMoney();
            foreach (var item in rechargeMoney)
            {
                info.WebPayRebateMoney.Add(item.Key, item.Value);
            }
           
            Dictionary<int, WebPayRebateItem> webPayRebateList = ActivityMng.GetWebPayRebateItemList();
            foreach (var rebateInfo in webPayRebateList)
            {
                MSG_ZMZ_WEBPAY_REBATE_INFO item = new MSG_ZMZ_WEBPAY_REBATE_INFO();
                item.Id = rebateInfo.Value.Id;
                item.IsGet = rebateInfo.Value.isGet;
                rebateInfo.Value.ConditionCurNum.ForEach(kv => item.ConditionCurNum.Add(kv.Key, kv.Value));
                item.PayMode = rebateInfo.Value.PayMode;
                info.WebPayRebateList.Add(item);
            }

            Dictionary<int, int> loginMark = ActivityMng.GetWebPayRebateLoginMark();
            foreach (var item in loginMark)
            {
                info.WebPayRebateLoginMark.Add(item.Key, item.Value);
            }

            return info;
        }

        #region 返利活动
        public void LoadSpecialActivityList(List<SpecialActivityItem> list)
        {
            List<int> removeList = new List<int>();
            foreach (var item in list)
            {
                SpecialActivityInfo info = ActivityLibrary.GetSpecialActivityInfoById(item.Id);
                if (info == null)
                {
                    //说明今天没有这个活动，可以删除了
                    //Log.Warn("player {0} LoadSpecialActivityList not find activity info: {1}", Uid, item.Id);
                    removeList.Add(item.Id);
                    continue;
                }
                if (info.CheckTime(ZoneServerApi.now))
                {
                    //在活动时间内
                    ActivityMng.AddSpecialActivityItem(info, item);
                }
                else
                {
                    //活动已过期
                    removeList.Add(item.Id);
                }
            }


            SyncDbDeleteSpecialActivityItem(removeList);
        }

        private void CheckSpecialActivity()
        {
            if (CheckLimitOpen(LimitType.SpecialActivity))
            {
                SpecialActivityOpen();

                SendSpecialActivityListMessage();
            }
        }

        private void SpecialActivityOpen()
        {
            ActivityLibrary.RefreshTodayActivityList(server.Now(), true);

            List<SpecialActivityItem> updateList = new List<SpecialActivityItem>();

            foreach (var kv in ActivityLibrary.todaySpecialTypeList)
            {
                foreach (var activityId in kv.Value)
                {

                    SpecialActivityItem item = ActivityMng.GetSpecialActivityItemForId(activityId);
                    if (item == null)
                    {
                        //查看今天是否有这个活动
                        SpecialActivityInfo info = ActivityLibrary.GetSpecialActivityInfoById(activityId);
                        if (info == null)
                        {
                            //说明今天没有这个活动
                            Log.Warn("player {0} LoadSpecialActivityList {1} not find activity info.", Uid, activityId);
                            continue;
                        }

                        SpecialActivityItem specialItem = CreatSpecialActivityItem(info.Id);
                        if (info.SpecialType == SpecialAction.SignIn)
                        {
                            specialItem.CurNum = 1;
                            updateList.Add(specialItem);
                        }
                        ActivityMng.AddSpecialActivityItem(info, specialItem);
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            SyncDbInsertSpecialActivityItem(updateList);
        }

        public void DailySpecialActivityRefresh()
        {
            List<int> removeList = new List<int>();

            Dictionary<int, SpecialActivityItem> list = ActivityMng.GetSpecialActivityList();
            foreach (var kv in list)
            {
                SpecialActivityInfo info = ActivityLibrary.GetSpecialActivityInfoById(kv.Value.Id);
                if (info == null)
                {
                    //说明今天没有这个活动，可以删除了
                    //Log.Warn("player {0} LoadSpecialActivityList not find activity info: {1}", Uid, item.Id);
                    removeList.Add(kv.Value.Id);
                    continue;
                }
                if (!info.CheckTime(ZoneServerApi.now))
                {
                    //不在活动时间内
                    removeList.Add(kv.Value.Id);
                }
            }

            AddSpecialActivityNumForType(SpecialAction.SignIn);
            AddRunawayActivityNumForType(RunawayAction.SignIn);

            SyncDbDeleteSpecialActivityItem(removeList);

            if (CheckLimitOpen(LimitType.SpecialActivity))
            {
                SpecialActivityOpen();
            }
        }

        public void LoadSpecialActivityTransform(RepeatedField<MSG_ZMZ_SPECIAL_ACTIVITY_INFO> activitys)
        {
            List<int> removeList = new List<int>();
            foreach (var item in activitys)
            {
                SpecialActivityInfo info = ActivityLibrary.GetSpecialActivityInfoById(item.Id);
                if (info == null)
                {
                    //说明今天没有这个活动，可以删除了
                    //Log.Warn("player {0} LoadSpecialActivityList not find activity info: {1}", Uid, item.Id);
                    removeList.Add(item.Id);
                    continue;
                }
                if (info.CheckTime(ZoneServerApi.now))
                {
                    SpecialActivityItem activity = CreatSpecialActivityItem(item.Id);
                    activity.CurNum = item.CurNum;
                    activity.State = item.State;

                    //在活动时间内
                    ActivityMng.AddSpecialActivityItem(info, activity);
                }
                else
                {
                    //活动已过期
                    removeList.Add(item.Id);
                }
            }

            SyncDbDeleteSpecialActivityItem(removeList);

        }

        public void AddSpecialActivityNumForType(SpecialAction type, float num = 1, bool needSync = true)
        {
            List<SpecialActivityItem> updateList = new List<SpecialActivityItem>();

            //获取当前类型所有活动
            List<int> list = ActivityLibrary.GetTodaySpecialItemForType(type);
            if (list != null && list.Count > 0)
            {
                foreach (var activityId in list)
                {
                    AddSpecialActivityNumForId(activityId, num, updateList);
                }
            }

            if (updateList.Count > 0)
            {
                SyncDbInsertSpecialActivityItem(updateList);

                if (needSync)
                {
                    //发消息给前台
                    SyncSpecialActivityChangeMessage(updateList, null);
                }
            }
        }

        public void SyncSpecialActivityChangeMessage(List<SpecialActivityItem> updateList, List<int> removeList)
        {
            MSG_ZGC_SPECIAL_ACTIVITY_CHANGE msg = new MSG_ZGC_SPECIAL_ACTIVITY_CHANGE();
            if (updateList != null)
            {
                foreach (var item in updateList)
                {
                    msg.UpdateList.Add(GetActivityInfo(item));
                }
            }
            if (removeList != null)
            {
                foreach (var task in removeList)
                {
                    msg.RemoveList.Add(task);
                }
            }
            msg.TotalCount = server.ManagerServer.TotalCount;
            msg.UseCount = server.ManagerServer.UseCount;
            Write(msg);
        }

        private void AddSpecialActivityNumForId(int activityId, float num, List<SpecialActivityItem> updateList)
        {
            bool isInsert = false;

            //查看今天是否有这个活动
            SpecialActivityInfo info = ActivityLibrary.GetSpecialActivityInfoById(activityId);
            if (info == null)
            {
                //说明今天没有这个活动
                Log.Warn("player {0} add special activity {1} num not find activity info.", Uid, activityId);
                return;
            }

            if (info.OpenStart > ZoneServerApi.now || ZoneServerApi.now > info.OpenEnd)
            {
                //说明时间不正确
                Log.Warn("player {0} add special activity {1} num data error: start {2} end {3}.", Uid, activityId, info.OpenStart, info.OpenEnd);
                return;
            }

            SpecialActivityItem item = ActivityMng.GetSpecialActivityItemForId(activityId);
            if (item == null)
            {
                //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                item = CreatSpecialActivityItem(activityId);
                isInsert = true;
            }
            else
            {
                if (item.State == (int)ActivityState.Get)
                {
                    //状态1表示已经领取
                    return;
                }
            }

            if (SpecialActivityAddCurNum(num, info, item))
            {
                if (isInsert)
                {
                    ActivityMng.AddSpecialActivityItem(info, item);
                }

                updateList.Add(item);
            }
        }

        public void SpecialActivityComplete(int activityId)
        {
            //领取成功通知客户端
            MSG_ZGC_SPECIAL_ACTIVITY_COMPLETE msg = new MSG_ZGC_SPECIAL_ACTIVITY_COMPLETE();
            msg.ActivityId = activityId;

            //查看今天是否有这个活动
            SpecialActivityInfo info = ActivityLibrary.GetSpecialActivityInfoById(activityId);
            if (info == null)
            {
                //说明今天没有这个活动
                Log.Warn("player {0} SpecialActivityComplete not find activity info: {1}", Uid, activityId);
                msg.Result = (int)ErrorCode.NotExist;
                Write(msg);
                return;
            }
            if (info.OpenStart > ZoneServerApi.now || ZoneServerApi.now > info.OpenEnd)
            {
                //说明时间不正确
                Log.Warn("player {0} SpecialActivityComplete activity {1} num data error: start {2} end {3}.", Uid, activityId, info.OpenStart, info.OpenEnd);
                msg.Result = (int)ErrorCode.NotExist;
                Write(msg);
                return;
            }

            //查看身上是否有这个活动
            SpecialActivityItem item = ActivityMng.GetSpecialActivityItemForId(activityId);
            if (item == null)
            {
                //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                item = CreatSpecialActivityItem(activityId);
                ActivityMng.AddSpecialActivityItem(info, item);
            }
            if (item.State == (int)ActivityState.Get)
            {
                //状态1表示已经领取
                Log.Warn("player {0} SpecialActivityComplete id {1} error: type {2} state is {3}", Uid, info.Id, info.SpecialType, item.State);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //检查任务完成条件
            if (!CheckSpecialActivityComplete(item, info))
            {
                Log.Warn("player {0} ActivityCSpecialActivityCompleteomplete activity id {1} error ", Uid, item.Id);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (server.ManagerServer.TotalCount > 0 && server.ManagerServer.TotalCount <= server.ManagerServer.UseCount)
            {
                Log.Warn("player {0} ActivityCSpecialActivityCompleteomplete count is {1} and {2} ", Uid, server.ManagerServer.TotalCount, server.ManagerServer.UseCount);
                msg.Result = (int)ErrorCode.MaxCount;
                Write(msg);
                return;
            }

            //检查库存
            MSG_ZM_GET_SPECIAL_ACTIVITY_ITEM getMsg = new MSG_ZM_GET_SPECIAL_ACTIVITY_ITEM();
            getMsg.Id = activityId;
            getMsg.Num = info.Num;
            server.ManagerServer.Write(getMsg, Uid);


        }

        public void SpecialActivityCompleteCallBack(int activityId, Dictionary<string, string> items)
        {
            //领取成功通知客户端
            MSG_ZGC_SPECIAL_ACTIVITY_COMPLETE msg = new MSG_ZGC_SPECIAL_ACTIVITY_COMPLETE();
            msg.ActivityId = activityId;

            //查看今天是否有这个活动
            SpecialActivityInfo info = ActivityLibrary.GetSpecialActivityInfoById(activityId);
            if (info == null)
            {
                //说明今天没有这个活动
                Log.Warn("player {0} SpecialActivityCompleteCallBack not find activity info: {1}", Uid, activityId);
                msg.Result = (int)ErrorCode.NotExist;
                Write(msg);
                return;
            }
            if (info.OpenStart > ZoneServerApi.now || ZoneServerApi.now > info.OpenEnd)
            {
                //说明时间不正确
                Log.Warn("player {0} SpecialActivityCompleteCallBack activity {1} num data error: start {2} end {3}.", Uid, activityId, info.OpenStart, info.OpenEnd);
                msg.Result = (int)ErrorCode.NotExist;
                Write(msg);
                return;
            }

            //查看身上是否有这个活动
            SpecialActivityItem item = ActivityMng.GetSpecialActivityItemForId(activityId);
            if (item == null)
            {
                //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                item = CreatSpecialActivityItem(activityId);
                ActivityMng.AddSpecialActivityItem(info, item);
            }
            if (item.State == (int)ActivityState.Get)
            {
                //状态1表示已经领取
                Log.Warn("player {0} SpecialActivityCompleteCallBack id {1} error: type {2} state is {3}", Uid, info.Id, info.SpecialType, item.State);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //检查任务完成条件
            if (!CheckSpecialActivityComplete(item, info))
            {
                Log.Warn("player {0} SpecialActivityCompleteCallBack activity id {1} error ", Uid, item.Id);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (items.Count == 0)
            {
                msg.Result = (int)ErrorCode.MaxCount;
            }
            else
            {
                //完成领取状态
                item.SetState(ActivityState.Get);

                //发送邮件
                switch (info.SpecialType)
                {
                    case SpecialAction.SignIn:
                    case SpecialAction.AnyMoney:
                        {
                            foreach (var msgItem in items)
                            {
                                string param = $"{CommonConst.ACCOUNT}:{msgItem.Key}|{CommonConst.PASSWORD}:{msgItem.Value}";
                                SendPersonEmail(info.EmailId, param: param);
                            }
                        }
                        break;
                    case SpecialAction.FixedMoney:
                        {
                            List<string> paramList = new List<string>();
                            foreach (var msgItem in items)
                            {
                                paramList.Add($"{msgItem.Key}-{msgItem.Value}");
                            }
                            string param = $"{CommonConst.ACCOUNT_PASSWORD}:" + string.Join("; ", paramList);
                            SendPersonEmail(info.EmailId, param: param);
                        }
                        break;
                    default:
                        break;
                }

                msg.Result = (int)ErrorCode.Success;
            }

            Write(msg);


            List<SpecialActivityItem> updateList = new List<SpecialActivityItem>() { item };

            SyncDbInsertSpecialActivityItem(updateList);
            //发消息给前台
            SyncSpecialActivityChangeMessage(updateList, null);


            //BIRecordActivityLog(info.SpecialType, activityId);
            //BI 活动
            KomoeEventLogOperationalActivity(activityId, "充值返利", info.SpecialType.ToString(), (int)info.SpecialType, 3, item.CurNum, item.CurNum, "", "", null, "");
        }

        public void CheckSignInSpecialActivity()
        {
            if (LastOfflineTime.Date != ZoneServerApi.now.Date && LastRefreshTime.Date == ZoneServerApi.now.Date)
            {
                AddSpecialActivityNumForType(SpecialAction.SignIn, needSync: false);
                AddRunawayActivityNumForType(RunawayAction.SignIn, needSync: false);
            }
        }

        public bool CheckSpecialActivityComplete(SpecialActivityItem item, SpecialActivityInfo info)
        {
            switch (info.SpecialType)
            {
                case SpecialAction.SignIn:
                    {
                        if (item.CurNum >= info.Params.day)
                        {
                            return true;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.SpecialType, info.Params.day, item.CurNum);
                            return false;
                        }
                    }
                case SpecialAction.AnyMoney:
                    {
                        if (item.CurNum >= info.Params.num)
                        {
                            return true;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.SpecialType, 1, item.CurNum);
                            return false;
                        }
                    }
                case SpecialAction.FixedMoney:
                    {
                        if (item.CurNum >= info.Params.money)
                        {
                            return true;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.SpecialType, info.Params.money, item.CurNum);
                            return false;
                        }
                    }
                default:
                    {
                        Log.Warn("player {0} CheckActivityComplete id {1} error: not find type {2}", Uid, info.Id, info.SpecialType);
                        return false;
                    }
            }
        }

        private bool SpecialActivityAddCurNum(float num, SpecialActivityInfo info, SpecialActivityItem item)
        {
            switch (info.SpecialType)
            {
                case SpecialAction.SignIn:
                    {
                        if (item.CurNum < info.Params.day)
                        {
                            item.CurNum += (int)num;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case SpecialAction.AnyMoney:
                    {
                        if (item.CurNum < info.Params.num)
                        {
                            item.CurNum = (int)num;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case SpecialAction.FixedMoney:
                    {
                        if (item.CurNum < info.Params.money)
                        {
                            item.CurNum = Math.Min(item.CurNum + num, info.Params.money);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }

        public void SyncDbInsertSpecialActivityItem(List<SpecialActivityItem> addList)
        {
            if (addList.Count > 0)
            {
                server.GameDBPool.Call(new QueryInsertSpecialActivityInfo(Uid, addList));
            }
        }

        public void SyncDbDeleteSpecialActivityItem(List<int> ids)
        {
            if (ids.Count > 0)
            {
                server.GameDBPool.Call(new QueryDeleteSpecialActivityInfo(Uid, ids));
            }
        }

        private static SpecialActivityItem CreatSpecialActivityItem(int activityId)
        {
            SpecialActivityItem item = new SpecialActivityItem();
            item.Id = activityId;
            item.CurNum = 0;
            return item;
        }

        public void SendSpecialActivityListMessage()
        {
            if (server.ManagerServer.TotalCount - server.ManagerServer.UseCount > 0)
            {
                MSG_ZGC_SPECIAL_ACTIVITY_MANAGER msg = new MSG_ZGC_SPECIAL_ACTIVITY_MANAGER();
                Dictionary<int, SpecialActivityItem> activityList = ActivityMng.GetSpecialActivityList();
                foreach (var activity in activityList)
                {
                    msg.List.Add(GetActivityInfo(activity.Value));
                }
                msg.TotalCount = server.ManagerServer.TotalCount;
                msg.UseCount = server.ManagerServer.UseCount;
                Write(msg);
            }
        }
        private MSG_ZGC_SPECIAL_ACTIVITY_INFO GetActivityInfo(SpecialActivityItem item)
        {
            MSG_ZGC_SPECIAL_ACTIVITY_INFO info = new MSG_ZGC_SPECIAL_ACTIVITY_INFO();
            info.Id = item.Id;
            info.CurNum = item.CurNum;
            info.State = item.State;
            return info;
        }
        #endregion


        #region 流失干预

        public void LoadRunawayActivityList(int runawayType, int runawayTime, string dataBox, List<RunawayActivityItem> list)
        {
            ActivityMng.RunawayType = runawayType;
            ActivityMng.RunawayTime = runawayTime;
            ActivityMng.DataBox = dataBox;

            if (runawayType > 0)
            {
                //活动开启
                if (runawayTime > 0)
                {
                    DateTime time = Timestamp.TimeStampToDateTime(runawayTime);
                    int dayCount = GetRunawayActivityDurringTime();
                    double passDay = (ZoneServerApi.now.Date - time).TotalDays;
                    if (passDay > dayCount)
                    {
                        //超过活动时间关闭，清理数据
                        if (list.Count > 0)
                        {
                            //活动哦已经结束，删除数据
                            List<int> removeList = new List<int>();
                            foreach (var item in list)
                            {
                                //活动已过期
                                removeList.Add(item.Id);
                            }
                            SyncDbDeleteRunAwayActivityItem(removeList);
                        }
                    }
                    else
                    {
                        foreach (var item in list)
                        {
                            ActivityMng.AddRunawayActivityItem(item);
                        }
                    }
                }
            }
            else
            {
                //活动未开始
                if (list.Count > 0)
                {
                    //活动哦已经结束，删除数据
                    List<int> removeList = new List<int>();
                    foreach (var item in list)
                    {
                        //活动已过期
                        removeList.Add(item.Id);
                    }
                    SyncDbDeleteRunAwayActivityItem(removeList);
                }
            }
        }


        private void CheckRunawayActivity()
        {
            if (ActivityMng.RunawayType > 0)
            {
                //活动开启
                if (ActivityMng.RunawayTime == 0)
                {
                    if (CheckLimitOpen(LimitType.RunAway))
                    {
                        RunawayActivityOpen(ActivityMng.DataBox);
                        SendRunawayActivityListMessage();
                    }
                }
                else
                {
                    DateTime time = Timestamp.TimeStampToDateTime(ActivityMng.RunawayTime);
                    int dayCount = GetRunawayActivityDurringTime();
                    double passDay = (BaseApi.now.Date - time).TotalDays;
                    if (passDay > dayCount)
                    {
                        if (passDay >= ActivityLibrary.RunawayActivityNextTime)
                        {
                            ActivityMng.RunawayType = 0;
                            ActivityMng.RunawayTime = 0;
                            SyncDbUpdateRunAwayActivity();

                            GetRunawayType();
                        }
                    }
                    else
                    {
                        SendRunawayActivityListMessage();
                    }
                }
            }
            else
            {
                if (ActivityMng.RunawayTime > 0)
                {
                    ActivityMng.RunawayTime = 0;
                    SyncDbUpdateRunAwayActivity();
                }
                GetRunawayType();
            }
        }

        private void GetRunawayType()
        {
            MSG_ZM_GET_RUNAWA_TYPE msg = new MSG_ZM_GET_RUNAWA_TYPE()
            {
                Account = AccountName,
                Uid = uid,
                ServerId = server.MainId.ToString(),
                GameId = GameId,
            };
            server.ManagerServer.Write(msg);
        }

        public void GetRunawayTypeReturn(int runawayType, string interveneId, string dataBox)
        {
            ActivityMng.RunawayType = runawayType;
            ActivityMng.RunawayTime = 0;
            ActivityMng.DataBox = dataBox;

            if (CheckLimitOpen(LimitType.RunAway))
            {
                RunawayActivityOpen(dataBox);

                SendRunawayActivityListMessage();
            }
        }

        public void LoadRunawayActivityTransform(RepeatedField<MSG_ZMZ_RUNAWAY_ACTIVITY_INFO> activitys)
        {
            foreach (var item in activitys)
            {
                RunawayActivityItem activity = CreatRunawayActivityItem(item.Id);
                activity.CurNum = item.CurNum;
                activity.State = item.State;

                //在活动时间内
                ActivityMng.AddRunawayActivityItem(activity);
            }
        }


        public void AddRunawayActivityNumForType(RunawayAction type, int num = 1, bool needSync = true)
        {
            DateTime time = Timestamp.TimeStampToDateTime(ActivityMng.RunawayTime);
            int day = (int)(ZoneServerApi.now.Date - time).TotalDays;
            int dayCount = GetRunawayActivityDurringTime();
            if (day > dayCount)
            {
                return;
            }
            //获取当前类型所有活动
            List<int> list = ActivityLibrary.GetDailyRunAwayItemList(ActivityMng.RunawayType, day + 1, type);
            if (list == null || list.Count == 0)
            {
                return;
            }

            List<RunawayActivityItem> updateList = new List<RunawayActivityItem>();

            foreach (var activityId in list)
            {
                AddRunawayActivityNumForId(activityId, num, updateList);
            }

            if (updateList.Count > 0)
            {
                SyncDbInsertRunawayActivityItem(updateList);

                if (needSync)
                {
                    //发消息给前台
                    SyncRunawayActivityChangeMessage(updateList, null);
                }
            }
        }


        public void RunawayActivityComplete(int activityId)
        {
            //领取成功通知客户端
            MSG_ZGC_RUNAWAY_ACTIVITY_COMPLETE msg = new MSG_ZGC_RUNAWAY_ACTIVITY_COMPLETE();
            msg.ActivityId = activityId;

            //查看今天是否有这个活动
            RunawayActivityInfo info = ActivityLibrary.GetRunawayActivityInfoById(activityId);
            if (info == null)
            {
                //说明今天没有这个活动
                Log.Warn("player {0} RunawayActivityComplete not find activity info: {1}", Uid, activityId);
                msg.Result = (int)ErrorCode.NotExist;
                Write(msg);
                return;
            }

            //查看身上是否有这个活动
            RunawayActivityItem item = ActivityMng.GetRunawayActivityItemForId(activityId);
            if (item == null)
            {
                //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                item = CreatRunawayActivityItem(activityId);
                ActivityMng.AddRunawayActivityItem(item);
            }
            if (item.State == (int)ActivityState.Get)
            {
                //状态1表示已经领取
                Log.Warn("player {0} RunawayActivityComplete id {1} error: type {2} state is {3}", Uid, info.Id, info.RunawayType, item.State);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //检查任务完成条件
            if (!CheckRunawayActivityComplete(item, info))
            {
                Log.Warn("player {0} RunawayActivityComplete activity id {1} error ", Uid, item.Id);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //完成领取状态
            item.SetState(ActivityState.Get);

            RewardManager manager = new RewardManager();
            string rewardString = info.Reward;
            if (!string.IsNullOrEmpty(rewardString))
            {
                //获取任务奖励
                manager = GetSimpleReward(rewardString, ObtainWay.Activity);
                manager.GenerateRewardItemInfo(msg.Rewards);
            }


            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            List<RunawayActivityItem> updateList = new List<RunawayActivityItem>() { item };

            SyncDbInsertRunawayActivityItem(updateList);
            // 发消息给前台
            SyncRunawayActivityChangeMessage(updateList, null);

            //BI 活动
            KomoeEventLogOperationalActivity(activityId, "斗罗奇遇", info.RunawayType.ToString(), (int)info.RunawayType, 3, item.CurNum, item.CurNum, "", "", null, "");

            KomoeEventLogInterventionActivity(activityId, "斗罗奇遇", info.RunawayType.ToString(),
                   ActivityMng.RunawayType, 2, info.Day, manager.GetRewardDic(), ActivityMng.DataBox);
        }

        public void SyncRunawayActivityChangeMessage(List<RunawayActivityItem> updateList, List<int> removeList)
        {
            MSG_ZGC_RUNAWAY_ACTIVITY_CHANGE msg = new MSG_ZGC_RUNAWAY_ACTIVITY_CHANGE();
            if (updateList != null)
            {
                foreach (var item in updateList)
                {
                    msg.UpdateList.Add(GetActivityInfo(item));
                }
            }
            if (removeList != null)
            {
                foreach (var task in removeList)
                {
                    msg.RemoveList.Add(task);
                }
            }
            Write(msg);
        }

        public bool CheckRunawayActivityComplete(RunawayActivityItem item, RunawayActivityInfo info)
        {
            switch (info.RunawayType)
            {
                case RunawayAction.OnlinTime:
                    {
                        if (item.CurNum >= info.Params.num)
                        {
                            return true;
                        }
                        else
                        {
                            int addMinutes = (int)(ZoneServerApi.now - LastLoginTime).TotalMinutes;
                            if (item.CurNum + addMinutes >= info.Params.num)
                            {
                                item.CurNum = Math.Min(item.CurNum + addMinutes, info.Params.num);
                                return true;
                            }
                            else
                            {
                                Log.Warn("player {0} CheckRunawayActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.RunawayType, info.Params.num, item.CurNum);
                                return false;
                            }
                        }
                    }
                case RunawayAction.SignIn:
                case RunawayAction.DailyTask:
                case RunawayAction.IntegralBoss:
                case RunawayAction.Fight:
                case RunawayAction.Hunting:
                case RunawayAction.Delegation:
                case RunawayAction.Aren:
                case RunawayAction.Onhook:
                    {
                        if (item.CurNum >= info.Params.num)
                        {
                            return true;
                        }
                        else
                        {
                            Log.Warn("player {0} CheckRunawayActivityComplete id {1} error: type {2} num is {3} cur is {4}", Uid, info.Id, info.RunawayType, info.Params.num, item.CurNum);
                            return false;
                        }
                    }
                default:
                    {
                        Log.Warn("player {0} CheckRunawayActivityComplete id {1} error: not find type {2}", Uid, info.Id, info.RunawayType);
                        return false;
                    }
            }
        }
        private void AddRunawayActivityNumForId(int activityId, int num, List<RunawayActivityItem> updateList)
        {
            bool isInsert = false;

            //查看今天是否有这个活动
            RunawayActivityInfo info = ActivityLibrary.GetRunawayActivityInfoById(activityId);
            if (info == null)
            {
                //说明今天没有这个活动
                Log.Warn("player {0} add runaway activity {1} num not find activity info.", Uid, activityId);
                return;
            }

            RunawayActivityItem item = ActivityMng.GetRunawayActivityItemForId(activityId);
            if (item == null)
            {
                //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                item = CreatRunawayActivityItem(activityId);
                isInsert = true;
            }
            else
            {
                if (item.State == (int)ActivityState.Get)
                {
                    //状态1表示已经领取
                    return;
                }
            }

            if (RunawayActivityAddCurNum(num, info, item))
            {
                if (isInsert)
                {
                    ActivityMng.AddRunawayActivityItem(item);
                }

                updateList.Add(item);

                if (!CheckRunawayActivityComplete(item, info))
                {
                    KomoeEventLogInterventionActivity(activityId, "斗罗奇遇", info.RunawayType.ToString(),
                        ActivityMng.RunawayType, 1, info.Day, new List<Dictionary<string, object>>(), ActivityMng.DataBox);
                }
            }
        }

        private bool RunawayActivityAddCurNum(int num, RunawayActivityInfo info, RunawayActivityItem item)
        {
            switch (info.RunawayType)
            {
                default:
                    {
                        if (item.CurNum < info.Params.num)
                        {
                            //item.CurNum += (int)num;
                            item.CurNum = Math.Min(item.CurNum + num, info.Params.num);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
            }
        }

        private static RunawayActivityItem CreatRunawayActivityItem(int activityId)
        {
            RunawayActivityItem item = new RunawayActivityItem();
            item.Id = activityId;
            item.CurNum = 0;
            return item;
        }

        private void RunawayActivityOpen(string dataBox)
        {
            ActivityMng.RunawayTime = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now.Date);

            SyncDbUpdateRunAwayActivity();

            AddRunawayActivityNumForType(RunawayAction.SignIn, needSync: false);

            KomoeEventLogInterventionActivity(100100, "斗罗奇遇", RunawayAction.Open.ToString(),
                        ActivityMng.RunawayType, 0, 0, new List<Dictionary<string, object>>(), dataBox);
        }

        public void SyncDbInsertRunawayActivityItem(List<RunawayActivityItem> addList)
        {
            if (addList.Count > 0)
            {
                server.GameDBPool.Call(new QueryInsertRunawayActivityInfo(Uid, addList));
            }
        }

        public void SyncDbUpdateRunAwayActivity()
        {
            server.GameDBPool.Call(new QueryUpdateRunawayActivity(Uid, ActivityMng.RunawayType, ActivityMng.RunawayTime, ActivityMng.DataBox));
        }

        public void SyncDbDeleteRunAwayActivityItem(List<int> ids)
        {
            if (ids.Count > 0)
            {
                server.GameDBPool.Call(new QueryDeleteRunAwayActivityInfo(Uid, ids));
            }
        }

        public void SendRunawayActivityListMessage()
        {
            DateTime time = Timestamp.TimeStampToDateTime(ActivityMng.RunawayTime);
            int dayCount = GetRunawayActivityDurringTime();
            if ((ZoneServerApi.now.Date - time).TotalDays < dayCount)
            {
                MSG_ZGC_RUNAWAY_ACTIVITY_MANAGER msg = new MSG_ZGC_RUNAWAY_ACTIVITY_MANAGER();
                Dictionary<int, RunawayActivityItem> activityList = ActivityMng.GetRunawayActivityList();
                foreach (var activity in activityList)
                {
                    msg.List.Add(GetActivityInfo(activity.Value));
                }
                msg.OpenType = ActivityMng.RunawayType;
                msg.OpenTime = ActivityMng.RunawayTime;
                Write(msg);
            }
        }

        private MSG_ZGC_RUNAWAY_ACTIVITY_INFO GetActivityInfo(RunawayActivityItem item)
        {
            MSG_ZGC_RUNAWAY_ACTIVITY_INFO info = new MSG_ZGC_RUNAWAY_ACTIVITY_INFO();
            info.Id = item.Id;
            info.CurNum = item.CurNum;
            info.State = item.State;
            return info;
        }

        public int GetRunawayActivityDurringTime()
        {
            Dictionary<int, Dictionary<RunawayAction, List<int>>> dic = ActivityLibrary.GetRunAwayTypeList(ActivityMng.RunawayType);
            if (dic != null)
            {
                return dic.Count;
            }
            else
            {
                return 0;
            }
        }
        #endregion


        #region 网页支付充值返利
        public void LoadWebPayRebateActivityLit(List<WebPayRebateItem> list, Dictionary<int, float> moneyDic, Dictionary<int, int> loginMarkDic)
        {
            ActivityMng.InitWebPayRechargeMoney(moneyDic);
            ActivityMng.InitWebPayRebateLoginMark(loginMarkDic);

            foreach (var item in list)
            {
                WebPayRebateInfo info = ActivityLibrary.GetWebPayRebateInfo(item.Id);
                if (info == null)
                {
                    continue;
                }
                ActivityMng.AddWebPayRebateItem(item);
            }
            //没有的需要新加
            CheckAddWebPayRechargeRebateInfo();
        }

        public void CheckSendWebPayRechargeRebateInfo(DateTime lastLoginTime, bool refresh = false)
        {
            if (refresh)
            {
                CheckAddWebPayRechargeRebateInfo();
            }

            OverSeasActivityTable curActivity = ActivityLibrary.GetCurrentOverSeasActivityModel(OverseasActivityType.WebPayRebate, ZoneServerApi.now, true);
            if (curActivity != null)
            {
                CheckUpdateWebPayRebateSignInInfo(curActivity, lastLoginTime);
                SendWebPayRechargeRebateInfo(curActivity.SubType);
            }
        }

        private void SendWebPayRechargeRebateInfo(int payMode)
        {
            MSG_ZGC_WEBPAY_RECHARGE_REBATE msg = new MSG_ZGC_WEBPAY_RECHARGE_REBATE();
            msg.Money = ActivityMng.GetWebPayRebateRechargeMoney(payMode);
            Dictionary<int, WebPayRebateItem> rebateItemList = ActivityMng.GetWebPayRebateItemList();
            foreach (var item in rebateItemList.Where(kv => kv.Value.PayMode == payMode))
            {
                msg.List.Add(GenerateWebPayRabateInfo(item.Value));
            }
            Write(msg);
        }

        private ZGC_WEBPAY_RECHARGE_REBATE GenerateWebPayRabateInfo(WebPayRebateItem item)
        {
            ZGC_WEBPAY_RECHARGE_REBATE info = new ZGC_WEBPAY_RECHARGE_REBATE();
            info.Id = item.Id;
            //foreach (var kv in item.ConditionCurNum)
            //{
            //    info.Param.Add(kv.Key, kv.Value);
            //}          
            info.State = item.isGet ? 1 : 0;
            info.CurNum = item.ConditionCurNum.Values.Count > 0 ? item.ConditionCurNum.Values.First() : 0.0f;
            return info;
        }

        public void GetWebPayRebateReward(int id)
        {
            MSG_ZGC_GET_WEBPAY_REBATE_REWARD response = new MSG_ZGC_GET_WEBPAY_REBATE_REWARD();
            response.Id = id;

            OverSeasActivityTable curActivity = ActivityLibrary.GetCurrentOverSeasActivityModel(OverseasActivityType.WebPayRebate, ZoneServerApi.now, true);
            if (curActivity == null)
            {
                Log.Warn($"player {uid} get webpay rebate reward {id} failed: activity not open");
                response.Result = (int)ErrorCode.NotOnTime;
                Write(response);
                return;
            }

            WebPayRebateInfo info = ActivityLibrary.GetWebPayRebateInfo(id);
            if (info == null)
            {
                Log.Warn($"player {uid} get webpay rebate reward {id} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (info.PayMode != curActivity.SubType)
            {
                Log.Warn($"player {uid} get webpay rebate reward {id} failed: cur activity subType {curActivity.SubType}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (info.GetRewardAfterEnd && ZoneServerApi.now < curActivity.ActivityEnd)
            {
                Log.Warn($"player {uid} get webpay rebate reward {id} failed: activity not end");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            WebPayRebateItem item = ActivityMng.GetWebPayRebateItem(id);
            if (item == null)
            {
                Log.Warn($"player {uid} get webpay rebate reward {id} failed: not find item in memory");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (item.isGet)
            {
                Log.Warn($"player {uid} get webpay rebate reward {id} failed: already got reward");
                response.Result = (int)ErrorCode.AlreadyGot;
                Write(response);
                return;
            }

            float rechargeMoney = ActivityMng.GetWebPayRebateRechargeMoney(info.PayMode);
            if (rechargeMoney <= 0)
            {
                Log.Warn($"player {uid} get webpay rebate reward {id} failed: not recharge");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!CheckWebPayRebateCondition(item, info.ConditionTypes, info.ConditionParams))
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            item.isGet = true;
            SyncDbUpdateWebPayRechargeRebateInfo(item);

            string rewards = "";
            if (!string.IsNullOrEmpty(info.Rewards))
            {
                rewards = info.Rewards;
            }
            if (info.RebateParams != null && !string.IsNullOrEmpty(info.RebateParams.diamond))
            {
                string[] rebateParam = StringSplit.GetArray("_", info.RebateParams.diamond);
                string rebateRewards = item.GetRebateRewardsByType(CommonConst.DIAMOND, rebateParam);
                rewards += "|" + rebateRewards;
            }

            RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewards);
            List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);

            RewardManager manager = new RewardManager();
            manager.AddReward(rewardItems);
            manager.BreakupRewards(true);
            AddRewards(manager, ObtainWay.WebPayRebate);
            manager.GenerateRewardMsg(response.Rewards);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private bool CheckWebPayRebateCondition(WebPayRebateItem item, List<int> conditionTypes, List<SpecialActivityParam> conditionParams)
        {
            int index = 0;
            foreach (int type in conditionTypes)//考虑可能有多个条件
            {
                float curNum = 0;
                switch ((WebPayRebateAction)type)
                {
                    case WebPayRebateAction.Money:
                        item.ConditionCurNum.TryGetValue(CommonConst.MONEY, out curNum);
                        if (conditionParams[index].money == 0)
                        {
                            if (curNum <= conditionParams[index].money)
                            {
                                Log.Warn($"player {uid} get webpay rebate reward {item.Id} failed: money need greater than zero");
                                return false;
                            }
                        }
                        else
                        {
                            if (curNum < conditionParams[index].money)
                            {
                                Log.Warn($"player {uid} get webpay rebate reward {item.Id} failed: money curNum {curNum} limit {conditionParams[index].money}");
                                return false;
                            }
                        }
                        break;
                    case WebPayRebateAction.SignIn:
                        item.ConditionCurNum.TryGetValue(CommonConst.DAY, out curNum);
                        if (curNum < conditionParams[index].day)
                        {
                            Log.Warn($"player {uid} get webpay rebate reward {item.Id} failed: day curNum {curNum} limit {conditionParams[index].day}");
                            return false;
                        }
                        break;
                    case WebPayRebateAction.Diamond:
                        item.ConditionCurNum.TryGetValue(CommonConst.DIAMOND, out curNum);
                        if (curNum < conditionParams[index].diamond)
                        {
                            Log.Warn($"player {uid} get webpay rebate reward {item.Id} failed: diamond curNum {curNum} limit {conditionParams[index].diamond}");
                            return false;
                        }
                        break;
                    default:
                        break;
                }
                index++;
            }
            return true;
        }

        public void UpdateWebPayRebateRechargeInfo(string rewards, float price)
        {
            OverSeasActivityTable curActivity = ActivityLibrary.GetCurrentOverSeasActivityModel(OverseasActivityType.WebPayRebate, ZoneServerApi.now, false);
            if (curActivity == null)
            {
                return;
            }
            int diamond = GetRewardsDiamond(rewards);
            CheckUpdateWebPayRebateRechargeInfo(price, diamond, curActivity.SubType);
            SendWebPayRechargeRebateInfo(curActivity.SubType);
        }

        private int GetRewardsDiamond(string rewards)
        {
            int diamond = 0;
            if (!string.IsNullOrEmpty(rewards))
            {
                string[] rewardsArr = StringSplit.GetArray("|", rewards);
                string[] unitRewardArr;
                foreach (string reward in rewardsArr)
                {
                    unitRewardArr = StringSplit.GetArray(":", reward);
                    if (unitRewardArr[0].ToInt() == (int)CurrenciesType.diamond)
                    {
                        diamond += unitRewardArr[2].ToInt();
                    }
                }
            }
            return diamond;
        }

        private void CheckUpdateWebPayRebateRechargeInfo(float money, int diamond, int payMode)
        {
            Dictionary<int, WebPayRebateItem> itemList = ActivityMng.GetWebPayRebateItemList();
            foreach (var item in itemList.Where(kv => kv.Value.PayMode == payMode))
            {
                WebPayRebateInfo info = ActivityLibrary.GetWebPayRebateInfo(item.Value.Id);
                ActivityMng.UpdateWebPayRebateRechargeInfo(item.Value, info.ConditionParams, money, diamond);
            }
            ActivityMng.UpdateWebPayRebateRechargeMoney(payMode, money);

            Dictionary<int, WebPayRebateItem> updateList = itemList.Where(kv => kv.Value.PayMode == payMode).ToDictionary(kv => kv.Key, kv => kv.Value);
            SyncDbBatchUpdateWebPayRebateInfo(updateList, payMode);
        }

        private void CheckUpdateWebPayRebateSignInInfo(OverSeasActivityTable activity, DateTime lastLoginTime)
        {
            if (ZoneServerApi.now > activity.ActivityEnd)
            {
                return;
            }
            bool update = false;
            int loginMark = ActivityMng.GetWebPayRebateLoginMark(activity.SubType);
            if (loginMark == 0 || !lastLoginTime.Date.Equals(ZoneServerApi.now.Date))
            {
                update = true;
            }
            if (update)
            {
                Dictionary<int, WebPayRebateItem> itemList = ActivityMng.GetWebPayRebateItemList();
                foreach (var item in itemList.Where(kv=>kv.Value.PayMode == activity.SubType))
                {
                    WebPayRebateInfo info = ActivityLibrary.GetWebPayRebateInfo(item.Value.Id);
                    ActivityMng.UpdateWebPayRebateSignInInfo(item.Value, info.ConditionParams);
                }

                ActivityMng.UpdateWebPayRebateLoginMark(activity.SubType, 1);

                Dictionary<int, WebPayRebateItem> updateList = itemList.Where(kv => kv.Value.PayMode == activity.SubType).ToDictionary(kv => kv.Key, kv => kv.Value);
                SyncDbBatchUpdateWebPayRebateInfo(updateList, activity.SubType);
            }
        }

        private void CheckAddWebPayRechargeRebateInfo()
        {
            Dictionary<int, OverSeasActivityTable> activitys = ActivityLibrary.GetOverSeasActivityModelsByType(OverseasActivityType.WebPayRebate);
            if (activitys == null)
            {
                return;
            }

            //没有的需要新加
            List<WebPayRebateItem> insertList = new List<WebPayRebateItem>();
            foreach (var activity in activitys)
            {
                int payMode = activity.Value.SubType;
                Dictionary<int, WebPayRebateInfo> infoList = ActivityLibrary.GetWebPayRebateInfosByType(payMode);
                if (infoList != null)
                {
                    ActivityMng.CheckAddNewWebPayRebateItem(infoList, insertList);
                }
            }

            SyncDbBatchInsertWebPayRechargeRebateInfo(insertList);
        }

        private void SyncDbUpdateWebPayRechargeRebateInfo(WebPayRebateItem item)
        {
            server.GameDBPool.Call(new QueryUpdateWebPayRechargeRebateInfo(Uid, item.Id, item.isGet));
        }

        private void SyncDbBatchUpdateWebPayRebateInfo(Dictionary<int, WebPayRebateItem> itemList, int payMode)
        {
            if (itemList.Count <= 0)
            {
                return;
            }
            float money = ActivityMng.GetWebPayRebateRechargeMoney(payMode);
            int loginMark = ActivityMng.GetWebPayRebateLoginMark(payMode);
            server.GameDBPool.Call(new QueryBatchUpdateWebPayRechargeRebateInfo(Uid, itemList, money, loginMark));
        }

        private void SyncDbDeleteWebPayRechargeRebateInfo(List<int> idList)
        {
            if (idList.Count > 0)
            {
                server.GameDBPool.Call(new QueryDeleteWebPayRechargeRebateInfo(Uid, idList));
            }
        }

        private void SyncDbBatchInsertWebPayRechargeRebateInfo(List<WebPayRebateItem> itemList)
        {
            if (itemList.Count > 0)
            {
                server.GameDBPool.Call(new QueryBatchInsertWebPayRebateInfo(Uid, itemList));
            }
        }

        public void LoadWebpayRechargeRebateTransform(MapField<int, float> moneyDic, MapField<int, int> loginMarkDic, RepeatedField<MSG_ZMZ_WEBPAY_REBATE_INFO> infoList)
        {
            ActivityMng.InitWebPayRechargeMoney(moneyDic);
            ActivityMng.InitWebPayRebateLoginMark(loginMarkDic);

            foreach (var info in infoList)
            {
                WebPayRebateItem item = CreateWebPayRebateItem(info);
                ActivityMng.AddWebPayRebateItem(item);
            }
        }

        private WebPayRebateItem CreateWebPayRebateItem(MSG_ZMZ_WEBPAY_REBATE_INFO info)
        {
            WebPayRebateItem item = new WebPayRebateItem();
            item.Id = info.Id;
            item.isGet = info.IsGet;
            foreach (var kv in info.ConditionCurNum)
            {
                item.ConditionCurNum.Add(kv.Key, kv.Value);
            }
            item.PayMode = info.PayMode;
            return item;
        }
        #endregion
    }
}
