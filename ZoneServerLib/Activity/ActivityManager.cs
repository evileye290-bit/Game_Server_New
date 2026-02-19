using CommonUtility;
using EnumerateUtility.Activity;
using ServerModels;
using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;

namespace ZoneServerLib
{
    public class ActivityManager
    {
        //private ZoneServerApi server { get; set; }
        /// <summary>
        /// 所有活动
        /// </summary>
        Dictionary<int, ActivityItem> activityItemList = new Dictionary<int, ActivityItem>();
        /// <summary>
        /// 维护活动类型关系表
        /// </summary>
        Dictionary<ActivityAction, List<int>> activityTypeList = new Dictionary<ActivityAction, List<int>>();

        /// <summary>
        /// 所有活动
        /// </summary>
        Dictionary<int, SpecialActivityItem> specialItemList = new Dictionary<int, SpecialActivityItem>();
        ///// <summary>
        ///// 维护活动类型关系表
        ///// </summary>
        //Dictionary<SpecialAction, List<int>> specialTypeList = new Dictionary<SpecialAction, List<int>>();

        Dictionary<int, RunawayActivityItem> runawayItemList = new Dictionary<int, RunawayActivityItem>();

        private Dictionary<int, float> webPayRebateRechargeMoney = new Dictionary<int, float>();
        private Dictionary<int, int> webPayRebateLoginMark = new Dictionary<int, int>();
        Dictionary<int, WebPayRebateItem> webPayRebateItemList = new Dictionary<int, WebPayRebateItem>();

        public int RunawayType { get; set; }
        public int RunawayTime{ get; set; }
        public string DataBox { get; set; }
        public bool AddActivityItem(ActivityInfo info, ActivityItem item)
        {
            ActivityItem tempTask;
            if (!activityItemList.TryGetValue(item.Id, out tempTask))
            {
                activityItemList.Add(item.Id, item);
                AddTaskTypeItem(info.Type, item.Id);
                return true;
            }
            else
            {
                //有多余任务需要删除
                if (tempTask.CurNum < item.CurNum)
                {
                    //TODO:保留数大的，删除数小的，清除掉 tempTask 添加新的task
                    //不用跟新type list，因为是同类型和id号
                    activityItemList.Remove(item.Id);

                    //TODO:更新数据库，直接更新num值
                    return false;
                }
                else
                {
                    //不添加item,使用原来list中的值
                    return false;
                }
            }
        }

        public void RemoveActivityItem(ActivityAction type, int id)
        {
            List<int> list;
            if (activityTypeList.TryGetValue(type, out list))
            {
                list.Remove(id);
            }

            activityItemList.Remove(id);
        }

        public void RemoveActivityItem(int id)
        {
            activityItemList.Remove(id);

            foreach (var list in activityTypeList)
            {
                foreach (var item in list.Value)
                {
                    if (item == id)
                    {
                        list.Value.Remove(id);
                        return;
                    }
                }
            }
        }

        public void AddTaskTypeItem(ActivityAction type, int id)
        {
            List<int> list;
            if (activityTypeList.TryGetValue(type, out list))
            {
                list.Add(id);
            }
            else
            {
                list = new List<int>();
                list.Add(id);
                activityTypeList.Add(type, list);
            }
        }

        public ActivityItem GetActivityItemForId(int id)
        {
            ActivityItem item;
            activityItemList.TryGetValue(id, out item);
            return item;
        }

        public List<int> GetActivityItemForType(ActivityAction type)
        {
            List<int> list;
            activityTypeList.TryGetValue(type, out list);
            return list;
        }

        public List<int> GetActivityCompleteTypes()
        {
            List<int> actions = new List<int>();
            foreach (var list in activityTypeList)
            {
                bool complete = true;
                list.Value.ForEach(id =>
                {
                    ActivityItem item=GetActivityItemForId(id);
                    if (item != null && item.State != (int)ActivityState.Get)
                    {
                        complete = false;
                    }
                });
                if (complete)
                {
                    actions.Add((int)list.Key);
                }
            }

            return actions;
        }

        public Dictionary<int, ActivityItem> GetActivityList()
        {
            return activityItemList;
        }


        public SpecialActivityItem GetSpecialActivityItemForId(int id)
        {
            SpecialActivityItem item;
            specialItemList.TryGetValue(id, out item);
            return item;
        }
        public bool AddSpecialActivityItem(SpecialActivityInfo info, SpecialActivityItem item)
        {
            SpecialActivityItem tempTask;
            if (!specialItemList.TryGetValue(item.Id, out tempTask))
            {
                specialItemList.Add(item.Id, item);
                //AddSpecialTaskTypeItem(info.SpecialType, item.Id);
                return true;
            }
            else
            {
                //有多余任务需要删除
                if (tempTask.CurNum < item.CurNum)
                {
                    tempTask.CurNum = item.CurNum;
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }

        //public void AddSpecialTaskTypeItem(SpecialAction type, int id)
        //{
        //    List<int> list;
        //    if (specialTypeList.TryGetValue(type, out list))
        //    {
        //        list.Add(id);
        //    }
        //    else
        //    {
        //        list = new List<int>();
        //        list.Add(id);
        //        specialTypeList.Add(type, list);
        //    }
        //}

        public Dictionary<int, SpecialActivityItem> GetSpecialActivityList()
        {
            return specialItemList;
        }

        public bool AddRunawayActivityItem(RunawayActivityItem item)
        {
            RunawayActivityItem tempTask;
            if (!runawayItemList.TryGetValue(item.Id, out tempTask))
            {
                runawayItemList.Add(item.Id, item);
                return true;
            }
            else
            {
                //有多余任务需要删除
                if (tempTask.CurNum < item.CurNum)
                {
                    tempTask.CurNum = item.CurNum;
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }
        public RunawayActivityItem GetRunawayActivityItemForId(int id)
        {
            RunawayActivityItem item;
            runawayItemList.TryGetValue(id, out item);
            return item;
        }

        public Dictionary<int, RunawayActivityItem> GetRunawayActivityList()
        {
            return runawayItemList;
        }

        #region 网页支付返利

        public void AddWebPayRebateItem(WebPayRebateItem item)
        {
            webPayRebateItemList.Add(item.Id, item);
        }

        public void CheckAddNewWebPayRebateItem(Dictionary<int, WebPayRebateInfo> infoList, List<WebPayRebateItem> insertList)
        {
            WebPayRebateItem item;
            foreach (var info in infoList)
            {
                if (!webPayRebateItemList.TryGetValue(info.Key, out item))
                {
                    item = new WebPayRebateItem() { Id = info.Key, PayMode = info.Value.PayMode};
                    webPayRebateItemList.Add(item.Id, item);
                    insertList.Add(item);
                }
            }
        }

        public void InitWebPayRechargeMoney(Dictionary<int, float> moneyDic)
        {
            foreach (var item in moneyDic)
            {
                webPayRebateRechargeMoney.Add(item.Key, item.Value);
            }
        }

        public void InitWebPayRechargeMoney(MapField<int, float> moneyDic)
        {
            foreach (var item in moneyDic)
            {
                webPayRebateRechargeMoney.Add(item.Key, item.Value);
            }
        }

        public void InitWebPayRebateLoginMark(Dictionary<int, int> loginMarkDic)
        {
            foreach (var item in loginMarkDic)
            {
                webPayRebateLoginMark.Add(item.Key, item.Value);
            }
        }

        public void InitWebPayRebateLoginMark(MapField<int, int> loginMarkDic)
        {
            foreach (var item in loginMarkDic)
            {
                webPayRebateLoginMark.Add(item.Key, item.Value);
            }
        }

        public Dictionary<int, WebPayRebateItem> GetWebPayRebateItemList()
        {
            return webPayRebateItemList;
        }

        public Dictionary<int, float> GetWebPayRebateRechargeMoney()
        {
            return webPayRebateRechargeMoney;
        }

        public Dictionary<int, int> GetWebPayRebateLoginMark()
        {
            return webPayRebateLoginMark;
        }

        public WebPayRebateItem GetWebPayRebateItem(int id)
        {
            WebPayRebateItem item;
            webPayRebateItemList.TryGetValue(id, out item);
            return item;
        }

        public void UpdateWebPayRebateRechargeMoney(int payMode, float money)
        {
            if (!webPayRebateRechargeMoney.ContainsKey(payMode))
            {
                webPayRebateRechargeMoney.Add(payMode, money);
            }
            else
            {
                webPayRebateRechargeMoney[payMode] += money;
            }
        }

        public void UpdateWebPayRebateLoginMark(int payMode, int loginMark)
        {
            if (!webPayRebateLoginMark.ContainsKey(payMode))
            {
                webPayRebateLoginMark.Add(payMode, loginMark);
            }
            else
            {
                webPayRebateLoginMark[payMode] = loginMark;
            }
        }

        public void UpdateWebPayRebateRechargeInfo(WebPayRebateItem item, List<SpecialActivityParam> paramList, float money, int diamond)
        {
            foreach (var param in paramList)
            {
                if (money > 0 && param.money > 0)
                {
                    if (!item.ConditionCurNum.ContainsKey(CommonConst.MONEY))
                    {
                        item.ConditionCurNum.Add(CommonConst.MONEY, money);
                    }
                    else
                    {
                        item.ConditionCurNum[CommonConst.MONEY] += money;
                    }
                }
                if (diamond > 0 && param.diamond > 0)
                {
                    if (!item.ConditionCurNum.ContainsKey(CommonConst.DIAMOND))
                    {
                        item.ConditionCurNum.Add(CommonConst.DIAMOND, diamond);
                    }
                    else
                    {
                        item.ConditionCurNum[CommonConst.DIAMOND] += diamond;
                    }
                }
            }
        }

        public void UpdateWebPayRebateSignInInfo(WebPayRebateItem item, List<SpecialActivityParam> paramList)
        {
            foreach (var param in paramList)
            {
                if (param.day > 0)
                {
                    if (!item.ConditionCurNum.ContainsKey(CommonConst.DAY))
                    {
                        item.ConditionCurNum.Add(CommonConst.DAY, 1);
                    }
                    else
                    {
                        item.ConditionCurNum[CommonConst.DAY] += 1;
                    }
                }
            }
        }

        public float GetWebPayRebateRechargeMoney(int payMode)
        {
            float money;
            webPayRebateRechargeMoney.TryGetValue(payMode, out money);
            return money;
        }

        public int GetWebPayRebateLoginMark(int payMode)
        {
            int loginMark;
            webPayRebateLoginMark.TryGetValue(payMode, out loginMark);
            return loginMark;
        }
        #endregion
    }
}
