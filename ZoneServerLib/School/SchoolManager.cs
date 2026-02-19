using System;
using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerFrame;
using ServerModels.School;
using ServerShared;
using ServerModels;
using System.Collections.Generic;
using EnumerateUtility.Activity;
using Logger;
using System.Linq;
using Google.Protobuf.Collections;

namespace ZoneServerLib
{
    public class SchoolManager
    {
        private PlayerChar owner;
        private SchoolInfo schoolInfo;
        private DateTime buffEndTime;

        public SchoolInfo SchoolInfo => schoolInfo;
        public int SchoolId => schoolInfo.SchoolId;
        public int PoolLevel => schoolInfo.PoolLevel;
        public SchoolType SchoolType => (SchoolType)schoolInfo.SchoolId;


        public SchoolManager(PlayerChar player)
        {
            owner = player;
        }

        public void Init(SchoolInfo schoolInfo)
        {
            this.schoolInfo = schoolInfo;
            buffEndTime = schoolInfo.BuffStartTime.Add(SchoolLibrary.BuffValidTime);
        }

        public bool IsBuffValid()
        {
            return buffEndTime > BaseApi.now;
        }

        public void SetSchoolId(int schoolId)
        {
            schoolInfo.SchoolId = schoolId;
            SyncDbSchoolInfo();
        }

        public void UseItem(int itemId)
        {
            schoolInfo.PoolItemId = itemId;
            schoolInfo.BuffStartTime = BaseApi.now;
            SyncDbSchoolInfo();
        }

        public void AddLevel()
        {
            schoolInfo.PoolLevel+=1;
            SyncDbSchoolInfo();
        }

        public MSG_ZGC_SCHOOL_INFO GenerateSchoolInfo()
        {
            MSG_ZGC_SCHOOL_INFO msg = new MSG_ZGC_SCHOOL_INFO()
            {
                SchoolId = schoolInfo.SchoolId,
                PoolLevel = schoolInfo.PoolLevel,
                PoolItemId = schoolInfo.PoolItemId,
                BuffStartTime = Timestamp.GetUnixTimeStampSeconds(schoolInfo.BuffStartTime),
                RandomSchoolReward = schoolInfo.RandomSchoolReward,
            };
            return msg;
        }

        public MSG_ZMZ_SCHOOL_INFO GenerateTransformInfo()
        {
            MSG_ZMZ_SCHOOL_INFO msg = new MSG_ZMZ_SCHOOL_INFO()
            {
                SchoolId = schoolInfo.SchoolId,
                PoolLevel = schoolInfo.PoolLevel,
                PoolItemId = schoolInfo.PoolItemId,
                BuffStartTime = Timestamp.GetUnixTimeStampSeconds(schoolInfo.BuffStartTime),
                RandomSchoolReward = schoolInfo.RandomSchoolReward,
            };
            return msg;
        }


        public void LoadSchoolTransformMsg(MSG_ZMZ_SCHOOL_INFO info)
        {
            SchoolInfo schoolInfo = new SchoolInfo()
            {
                Uid = owner.Uid,
                SchoolId = info.SchoolId,
                PoolLevel = info.PoolLevel,
                PoolItemId = info.PoolItemId,
                BuffStartTime = Timestamp.TimeStampToDateTime(info.BuffStartTime),
                RandomSchoolReward = info.RandomSchoolReward
            };
            Init(schoolInfo);
        }

        private void SyncDbSchoolInfo()
        {
            QueryUpdateSchoolInfo query = new QueryUpdateSchoolInfo(schoolInfo);
            owner.server.GameDBPool.Call(query);
        }

        #region 学院任务     
        private TaskFinishInfo schoolTaskFinish;
        public TaskFinishInfo SchoolTaskFinish { get { return schoolTaskFinish; } }

        private Dictionary<int, CommonTaskInfo> schoolTasks;
        public Dictionary<int, CommonTaskInfo> SchoolTasks { get { return schoolTasks; } }

        public void InitSchoolTaskInfo(TaskFinishInfo schoolTaskFinish, List<CommonTaskInfo> taskList)
        {
            InitSchoolTaskFinishInfo(schoolTaskFinish);
            InitSchoolTasksInfo(taskList);
        }

        private void InitSchoolTaskFinishInfo(TaskFinishInfo schoolTaskFinish)
        {
            this.schoolTaskFinish = schoolTaskFinish;
            CheckSchoolTaskPeriod();
        }

        private void InitSchoolTasksInfo(List<CommonTaskInfo> taskList)
        {
            schoolTasks = new Dictionary<int, CommonTaskInfo>();

            Dictionary<int, CommonTaskModel> curTasksModels = SchoolLibrary.GetPeriodSchoolTasks(schoolTaskFinish.Period);
            if (curTasksModels == null)
            {
                DeleteSchoolTasks(taskList);
                return;
            }

            List<CommonTaskInfo> updateList = new List<CommonTaskInfo>();
            List<CommonTaskInfo> addList = new List<CommonTaskInfo>();          
            List<CommonTaskInfo> deleteList = new List<CommonTaskInfo>();

            foreach (var task in taskList)
            {
                //check delete
                CommonTaskModel taskModel;
                curTasksModels.TryGetValue(task.Id, out taskModel);
                if (taskModel == null || !taskModel.PeriodList.Contains(schoolTaskFinish.Period))
                {
                    deleteList.Add(task);
                }
                else
                {
                    schoolTasks.Add(task.Id, task);
                }
            }

            CommonTaskInfo taskInfo;
            foreach (var kv in curTasksModels)
            {
                if (schoolTasks.TryGetValue(kv.Key, out taskInfo))
                {
                    //check update
                    CheckUpdateSchoolTaskParamInfo(taskInfo, kv.Value, updateList);
                    taskInfo.LoadXmlData(kv.Value);
                }
                else
                {
                    //insert
                    AddNewSchoolTask(kv.Value, addList);
                }
            }

            SyncUpdateSchoolTasks2DB(updateList);
            SyncInsertSchoolTasks2DB(addList);
            DeleteSchoolTasks(deleteList);
        }

        private void CheckUpdateSchoolTaskParamInfo(CommonTaskInfo taskInfo, CommonTaskModel taskModel, List<CommonTaskInfo> updateList)
        {
            if (taskInfo.TaskType != taskModel.TaskType)
            {
                taskInfo.TaskType = taskModel.TaskType;
                taskInfo.CurNum = 0;
                updateList.Add(taskInfo);
            }
        }

        private void AddNewSchoolTask(CommonTaskModel taskModel, List<CommonTaskInfo> addList)
        {
            CommonTaskInfo taskInfo = GenerateSchoolTaskInfo(taskModel);
            schoolTasks.Add(taskInfo.Id, taskInfo);
            addList.Add(taskInfo);
        }

        private CommonTaskInfo GenerateSchoolTaskInfo(CommonTaskModel taskModel)
        {
            CommonTaskInfo taskInfo = new CommonTaskInfo()
            {
                Id = taskModel.Id,
                TaskType = taskModel.TaskType,
                ParamNum = taskModel.ParamNum,
                TimeType = taskModel.TimeType
            };
            return taskInfo;
        }

        public void CheckSchoolTaskPeriod()
        {
            int period = SchoolLibrary.GetSchoolTaskPeriodByTime(owner.server.Now());
            if (period != schoolTaskFinish.Period)
            {
                schoolTaskFinish.Period = period;
            }
        }

        public void SendSchoolTaskFinishInfo()
        {
            MSG_ZGC_SCHOOLTASK_FINISH_INFO msg = new MSG_ZGC_SCHOOLTASK_FINISH_INFO()
            {
                Period = schoolTaskFinish.Period,
                DailyFinishNum = schoolTaskFinish.DailyNum,
                WeeklyFinishNum = schoolTaskFinish.WeeklyNum
            };
            msg.DailyRewardList.AddRange(schoolTaskFinish.DailyRewarded);
            msg.WeeklyRewardList.AddRange(schoolTaskFinish.WeeklyRewarded);

            owner.Write(msg);
        }

        public void SendSchoolTasksInfo()
        {
            MSG_ZGC_INIT_SCHOOLTASKS_INFO msg = new MSG_ZGC_INIT_SCHOOLTASKS_INFO();

            DateTime today = ZoneServerApi.now.Date;
            msg.DailyEndTime = Timestamp.GetUnixTimeStampSeconds(today.AddDays(1));

            int addDays = today.DayOfWeek == 0 ? 1 : (8 - (int)today.DayOfWeek);
            DateTime weekEnd = today.AddDays(addDays);
            msg.WeeklyEndTime = Timestamp.GetUnixTimeStampSeconds(weekEnd);

            foreach (var item in schoolTasks.Values)
            {
                msg.Tasks.Add(new ZGC_SCHOOL_TASK() { TaskId = item.Id, TaskType = item.TaskType, CurNum = item.CurNum, ParamNum = item.ParamNum, Rewarded = item.Rewarded, TimeType = item.TimeType });
            }

            owner.Write(msg);
        }

        public void AddTypeTaskNum(TaskType type, int num, bool replace)
        {
            CheckSchoolTaskPeriod();

            List<CommonTaskInfo> tasks = new List<CommonTaskInfo>();
            foreach (var kv in schoolTasks)
            {
                CommonTaskModel task = SchoolLibrary.GetTaskModel(kv.Value.Id, schoolTaskFinish.Period);
                if (task == null || kv.Value.TaskType != (int)type)
                {
                    continue;
                }
                if (!replace)
                {
                    kv.Value.CurNum += num;
                }
                else
                {
                    kv.Value.CurNum = num;
                }
                tasks.Add(kv.Value);
            }
            SyncSchoolTask2Client(tasks);
            SyncUpdateSchoolTasks2DB(tasks);
        }

        public void AddTypeTaskNum(TaskType type, int param, string paramType, int num = 1)
        {
            CheckSchoolTaskPeriod();

            List<CommonTaskInfo> tasks = new List<CommonTaskInfo>();
            foreach (var kv in schoolTasks)
            {
                CommonTaskModel task = SchoolLibrary.GetTaskModel(kv.Value.Id, schoolTaskFinish.Period);
                if (task == null || kv.Value.TaskType != (int)type)
                {
                    continue;
                }
                if (task.ParamChecksList.ContainsKey(paramType) && task.ParamChecksList[paramType].Contains(param))
                {
                    kv.Value.CurNum += num;
                    tasks.Add(kv.Value);
                }
            }
            SyncSchoolTask2Client(tasks);
            SyncUpdateSchoolTasks2DB(tasks);
        }

        public void AddTypeTaskNum(TaskType type, int[] param, string[] paramType)
        {
            if (param.Count() != paramType.Count())
            {
                return;
            }

            CheckSchoolTaskPeriod();

            List<CommonTaskInfo> tasks = new List<CommonTaskInfo>();
            foreach (var kv in schoolTasks)
            {
                CommonTaskModel task = SchoolLibrary.GetTaskModel(kv.Value.Id, schoolTaskFinish.Period);
                if (task == null || kv.Value.TaskType != (int)type)
                {
                    continue;
                }
                bool add = true;
                for (int i = 0; i < paramType.Count(); i++)
                {
                    if (!task.ParamChecksList.ContainsKey(paramType[i]) || !task.ParamChecksList[paramType[i]].Contains(param[i]))
                    {
                        add = false;
                    }
                }

                if (add)
                {
                    kv.Value.CurNum++;
                    tasks.Add(kv.Value);
                }
            }
            SyncSchoolTask2Client(tasks);
            SyncUpdateSchoolTasks2DB(tasks);
        }

        private void SyncSchoolTask2Client(List<CommonTaskInfo> tasks)
        {
            if (tasks.Count > 0)
            {
                MSG_ZGC_UPDATE_SCHOOLTASKS_INFO msg = new MSG_ZGC_UPDATE_SCHOOLTASKS_INFO();
                foreach (var task in tasks)
                {
                    msg.ChangedTasks.Add(GenerateSchoolTaskMsg(task));
                }
                owner.Write(msg);
            }
        }

        private ZGC_SCHOOL_TASK GenerateSchoolTaskMsg(CommonTaskInfo task)
        {
            ZGC_SCHOOL_TASK msg = new ZGC_SCHOOL_TASK()
            {
                TaskId = task.Id,
                TaskType = task.TaskType,
                CurNum = task.CurNum,
                ParamNum = task.ParamNum,
                Rewarded = task.Rewarded,
                TimeType = task.TimeType
            };
            return msg;
        }

        public string GetAllSchoolTasksFinishReward(MSG_ZGC_GET_SCHOOLTASK_FINISH_REWARD response, out int num)
        {
            CheckSchoolTaskPeriod();

            num = 0;
            CommonTaskInfo task = null;
            List<CommonTaskInfo> updateTasks = new List<CommonTaskInfo>();         
            string rewardString = "";
            foreach (var kv in schoolTasks)
            {
                task = kv.Value;
                CommonTaskModel taskModel = SchoolLibrary.GetTaskModel(task.Id, schoolTaskFinish.Period);
                if (taskModel == null)
                {
                    Log.Warn($"player {owner.Uid} GetAllSchoolTasksFinishReward not find {task.Id} ");
                    continue;
                }
                if (!task.Rewarded && task.CurNum >= task.ParamNum)
                {
                    rewardString += "|" + taskModel.Reward;
                    num++;
                    task.Rewarded = true;

                    updateTasks.Add(task);
                    response.ChangedTasks.Add(GenerateSchoolTaskMsg(task));

                    //任务BI
                    //owner.BIRecordRecordTaskLog((TaskType)taskModel.TaskType, task.Id, 4);

                    //统计任务完成数量
                    UpdateTaskFinishNum((TaskFinishType)taskModel.TimeType);
                }
            }
            SyncUpdateSchoolTasks2DB(updateTasks);
            SyncTaskFinishInfoToDBAndClient();
            return rewardString;
        }

        public void GetSchoolTaskFinishReward(CommonTaskInfo task, int finishType, MSG_ZGC_GET_SCHOOLTASK_FINISH_REWARD response)
        {
            task.Rewarded = true;
            //统计任务完成数量
            UpdateTaskFinishNum((TaskFinishType)finishType);

            SyncUpdateSchoolTasks2DB(new List<CommonTaskInfo>() { task });
            SyncTaskFinishInfoToDBAndClient();
            response.ChangedTasks.Add(GenerateSchoolTaskMsg(task));
        }

        private void UpdateTaskFinishNum(TaskFinishType finishType)
        {
            switch (finishType)
            {
                case TaskFinishType.Daily:
                    AddTaskDailyFinishNum();
                    break;
                case TaskFinishType.Weekly:
                    AddTaskWeeklyFinishNum();
                    break;
            }
        }

        private void AddTaskDailyFinishNum()
        {
            schoolTaskFinish.DailyNum += 1;
        }

        private void AddTaskWeeklyFinishNum()
        {
            schoolTaskFinish.WeeklyNum += 1;
        }

        private void SyncTaskFinishInfoToDBAndClient()
        {
            SyncDbUpdateTaskFinishInfo();
            SendSchoolTaskFinishInfo();
        }

        public bool CheckGotSchoolTaskFinishReward(bool isDaily, int index)
        {
            return isDaily ? schoolTaskFinish.DailyRewarded.Contains(index) : schoolTaskFinish.WeeklyRewarded.Contains(index);
        }

        public bool CheckSchoolTaskFinishNum(TaskFinishType type, int limit)
        {
            switch (type)
            {
                case TaskFinishType.Daily:
                    return schoolTaskFinish.DailyNum >= limit;
                case TaskFinishType.Weekly:
                    return schoolTaskFinish.WeeklyNum >= limit;
            }
            return false;
        }

        public void UpdateSchoolTaskFinishReward(TaskFinishType type, int index)
        {
            switch (type)
            {
                case TaskFinishType.Daily:
                    schoolTaskFinish.DailyRewarded.Add(index);
                    break;
                case TaskFinishType.Weekly:
                    schoolTaskFinish.WeeklyRewarded.Add(index);
                    break;
            }
            SyncTaskFinishInfoToDBAndClient();
        }

        public void ResetDailySchoolTaskFinishInfo()
        {
            schoolTaskFinish.DailyNum = 0;
            schoolTaskFinish.DailyRewarded.Clear();

            SyncTaskFinishInfoToDBAndClient();
        }

        public void ResetWeeklySchoolTaskFinishInfo()
        {
            schoolTaskFinish.WeeklyNum = 0;
            schoolTaskFinish.WeeklyRewarded.Clear();

            SyncTaskFinishInfoToDBAndClient();
        }

        public void RefreshSchoolTaskInfoByType(int type)
        {
            CheckSchoolTaskPeriod();
            Dictionary<int, CommonTaskModel> curTasksModels = SchoolLibrary.GetPeriodSchoolTasks(schoolTaskFinish.Period);
            if (curTasksModels == null)
            {
                return;
            }
            List<CommonTaskInfo> addList = new List<CommonTaskInfo>();
            List<CommonTaskInfo> updateList = new List<CommonTaskInfo>();
            foreach (var kv in curTasksModels.Where(kv=>kv.Value.TimeType == type))
            {
                CommonTaskInfo info;
                if (!schoolTasks.TryGetValue(kv.Value.Id, out info))
                {
                    AddNewSchoolTask(kv.Value, addList);
                }
                else
                {
                    info.CurNum = 0;
                    info.Rewarded = false;
                    updateList.Add(info);
                }
            }

            SyncInsertSchoolTasks2DB(addList);
            SyncUpdateSchoolTasks2DB(updateList);
            SendSchoolTasksInfo();
        }

        private void DeleteSchoolTasks(List<CommonTaskInfo> taskList)
        {
            List<int> deleteList = new List<int>();
            foreach (var item in taskList)
            {
                deleteList.Add(item.Id);
            }
            SyncDbDeleteSchookTasks(deleteList);
        }

        private void SyncUpdateSchoolTasks2DB(List<CommonTaskInfo> tasks)
        {
            if (tasks.Count <= 0)
            {
                return;
            }
            owner.server.GameDBPool.Call(new QueryUpdateSchoolTasks(owner.Uid, tasks));
        }

        private void SyncDbUpdateTaskFinishInfo()
        {
            owner.server.GameDBPool.Call(new QueryUpdateSchoolTaskFinishInfo(owner.Uid, schoolTaskFinish));
        }

        private void SyncInsertSchoolTasks2DB(List<CommonTaskInfo> tasks)
        {
            if (tasks.Count <= 0)
            {
                return;
            }
            owner.server.GameDBPool.Call(new QueryInsertSchoolTasks(owner.Uid, tasks));
        }

        private void SyncDbDeleteSchookTasks(List<int> tasks)
        {
            if (tasks.Count <= 0)
            {
                return;
            }
            owner.server.GameDBPool.Call(new QueryDeleteSchoolTasks(owner.Uid, tasks));
        }

        public MSG_ZMZ_SCHOOL_TASK_INFO GenerateSchoolTaskTransformMsg()
        {
            MSG_ZMZ_SCHOOL_TASK_INFO msg = new MSG_ZMZ_SCHOOL_TASK_INFO();
            msg.FinishInfo = GenerateSchoolTaskFinishTransformMsg();
            schoolTasks.Values.ForEach(x => msg.TaskList.Add(GetSchoolTaskTransformMsg(x)));
           
            return msg;
        }

        private ZMZ_SCHOOL_TASK_FINISH_INFO GenerateSchoolTaskFinishTransformMsg()
        {
            ZMZ_SCHOOL_TASK_FINISH_INFO msg = new ZMZ_SCHOOL_TASK_FINISH_INFO()
            {
                Period = schoolTaskFinish.Period,
                DailyNum = schoolTaskFinish.DailyNum,
                WeeklyNum = schoolTaskFinish.WeeklyNum              
            };
            msg.DailyRewarded.AddRange(schoolTaskFinish.DailyRewarded);
            msg.WeeklyRewarded.AddRange(schoolTaskFinish.WeeklyRewarded);
            return msg;
        }

        private ZMZ_SCHOOL_TASK_ITEM GetSchoolTaskTransformMsg(CommonTaskInfo taskInfo)
        {
            ZMZ_SCHOOL_TASK_ITEM msg = new ZMZ_SCHOOL_TASK_ITEM()
            {
                Id = taskInfo.Id,
                TaskType = taskInfo.TaskType,
                CurNum = taskInfo.CurNum,
                ParamNum = taskInfo.ParamNum,
                Rewarded = taskInfo.Rewarded,
                TimeType = taskInfo.TimeType
            };
            return msg;
        }

        public void LoadSchoolTaskTransformMsg(MSG_ZMZ_SCHOOL_TASK_INFO msg)
        {
            LoadSchoolTaskFinishTransformMsg(msg.FinishInfo);
            LoadSchoolTasksTransformMsg(msg.TaskList);
        }

        private void LoadSchoolTaskFinishTransformMsg(ZMZ_SCHOOL_TASK_FINISH_INFO msg)
        {
            schoolTaskFinish = new TaskFinishInfo();
            schoolTaskFinish.Period = msg.Period;
            schoolTaskFinish.DailyNum = msg.DailyNum;
            schoolTaskFinish.WeeklyNum = msg.WeeklyNum;
            schoolTaskFinish.DailyRewarded.AddRange(msg.DailyRewarded);
            schoolTaskFinish.WeeklyRewarded.AddRange(msg.WeeklyRewarded);
        }

        private void LoadSchoolTasksTransformMsg(RepeatedField<ZMZ_SCHOOL_TASK_ITEM> taskList)
        {
            schoolTasks = new Dictionary<int, CommonTaskInfo>();
            foreach (var item in taskList)
            {
                schoolTasks.Add(item.Id, new CommonTaskInfo() { Id = item.Id, TaskType = item.TaskType, CurNum = item.CurNum, ParamNum = item.ParamNum, Rewarded = item.Rewarded, TimeType = item.TimeType});
            }
        }
        #endregion

        #region 学院答题

        private Dictionary<int, AnswerQuestionInfo> answerInfoDic;
        public Dictionary<int, AnswerQuestionInfo> AnswerInfoDic { get { return answerInfoDic; } }

        public void InitAnswerQuestionInfo(Dictionary<int, AnswerQuestionInfo> dic)
        {
            answerInfoDic = dic;
            CheckUpdateAnswerQuestion();
        }

        private void CheckUpdateAnswerQuestion()
        {
            List<int> types = SchoolLibrary.GetAnswerQuestionTypes();
            AnswerQuestionInfo info;
            foreach (int type in types)
            {
                if (!answerInfoDic.TryGetValue(type, out info))
                {
                    info = CreateNewAnswerQuestionInfo(type);
                    answerInfoDic.Add(info.Type, info);
                }
                else
                {
                    CheckRefreshQuestions(info);
                    CheckAnswered(info);
                }
            }
        }

        private AnswerQuestionInfo CreateNewAnswerQuestionInfo(int type)
        {
            AnswerQuestionInfo info = new AnswerQuestionInfo()
            {
                Type = type,
                IsInsert = true
            };
            info.Questions.AddRange(RandomQuestions(type));
            info.NeedUpdate = true;
            return info;
        }

        private void CheckRefreshQuestions(AnswerQuestionInfo info)
        {
            if (info.Questions.Count == 0)// || !info.StartTime.Date.Equals(ZoneServerApi.now.Date))
            {
                info.Questions.AddRange(RandomQuestions(info.Type));
                info.NeedUpdate = true;
            }
        }

        private void CheckAnswered(AnswerQuestionInfo info)
        {
            if (info.StartTime.Date.Equals(ZoneServerApi.now.Date))
            {
                info.Answered = true;
                if (!info.Rewarded && info.AccumulateRewards.Count > 0)
                {
                    info.NeedUpdate = true;
                }
            }
        }

        private List<int> RandomQuestions(int type)
        {
            List<int> result = new List<int>();
            AnswerQuestionConfig config = SchoolLibrary.GetAnswerQuestionConfig(type);
            List<int> questions = SchoolLibrary.GetQuestionsIdByType(type);
            if (config == null || questions == null || questions.Count == 0)
            {
                return result;
            }
            questions.Sort();
            int totalNum = questions.Count;
            for (int i = 1; i <= config.QuestionsNum && totalNum >= i; i++)
            {
                int index = NewRAND.Next(0, totalNum-i);
                result.Add(questions[index]);
                questions.RemoveAt(index);               
            }
            return result;
        }

        public void SendAnswerQuestionInfo()
        {
            MSG_ZGC_ANSWER_QUESTION_INFO msg = new MSG_ZGC_ANSWER_QUESTION_INFO();

            foreach (var item in answerInfoDic)
            {
                msg.List.Add(GenerateAnswerQuestionInfoMsg(item.Value));
            }

            owner.Write(msg);
        }

        private ZGC_ANSWER_QUESTION GenerateAnswerQuestionInfoMsg(AnswerQuestionInfo info)
        {
            ZGC_ANSWER_QUESTION msg = new ZGC_ANSWER_QUESTION();

            msg.Type = info.Type;
            msg.Answered = info.Answered;
            msg.Questions.AddRange(info.Questions);
            msg.AccumulateRewards.AddRange(info.AccumulateRewards.Values);

            return msg;
        }

        public void RecordAnswerQuestionStart(int type)
        {
            AnswerQuestionInfo info;
            answerInfoDic.TryGetValue(type, out info);
            if (info != null && !info.StartTime.Date.Equals(ZoneServerApi.now.Date))
            {
                info.StartTime = ZoneServerApi.now;
                SyncDbUpdateAnswerQuestionInfo(info);
            }
        }

        public void CheckSyncDbUpdateAnswerQuestion()
        {
            //处理离线缓存登录 断线重连发奖
            answerInfoDic.ForEach(kv => CheckAnswered(kv.Value));

            List<AnswerQuestionInfo> updateList = new List<AnswerQuestionInfo>();
            foreach (var item in answerInfoDic.Values.Where(x => x.NeedUpdate))
            {
                item.NeedUpdate = false;
                CheckSendAnswerQuestionRewardEmail(item);
                updateList.Add(item);
            }
            SyncDbBatchUpdateAnswerQuestion(updateList);
        }

        public void CheckSyncDbInsertAnswerQuestion()
        {
            List<AnswerQuestionInfo> insertList = new List<AnswerQuestionInfo>();
            foreach (var item in answerInfoDic.Values.Where(x => x.IsInsert))
            {
                item.IsInsert = false;
                insertList.Add(item);
            }
            SyncDbBatchInsertAnswerQuestion(insertList);
        }

        public void RefreshAnswerQuestionInfo()
        {
            foreach (var item in answerInfoDic)
            {
                List<int> questions = RandomQuestions(item.Key);
                CheckSendAnswerQuestionRewardEmail(item.Value);
                item.Value.Refresh(questions);
            }
            List<AnswerQuestionInfo> updateList = answerInfoDic.Values.ToList();
            SyncDbBatchUpdateAnswerQuestion(updateList);
        }

        private void CheckSendAnswerQuestionRewardEmail(AnswerQuestionInfo info)
        {
            if (info.Answered && !info.Rewarded && info.AccumulateRewards.Count > 0)
            {
                info.Rewarded = true;
                AnswerQuestionConfig config = SchoolLibrary.GetAnswerQuestionConfig(info.Type);
                owner.SendPersonEmail(config.Email, "", info.GetAccumulateRewardsStr());
                owner.AnswerQuestionAddTaskNum(info.Type);
            }
        }

        private void SyncDbBatchInsertAnswerQuestion(List<AnswerQuestionInfo> insertList)
        {
            if (insertList.Count > 0)
            {
                owner.server.GameDBPool.Call(new QueryBatchInsertAnswerQuestion(owner.Uid, insertList));
            }
        }

        private void SyncDbBatchUpdateAnswerQuestion(List<AnswerQuestionInfo> updateList)
        {
            if (updateList.Count > 0)
            {
                owner.server.GameDBPool.Call(new QueryBatchUpdateAnswerQuestionInfo(owner.Uid, updateList));
            }
        }

        public void SyncDbUpdateAnswerQuestionInfo(AnswerQuestionInfo info)
        {
            owner.server.GameDBPool.Call(new QueryUpdateAnswerQuestionInfo(owner.Uid, info));
        }
        
        public MSG_ZMZ_ANSWER_QUESTION_INFO GenerateAnswerQuestionTransformMsg()
        {
            MSG_ZMZ_ANSWER_QUESTION_INFO msg = new MSG_ZMZ_ANSWER_QUESTION_INFO();
            foreach (var info in answerInfoDic)
            {
                msg.List.Add(GetAnswerQuestionMsg(info.Value));
            }
            return msg;
        }

        private ZMZ_ANSWER_QUESTION GetAnswerQuestionMsg(AnswerQuestionInfo info)
        {
            ZMZ_ANSWER_QUESTION msg = new ZMZ_ANSWER_QUESTION();
            msg.Type = info.Type;
            msg.Questions.AddRange(info.Questions);
            info.AccumulateRewards.ForEach(x => msg.AccumulateRewards.Add(x.Key, x.Value));
            msg.Answered = info.Answered;
            msg.StartTime = Timestamp.GetUnixTimeStampSeconds(info.StartTime);
            msg.CurIndex = info.CurIndex;
            msg.Rewarded = info.Rewarded;
            return msg;
        }

        public void LoadAnswerQuestionTransformMsg(MSG_ZMZ_ANSWER_QUESTION_INFO msg)
        {
            answerInfoDic = new Dictionary<int, AnswerQuestionInfo>();
            foreach (var item in msg.List)
            {
                AnswerQuestionInfo info = LoadAnswerQuestionInfo(item);
                answerInfoDic.Add(info.Type, info);
            }
        }

        private AnswerQuestionInfo LoadAnswerQuestionInfo(ZMZ_ANSWER_QUESTION msg)
        {
            AnswerQuestionInfo info = new AnswerQuestionInfo();
            info.Type = msg.Type;
            info.Questions.AddRange(msg.Questions);
            foreach (var item in msg.AccumulateRewards)
            {
                info.AccumulateRewards.Add(item.Key, item.Value);
            }
            info.Answered = msg.Answered;
            info.StartTime = Timestamp.TimeStampToDateTime(msg.StartTime);
            info.CurIndex = msg.CurIndex;
            info.Rewarded = msg.Rewarded;
            return info;
        }
        #endregion
    }
}
