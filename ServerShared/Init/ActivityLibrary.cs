using DataProperty;
using EnumerateUtility;
using EnumerateUtility.Activity;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace ServerShared
{
    public static class ActivityLibrary
    {
        private static Dictionary<int, ActivityInfo> activityInfoList = new Dictionary<int, ActivityInfo>();
        private static Dictionary<int, DailyAccumulateGetInfo> dailyAccumulateGetInfoList = new Dictionary<int, DailyAccumulateGetInfo>();
        private static Dictionary<int, DailyRewardInfo> dailyRewardInfoList = new Dictionary<int, DailyRewardInfo>();

        /// <summary>
        /// 维护活动类型关系表
        /// </summary>
        private static Dictionary<ActivityAction, List<int>> todayActivityTypeList = new Dictionary<ActivityAction, List<int>>();
        public static List<int> OnlineRewardOnceList = new List<int>();


        private static Dictionary<int, SpecialActivityInfo> specialInfoList = new Dictionary<int, SpecialActivityInfo>();
        public static Dictionary<SpecialAction, List<int>> todaySpecialTypeList = new Dictionary<SpecialAction, List<int>>();

        private static Dictionary<int, RunawayActivityInfo> runAwayInfoList = new Dictionary<int, RunawayActivityInfo>();
        public static Dictionary<int, Dictionary<int, Dictionary<RunawayAction, List<int>>>> runAwayTypeList = new Dictionary<int, Dictionary<int, Dictionary<RunawayAction, List<int>>>>();

        private static Dictionary<int, WebPayRebateInfo> webPayRebateInfoList = new Dictionary<int, WebPayRebateInfo>();
        private static DoubleDepthMap<int, int, WebPayRebateInfo> webPayRebateInfoByType = new DoubleDepthMap<int, int, WebPayRebateInfo>();
        private static DoubleDepthMap<int, int, OverSeasActivityTable> overseasActivityList = new DoubleDepthMap<int, int, OverSeasActivityTable>();

        private static float CheckRefreshTime { get; set; }
        public static int RunawayActivityNextTime { get; set; }
        public static int RunawayActivityDurringTime { get; set; }
        public static void Init(DateTime openServer)
        {
            InitActivityConfig();

            BindActivityInfo(openServer);

            BindSpecialInfo();

            BindRunAwayInfo();

            BindDailyAccumulateGetInfo();

            BindDailySignInReward();

            BindWebPayRechargeRebateInfo();

            BindOverSeasActivityTable();
        }

        private static void InitActivityConfig()
        {
            DataList dataList = DataListManager.inst.GetDataList("ActivityConfig");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CheckRefreshTime = data.GetFloat("CheckRefreshTime");
                RunawayActivityNextTime = data.GetInt("RunawayActivityNextTime");
                RunawayActivityDurringTime = data.GetInt("RunawayActivityDurringTime");
            }
        }
        public static void BindActivityInfo(DateTime openServer)
        {
            Dictionary<int, ActivityInfo> activityInfoList = new Dictionary<int, ActivityInfo>();

            DataList dataList = DataListManager.inst.GetDataList("ActivityInfo");
            foreach (var item in dataList)
            {
                //初始化活动基本信息
                ActivityInfo activity = new ActivityInfo(item.Value, openServer);
                activityInfoList.Add(activity.Id, activity);
            }

            ActivityLibrary.activityInfoList = activityInfoList;
            RefreshTodayActivityList(DateTime.Now);
        }

        public static void BindSpecialInfo()
        {
            Dictionary<int, SpecialActivityInfo> specialInfoList = new Dictionary<int, SpecialActivityInfo>();

            JavaScriptSerializer json = new JavaScriptSerializer();
            DataList dataList = DataListManager.inst.GetDataList("SpecialActivityInfo");
            foreach (var item in dataList)
            {
                //初始化活动基本信息
                SpecialActivityInfo activity = new SpecialActivityInfo(item.Value);
                activity.Params = json.Deserialize<SpecialActivityParam>(item.Value.GetString("Param"));
                specialInfoList.Add(activity.Id, activity);
            }

            ActivityLibrary.specialInfoList = specialInfoList;
        }

        public static void BindRunAwayInfo()
        {
            Dictionary<int, RunawayActivityInfo> runAwayInfoList = new Dictionary<int, RunawayActivityInfo>();
            Dictionary<int, Dictionary<int, Dictionary<RunawayAction, List<int>>>> runAwayTypeList = new Dictionary<int, Dictionary<int, Dictionary<RunawayAction, List<int>>>>();

            Dictionary<int, Dictionary<RunawayAction, List<int>>> typeDic;
            Dictionary<RunawayAction, List<int>> dailyDic;
            List<int> list;

            JavaScriptSerializer json = new JavaScriptSerializer();
            DataList dataList = DataListManager.inst.GetDataList("RunAwayActivityInfo");
            foreach (var item in dataList)
            {
                //初始化活动基本信息
                RunawayActivityInfo activity = new RunawayActivityInfo(item.Value);
                activity.Params = json.Deserialize<SpecialActivityParam>(item.Value.GetString("Param"));
                runAwayInfoList.Add(activity.Id, activity);

                if (runAwayTypeList.TryGetValue(activity.Type, out typeDic))
                {
                    dailyDic = GetDailyRunAwayList(typeDic, activity.Day);
                    if (dailyDic != null)
                    {
                        list = GetRunAwayActionList(dailyDic, activity.RunawayType);
                        if (list != null)
                        {
                            list.Add(activity.Id);
                        }
                        else
                        {
                            list = new List<int>();
                            list.Add(activity.Id);
                            dailyDic.Add(activity.RunawayType, list);
                        }
                    }
                    else
                    {
                        dailyDic = new Dictionary<RunawayAction, List<int>>();
                        list = new List<int>();
                        list.Add(activity.Id);
                        dailyDic.Add(activity.RunawayType, list);
                        typeDic.Add(activity.Day, dailyDic);
                    }
                }
                else
                {
                    typeDic = new Dictionary<int, Dictionary<RunawayAction, List<int>>>();
                    dailyDic = new Dictionary<RunawayAction, List<int>>();
                    list = new List<int>();
                    list.Add(activity.Id);
                    dailyDic.Add(activity.RunawayType, list);
                    typeDic.Add(activity.Day, dailyDic);
                    runAwayTypeList.Add(activity.Type, typeDic);
                }
            }

            ActivityLibrary.runAwayInfoList = runAwayInfoList;
            ActivityLibrary.runAwayTypeList = runAwayTypeList;
        }

        public static List<int> GetRunAwayActionList(Dictionary<RunawayAction, List<int>> dic, RunawayAction action)
        {
            List<int> activity;
            dic.TryGetValue(action, out activity);
            return activity;
        }

        public static Dictionary<RunawayAction, List<int>> GetDailyRunAwayList(Dictionary<int, Dictionary<RunawayAction, List<int>>> dic, int day)
        {
            Dictionary<RunawayAction, List<int>> activity;
            dic.TryGetValue(day, out activity);
            return activity;
        }

        public static Dictionary<int, Dictionary<RunawayAction, List<int>>> GetRunAwayTypeList(int typeId)
        {
            Dictionary<int, Dictionary<RunawayAction, List<int>>> activity;
            runAwayTypeList.TryGetValue(typeId, out activity);
            return activity;
        }

        public static Dictionary<RunawayAction, List<int>> GetDailyRunAwayList(int typeId, int day)
        {
            Dictionary<int, Dictionary<RunawayAction, List<int>>> dic = GetRunAwayTypeList(typeId);
            if (dic != null)
            {
                return GetDailyRunAwayList(dic, day);
            }
            return null;
        }

        public static List<int> GetDailyRunAwayItemList(int typeId, int day, RunawayAction type)
        {
            Dictionary<int, Dictionary<RunawayAction, List<int>>> dic = GetRunAwayTypeList(typeId);
            if (dic != null)
            {
                Dictionary<RunawayAction, List<int>> dailyDic = GetDailyRunAwayList(dic, day);
                if (dailyDic != null)
                {
                    return GetRunAwayActionList(dailyDic, type);
                }
            }
            return null;
        }

        public static RunawayActivityInfo GetRunawayActivityInfoById(int id)
        {
            RunawayActivityInfo activity;
            runAwayInfoList.TryGetValue(id, out activity);
            return activity;
        }

        public static void BindDailyAccumulateGetInfo()
        {
            Dictionary<int, DailyAccumulateGetInfo> dailyAccumulateGetInfoList = new Dictionary<int, DailyAccumulateGetInfo>();

            DataList dataList = DataListManager.inst.GetDataList("DailyAccumulateGet");
            foreach (var item in dataList)
            {
                //初始化活动基本信息
                DailyAccumulateGetInfo activity = new DailyAccumulateGetInfo(item.Value);
                dailyAccumulateGetInfoList.Add(activity.Id, activity);
            }
            ActivityLibrary.dailyAccumulateGetInfoList = dailyAccumulateGetInfoList;
        }

        public static void BindDailySignInReward()
        {
            Dictionary<int, DailyRewardInfo> dailyRewardInfoList = new Dictionary<int, DailyRewardInfo>();

            DataList dataList = DataListManager.inst.GetDataList("DailySignInReward");
            foreach (var item in dataList)
            {
                //初始化活动基本信息
                DailyRewardInfo activity = new DailyRewardInfo(item.Value);
                dailyRewardInfoList.Add(activity.Id, activity);
            }
            
            ActivityLibrary.dailyRewardInfoList = dailyRewardInfoList;
        }

        public static DailyRewardInfo GetDailyRewardInfo(int id)
        {
            DailyRewardInfo activity;
            dailyRewardInfoList.TryGetValue(id, out activity);
            return activity;
        }

        public static DailyRewardInfo GetDailyRewardInfoByDay(int day)
        {
            DailyRewardInfo activity = null;
            foreach (var item in dailyRewardInfoList)
            {
                activity = item.Value;
                if (activity.Day > day)
                {
                    break;
                }
            }
            return activity;
        }

        public static DailyAccumulateGetInfo GetDailyAccumulateGetInfo(int id)
        {
            DailyAccumulateGetInfo activity;
            dailyAccumulateGetInfoList.TryGetValue(id, out activity);
            return activity;
        }

        //private static void AddDailyRefreshId(int id)
        //{
        //    todayDailyRefreshIds.Add(id, 1);
        //}

        //public static bool CheckDailyRefreshActivity(int id)
        //{
        //    return todayDailyRefreshIds.ContainsKey(id);
        //}

        public static ActivityInfo GetActivityInfoById(int id)
        {
            ActivityInfo activity;
            activityInfoList.TryGetValue(id, out activity);
            return activity;
        }

        public static SpecialActivityInfo GetSpecialActivityInfoById(int id)
        {
            SpecialActivityInfo activity;
            specialInfoList.TryGetValue(id, out activity);
            return activity;
        }

        public static ActivityInfo GetRelatedActivityInfoById(int id)
        {
            ActivityInfo activity;
            ActivityInfo ans = null;
            activityInfoList.TryGetValue(id, out activity);
            if (activity != null)
            {
                int relatedId = activity.RelatedId;
                activityInfoList.TryGetValue(relatedId, out ans);
            }
            return ans;
        }

        private static DateTime latRefreshTime = DateTime.Now.AddDays(-1);

        public static void RefreshTodayActivityList(DateTime now, bool checkTime = false)
        {
            if (checkTime)
            {
                if ((now - latRefreshTime).TotalMinutes < CheckRefreshTime)
                {
                    return;
                }
                else
                {
                    latRefreshTime = now;
                }
            }
            Dictionary<ActivityAction, List<int>> todayActivityTypeList = new Dictionary<ActivityAction, List<int>>();

            List<int> OnlineRewardOnceList = new List<int>();
            
            foreach (var item in activityInfoList)
            {
                if (item.Value.Type == ActivityAction.OnlineRewardOnce)
                {
                    OnlineRewardOnceList.Add(item.Key);
                }
                else
                {
                    if (item.Value.IsEveryDate)
                    {
                        AddTaskTypeItem(todayActivityTypeList, item.Value.Type, item.Key);
                    }
                    else
                    {
                        if (item.Value.StartDate.Date <= now.Date && now < item.Value.EndDate)
                        {
                            AddTaskTypeItem(todayActivityTypeList, item.Value.Type, item.Key);
                        }
                    }
                }
            }

            Dictionary<SpecialAction, List<int>> todaySpecialTypeList = new Dictionary<SpecialAction, List<int>>();
            foreach (var item in specialInfoList)
            {
                if (item.Value.OpenStart.Date <= now.Date && now <= item.Value.OpenEnd)
                {
                    List<int> list;
                    if (todaySpecialTypeList.TryGetValue(item.Value.SpecialType, out list))
                    {
                        list.Add(item.Key);
                    }
                    else
                    {
                        list = new List<int>();
                        list.Add(item.Key);
                        todaySpecialTypeList.Add(item.Value.SpecialType, list);
                    }
                }
            }
            ActivityLibrary.todaySpecialTypeList = todaySpecialTypeList;
            ActivityLibrary.OnlineRewardOnceList = OnlineRewardOnceList;
            ActivityLibrary.todayActivityTypeList = todayActivityTypeList;
        }

        public static void AddTaskTypeItem(Dictionary<ActivityAction, List<int>> todayActivityTypeList, ActivityAction type, int id)
        {
            List<int> list;
            if (todayActivityTypeList.TryGetValue(type, out list))
            {
                list.Add(id);
            }
            else
            {
                list = new List<int>();
                list.Add(id);
                todayActivityTypeList.Add(type, list);
            }
        }

        //public static ActivityInfo GetTodayActivityInfoById(int id)
        //{
        //    ActivityInfo activity;
        //    todayActivityList.TryGetValue(id, out activity);
        //    return activity;
        //}

        public static List<int> GetTodayActivityItemForType(ActivityAction type)
        {
            List<int> list;
            todayActivityTypeList.TryGetValue(type, out list);
            return list;
        }

        public static List<int> GetTodaySpecialItemForType(SpecialAction type)
        {
            List<int> list;
            todaySpecialTypeList.TryGetValue(type, out list);
            return list;
        }

        public static Dictionary<ActivityAction, List<int>> GetTodayActivityList()
        {
            return todayActivityTypeList;
        }

        public static bool CheckTodayActivity(ActivityAction type, int id)
        {
            if (OnlineRewardOnceList.Contains(id))
            {
                return true;
            }
            List<int> list = GetTodayActivityItemForType(type);
            if (list != null)
            {
                if (list.Contains(id))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #region 网页支付返利
        private static void BindWebPayRechargeRebateInfo()
        {
            Dictionary<int, WebPayRebateInfo> webPayRebateInfoList = new Dictionary<int, WebPayRebateInfo>();
            DoubleDepthMap<int, int, WebPayRebateInfo> webPayRebateInfoByType = new DoubleDepthMap<int, int, WebPayRebateInfo>();

            JavaScriptSerializer json = new JavaScriptSerializer();
            DataList dataList = DataListManager.inst.GetDataList("WebPayRechargeRabate");
            foreach (var item in dataList)
            {
                //初始化活动基本信息
                WebPayRebateInfo info = new WebPayRebateInfo(item.Value);
                foreach (string conditionParam in info.ConditionParamArr)
                {
                    SpecialActivityParam param = json.Deserialize<SpecialActivityParam>(conditionParam);
                    if (param == null)
                    {
                        continue;
                    }
                    info.ConditionParams.Add(param);
                }
                info.RebateParams = json.Deserialize<RechargeRebateParam>(item.Value.GetString("RebateParam"));
                webPayRebateInfoList.Add(info.Id, info);
                webPayRebateInfoByType.Add(info.PayMode, info.Id, info);
            }

            ActivityLibrary.webPayRebateInfoList = webPayRebateInfoList;
            ActivityLibrary.webPayRebateInfoByType = webPayRebateInfoByType;
        }

        private static void BindOverSeasActivityTable()
        {
            DoubleDepthMap<int, int, OverSeasActivityTable> overseasActivityList = new DoubleDepthMap<int, int, OverSeasActivityTable>();
           
            DataList dataList = DataListManager.inst.GetDataList("OverSeasActivityTable");
            foreach (var item in dataList)
            {
                OverSeasActivityTable activity = new OverSeasActivityTable(item.Value);
                overseasActivityList.Add(activity.MainType, activity.SubType, activity);
            }

            ActivityLibrary.overseasActivityList = overseasActivityList;
        }

        public static OverSeasActivityTable GetCurrentOverSeasActivityModel(OverseasActivityType mainType, DateTime now, bool showEnd)
        {
            Dictionary<int, OverSeasActivityTable> activityDic;
            overseasActivityList.TryGetValue((int)mainType, out activityDic);
            if (activityDic == null)
            {
                return null;
            }
            foreach (var item in activityDic)
            {
                if (!showEnd)
                {
                    if (now >= item.Value.ActivityStart && now <= item.Value.ActivityEnd)
                    {
                        return item.Value;
                    }
                }
                else
                {
                    if (now >= item.Value.ActivityStart && now <= item.Value.TabShowEnd)
                    {
                        return item.Value;
                    }
                }
            }
            return null;
        }

        public static Dictionary<int, OverSeasActivityTable> GetOverSeasActivityModelsByType(OverseasActivityType mainType)
        {
            Dictionary<int, OverSeasActivityTable> activityDic;
            overseasActivityList.TryGetValue((int)mainType, out activityDic);
            return activityDic;
        }

        public static WebPayRebateInfo GetWebPayRebateInfo(int id)
        {
            WebPayRebateInfo info;
            webPayRebateInfoList.TryGetValue(id, out info);
            return info;
        }

        public static Dictionary<int, WebPayRebateInfo> GetWebPayRebateInfosByType(int payMode)
        {
            Dictionary<int, WebPayRebateInfo> dic;
            webPayRebateInfoByType.TryGetValue(payMode, out dic);
            return dic;
        }
        #endregion
    }
}
