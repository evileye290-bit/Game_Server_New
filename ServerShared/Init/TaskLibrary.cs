using System;
using DataProperty;
using EnumerateUtility.Activity;
using ServerModels;
using System.Collections.Generic;
using System.Web.SessionState;

namespace ServerShared
{
    public class StartEndDatePair
    { 
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    
    public static class TaskLibrary
    {
        private static Dictionary<int, TaskInfo> taskInfoList = new Dictionary<int, TaskInfo>();

        private static Dictionary<int, Dictionary<int,int>> nextTaskIds = new Dictionary<int, Dictionary<int, int>>();

        private static Dictionary<int, Dictionary<int, int>> taskZoneNpcIds = new Dictionary<int, Dictionary<int, int>>();

        private static Dictionary<int, Dictionary<int, int>> optionTaskIds = new Dictionary<int, Dictionary<int, int>>();

        private static Dictionary<int, int> taskId2Chapter = new Dictionary<int, int>();

        //任务完成奖励
        private static Dictionary<int, TaskFinishRewardModel> taskFinishRewardList = new Dictionary<int, TaskFinishRewardModel>();
        private static Dictionary<TaskFinishType, Dictionary<int, List<int>>> activityFinishRewardList = new Dictionary<TaskFinishType, Dictionary<int, List<int>>>();
        private static SortedDictionary<int, StartEndDatePair> periodStartEndInfos = new SortedDictionary<int, StartEndDatePair>();
        
        private static int tempTaskId { get; set; }

        private static int period = 1;
        public static int CurrPeriod => period;

        public static void BindTaskInfo()
        {
            tempTaskId = 0;
            //taskInfoList.Clear();
            //nextTaskIds.Clear();
            //taskZoneNpcIds.Clear();
            //optionTaskIds.Clear();
            //taskId2Chapter.Clear();
            //taskFinishRewardList.Clear();
            //activityFinishRewardList.Clear();

            InitTaskInfos();
            InitOptionTasks();
            InitTask2Chapter();
            InitActivityFinishReward();
            InitPeriodInfo();
        }

        private static void InitTaskInfos()
        {
            Dictionary<int, TaskInfo> taskInfoList = new Dictionary<int, TaskInfo>();
            Dictionary<int, Dictionary<int, int>> nextTaskIds = new Dictionary<int, Dictionary<int, int>>();
            Dictionary<int, Dictionary<int, int>> taskZoneNpcIds = new Dictionary<int, Dictionary<int, int>>();

            DataList taskDataList = DataListManager.inst.GetDataList("Task");
            foreach (var item in taskDataList)
            {
                Data data = item.Value;
                // 初始化任务基本信息
                TaskInfo taskItem = InitTaskInfoList(data);
                taskInfoList.Add(taskItem.Id, taskItem);

                AddTaskNpcName(taskZoneNpcIds, taskItem.Id, taskItem.CompleteNpcId);

                // 初始化任务链
                AddNextTaskId(nextTaskIds, taskItem);
            }
            TaskLibrary.taskInfoList = taskInfoList;
            TaskLibrary.nextTaskIds = nextTaskIds;
            TaskLibrary.taskZoneNpcIds = taskZoneNpcIds;
        }

        private static void InitOptionTasks()
        {
            Dictionary<int, Dictionary<int, int>> optionTaskIds = new Dictionary<int, Dictionary<int, int>>();

            DataList taskDataList = DataListManager.inst.GetDataList("OptionTaskEmotion");
            foreach (var item in taskDataList)
            {
                Data data = item.Value;
                // 初始化任务基本信息
                int taskId = data.ID;
                Dictionary<int, int> list = new Dictionary<int, int>();
                for (int i = 1; i <= 4; i++)
                {
                    int selectId = data.GetInt("TurnToTask" + i);
                    if (selectId > 0)
                    {
                        list.Add(i, selectId);
                    }
                }
                optionTaskIds[taskId] = list;
            }
            TaskLibrary.optionTaskIds = optionTaskIds;
        }

        private static void InitTask2Chapter()
        {
            Dictionary<int, int> taskId2Chapter = new Dictionary<int, int>();
            DataList taskDataList = DataListManager.inst.GetDataList("TaskChapter");
            foreach (var item in taskDataList)
            {
                Data data = item.Value;
                taskId2Chapter[data.ID] = data.GetInt("Chapter");
            }
            TaskLibrary.taskId2Chapter = taskId2Chapter;
        }

        private static TaskInfo InitTaskInfoList(Data taskData)
        {
            TaskInfo taskItem = new TaskInfo();

            taskItem.Id = taskData.ID;
            taskItem.MainType = taskData.GetInt("MainType");
            taskItem.TaskChain = taskData.GetInt("TaskChain");
            taskItem.ParamType = taskData.GetInt("ProgType");
            taskItem.CurStep = taskData.GetInt("CurrentStep");
            taskItem.TotalStep = taskData.GetInt("TotalStep");
            taskItem.LoadParams(taskData.GetString("ProgPrama"));
            taskItem.LoadBranchTasks(taskData.GetString("BranchTasks"));
            taskItem.DetailReward = taskData.GetString("DetailReward");

            int zoneNpcId = taskData.GetInt("CompleteNpcId");
            taskItem.CompleteNpcId = zoneNpcId;


            return taskItem;
        }

        private static void AddTaskNpcName(Dictionary<int, Dictionary<int, int>> taskZoneNpcIds, int taskId, int zoneNpcId)
        {
            Dictionary<int, int> taskIds;
            if (taskZoneNpcIds.TryGetValue(zoneNpcId, out taskIds))
            {
                taskIds[taskId] = 0;
            }
            else
            {
                taskIds = new Dictionary<int, int>();
                taskIds[taskId] = 0;
                taskZoneNpcIds.Add(zoneNpcId, taskIds);
            }
        }

        private static void AddNextTaskId(Dictionary<int, Dictionary<int, int>> nextTaskIds, TaskInfo taskItem)
        {
            if (taskItem.TaskChain > 0)
            {
                Dictionary<int, int> taskChains;
                if (nextTaskIds.TryGetValue(taskItem.TaskChain, out taskChains))
                {
                    if (taskChains.ContainsKey(tempTaskId))
                    {
                        taskChains[tempTaskId] = taskItem.Id;
                    }
                    taskChains.Add(taskItem.Id, 0);
                }
                else
                {
                    taskChains = new Dictionary<int, int>();
                    taskChains.Add(taskItem.Id, 0);
                    nextTaskIds.Add(taskItem.TaskChain, taskChains);
                }
                tempTaskId = taskItem.Id;
            }
            else
            {
                tempTaskId = 0;
            }
        }

        public static TaskInfo GetNextTaskInfo(int taskChain, int taskId)
        {
            Dictionary<int, int> taskChains;
            if (nextTaskIds.TryGetValue(taskChain, out taskChains))
            {
                int nextTaskId = 0;
                if (taskChains.TryGetValue(taskId, out nextTaskId))
                {
                    if (nextTaskId > 0)
                    {
                        TaskInfo info;
                        info = GetTaskInfoById(nextTaskId);
                        return info;
                    }
                }
            }
            return null;
        }

        public static TaskInfo GetTaskInfoById(int id)
        {
            TaskInfo newTask;
            taskInfoList.TryGetValue(id, out newTask);
            return newTask;
        }

        public static int GetOptionTaskId(int taskId, int index)
        {
            int newTask = 0;
            Dictionary<int, int> list;
            if (optionTaskIds.TryGetValue(taskId, out list))
            {
                list.TryGetValue(index, out newTask);
            }
            return newTask;
        }

        public static bool CheckNpcTaskId(int taskId, int zoneNpcId)
        {
            Dictionary<int, int> taskIds;
            if (taskZoneNpcIds.TryGetValue(zoneNpcId, out taskIds))
            {
                return taskIds.ContainsKey(taskId);
            }
            return false;
        }

        public static int GetTaskChapter(int taskId)
        {
            int chapterId = 0;
            taskId2Chapter.TryGetValue(taskId, out chapterId);
            return chapterId;
        }

        #region 任务完成奖励

        private static void InitActivityFinishReward()
        {
            Dictionary<int, TaskFinishRewardModel> taskFinishRewardList = new Dictionary<int, TaskFinishRewardModel>();
            Dictionary<TaskFinishType, Dictionary<int, List<int>>> activityFinishRewardList = new Dictionary<TaskFinishType, Dictionary<int, List<int>>>();
            //activityFinishRewardList.Clear();

            DataList dataList = DataListManager.inst.GetDataList("TaskFinishReward");
            foreach (var item in dataList)
            {
                TaskFinishRewardModel model = new TaskFinishRewardModel(item.Value);
                taskFinishRewardList.Add(model.Id, model);

                Dictionary<int, List<int>> dic;
                if (!activityFinishRewardList.TryGetValue(model.TaskFinishType, out dic))
                {
                    dic = new Dictionary<int, List<int>>();
                    activityFinishRewardList[model.TaskFinishType] = dic;
                }

                List<int> ids;
                if (!dic.TryGetValue(model.Period, out ids))
                {
                    ids = new List<int>();
                    dic.Add(model.Period, ids);
                }
                ids.Add(model.Id);
            }
            TaskLibrary.taskFinishRewardList = taskFinishRewardList;
            TaskLibrary.activityFinishRewardList = activityFinishRewardList;
        }

        public static TaskFinishRewardModel GetActivityFinishRewardModel(TaskFinishType type, int period, int index)
        {
            Dictionary<int, List<int>> dic;
            if (!activityFinishRewardList.TryGetValue(type, out dic)) return null;


            List<int> ids;
            if (dic.TryGetValue(period, out ids))
            {
                if (ids.Count > index)
                {
                    TaskFinishRewardModel model = null;
                    taskFinishRewardList.TryGetValue(ids[index], out model);
                    return model;
                }
            }
            return null;
        }
        
        private static void InitPeriodInfo()
        {
            //periodStartEndInfos.Clear();
            SortedDictionary<int, StartEndDatePair> periodStartEndInfos = new SortedDictionary<int, StartEndDatePair>();

            DataList dataList = DataListManager.inst.GetDataList("TaskFinshRewardPeriod");
            foreach(var kv in dataList)
            {
                periodStartEndInfos.Add(kv.Key, new StartEndDatePair()
                {
                    StartTime = DateTime.Parse(kv.Value.GetString("StartTime")),
                    EndTime = DateTime.Parse(kv.Value.GetString("EndTime")),
                });
            }

            TaskLibrary.periodStartEndInfos = periodStartEndInfos;
        }
        
        //每次passDay检查
        public static int CheckPeriod(DateTime open)
        {
            int periodTemp = GetPeriodByTime(open);
            period = periodTemp;
            return period;
        }

        public static int GetPeriodByTime(DateTime open)
        {
            int period = 1;
            foreach (var kv in periodStartEndInfos)
            {
                if (open >= kv.Value.StartTime && open <= kv.Value.EndTime)
                {
                    period = kv.Key;
                    break;
                }
            }
            return period;
        }

        public static bool CheckPeriodUpdate(DateTime now)
        {
            int periodTemp = GetPeriodByTime(now);
            if (period != periodTemp)
            {
                period = periodTemp;
                return true;
            }
            return false;
        }
        #endregion
    }
}