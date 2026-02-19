using System;
using System.Collections.Generic;
using System.Linq;
using DataProperty;
using EnumerateUtility;
using ServerModels;
using EnumerateUtility.Activity;

namespace ServerShared
{
    public class SchoolLibrary
    {
        private static Dictionary<int, ListMap<SchoolType, int>> poolItemBuff =
            new Dictionary<int, ListMap<SchoolType, int>>();

        private static Dictionary<int, PoolGrowthModel> poolGrowthModels = new Dictionary<int, PoolGrowthModel>();

        private static Dictionary<int, SchoolSpecialModel> schoolSpec = new Dictionary<int, SchoolSpecialModel>();

        public static ItemBasicInfo RandomSchoolReward { get; private set; }

        public static int PoolLevelMax { get; private set; }
        public static TimeSpan BuffValidTime { get; private set; }

        //学院任务奖励
        private static Dictionary<int, TaskFinishRewardModel> schoolTaskRewardList = new Dictionary<int, TaskFinishRewardModel>();
        private static Dictionary<TaskFinishType, Dictionary<int, List<int>>> schoolTaskRewardListByType = new Dictionary<TaskFinishType, Dictionary<int, List<int>>>();
        private static SortedDictionary<int, StartEndDatePair> schoolTaskPeriodTime = new SortedDictionary<int, StartEndDatePair>();
        private static Dictionary<int, Dictionary<int, CommonTaskModel>> periodSchoolTasks = new Dictionary<int, Dictionary<int, CommonTaskModel>>();

        //学院答题
        private static Dictionary<int, AnswerQuestionConfig> answerQConfigs = new Dictionary<int, AnswerQuestionConfig>();
        private static Dictionary<int, Dictionary<int, AnswerQuestionModel>> answerQList = new Dictionary<int, Dictionary<int, AnswerQuestionModel>>();

        public static void Init()
        {
            InitConfig();

            InitPoolItem();

            InitPoolGrowth();

            InitSpec();

            InitSchoolTaskReward();

            InitSchoolPeriodTime();

            InitSchoolTasks();

            InitAnswerQuestionInfo();

            InitAnswerQuestionConfig();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("SchoolConfig", 1);

            RandomSchoolReward = ItemBasicInfo.Parse(data.GetString("RandomSchoolReward"));
            BuffValidTime = TimeSpan.Parse(data.GetString("BuffValidTime"));
        }

        private static void InitPoolItem()
        {
            Dictionary<int, ListMap<SchoolType, int>> poolItemBuff = new Dictionary<int, ListMap<SchoolType, int>>();

            DataList dataList = DataListManager.inst.GetDataList("SchoolPoolItem");
            foreach (var kv in dataList)
            {
                ListMap<SchoolType, int> itemAdd = new ListMap<SchoolType, int>();
                itemAdd.Add(SchoolType.TianDou, kv.Value.GetString("TianDou").ToList('|'));
                itemAdd.Add(SchoolType.XingLuo, kv.Value.GetString("XingLuo").ToList('|'));
                itemAdd.Add(SchoolType.ShiLaiKe, kv.Value.GetString("ShiLaiKe").ToList('|'));
                poolItemBuff.Add(kv.Key, itemAdd);
            }

            SchoolLibrary.poolItemBuff = poolItemBuff;
        }

        private static void InitPoolGrowth()
        {
            Dictionary<int, PoolGrowthModel> poolGrowthModels = new Dictionary<int, PoolGrowthModel>();

            DataList dataList = DataListManager.inst.GetDataList("SchoolPoolGrowth");
            foreach (var kv in dataList)
            {
                PoolGrowthModel model = new PoolGrowthModel(kv.Value);
                poolGrowthModels.Add(model.Id, model);
            }

            PoolLevelMax = poolGrowthModels.Keys.Max();

            SchoolLibrary.poolGrowthModels = poolGrowthModels;
        }

        private static void InitSpec()
        {
            Dictionary<int, SchoolSpecialModel> schoolSpec = new Dictionary<int, SchoolSpecialModel>();
            DataList dataList = DataListManager.inst.GetDataList("SchoolSpec");

            foreach (var data in dataList)
            {
                SchoolSpecialModel model = new SchoolSpecialModel(data.Value);
                schoolSpec.Add(model.Id, model);
            }

            SchoolLibrary.schoolSpec = schoolSpec;
        }


        public static List<int> GetPoolItemBuffList(int itemId, SchoolType schoolType)
        {
            ListMap<SchoolType, int> itemBuff;
            if (!poolItemBuff.TryGetValue(itemId, out itemBuff)) return null;

            List<int> buffList;
            itemBuff.TryGetValue(schoolType, out buffList);
            return buffList;
        }

        public static PoolGrowthModel GetPoolGrowthModel(int level)
        {
            PoolGrowthModel model;
            poolGrowthModels.TryGetValue(level, out model);
            return model;
        }

        public static SchoolSpecialModel GetSchoolSpecialModel(int id)
        {
            SchoolSpecialModel model;
            schoolSpec.TryGetValue(id, out model);
            return model;
        }

        #region 学院任务
        private static void InitSchoolTaskReward()
        {
            Dictionary<int, TaskFinishRewardModel> schoolTaskRewardList = new Dictionary<int, TaskFinishRewardModel>();
            Dictionary<TaskFinishType, Dictionary<int, List<int>>> schoolTaskRewardListByType = new Dictionary<TaskFinishType, Dictionary<int, List<int>>>();

            DataList dataList = DataListManager.inst.GetDataList("SchoolTaskReward");
            foreach (var item in dataList)
            {
                TaskFinishRewardModel model = new TaskFinishRewardModel(item.Value);
                schoolTaskRewardList.Add(model.Id, model);

                Dictionary<int, List<int>> dic;
                if (!schoolTaskRewardListByType.TryGetValue(model.TaskFinishType, out dic))
                {
                    dic = new Dictionary<int, List<int>>();
                    schoolTaskRewardListByType[model.TaskFinishType] = dic;
                }

                List<int> ids;
                if (!dic.TryGetValue(model.Period, out ids))
                {
                    ids = new List<int>();
                    dic.Add(model.Period, ids);
                }
                ids.Add(model.Id);
            }
            SchoolLibrary.schoolTaskRewardList = schoolTaskRewardList;
            SchoolLibrary.schoolTaskRewardListByType = schoolTaskRewardListByType;
        }

        private static void InitSchoolPeriodTime()
        {
            SortedDictionary<int, StartEndDatePair> schoolTaskPeriodTime = new SortedDictionary<int, StartEndDatePair>();

            DataList dataList = DataListManager.inst.GetDataList("SchoolTaskPeriod");
            foreach (var kv in dataList)
            {
                schoolTaskPeriodTime.Add(kv.Key, new StartEndDatePair()
                {
                    StartTime = DateTime.Parse(kv.Value.GetString("StartTime")),
                    EndTime = DateTime.Parse(kv.Value.GetString("EndTime")),
                });
            }

            SchoolLibrary.schoolTaskPeriodTime = schoolTaskPeriodTime;
        }

        private static void InitSchoolTasks()
        {
            Dictionary<int, Dictionary<int, CommonTaskModel>> periodSchoolTasks = new Dictionary<int, Dictionary<int, CommonTaskModel>>();

            CommonTaskModel item;
            Dictionary<int, CommonTaskModel> dic;
            DataList dataList = DataListManager.inst.GetDataList("SchoolTask");
            foreach (var kv in dataList)
            {
                item = new CommonTaskModel(kv.Value);
                foreach (int period in item.PeriodList)
                {
                    if (!periodSchoolTasks.TryGetValue(period, out dic))
                    {
                        dic = new Dictionary<int, CommonTaskModel>();
                        periodSchoolTasks.Add(period, dic);
                    }
                    dic.Add(item.Id, item);
                }
            }

            SchoolLibrary.periodSchoolTasks = periodSchoolTasks;
        }

        public static int GetSchoolTaskPeriodByTime(DateTime open)
        {
            int period = 1;
            foreach (var kv in schoolTaskPeriodTime)
            {
                if (open >= kv.Value.StartTime && open <= kv.Value.EndTime)
                {
                    period = kv.Key;
                    break;
                }
            }
            return period;
        }

        public static CommonTaskModel GetTaskModel(int taskId, int period)
        {
            CommonTaskModel taskModel = null;
            Dictionary<int, CommonTaskModel> dic;
            if (periodSchoolTasks.TryGetValue(period, out dic) && dic.TryGetValue(taskId, out taskModel))
            {
                return taskModel;
            }
            return taskModel;
        }

        public static TaskFinishRewardModel GetSchoolTaskFinishRewardModel(TaskFinishType type, int period, int index)
        {
            Dictionary<int, List<int>> dic;
            if (!schoolTaskRewardListByType.TryGetValue(type, out dic)) return null;

            List<int> ids;
            if (dic.TryGetValue(period, out ids))
            {
                if (ids.Count > index)
                {
                    TaskFinishRewardModel model = null;
                    schoolTaskRewardList.TryGetValue(ids[index], out model);
                    return model;
                }
            }
            return null;
        }

        public static Dictionary<int, CommonTaskModel> GetPeriodSchoolTasks(int period)
        {
            Dictionary<int, CommonTaskModel> dic;
            periodSchoolTasks.TryGetValue(period, out dic);
            return dic;
        }
        #endregion

        #region 学院答题

        private static void InitAnswerQuestionConfig()
        {
            Dictionary<int, AnswerQuestionConfig> answerQConfigs = new Dictionary<int, AnswerQuestionConfig>();

            DataList dataList = DataListManager.inst.GetDataList("AnswerQuestionConfig");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                AnswerQuestionConfig config = new AnswerQuestionConfig(data);
                answerQConfigs.Add(config.Id, config);
            }

            SchoolLibrary.answerQConfigs = answerQConfigs;
        }

        private static void InitAnswerQuestionInfo()
        {
            Dictionary<int, Dictionary<int, AnswerQuestionModel>> answerQList = new Dictionary<int, Dictionary<int, AnswerQuestionModel>>();

            AnswerQuestionModel model;
            Dictionary<int, AnswerQuestionModel> dic;

            DataList dataList = DataListManager.inst.GetDataList("AnswerQuestionInfo");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                model = new AnswerQuestionModel(data);                
                if (!answerQList.TryGetValue(model.Type, out dic))
                {
                    dic = new Dictionary<int, AnswerQuestionModel>();
                    answerQList.Add(model.Type, dic);
                }
                dic.Add(model.Id, model);
            }

            SchoolLibrary.answerQList = answerQList;
        }    

        public static AnswerQuestionConfig GetAnswerQuestionConfig(int id)
        {
            AnswerQuestionConfig config;
            answerQConfigs.TryGetValue(id, out config);
            return config;
        }

        public static List<int> GetQuestionsIdByType(int type)
        {
            Dictionary<int, AnswerQuestionModel> dic;
            answerQList.TryGetValue(type, out dic);
            if (dic == null)
            {
                return null;
            }
            return dic.Keys.ToList();
        }

        public static AnswerQuestionModel GetAnswerQuestionModel(int type, int id)
        {
            Dictionary<int, AnswerQuestionModel> dic;
            answerQList.TryGetValue(type, out dic);
            if (dic == null)
            {
                return null;
            }
            AnswerQuestionModel model;
            dic.TryGetValue(id, out model);
            return model;
        }

        public static List<int> GetAnswerQuestionTypes()
        {
            return answerQConfigs.Keys.ToList();
        }
        #endregion
    }
}
