using System;
using System.Collections.Generic;
using CommonUtility;
using DataProperty;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using ZoneServerLib.Task;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public int ChapterId { get; set; }

        //任务
        public TaskFinishState taskFinishState;
        public TaskManager TaskMng { get; set; }

        public void InitTaskManager()
        {
            TaskMng = new TaskManager(this);
        }

        public void InitTaskItemList(List<TaskItem> list, TaskFinishState taskFinishState)
        {
            int mainTaskType = (int)TaskMainType.Main;
            foreach (var item in list)
            {
                TaskInfo info = TaskLibrary.GetTaskInfoById(item.Id);
                if (info == null)
                {
                    //任务信息未找到
                    Log.Warn("player {0} InitTaskItemList not find task info: {1}", Uid, item.Id);
                    continue;
                }
                item.MainType = info.MainType;
                TaskMng.AddTaskListItem(item);

                if (info.MainType == mainTaskType && TaskMng.CurrMaxMainTaskId < item.Id)
                {
                    TaskMng.SetCurrFinishedMaxMainTaskId(item.Id);
                }
            }

            this.taskFinishState = taskFinishState;
            CheckTaskPeriod();
        }

        public void TaskComplete(int taskId, int zoneNpcId)
        {
            List<TaskItem> addList = new List<TaskItem>();
            TaskItem add = null;
            List<int> removeList = new List<int>();

            TaskItem task = TaskMng.GetTaskItemForId(taskId);
            if (task == null)
            {
                //任务信息未找到
                Log.Warn("player {0} complete task not find task item {1}.", Uid, taskId);
                return;
            }
            TaskInfo info = TaskLibrary.GetTaskInfoById(task.Id);
            if (info == null)
            {
                //任务信息未找到
                Log.Warn("player {0} complete task not find task info: {1}", Uid, task.Id);
                return;
            }

            //检查任务完成条件
            if (!TaskMng.CheckTaskComplete(task))
            {
                Log.Warn("player {0} check complete task id {1} cur num {2}", Uid, task.Id, task.CurNum);
                return;
            }

            if (info.CompleteNpcId > 0)
            {
                if (zoneNpcId != info.CompleteNpcId)
                {
                    Log.Warn("player {0} complete task npc is {1} not {2}", uid, info.CompleteNpcId, zoneNpcId);
                    return;
                }
                //检查NPC是否包含任务ID
                NPC npc = CurrentMap.GetNpcById(zoneNpcId);
                if (npc == null)
                {
                    Log.Warn("player {0} complete task not find npc {1} in map {2}", uid, zoneNpcId, CurrentMap.MapId);
                    return;
                }
                if (!TaskLibrary.CheckNpcTaskId(taskId, zoneNpcId))
                {
                    Log.Warn("player {0} complete task not find npc {1} task id {2}", uid, zoneNpcId, taskId);
                    return;
                }
            }

            bool canComplete = true;

            //当前章节未开启
            //if (info.Chapter > server.WorldLevelManager.ChapterOpenId)
            //{
            //    //当前提交的任务是当前章节的最后一个任务 下一个章节的任务暂时还没有开启，则不允许提交该任务
            //    canComplete = false;
            //}
            //else if (!ChapterLibrary.CheckLevelLimit(info.Chapter, Level))
            //{
            //    canComplete = false;
            //}
            //else
            //{
            //    if (info.TaskChain > 0)
            //    {
            //        TaskInfo nextInfo = TaskLibrary.GetNextTaskInfo(info.TaskChain, info.Id);
            //        if (nextInfo != null)
            //        {
            //            canComplete = !(nextInfo.Chapter > server.WorldLevelManager.ChapterOpenId);
            //        }
            //    }
            //}

            //if (!canComplete)
            //{
            //    MSG_ZGC_TASK_COMPLETE msg = new MSG_ZGC_TASK_COMPLETE
            //    {
            //        Result = MSG_ZGC_TASK_COMPLETE.Types.RESULT.ChapterBotOpend,
            //        TaskId = taskId
            //    };
            //    Write(msg);
            //    return;
            //}

            //bool getReward = true;
            TaskType taskType = (TaskType)task.ParamType;
            // 接任务类型不提示任务完成
            if (taskType != TaskType.GetTask)
            {
                switch (taskType)
                {
                    case TaskType.Handin:
                        //检查背包中物品数量是否足够
                        if (info.CheckParamKey(TaskParamType.CONSUMABLE))
                        {
                            int itemTypeId = info.GetParamIntValue(TaskParamType.CONSUMABLE);

                            BaseItem item = this.bagManager.GetItem(MainType.Consumable, itemTypeId);
                            if (item == null || item.PileNum < task.ParamNum)
                            {
                                //物品数量不足 type id errpr
                                Log.Warn("player {0} check Handin task complete task id {1} item {2} num is not {3}", Uid, task.Id, itemTypeId, task.ParamNum);
                                return;
                            }
                            else
                            {
                                //TODO:将物品添加到删除队列
                                item = DelItem2Bag(item, RewardType.NormalItem, task.ParamNum, ConsumeWay.TaskItemUse);
                                if (item != null)
                                {
                                    SyncClientItemInfo(item);
                                }
                            }
                        }
                        else
                        {
                            Log.Warn("player {0} check Handin task complete task id {1} not find  ParamKey type id ", Uid, task.Id);
                            return;
                        }
                        break;
                        //case TaskType.Fighting:
                        //    //战斗任务奖励提前给了
                        //    //getReward = false;
                        //    break;
                    default:
                        break;
                }

                //CheckStoryRelation(info);

                //获取任务奖励
                RewardManager rewards = GetSimpleReward(info.DetailReward, ObtainWay.Task);

                //通知客户端获得奖励
                SendTaskRewardMsg(taskId, rewards);

                //komoelog
                long totalPower;
                List<Dictionary<string, object>> heroPosPower = ParseMainHeroPosPowerList(HeroMng.GetHeroPos(), out totalPower);
                List<Dictionary<string, object>> award = ParseRewardInfoToList(rewards.RewardList);
                KomoeEventLogMainTask(info.MainType.ToString(), taskId.ToString(), (int)taskType, 2, heroPosPower, award, GetCoins( CurrenciesType.exp), HeroMng.CalcBattlePower());
            }

            ////生涯 完成章节时间
            //CheckHeroTaskStatData(info);

            //保存任务ID和limit开启
            SaveTaskAndLimitOpen(taskId, info);

            //删除任务
            TaskManagerRemoveTask(removeList, task);

            //接取任务链下一个任务
            if (info.TaskChain > 0)
            {
                TaskInfo nextInfo = TaskLibrary.GetNextTaskInfo(info.TaskChain, info.Id);
                if (nextInfo != null)
                {
                    TaskItem nextTask = TaskMng.AddNewTask(nextInfo);
                    if (nextTask != null)
                    {
                        //记录数据库，通知前端
                        add = nextTask;
                    }
                    else
                    {
                        //出错
                        Log.Warn("player {0} add task list item {1} has same key in list", Uid, nextInfo.Id);
                    }
                    if (nextInfo.MainType == (int)TaskMainType.Main && TaskMng.CurrMaxMainTaskId < nextInfo.Id)
                    {
                        TaskMng.SetCurrFinishedMaxMainTaskId(nextInfo.Id);
                    }
                }
            }

            //TODO:接取分支任务
            if (info.HasBranchTsk)
            {
                foreach (var branchTaskId in info.BranchTasks)
                {
                    TaskInfo branchTaskInfo = TaskLibrary.GetTaskInfoById(branchTaskId);
                    if (branchTaskInfo != null)
                    {
                        TaskItem branchTask = TaskMng.AddNewTask(branchTaskInfo);
                        if (branchTask != null)
                        {
                            //记录数据库，通知前端
                            addList.Add(branchTask);
                        }
                        else
                        {
                            //出错
                            Log.Warn("player {0} add branch task list item {1} has same key in list", Uid, branchTaskInfo.Id);
                        }
                    }
                }
            }

            //更新数据库和通知客户端
            SyncTaskDbAndMessage(add, removeList, addList);

            //章节任务完成检测
            //CheckChapterTask(taskId);

            //开启挂机奖励
            //ChecnkAndOnhookOpen();

            CheckTaskOpenNew(taskId);

            //任务BI
            BIRecordRecordTaskLog(taskType, info.Id, 4);           
            //BI 任务
            KomoeEventLogMissionFlow("任务", taskId, 1, 2, GetCoins(CurrenciesType.exp), HeroMng.CalcBattlePower());
        }

        private void CheckTaskOpenNew(int taskId)
        {
            //章节进度更新，更新挂机奖励
            OnhookManager.CheckAndOpenNew(taskId);

            //推图更新
            pushFigureManager.CheckAndOpenNext(taskId);
        }

        public bool AcceptTask(int taskId, List<int> removeList = null)
        {
            TaskItem task = TaskMng.GetTaskItemForId(taskId);
            if (task != null)
            {
                Log.Warn("player {0} accept task has same task {1} type {2} num {3} time {4}", Uid, taskId, task.ParamType, task.CurNum, task.Time);
                return false;
            }

            TaskInfo info = TaskLibrary.GetTaskInfoById(taskId);
            if (info == null)
            {
                //任务信息未找到
                Log.Warn("player {0} accept task not find task info: {1}", Uid, taskId);
                return false;
            }

            //检查当前身上是否有这个任务链
            Dictionary<int, TaskItem> taskList = TaskMng.GetTaskList();
            foreach (var checkItem in taskList)
            {
                TaskInfo checkInfo = TaskLibrary.GetTaskInfoById(checkItem.Value.Id);
                if (checkInfo != null)
                {
                    if (checkInfo.TaskChain > 0 && checkInfo.TaskChain == info.TaskChain)
                    {
                        //任务信息未找到
                        Log.Warn("player {0} accept task {1} has same TaskChain {2} for task {3}", Uid, taskId, info.TaskChain, checkItem.Value.Id);
                        return false;
                    }
                }
            }

            task = TaskMng.AddNewTask(info);
            if (task == null)
            {
                //任务信息未找到
                Log.Warn("player {0} accept task not find task item {1}.", Uid, taskId);
                return false;
            }

            //TODO 更新数据库
            SyncDbInsertTaskItem(task);

            //TODO 发消息给前台
            SyncTaskChangeMessage(task, null, removeList);

            return true;
        }

        public void TaskSelect(int taskId, int index)
        {
            TaskItem add = null;
            List<int> removeList = new List<int>();

            TaskItem task = TaskMng.GetTaskItemForId(taskId);
            if (task == null)
            {
                //任务信息未找到
                Log.Warn("player {0} task select not find task item {1}.", Uid, taskId);
                return;
            }
            if (task.ParamType != (int)TaskType.Select)
            {
                Log.Warn("player {0} task select task {1} type is {2}.", Uid, taskId, task.ParamType);
                return;
            }

            TaskInfo info = TaskLibrary.GetTaskInfoById(task.Id);
            if (info == null)
            {
                //任务信息未找到
                Log.Warn("player {0} task selectnot find task info: {1}", Uid, task.Id);
                return;
            }

            int nextTaskId = TaskLibrary.GetOptionTaskId(taskId, index);
            if (nextTaskId <= 0)
            {
                Log.Warn("player {0} task {1} select {2} not get next task id {3}", Uid, taskId, index, nextTaskId);
                return;
            }
            else if (nextTaskId > 1)
            {
                TaskInfo nextInfo = TaskLibrary.GetTaskInfoById(nextTaskId);
                if (nextInfo != null)
                {
                    TaskItem nextTask = TaskMng.AddNewTask(nextInfo);
                    if (nextTask != null)
                    {
                        //记录数据库，通知前端
                        add = nextTask;
                    }
                    else
                    {
                        //出错
                        Log.Warn("player {0} task select add task list item {1} has same key in list", Uid, nextInfo.Id);
                        return;
                    }
                }
                else
                {
                    Log.Warn("player {0} task select add task list item {1} not find task ", Uid, nextTaskId);
                    return;
                }
            }

            //获取任务奖励
            RewardManager rewards = GetSimpleReward(info.DetailReward, ObtainWay.Task);

            //通知客户端获得奖励
            SendTaskRewardMsg(taskId, rewards);

            //保存任务ID和limit开启
            SaveTaskAndLimitOpen(taskId, info);

            //删除任务
            TaskManagerRemoveTask(removeList, task);

            // 更新数据库和通知客户端
            SyncTaskDbAndMessage(add, removeList, null);
        }

        private void AcceptEmailTask(EmailItem email, int taskId)
        {
            string[] rewards;
            int task = taskId;
            if (taskId <= 0)
            {
                rewards = email.Rewards.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                task = int.Parse(rewards[0]);
            }
            //添加任务
            AcceptHideTask(task);

            //通知客户端
            SyncGetTaskResult(email, task);
        }

        private void AddHideTasks(RewardManager rewards, RewardResult resulet)
        {
            Dictionary<int, int> taskList = rewards.GetRewardList(RewardType.Task);
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    //添加任务
                    AcceptHideTask(task.Key);
                    resulet.TaskId = task.Key;
                }
            }
        }

        //激活隐藏任务
        private void AcceptHideTask(int taskId)
        {
            List<int> removeIds = new List<int>();
            //删除之前的任务
            Dictionary<int, int> removeList = TaskMng.GetEmailTaskIds((int)TaskMainType.Hide);
            if (removeList != null && removeList.Count > 0)
            {
                foreach (var remove in removeList)
                {
                    //TODO:任务列表中删除任务
                    if (TaskMng.RemoveTaskTypeItem(remove.Value, remove.Key))
                    {
                        SyncDbDeleteTaskItem(remove.Key);

                        removeIds.Add(remove.Key);
                    }
                    else
                    {
                        //删除任务未找到
                        Log.Warn("player {0} accept task remove task not find task id: {1}", Uid, remove.Key);
                    }
                }
            }

            //添加任务
            AcceptTask(taskId, removeIds);
        }

        /// <summary>
        /// 整个任务类型计数
        /// </summary>
        /// <param name="taskType">任务类型</param>
        /// <param name="num">增加的计数</param>
        /// <param name="isAdd">true为添加num，false为赋值</param>
        /// <param name="obj">额外参数</param>
        public void AddTaskNumForType(TaskType taskType, int num = 1, bool isAdd = true, object obj = null)
        {
            int type = (int)taskType;
            List<TaskItem> updateList = new List<TaskItem>();
            //List<int> erroList = new List<int>();
            List<int> list = TaskMng.GetTaskItemForType(type);
            if (list != null && list.Count > 0)
            {
                foreach (var taskId in list)
                {
                    TaskItem taskItem = CheckTaskAddNum(taskId, type, num, isAdd, obj);
                    if (taskItem != null)
                    {
                        updateList.Add(taskItem);
                    }
                }
            }
            else
            {
                //没有指定类型任务
                return;
            }
            if (updateList.Count > 0)
            {
                //TODO 记录数据库
                foreach (var item in updateList)
                {
                    SyncDbUpdateTaskItem(item);
                }
                //TODO 发消息给前台
                SyncTaskChangeMessage(null, updateList, null);
            }
        }

        /// <summary>
        /// 指定任务计数
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="type">任务类型</param>
        /// <param name="num">增加的计数</param>
        /// <param name="isAdd">true为添加num，false为赋值</param>
        /// <param name="obj">额外参数</param>
        public void AddTaskNumForId(int taskId, int type, int num = 1, bool isAdd = true, object obj = null)
        {
            List<TaskItem> updateList = new List<TaskItem>();
            //List<int> erroList = new List<int>();
            TaskItem taskItem = CheckTaskAddNum(taskId, type, num, isAdd, obj);
            if (taskItem != null)
            {
                updateList.Add(taskItem);
            }
            if (updateList.Count > 0)
            {
                //TODO 记录数据库
                foreach (var item in updateList)
                {
                    SyncDbUpdateTaskItem(item);
                }
                //TODO 发消息给前台
                SyncTaskChangeMessage(null, updateList, null);
            }
        }

        private TaskItem CheckTaskAddNum(int taskId, int type, int num, bool isAdd, object obj)
        {
            TaskItem taskItem = TaskMng.GetTaskItemForId(taskId);
            if (taskItem != null)
            {
                TaskInfo info = TaskLibrary.GetTaskInfoById(taskId);
                if (info != null)
                {
                    CheckTaskErrorType errorCode = TaskMng.CheckTaskAdd(taskItem, info, obj);
                    switch (errorCode)
                    {
                        case CheckTaskErrorType.Success:
                            {
                                //特殊任务需要直接取值
                                num = TaskMng.ChangeTaskAddNum(info, num);
                                //更新任务
                                if (TaskMng.SetTaskNum(taskItem, num, isAdd))
                                {
                                    //有变动需要通知客户端
                                    return taskItem;
                                }
                                else
                                {
                                    //没有变动不需要发消息
                                    return null;
                                }
                            }
                        case CheckTaskErrorType.NoChange:
                            //没有变动不需要发消息
                            return null;
                        default:
                            //错误
                            Log.Warn("player {0} add task {1} item {2} num {3} error code {4}.", Uid, taskItem.ParamType, taskId, num, errorCode);
                            break;
                    }
                }
                else
                {
                    Log.Warn("player {0} add task {1} num {2} not find task info {3}.", Uid, taskItem.ParamType, num, taskId);
                }
            }
            else
            {
                //ERROR
                Log.Warn("player {0} add task {1} num {2} not find task item {3}.", Uid, taskItem.ParamType, num, taskId);
                TaskMng.RemoveTaskTypeItem(type, taskId);
            }
            return null;
        }

        public void SetMainTaskId(int taskId)
        {
            MainTaskId = taskId;
            ChapterId = TaskLibrary.GetTaskChapter(MainTaskId);
        }

        private void SaveMainTaskId(int id)
        {
            if (MainTaskId < id)
            {
                MainTaskId = id;
                //更新到数据库
                //string tableName = "character";
                server.GameDBPool.Call(new QueryTaskId(uid, id));

                CheckChapterChange();
            }

            //玩家行为
            RecordAction(ActionType.MainTask, MainTaskId);

            //检查触发养成礼包
            CheckTriggerCultivateGift(TriggerGiftType.MainTask, MainTaskId);
        }

        public void CheckChapterChange()
        {
            int chapterId = TaskLibrary.GetTaskChapter(MainTaskId);
            if (chapterId > this.ChapterId)
            {
                this.ChapterId = chapterId;

                MSG_ZR_UPDATE_CHAPTERID msg = new MSG_ZR_UPDATE_CHAPTERID()
                {
                    ChapterId = chapterId,
                };
                server.RelationServer.Write(msg, uid);

                //成长基金
                AddActivityNumForType(ActivityAction.GrowthFund, ChapterId - 1);
                AddActivityNumForType(ActivityAction.GrowthFundEx, ChapterId - 1);
            }
        }

        public void SaveGuideId(int id, int time)
        {
            GuideId = id;
            server.GameDBPool.Call(new QueryGuideId(uid, id));

            KomoeEventLogGuideFlow(id, time);
        }

        public void SaveMainLineId(int id)
        {
            //if (MainLineId < id)
            //{
            MainLineId = id;
            //更新到数据库
            //string tableName = "character";
            server.GameDBPool.Call(new QueryMainLineId(uid, id));
            //}
        }

        private void SaveBranchTaskIds(int id)
        {
            if (!BranchTaskIds.Contains(id))
            {
                BranchTaskIds.Add(id);
                string brachStr = string.Empty;
                foreach (var taskId in BranchTaskIds)
                {
                    brachStr += string.Format("{0}|", taskId);
                }
                //更新到数据库
                //string tableName = "character";
                server.GameDBPool.Call(new QueryBranchTaskIds(uid, brachStr));

                //检查触发养成礼包
                CheckTriggerCultivateGift(TriggerGiftType.BranchTask, id);
            }
        }

        private void SaveTaskAndLimitOpen(int taskId, TaskInfo info)
        {
            TaskMainType tagType = (TaskMainType)info.MainType;
            switch (tagType)
            {
                case TaskMainType.Main:
                    //保存剧情任务ID
                    SaveMainTaskId(taskId);
                    //检查任务开启
                    CheckTaskIdLimitOpen(taskId);
                    //任务开启福利邮件
                    AddWelfareTriggerItem(WelfareConditionType.Task, taskId);
                    break;
                case TaskMainType.Branch:
                    //判断是否是limit task
                    if (LimitLibrary.CheckBranchTaskId(taskId))
                    {
                        //保存剧情任务ID
                        SaveBranchTaskIds(taskId);
                        //检查任务开启
                        CheckBranchTaskIdLimitOpen(taskId);
                    }
                    break;
                default:
                    break;
            }
        }

        private void TaskManagerRemoveTask(List<int> removeList, TaskItem task)
        {
            //任务列表中删除任务
            if (TaskMng.RemoveTaskTypeItem(task.ParamType, task.Id))
            {
                //记录数据库，通知前端
                removeList.Add(task.Id);
            }
            else
            {
                //删除任务未找到
                Log.Warn("player {0} remove task not find task id: {1}", Uid, task.Id);
            }
        }

        private void SyncTaskDbAndMessage(TaskItem add, List<int> removeList, List<TaskItem> addList)
        {
            //TODO 更新数据库
            if (add != null && removeList.Count > 0)
            {
                SyncDbReplaceTaskItem(add, removeList[0]);
            }
            else if (add == null && removeList.Count > 0)
            {
                SyncDbDeleteTaskItem(removeList[0]);
            }
            else if (add != null && removeList.Count <= 0)
            {
                SyncDbInsertTaskItem(add);
            }
            if (addList != null && addList.Count > 0)
            {
                foreach (var item in addList)
                {
                    SyncDbInsertTaskItem(item);
                }
            }
            //TODO 发消息给前台
            SyncTaskChangeMessage(add, null, removeList, addList);
        }

        private void SendTaskRewardMsg(int taskId, RewardManager rewards)
        {
            MSG_ZGC_TASK_COMPLETE msg = new MSG_ZGC_TASK_COMPLETE();
            msg.Result = MSG_ZGC_TASK_COMPLETE.Types.RESULT.Success;
            msg.TaskId = taskId;

            rewards.GenerateRewardItemInfo(msg.Rewards);
            Write(msg);
        }

        //private void SendCompleteTaskMessage(int  rewards)
        //{
        //    MSG_ZGC_TASK_COMPLETE msg = new MSG_ZGC_TASK_COMPLETE();
        //    msg.Result = MSG_ZGC_TASK_COMPLETE.Types.RESULT.Success;
        //    if (rewards.AllRewards.Count > 0)
        //    {
        //        foreach (var item in rewards.AllRewards)
        //        {
        //            msg.Ids.Add(item.Key);
        //            msg.Nums.Add(item.Value);
        //        }
        //    }
        //    Write(msg);
        //}

        //private void CheckStoryRelation(TaskInfo info)
        //{
        //    if (info.CheckParamKey(TaskParamType.STORY_ID))
        //    {
        //        int storyId = info.GetParamIntValue(TaskParamType.STORY_ID);

        //        //SetFinishStory(storyId);

        //        //AddActivityNumForType(ActivityAction.StoryGameLevel, storyId);
        //    }
        //}

        //private void CheckHeroTaskStatData(TaskInfo info)
        //{
        //    if (info.CheckParamKey(TaskParamType.END_TIME))
        //    {
        //        string stat = info.GetParamValue(TaskParamType.END_TIME);
        //        //SetChapterEndTime(stat);
        //    }
        //}

        //public int GetHeroTaskId(int heroId, int index)
        //{
        //    int taskId = 0;
        //    Data data = DataListManager.inst.GetData("HeroCard", heroId);
        //    if (data != null)
        //    {
        //        switch (index)
        //        {
        //            case 0:
        //                taskId = data.GetInt("Story0");
        //                break;
        //            case 1:
        //                taskId = data.GetInt("Story1");
        //                break;
        //            case 2:
        //                taskId = data.GetInt("Story2");
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    return taskId;
        //}

        public int GetTaskEmailId()
        {
            int emailId = 0;

            Dictionary<int, int> groups = EmailLibrary.GetEmailTaskLevelGroups(Level);

            EmailInfo info;
            TaskItem item;
            int i = 0;
            while (i < 10)
            {
                i++;
                //int taskId = TaskMng.GetTaskItemForId();
                int groupId = ScriptManager.Task.GetGroupId(groups);
                if (groupId > 0)
                {
                    List<int> emailIds = EmailLibrary.GetEmailGroupTasks(groupId);
                    if (emailIds != null)
                    {
                        emailId = ScriptManager.Task.GetEmailId(emailIds);

                        info = EmailLibrary.GetEmailInfo(emailId);
                        if (info != null)
                        {
                            if (info.TaskId > 0)
                            {
                                item = TaskMng.GetTaskItemForId(info.TaskId);
                                if (item == null)
                                {
                                    return emailId;
                                }
                                else
                                {
                                    Log.Warn("player {0} GetTaskEmailId  email info {1} has task item id {2}", Uid, emailId, info.TaskId);
                                    continue;
                                }
                            }
                            else
                            {
                                Log.Warn("player {0} GetTaskEmailId  email info {1} task id {2} error", Uid, emailId, info.TaskId);
                                return 0;
                            }
                        }
                        else
                        {
                            Log.Warn("player {0} GetTaskEmailId not find email info {1}", Uid, emailId);
                            return 0;
                        }
                    }
                    else
                    {
                        Log.Warn("player {0} GetTaskEmailId not find group id {1} email list", Uid, groupId);
                        return 0;
                    }
                }
                else
                {
                    Log.Warn("player {0} GetTaskEmailId not find group id {1}", Uid, groupId);
                    return 0;
                }
            }
            return 0;
        }
        public List<MSG_ZGC_TASK_INFO> GetTaskListMessage()
        {
            List<MSG_ZGC_TASK_INFO> list = new List<MSG_ZGC_TASK_INFO>();
            Dictionary<int, TaskItem> taskinfo = TaskMng.GetTaskList();
            foreach (var task in taskinfo)
            {
                list.Add(GetTaskInfo(task.Value));
            }
            return list;
        }

        private MSG_ZGC_TASK_INFO GetTaskInfo(TaskItem task)
        {
            MSG_ZGC_TASK_INFO info = new MSG_ZGC_TASK_INFO();
            info.Id = task.Id;
            info.ParamType = task.ParamType;
            info.ParamNum = task.ParamNum;
            info.CurNum = task.CurNum;
            info.Time = task.Time;
            info.MainType = task.MainType;
            return info;
        }

        public void SyncDbUpdateTaskItem(TaskItem task)
        {
            //string tableName = "task_current";
            server.GameDBPool.Call(new QueryUpdateTaskInfo(Uid, task.Id, task.CurNum));
        }

        public void SyncDbReplaceTaskItem(TaskItem task, int oldTaskId)
        {
            //string tableName = "task_current";
            server.GameDBPool.Call(new QueryReplaceTaskInfo(Uid, task.Id, task.ParamType, task.ParamNum, task.CurNum, task.Time, oldTaskId));
        }

        public void SyncDbDeleteTaskItem(int taskId)
        {
            //string tableName = "task_current";
            server.GameDBPool.Call(new QueryDeleteTaskInfo(Uid, taskId));
        }

        public void SyncDbInsertTaskItem(TaskItem task)
        {
            //string tableName = "task_current";
            server.GameDBPool.Call(new QueryInsertTaskInfo(Uid, task));
        }

        public void SyncTaskChangeMessage(TaskItem add, List<TaskItem> updateList, List<int> removeList, List<TaskItem> addList = null)
        {
            MSG_ZGC_TASK_CHANGE msg = new MSG_ZGC_TASK_CHANGE();
            if (add != null)
            {
                msg.AddList.Add(GetTaskInfo(add));
            }
            if (updateList != null)
            {
                foreach (var task in updateList)
                {
                    msg.UpdateList.Add(GetTaskInfo(task));
                }
            }
            if (removeList != null)
            {
                foreach (var task in removeList)
                {
                    msg.RemoveList.Add(task);
                }
            }
            if (addList != null)
            {
                foreach (var task in addList)
                {
                    msg.AddList.Add(GetTaskInfo(task));
                }
            }
            Write(msg);
        }

        private bool IsGetTaskEmailTimeOut(EmailItem email, int taskId)
        {
            MSG_ZGC_GET_TASK_RESULT msg = new MSG_ZGC_GET_TASK_RESULT();
            string[] rewards;
            int task = taskId;
            if (taskId <= 0)
            {
                rewards = email.Rewards.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                task = int.Parse(rewards[0]);
            }

            if (CheckEmailTimeout(email))
            {
                msg.Result = MSG_ZGC_GET_TASK_RESULT.Types.RESULT.Error;
                msg.TaskId = task;
                Write(msg);
                return true;
            }
            else
            {
                return false;
            }

        }


        #region 日周任务完成进度

        public void TaskFinishStateReward(TaskFinishType type, int index)
        {
            MSG_ZGC_TASK_FINISH_STATE_REWARD msg = new MSG_ZGC_TASK_FINISH_STATE_REWARD();

            var model = TaskLibrary.GetActivityFinishRewardModel(type, taskFinishState.Period, index - 1);
            if (model == null || model.Period != taskFinishState.Period)
            {
                msg.Result = (int)ErrorCode.HaveNoThisReward;
                Write(msg);
                return;
            }

            CheckTaskPeriod();

            if (CheckRewarded(type == TaskFinishType.Daily, index))
            {
                msg.Result = (int)ErrorCode.ActivityFinishStateHadReward;
                Write(msg);
                return;
            }

            if (!CheckActivityFinishNum(type, model.Num))
            {
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            UpdateTaskFinishReward(type, index);

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(model.Reward);
            AddRewards(manager, ObtainWay.Activity);

            manager.GenerateRewardMsg(msg.Rewards);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public bool CheckRewarded(bool isDaily, int index)
        {
            return isDaily ? taskFinishState.DailyRewarded.Contains(index) : taskFinishState.WeeklyRewarded.Contains(index);
        }

        public bool CheckActivityFinishNum(TaskFinishType type, int limit)
        {
            switch (type)
            {
                case TaskFinishType.Daily:
                    return taskFinishState.DailyNum >= limit;
                case TaskFinishType.Weekly:
                    return taskFinishState.WeeklyNum >= limit;
            }
            return false;
        }

        public void UpdateTaskFinishNum(PassTaskLoopType loopType, bool synDb = true)
        {
            switch (loopType)
            {
                case PassTaskLoopType.Daily:
                    AddTaskDailyFinishNum(1);
                    break;
                case PassTaskLoopType.WeekLy:
                    AddTaskWeeklyFinishNum(1);
                    break;
            }
            if (synDb)
            { 
                SyncTaskFinishStateToDBAndClient();
            }
        }

        private  void UpdateTaskFinishReward(TaskFinishType type, int index)
        {
            switch (type)
            {
                case TaskFinishType.Daily:
                    taskFinishState.DailyRewarded.Add(index);
                    break;
                case TaskFinishType.Weekly:
                    taskFinishState.WeeklyRewarded.Add(index);
                    break;
            }
            SyncTaskFinishStateToDBAndClient();
        }

        private  void AddTaskDailyFinishNum(int num)
        {
            taskFinishState.DailyNum += 1;
        }

        private  void AddTaskWeeklyFinishNum(int num)
        {
            taskFinishState.WeeklyNum += 1;
        }

        private  void ResetTaskDailyFinishState(bool synDB = true)
        {
            taskFinishState.DailyNum = 0;
            taskFinishState.DailyRewarded.Clear();
            taskFinishState.DailyRefreshTime = server.Now();
            if (synDB)
            {
                SyncTaskFinishStateToDBAndClient();
            }
        }

        private void ResetWeeklyTaskFinishState(bool synDB = true)
        {
            taskFinishState.WeeklyNum = 0;
            taskFinishState.WeeklyRewarded.Clear();
            taskFinishState.WeeklyRefreshTime = server.Now();
            if (synDB)
            {
                SyncTaskFinishStateToDBAndClient();
            }
        }

        /// <summary>
        /// update 20210514周期变动的时候不重置奖励
        /// </summary>
        private void CheckTaskPeriod()
        {
            int period = TaskLibrary.GetPeriodByTime(server.Now());
            if (period != taskFinishState.Period)
            {
                taskFinishState.Period = period;
                //ResetTaskDailyFinishState(false);
                //ResetWeeklyTaskFinishState(false);
                SyncTaskFinishStateToDBAndClient();
            }
        }

        public void SyncTaskFinishStateToDBAndClient()
        {
            SyncDbUpdateTaskFinishState();
            SendTaskFinishState();
        }

        public void SendTaskFinishState()
        {
            MSG_ZGC_TASK_FINISH_STATE msg = new MSG_ZGC_TASK_FINISH_STATE()
            {
                DailyFinishNum = taskFinishState.DailyNum,
                WeeklyFinishNum = taskFinishState.WeeklyNum,
                Period = taskFinishState.Period
            };
            msg.DailyRewardList.Add(taskFinishState.DailyRewarded);
            msg.WeeklyRewardList.Add(taskFinishState.WeeklyRewarded);
            Write(msg);
        }

        public void SyncDbUpdateTaskFinishState()
        {
            server.GameDBPool.Call(new QueryUpdateTaskFinishState(Uid, taskFinishState));
        }

        #endregion


        private void SyncGetTaskResult(EmailItem email, int taskId)
        {
            MSG_ZGC_GET_TASK_RESULT msg = new MSG_ZGC_GET_TASK_RESULT();
            string[] rewards;
            int task = taskId;
            if (taskId <= 0)
            {
                rewards = email.Rewards.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                task = int.Parse(rewards[0]);
            }
            msg.Result = MSG_ZGC_GET_TASK_RESULT.Types.RESULT.Success;
            msg.TaskId = task;
            Write(msg);
        }

        private MSG_ZMZ_TASK_INFO GetTaskTransform()
        {
            MSG_ZMZ_TASK_INFO info = new MSG_ZMZ_TASK_INFO();
            info.Uid = Uid;
            Dictionary<int, TaskItem> taskinfo = TaskMng.GetTaskList();
            foreach (var task in taskinfo)
            {
                ZMZ_TASK_ITEM item = new ZMZ_TASK_ITEM();
                item.Id = task.Value.Id;
                item.ParamType = task.Value.ParamType;
                item.ParamNum = task.Value.ParamNum;
                item.CurNum = task.Value.CurNum;
                item.Time = task.Value.Time;
                info.TaskList.Add(item);
            }
            //Dictionary<int, ActivityItem> activityList = ActivityMng.GetActivityList();
            //foreach (var activity in activityList)
            //{
            //    MSG_ZMZ_ACTIVITY_INFO item = new MSG_ZMZ_ACTIVITY_INFO();
            //    item.Id = activity.Value.Id;
            //    item.CurNum = activity.Value.CurNum;
            //    item.State = activity.Value.State;
            //    item.Param = activity.Value.Param;
            //    info.ActivityList.Add(item);
            //}

            Dictionary<int, WelfareTriggerItem> triggerList = WelfareMng.GetTriggerList();
            foreach (var trigger in triggerList)
            {
                MSG_ZMZ_WELFARE_TRIGGER_INFO item = new MSG_ZMZ_WELFARE_TRIGGER_INFO();
                item.Id = trigger.Value.Id;
                item.StartTime = trigger.Value.StartTime;
                item.EndTime = trigger.Value.EndTime;
                item.State = (int)trigger.Value.State;
                info.WelfareTriggerList.Add(item);
            }

            MSG_ZMZ_TASK_FINISH_STATE activityFinishStateInfo = new MSG_ZMZ_TASK_FINISH_STATE()
            {
                DailyFinishNum = taskFinishState.DailyNum,
                WeeklyFinishNum = taskFinishState.WeeklyNum,
                Period = taskFinishState.Period,
                DailyRefreshTime = Timestamp.GetUnixTimeStampSeconds(taskFinishState.DailyRefreshTime),
                WeeklyRefreshTime = Timestamp.GetUnixTimeStampSeconds(taskFinishState.WeeklyRefreshTime),
            };

            activityFinishStateInfo.DailyRewardList.AddRange(taskFinishState.DailyRewarded);
            activityFinishStateInfo.WeeklyRewardList.AddRange(taskFinishState.WeeklyRewarded);
            info.ActivityFinishState = activityFinishStateInfo;

            return info;
        }

        public void LoadTaskTransform(MSG_ZMZ_TASK_INFO info)
        {
            foreach (var item in info.TaskList)
            {
                TaskInfo task = TaskLibrary.GetTaskInfoById(item.Id);
                if (task == null)
                {
                    //任务信息未找到
                    Log.Warn("player {0} InitTaskItemList not find task info: {1}", Uid, item.Id);
                    continue;
                }
                TaskItem ti = new TaskItem();
                ti.Id = item.Id;
                ti.ParamType = item.ParamType;
                ti.ParamNum = item.ParamNum;
                ti.CurNum = item.CurNum;
                ti.Time = item.Time;

                ti.MainType = task.MainType;
                TaskMng.AddTaskListItem(ti);
            }

            taskFinishState = new TaskFinishState()
            {
                DailyNum = info.ActivityFinishState.DailyFinishNum,
                WeeklyNum = info.ActivityFinishState.WeeklyFinishNum,
                DailyRefreshTime = Timestamp.TimeStampToDateTime(info.ActivityFinishState.DailyRefreshTime),
                WeeklyRefreshTime = Timestamp.TimeStampToDateTime(info.ActivityFinishState.WeeklyFinishNum),
            };
            taskFinishState.DailyRewarded.AddRange(info.ActivityFinishState.DailyRewardList);
            taskFinishState.WeeklyRewarded.AddRange(info.ActivityFinishState.WeeklyRewardList);
            taskFinishState.Period = info.ActivityFinishState.Period;
        }

    }
}
