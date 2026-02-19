using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnumerateUtility;

namespace ServerShared
{
    public class DriftExploreLibrary
    {
        private static Dictionary<int, DriftExploreConfigModel> driftExploreConfigs = new Dictionary<int, DriftExploreConfigModel>();
        private static Dictionary<string, DriftExploreTaskModel> taskList = new Dictionary<string, DriftExploreTaskModel>();
        private static Dictionary<int, Dictionary<int, DriftExploreTaskModel>> periodTasks = new Dictionary<int, Dictionary<int, DriftExploreTaskModel>>();

        public static int MaxPeriod { get; private set; }

        public static void Init()
        {
            InitDriftExploreTasks();
            InitTaskPeriod();
            InitConfig();
        }
        private static void InitConfig()
        {
            DataList dataLists = DataListManager.inst.GetDataList("DriftExploreConfig");
            if (dataLists != null)
            {
                foreach (var kv in dataLists)
                {
                    DriftExploreConfigModel model = new DriftExploreConfigModel(kv.Value);
                    driftExploreConfigs.Add(model.Id, model);
                }
            }
        }

        private static void InitDriftExploreTasks()
        {
            Dictionary<string, DriftExploreTaskModel> taskList = new Dictionary<string, DriftExploreTaskModel>();

            int periodCount = 1;
            while (true)
            {
                DataList passTaskDatas = DataListManager.inst.GetDataList("DriftExploreTask_" + periodCount);
                if (passTaskDatas != null)
                {
                    //InitPassTask(passTaskDatas, periodCount);
                    InitTask(taskList, passTaskDatas, periodCount);
                }
                else
                {
                    MaxPeriod = periodCount - 1;
                    break;
                }
                periodCount++;
            }
            DriftExploreLibrary.taskList = taskList;
        }

        private static void InitTask(Dictionary<string, DriftExploreTaskModel> taskList, DataList datas, int period)
        {
            foreach (var kv in datas)
            {
                Data data = kv.Value;
                DriftExploreTaskModel item = new DriftExploreTaskModel(data, period);
                taskList.Add(period + "_" + item.Id, item);
            }

        }
        
        private static void InitTaskPeriod()
        {
            Dictionary<int, Dictionary<int, DriftExploreTaskModel>> periodTasks = new Dictionary<int, Dictionary<int, DriftExploreTaskModel>>();
            
            Dictionary<int, DriftExploreTaskModel> dic = null;
            foreach (var kv in taskList)
            {
                if (!periodTasks.TryGetValue(kv.Value.Period, out dic))
                {
                    dic = new Dictionary<int, DriftExploreTaskModel>();
                    periodTasks.Add(kv.Value.Period, dic);
                }

                dic.Add(kv.Value.Id, kv.Value);
            }

            DriftExploreLibrary.periodTasks = periodTasks;
        }
        public static DriftExploreTaskModel GetTaskModel(int taskId, int curPeriod)
        {
            string key = curPeriod + "_" + taskId.ToString();
            DriftExploreTaskModel taskModel = null;
            taskList.TryGetValue(key, out taskModel);
            return taskModel;
        }
        
        public static DriftExploreTaskModel GetDriftExploreTaskByTaskId(int taskId, int curPeriod)
        {
            string key = curPeriod + "_" + taskId.ToString();
            DriftExploreTaskModel task;
            taskList.TryGetValue(key,out task);
            return task;
        }
        public static Dictionary<int, DriftExploreTaskModel> GetPeriodDriftExploreTasks(int period)
        {
            Dictionary<int, DriftExploreTaskModel> dic;
            periodTasks.TryGetValue(period, out dic);
            return dic;
        }


        public static DriftExploreConfigModel GetDriftExploreConfig(int curPeriod)
        {
            DriftExploreConfigModel model;
            driftExploreConfigs.TryGetValue(curPeriod,out model);
            return model;
        }
    }
}
