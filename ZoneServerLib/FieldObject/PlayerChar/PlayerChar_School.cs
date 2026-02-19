using System.Collections.Generic;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using EnumerateUtility.Activity;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        private SchoolManager schoolManager;
        public SchoolManager SchoolManager => schoolManager;

        private void InitSchoolManager()
        {
            schoolManager = new SchoolManager(this);
        }

        public void SendClientSchoolInfo()
        {
            MSG_ZGC_SCHOOL_INFO msg = schoolManager.GenerateSchoolInfo();
            Write(msg);
        }

        public void EnterSchool(int schoolId, bool random = false)
        {
            MSG_ZGC_ENTER_SCHOOL response = new MSG_ZGC_ENTER_SCHOOL();

            if (schoolManager.SchoolId > 0)
            {
                response.Result = (int)ErrorCode.HadEnteredSchool;
                Write(response);
                return;
            }

            if (schoolId == 0)
            {
                //随机加入学院
                server.ManagerServer.Write(new MSG_ZM_GET_SCHOOL_ID(), uid);
                return;
            }
            //加入选择的学院
            schoolManager.SetSchoolId(schoolId);
            SendClientSchoolInfo();

            if (random)
            {
                if (!schoolManager.SchoolInfo.RandomSchoolReward)
                {
                    RewardManager reward = new RewardManager();
                    reward.AddReward(SchoolLibrary.RandomSchoolReward);
                    reward.BreakupRewards();

                    AddRewards(reward, ObtainWay.School);

                    schoolManager.SchoolInfo.RandomSchoolReward = true;
                    server.GameDBPool.Call(new QueryUpdateSchoolGotRewardInfo(uid));
                }
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void LeaveSchool()
        {
            MSG_ZGC_LEAVE_SCHOOL response = new MSG_ZGC_LEAVE_SCHOOL();
            if (schoolManager.SchoolId == 0)
            {
                Log.Warn($"player {uid} leave school fail : had not in school");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //加入选择的学院
            schoolManager.SetSchoolId(0);
            SendClientSchoolInfo();

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void SchoolPoolUseItem(int itemId)
        {
            MSG_ZGC_SCHOOL_POOL_USE_ITEM response = new MSG_ZGC_SCHOOL_POOL_USE_ITEM();
            if (schoolManager.SchoolId == 0)
            {
                Log.Warn($"player {uid} school pool use item fail : had not in school");
                response.Result = (int)ErrorCode.HadNotEnteredSchool;
                Write(response);
                return;
            }

            var buffList = SchoolLibrary.GetPoolItemBuffList(itemId, schoolManager.SchoolType);
            if (buffList == null)
            {
                Log.Warn($"player {uid} school pool use item fail : had not item {itemId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            BaseItem item = bagManager.NormalBag.GetItem(itemId);
            if (item == null || item.PileNum < 1)
            {
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }

            DelItem2Bag(item, RewardType.NormalItem, 1, ConsumeWay.School);
            SyncClientItemInfo(item);

            //加入选择的学院
            schoolManager.UseItem(itemId);
            SendClientSchoolInfo();

            AddSchoolTaskNum(TaskType.PoolUseItem);
            NormalItem normalItem = item as NormalItem;
            if (normalItem != null)
            {
                AddSchoolTaskNum(TaskType.PoolUseQualityItem, normalItem.ItemModel.Quality, TaskParamType.QUALITY);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void SchoolPoolLevelUp()
        {
            MSG_ZGC_SCHOOL_POOL_LEVEL_UP response = new MSG_ZGC_SCHOOL_POOL_LEVEL_UP();
            if (schoolManager.SchoolId == 0)
            {
                Log.Warn($"player {uid} SchoolPoolLevelUp fail : had not in school");
                response.Result = (int)ErrorCode.HadNotEnteredSchool;
                Write(response);
                return;
            }

            if (schoolManager.PoolLevel >= SchoolLibrary.PoolLevelMax)
            {
                Log.Warn($"player {uid} SchoolPoolLevelUp fail : max level");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            PoolGrowthModel model = SchoolLibrary.GetPoolGrowthModel(schoolManager.PoolLevel + 1);
            if (model == null)
            {
                Log.Warn($"player {uid} SchoolPoolLevelUp fail : had not find model");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            BaseItem item = bagManager.NormalBag.GetItem(model.CostItem.Id);
            if (item == null || item.PileNum < model.CostItem.Num)
            {
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }

            DelItem2Bag(item, RewardType.NormalItem, model.CostItem.Num, ConsumeWay.School);
            SyncClientItemInfo(item);

            //加入选择的学院
            schoolManager.AddLevel();
            SendClientSchoolInfo();

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }


        #region 学院任务
        public void SendSchoolTaskInfo()
        {
            //任务完成信息
            schoolManager.SendSchoolTaskFinishInfo();

            schoolManager.SendSchoolTasksInfo();
        }

        public void AddSchoolTaskNum(TaskType type, int num = 1, bool replace = false)
        {
            schoolManager.AddTypeTaskNum(type, num, replace);
        }

        public void AddSchoolTaskNum(TaskType type, int param, string paramString, int num = 1)
        {
            schoolManager.AddTypeTaskNum(type, param, paramString, num);
        }

        public void AddSchoolTaskNum(TaskType type, int[] param, string[] paramString)
        {
            schoolManager.AddTypeTaskNum(type, param, paramString);
        }

        /// <summary>
        /// 领取学院任务完成奖励
        /// </summary>
        /// <param name="getAll"></param>
        /// <param name="taskId"></param>
        public void GetSchoolTasksFinishReward(bool getAll, int taskId)
        {
            MSG_ZGC_GET_SCHOOLTASK_FINISH_REWARD response = new MSG_ZGC_GET_SCHOOLTASK_FINISH_REWARD();

            if (getAll)
            {
                GetAllSchoolTasksFinishReward(response);
            }
            else
            {
                GetSchoolTaskFinishReward(taskId, response);
            }

            Write(response);
        }

        private void GetAllSchoolTasksFinishReward(MSG_ZGC_GET_SCHOOLTASK_FINISH_REWARD response)
        {
            int num = 0; 
            string rewardString = schoolManager.GetAllSchoolTasksFinishReward(response, out num);

            if (!string.IsNullOrEmpty(rewardString))
            {
                //获取奖励
                RewardManager manager = GetSimpleReward(rewardString, ObtainWay.SchoolTask);
                manager.GenerateRewardItemInfo(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            
            if (num > 0)
            {
                //AddTaskNumForType(TaskType.DailyTaskCount, num);

                //限时礼包
                //ActionManager.RecordActionAndCheck(ActionType.DailyTaskFinishCount, num);
            }
        }

        private void GetSchoolTaskFinishReward(int taskId, MSG_ZGC_GET_SCHOOLTASK_FINISH_REWARD response)
        {
            CommonTaskInfo task;
            schoolManager.SchoolTasks.TryGetValue(taskId, out task);
            if (task == null)
            {
                Log.Warn($"player {Uid} get school task {taskId} finish reward failed: not find task");
                response.Result = (int)ErrorCode.Fail;
                return;
            }

            schoolManager.CheckSchoolTaskPeriod();

            if (!task.Rewarded && task.CurNum >= task.ParamNum)
            {
                CommonTaskModel taskModel = SchoolLibrary.GetTaskModel(task.Id, schoolManager.SchoolTaskFinish.Period);
                if (taskModel == null)
                {
                    Log.Warn($"player {Uid} get school task finish reward not find {task.Id} in xml");
                    response.Result = (int)ErrorCode.Fail;
                }

                schoolManager.GetSchoolTaskFinishReward(task, taskModel.TimeType, response);

                string rewardString = taskModel.Reward;
                if (!string.IsNullOrEmpty(rewardString))
                {
                    //获取奖励
                    RewardManager manager = GetSimpleReward(rewardString, ObtainWay.SchoolTask);
                    manager.GenerateRewardItemInfo(response.Rewards);
                }
                response.Result = (int)ErrorCode.Success;

                //AddTaskNumForType(TaskType.DailyTaskCount);
                //任务BI
                //BIRecordRecordTaskLog((TaskType)taskModel.TaskType, taskId, 4);
                //限时礼包
                //ActionManager.RecordActionAndCheck(ActionType.DailyTaskFinishCount, 1);
            }
            else
            {
                Log.Warn($"player {Uid} get school task {taskId} finish reward failed: not complete");
                response.Result = (int)ErrorCode.Fail;
            }
        }
        
        public void GetSchoolTaskBoxReward(TaskFinishType type, int index)
        {
            MSG_ZGC_GET_SCHOOLTASK_BOX_REWARD response = new MSG_ZGC_GET_SCHOOLTASK_BOX_REWARD();

            schoolManager.CheckSchoolTaskPeriod();

            TaskFinishRewardModel model = SchoolLibrary.GetSchoolTaskFinishRewardModel(type, schoolManager.SchoolTaskFinish.Period, index - 1);
            if (model == null)
            {
                Log.Warn($"player {Uid} GetSchoolTaskBoxReward period {schoolManager.SchoolTaskFinish.Period}  type {type} index {index} failded: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
          
            //if (model.Period != schoolManager.SchoolTaskFinish.Period)
            //{
            //    Log.Warn($"player {Uid} GetSchoolTaskBoxReward type {type} index {index} failded: now period {schoolManager.SchoolTaskFinish.Period}");
            //    response.Result = (int)ErrorCode.Fail;
            //    Write(response);
            //    return;
            //}

            if (schoolManager.CheckGotSchoolTaskFinishReward(type == TaskFinishType.Daily, index))
            {
                Log.Warn($"player {Uid} GetSchoolTaskBoxReward type {type} index {index} failded: already rewarded");
                response.Result = (int)ErrorCode.ActivityFinishStateHadReward;
                Write(response);
                return;
            }

            if (!schoolManager.CheckSchoolTaskFinishNum(type, model.Num))
            {
                Log.Warn($"player {Uid} GetSchoolTaskBoxReward type {type} index {index} failded: not reach num limit");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            schoolManager.UpdateSchoolTaskFinishReward(type, index);

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(model.Reward);
            AddRewards(manager, ObtainWay.SchoolTask);

            manager.GenerateRewardMsg(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private void RefreshDailySchoolTaskInfo()
        {
            schoolManager.ResetDailySchoolTaskFinishInfo();
            schoolManager.RefreshSchoolTaskInfoByType((int)TaskFinishType.Daily);
        }

        private void RefreshWeeklySchoolTaskInfo()
        {
            schoolManager.ResetWeeklySchoolTaskFinishInfo();
            schoolManager.RefreshSchoolTaskInfoByType((int)TaskFinishType.Weekly);
        }
        #endregion

        #region 学院答题
        public void SendAnswerQuestionInfo()
        {
            //可能会点了答题但没提交，需要根据startTime变更answered状态
            schoolManager.CheckSyncDbInsertAnswerQuestion();
            schoolManager.CheckSyncDbUpdateAnswerQuestion();
            schoolManager.SendAnswerQuestionInfo();
        }

        /// <summary>
        /// 记录答题开始时间
        /// </summary>
        /// <param name="type"></param>
        public void RecordAnswerQuestionStart(int type)
        {
            schoolManager.RecordAnswerQuestionStart(type);
        }

        /// <summary>
        /// 提交答案
        /// </summary>
        /// <param name="type"></param>
        /// <param name="questionId">题目id 不是第几题</param>
        /// <param name="answerNum"></param>
        /// <param name="isEnd"></param>
        public void AnswerQuestionSubmit(int type, int questionId, int answerNum, bool isEnd)
        {
            MSG_ZGC_ANSWER_QUESTION_SUBMIT response = new MSG_ZGC_ANSWER_QUESTION_SUBMIT();
            response.Type = type;
            response.IsEnd = isEnd;

            AnswerQuestionConfig config = SchoolLibrary.GetAnswerQuestionConfig(type);
            if (config == null)
            {
                Log.Warn($"player {Uid} answer question submit {questionId} failed: not find type {type} in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            AnswerQuestionModel answerQModel = SchoolLibrary.GetAnswerQuestionModel(type, questionId);
            if (answerQModel == null)
            {
                Log.Warn($"player {Uid} answer question submit type {type} id {questionId} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            AnswerQuestionInfo info;
            schoolManager.AnswerInfoDic.TryGetValue(type, out info);
            if (info == null)
            {
                Log.Warn($"player {Uid} answer question submit {questionId} failed: not find info {type}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (info.Questions[info.CurIndex] != questionId || info.CurIndex >= config.QuestionsNum)
            {
                Log.Warn($"player {Uid} answer question submit type {type} id {questionId} failed: not current question");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!info.StartTime.Date.Equals(ZoneServerApi.now.Date))
            {
                Log.Warn($"player {Uid} answer question submit type {type} id {questionId} failed: not start or over time");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (info.Answered)
            {
                Log.Warn($"player {Uid} answer question submit type {type} id {questionId} failed: already answered");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (answerNum == answerQModel.RightAnswer)
            {
                info.AddAccumulateRewards(answerQModel.Reward);
            }
            info.CurIndex++;
            
            if (isEnd)
            {
                info.Answered = true;
                info.Rewarded = true;

                string finalRewards = info.GetAccumulateRewardsStr();
                if (!string.IsNullOrEmpty(finalRewards))
                {
                    //获取奖励
                    RewardManager manager = GetSimpleReward(finalRewards, ObtainWay.AnswerQuestion);
                    manager.GenerateRewardItemInfo(response.Rewards);
                }
                AnswerQuestionAddTaskNum(type);
            }
            //db
            schoolManager.SyncDbUpdateAnswerQuestionInfo(info);

            response.Result = (int)ErrorCode.Success;
            response.AccumulateRewards.AddRange(info.AccumulateRewards.Values);
            Write(response);
        }

        public void RefreshAnswerQuestionInfo()
        {
            schoolManager.RefreshAnswerQuestionInfo();
            schoolManager.SendAnswerQuestionInfo();
        }

        public void AnswerQuestionAddTaskNum(int type)
        {
            switch ((AnswerQuestionType)type)
            {
                case AnswerQuestionType.School:
                    AddSchoolTaskNum(TaskType.AnswerQuestion);
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
