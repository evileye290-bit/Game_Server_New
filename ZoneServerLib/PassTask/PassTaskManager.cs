using CommonUtility;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class PassTaskManager
    {
        public PlayerChar Owner { get; set; }

        private bool dbAligned { get; set; }
        //private bool dbAligning { get; set; }
        private bool needDailyRefresh { get; set; }
        private bool needWeeklyRefresh { get; set; }
        private bool needPanelInfo { get; set; }

        public int CurPeriod
        { get; private set; }

        public bool ChangePeriod = false;


        /// <summary>
        /// 所有自己的任务，和配表其实一致，不一致需要对齐
        /// </summary>
        Dictionary<int, PassCardTaskItem> taskItemList = new Dictionary<int, PassCardTaskItem>();

        /// <summary>
        /// 维护任务类型关系表
        /// </summary>
        //Dictionary<TaskType, List<int>> taskTypeList = new Dictionary<TaskType, List<int>>();

        SortedDictionary<int, bool> rewardedLevel = new SortedDictionary<int, bool>();

        string rewardedLevels = ""; // 领过奖的等级
        string superRewardedLevels = "";
        DateTime rewardedTime; //每日奖励的上次领奖时间
        string passcards = ""; //存储买过的通行证期数
        int passcardLevel;// 当前通行证等级
        int Exp;//当前通行证经验累积      -----每次动经验货币动一下这里

        SortedSet<int> rewardedSet = new SortedSet<int>();
        SortedSet<int> superRewardedSet = new SortedSet<int>();

        SortedSet<int> BoughtedPeriods = new SortedSet<int>();

        public PassTaskManager(PlayerChar oneself)
        {
            Owner = oneself;
            taskItemList.Clear();

            if (PassCardLibrary.DefaultBought)
            {
                for (int i = 1; i < 100; i++)
                {
                    BoughtedPeriods.Add(i);
                }
            }
        }

        public void InitTasks(bool checkAlign)
        {
            if (checkAlign)
            {
                CheckAlign();
            }
        }

        public bool IsSuper()
        {
            return BoughtedPeriods.Contains(CurPeriod);
        }

        public void InitPasscard(string passcard, int passcardLevel, int passcardExp, int period = 1)
        {
            this.passcardLevel = passcardLevel;
            this.passcards = passcard;
            CurPeriod = period;
            //BoughtedPeriods
            string[] temps = passcard.Split('|');
            foreach (var item in temps)
            {
                BoughtedPeriods.Add(item.ToInt());
            }
            Exp = passcardExp;
        }

        public string GeneratePasscardString()
        {
            string passcard = "";
            foreach (var item in BoughtedPeriods)
            {
                passcard += "|" + item;
            }
            return passcard;
        }

        public void SendPanelInfo()
        {
            Owner.Write(GeneratePanelInfo());
        }

        public MSG_ZGC_PASSCARD_PANEL_INFO GeneratePanelInfo()
        {
            MSG_ZGC_PASSCARD_PANEL_INFO info = new MSG_ZGC_PASSCARD_PANEL_INFO();
            info.BoughtPasscard = BoughtedPeriods.Contains(CurPeriod);
            info.CurPeriod = CurPeriod;
            info.Level = passcardLevel;
            info.DailyEndTime = Timestamp.GetUnixTimeStampSeconds(ServerFrame.BaseApi.now.Date.AddDays(1));
            info.DailyRewarded = rewardedTime.Date < ServerFrame.BaseApi.now.Date;
            info.Exp = Exp;
            DateTime today = ServerFrame.BaseApi.now.Date;
            info.PasscardEndTime = Timestamp.GetUnixTimeStampSeconds((PassCardLibrary.GetEndTime(Owner.server.OpenServerTime)));

            int addDays = today.DayOfWeek == 0 ? 1 : (8 - (int)today.DayOfWeek);
            DateTime weekEnd = today.AddDays(addDays);
            info.WeeklyEndTime = Timestamp.GetUnixTimeStampSeconds(weekEnd);
            foreach (var item in rewardedSet)
            {
                info.NormalRewarded.Add(item);
            }
            foreach (var item in superRewardedSet)
            {
                info.SuperRewarded.Add(item);
            }

            GetTaskInfos(info.Tasks);

            if (ChangePeriod)
            {
                ChangePeriod = false;
                info.ChangePeriod = true;
            }
            else
            {
                info.ChangePeriod = false;
            }
            return info;
        }

        private void GetTaskInfos(RepeatedField<MSG_ZGC_PASSCARD_TASK> taskInfos)
        {
            foreach (var kv in taskItemList)
            {
                PassCardTaskItem item = kv.Value;
                taskInfos.Add(GenerateTaskMsg(item));
            }
        }

        public MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT GetAllLevelReward(MSG_GateZ_GET_PASSCARD_LEVEL_REWARD info)
        {
            MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT res = new MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT();
            res.GetAll = info.GetAll;
            foreach (var item in info.RewardLevels)
            {
                res.RewardLevels.Add(item);
            }

            res.ErrorCode = (int)ErrorCode.Success;
            res.IsSuper = info.IsSuper;

            List<string> rewardStrings = PassCardLibrary.GetLeftLevelRewardUnderOrEqual(rewardedSet, passcardLevel, false);

            if (IsSuper())
            {
                List<string> temps = PassCardLibrary.GetLeftLevelRewardUnderOrEqual(superRewardedSet, passcardLevel, true);
                foreach (var item in temps)
                {
                    rewardStrings.Add(item);
                }
            }

            if (rewardStrings.Count > 0)
            {
                foreach (var rewardString in rewardStrings)
                {
                    RewardManager manager = new RewardManager();
                    manager.InitSimpleReward(rewardString);
                    Owner.AddRewards(manager, ObtainWay.PassTaskLevelReward);
                    manager.GenerateRewardItemInfo(res.Rewards);
                }

                RecordeLevelReward(1, info.IsSuper, true);
                SyncRewardInfo2DB();
            }
            else
            {
                Log.Warn($"player {Owner.Uid} get passcard all level reward failed: no reward info");
                res.ErrorCode = (int)ErrorCode.Already;
            }


            return res;
        }

        public MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT GetLevelReward(MSG_GateZ_GET_PASSCARD_LEVEL_REWARD info)
        {
            MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT res = new MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT();
            res.GetAll = info.GetAll;
            foreach (var item in info.RewardLevels)
            {
                res.RewardLevels.Add(item);
            }
            res.IsSuper = info.IsSuper;

            SortedSet<int> rewardDic;
            if (info.IsSuper)
            {
                rewardDic = superRewardedSet;
            }
            else
            {
                rewardDic = rewardedSet;
            }

            if (info.IsSuper && !IsSuper())
            {
                res.ErrorCode = (int)ErrorCode.NotAllowed;
            }
            else if (info.RewardLevels.Count < 1)
            {
                res.ErrorCode = (int)ErrorCode.NoData;
            }
            else if (info.RewardLevels.Count == 1 && rewardDic.Contains(info.RewardLevels.First()))
            {
                res.ErrorCode = (int)ErrorCode.Already;
            }
            else if (info.RewardLevels.Any(item => rewardDic.Contains(item)))
            {
                res.ErrorCode = (int)ErrorCode.Already;
            }
            else if (passcardLevel < info.RewardLevels.Max())
            {
                res.ErrorCode = (int)ErrorCode.NotAllowed;
            }
            else
            {
                res.ErrorCode = (int)ErrorCode.Success;
                Dictionary<int, string> rewardStrings = new Dictionary<int, string>();
                foreach (int item in info.RewardLevels)
                {
                    string rewardString = PassCardLibrary.GetLevelReward(item, info.IsSuper);
                    rewardStrings.Add(item, rewardString);
                }
                foreach (var kv in rewardStrings)
                {
                    string rewardString = kv.Value;
                    if (!string.IsNullOrEmpty(rewardString))
                    {
                        RewardManager manager = new RewardManager();
                        manager.InitSimpleReward(rewardString);
                        Owner.AddRewards(manager, ObtainWay.PassTaskLevelReward);
                        manager.GenerateRewardItemInfo(res.Rewards);

                        RecordeLevelReward(kv.Key, info.IsSuper, false);
                    }
                    else
                    {
                        res.ErrorCode = (int)ErrorCode.NoData;
                    }
                }

                SyncRewardInfo2DB();

            }
            if (res.ErrorCode != (int)ErrorCode.Success)
            {
                Log.Warn($"player {Owner.Uid} get passcard level reward failed: errorCode {res.ErrorCode}");
            }
            return res;
        }

        public void RecordeLevelReward(int rewardLevel, bool isSuper = false, bool all = false)
        {
            // update memory
            SortedSet<int> rewardDic;


            if (all)
            {
                if (IsSuper())
                {
                    superRewardedSet = PassCardLibrary.GetAllRewardLevelUnderOrEqual(passcardLevel, true);
                }
                rewardedSet = PassCardLibrary.GetAllRewardLevelUnderOrEqual(passcardLevel, false);
                rewardedLevels = GetRewardLevelString(false);
                superRewardedLevels = GetRewardLevelString(true);
            }
            else
            {
                if (isSuper)
                {
                    rewardDic = superRewardedSet;
                    rewardDic.Add(rewardLevel);
                    superRewardedLevels = GetRewardLevelString(isSuper);
                }
                else
                {
                    rewardDic = rewardedSet;
                    rewardDic.Add(rewardLevel);
                    rewardedLevels = GetRewardLevelString(isSuper);
                }

            }
        }

        public string GetRewardLevelString(bool isSuper)
        {
            string rewards = "";
            if (!isSuper)
            {
                foreach (var item in rewardedSet)
                {
                    rewards += "|" + item;
                }
            }
            else
            {
                foreach (var item in superRewardedSet)
                {
                    rewards += "|" + item;
                }
            }
            return rewards;
        }

        public MSG_ZGC_PASSCARD_DAILY_REWARD_RESULT GetDailyReward(MSG_GateZ_GET_PASSCARD_DAILY_REWARD info)
        {
            MSG_ZGC_PASSCARD_DAILY_REWARD_RESULT res = new MSG_ZGC_PASSCARD_DAILY_REWARD_RESULT();

            //rewardedTime
            DateTime now = ServerFrame.BaseApi.now.Date;
            if (rewardedTime.Date < ServerFrame.BaseApi.now.Date)
            {
                string reward = PassCardLibrary.GetDailyRewardString(passcardLevel);
                RewardManager manager = new RewardManager();
                manager.InitSimpleReward(reward);
                Owner.AddRewards(manager, ObtainWay.PassTaskDailyReward);
                manager.GenerateRewardItemInfo(res.Rewards);
                res.ErrorCode = (int)ErrorCode.Success;
                rewardedTime = ServerFrame.BaseApi.now;
            }
            else
            {
                res.ErrorCode = (int)ErrorCode.Already;
                Log.Warn($"player {Owner.Uid} get daily reward failed: already get");
            }

            SyncRewardInfo2DB();
            return res;

        }

        public MSG_ZGC_PASSCARD_RECHARGE_LEVEL_RESULT BuyPasscardLevel(MSG_GateZ_GET_PASSCARD_RECHARGED_LEVEL info)
        {
            int level = info.Level;
            int rechargeId = info.RechargeId;
            MSG_ZGC_PASSCARD_RECHARGE_LEVEL_RESULT res = new MSG_ZGC_PASSCARD_RECHARGE_LEVEL_RESULT();
            res.RechargeId = info.RechargeId;
            PassRechargeLevel temp = PassCardLibrary.GetRechargeLevel(rechargeId);
            if (temp != null && (temp.AddLevel + passcardLevel) > PassCardLibrary.MaxPassLevel)
            {
                res.ErrorCode = (int)ErrorCode.MaxPasscardLevel;
                res.LevelAdded = 0;
                return res;
            }

            if (temp == null)
            {
                res.ErrorCode = (int)ErrorCode.NoData;
                res.LevelAdded = 0;
            }
            else
            {
                res.ErrorCode = (int)ErrorCode.Success;
                res.LevelAdded = temp.AddLevel;
                //exp level db

                int tempAddExp = PassCardLibrary.GetRechargeLevelExp(passcardLevel, temp.AddLevel);
                string rewardString = ((int)CurrenciesType.PasscardExp) + ":1:" + tempAddExp;
                RewardManager manager = Owner.GetSimpleReward(rewardString, ObtainWay.PassCardTask);
                //Owner.AddCoins(CurrenciesType.PasscardExp, tempAddExp, ObtainWay.PassCardBuyExp);
                passcardLevel += temp.AddLevel;
                SyncCharacreInfo2DB();
            }

            res.CurExp = Exp;
            return res;
        }

        public MSG_ZGC_PASSCARD_RECHARGE_RESULT BuyPasscard()
        {
            MSG_ZGC_PASSCARD_RECHARGE_RESULT res = new MSG_ZGC_PASSCARD_RECHARGE_RESULT();

            if (BoughtedPeriods.Contains(CurPeriod))
            {
                res.BoughtPasscard = false;
                res.ErrorCode = (int)ErrorCode.Already;
            }
            else
            {
                res.BoughtPasscard = true;
                res.ErrorCode = (int)ErrorCode.Success;

                //todo update db
                BoughtedPeriods.Add(CurPeriod);
                passcards = GeneratePasscardString();
                SyncCharacreInfo2DB();
            }
            return res;
        }

        public MSG_ZGC_UPDATE_PASSCARD_TASK CompleteAllTask()
        {
            MSG_ZGC_UPDATE_PASSCARD_TASK msg = new MSG_ZGC_UPDATE_PASSCARD_TASK();
            PassCardTaskItem task = null;
            List<PassCardTaskItem> items = new List<PassCardTaskItem>();
            int num = 0;
            string rewardString = "";
            int power = Owner.HeroMng.CalcBattlePower();
            foreach (var kv in taskItemList)
            {
                task = kv.Value;
                PassTask temp = PassCardLibrary.GetTask(task.Id);
                if (temp == null)
                {
                    Log.Warn($"palyer {Owner.Uid} CompleteAllTask not find {task.Id} ");
                    continue;
                }
                if (!task.Rewarded && task.CurNum >= task.ParamNum && temp.AvailableLevel <= passcardLevel)
                {
                    //PassTask temp = PassCardLibrary.GetTask(task.Id);
                    //exp level db client
                    //Exp += task.Exp;
                    rewardString += "|" + temp.Reward;
                    num++;
                    task.Rewarded = true;

                    items.Add(task);
                    msg.Tasks.Add(GenerateTaskMsg(task));

                    //任务BI
                    Owner.BIRecordRecordTaskLog((TaskType)temp.PassTaskType, task.Id, 4);

                    //BI 任务
                    Owner.KomoeEventLogMissionFlow("魂师手札", task.Id, 3, 2, Owner.GetCoins(CurrenciesType.exp), power);

                    //统计任务完成数量
                    Owner.UpdateTaskFinishNum((PassTaskLoopType)temp.LoopType, false);

                    Owner.AddRunawayActivityNumForType(RunawayAction.DailyTask);
                }
            }

            if (!string.IsNullOrEmpty(rewardString))
            {
                //获取奖励
                RewardManager manager = Owner.GetSimpleReward(rewardString, ObtainWay.PassCardTask);
                manager.GenerateRewardItemInfo(msg.Rewards);
            }

            SyncPassCardTasks2DB(items);

            msg.ErrorCode = (int)ErrorCode.Success;
            msg.ErrorCode = CheckMaxLevel();
            CheckLevelUp();
            SyncCharacreInfo2DB();
            msg.Exp = Exp;
            msg.Level = passcardLevel;
            if (num > 0)
            {
                Owner.AddTaskNumForType(TaskType.DailyTaskCount, num);

                //限时礼包
                Owner.ActionManager.RecordActionAndCheck(ActionType.DailyTaskFinishCount, num);
            }

            Owner.SyncTaskFinishStateToDBAndClient();

            return msg;
        }

        public int AddExp(int add)
        {
            int max = PassCardLibrary.GetMaxLevelExp();
            if ((Exp + add) > max)
            {
                add = max - Exp;
                Exp = max;
                return add;
            }
            Exp += add;
            return add;
        }

        public MSG_ZGC_UPDATE_PASSCARD_TASK CompleteTask(int taskId)
        {
            MSG_ZGC_UPDATE_PASSCARD_TASK msg = new MSG_ZGC_UPDATE_PASSCARD_TASK();
            msg.ErrorCode = (int)ErrorCode.Success;
            PassCardTaskItem task = null;
            if (taskItemList.TryGetValue(taskId, out task))
            {

                if (!task.Rewarded && task.CurNum >= task.ParamNum)
                {
                    PassTask temp = PassCardLibrary.GetTask(taskId);
                    if (temp == null)
                    {
                        Log.Warn($"palyer {Owner.Uid} CompleteTask not find {task.Id} ");
                        msg.ErrorCode = (int)ErrorCode.Fail;
                    }
                    //exp level db client
                    //Exp += task.Exp;
                    string rewardString = temp.Reward;
                    if (!string.IsNullOrEmpty(rewardString))
                    {
                        //获取奖励
                        RewardManager manager = Owner.GetSimpleReward(rewardString, ObtainWay.PassCardTask);
                        manager.GenerateRewardItemInfo(msg.Rewards);
                    }
                    task.Rewarded = true;
                    msg.ErrorCode = CheckMaxLevel();
                    CheckLevelUp();
                    SyncCharacreInfo2DB();
                    SyncPassCardTasks2DB(new List<PassCardTaskItem>() { task });
                    msg.Tasks.Add(GenerateTaskMsg(task));

                    Owner.AddTaskNumForType(TaskType.DailyTaskCount);
                    //任务BI
                    Owner.BIRecordRecordTaskLog((TaskType)temp.PassTaskType, taskId, 4);
                    //BI 任务
                    Owner.KomoeEventLogMissionFlow("魂师手札", task.Id, 3, 2, Owner.GetCoins(CurrenciesType.exp), Owner.HeroMng.CalcBattlePower());

                    //统计任务完成数量
                    Owner.UpdateTaskFinishNum((PassTaskLoopType)temp.LoopType);
                    //限时礼包
                    Owner.ActionManager.RecordActionAndCheck(ActionType.DailyTaskFinishCount, 1);

                    Owner.AddRunawayActivityNumForType(RunawayAction.DailyTask);
                }
                else
                {
                    Log.Warn($"player {Owner.Uid} complete passcard task {taskId} failed: not complete");
                    msg.ErrorCode = (int)ErrorCode.PasscardNotComplete;
                }
            }
            else
            {
                Log.Warn($"player {Owner.Uid} complete passcard task {taskId} failed: not find task");
                msg.ErrorCode = (int)ErrorCode.Fail;
            }

            msg.Exp = Exp;
            msg.Level = passcardLevel;

            return msg;
        }



        public void SyncTask2Client(List<PassCardTaskItem> tasks)
        {
            if (tasks.Count <= 0)
            {
                return;
            }
            MSG_ZGC_UPDATE_PASSCARD_TASK msg = new MSG_ZGC_UPDATE_PASSCARD_TASK();
            msg.ErrorCode = (int)ErrorCode.Success;
            foreach (var task in tasks)
            {
                msg.Tasks.Add(GenerateTaskMsg(task));
            }

            msg.Exp = Exp;
            msg.Level = passcardLevel;
            Owner.Write(msg);
        }

        private void SyncRewardInfo2DB()
        {
            QueryUpdatePassCardReward info = new QueryUpdatePassCardReward(Owner.Uid, rewardedLevels, superRewardedLevels, rewardedTime);
            Owner.server.GameDBPool.Call(info);
        }

        private void SyncCharacreInfo2DB()
        {
            QueryUpdatePasscard info = new QueryUpdatePasscard(Owner.Uid, passcards, passcardLevel, CurPeriod);
            Owner.server.GameDBPool.Call(info);
        }

        private bool CheckLevelUp()
        {
            int curLevel = PassCardLibrary.GetCurLevel(Exp, passcardLevel);
            if (curLevel > passcardLevel)
            {
                passcardLevel = curLevel;
                return true;
            }
            else
            {
                return false;
            }
        }

        private int CheckMaxLevel()
        {
            if (passcardLevel >= PassCardLibrary.MaxPassLevel)
            {
                return (int)ErrorCode.MaxPasscardLevel;
            }
            return (int)ErrorCode.Success;
        }

        public MSG_ZGC_PASSCARD_TASK GenerateTaskMsg(PassCardTaskItem task)
        {
            MSG_ZGC_PASSCARD_TASK msg = new MSG_ZGC_PASSCARD_TASK();
            msg.CurNum = task.CurNum;
            msg.TaskId = task.Id;
            msg.ParamNum = task.ParamNum;
            msg.ParamType = task.ParamType;
            msg.Rewarded = task.Rewarded;
            msg.LoopType = (int)task.LoopType;
            msg.DeltaExp = task.Exp;
            return msg;
        }

        public void AddTypeTaskNum(TaskType type, int num)
        {
            List<PassCardTaskItem> items = new List<PassCardTaskItem>();
            foreach (var kv in taskItemList)
            {
                PassTask task = PassCardLibrary.GetTask(kv.Value.Id);
                if (task != null)
                {
                    if (kv.Value.ParamType == (int)type)
                    {
                        kv.Value.CurNum += num;
                        items.Add(kv.Value);
                    }
                }
            }
            SyncTask2Client(items);
            SyncPassCardTasks2DB(items);
        }

        /// <summary>
        /// 带比对特殊参数的添加特定任务类型的数量
        /// </summary>
        /// <param name="type">任务类型</param>
        /// <param name="param">任务参数如：副本id</param>
        /// <param name="paramType">任务参数类型 如 dungeon </param>
        public void AddTypeTaskNum(TaskType type, int param, string paramType, int num = 1)
        {
            List<PassCardTaskItem> items = new List<PassCardTaskItem>();
            foreach (var kv in taskItemList)
            {
                PassTask task = PassCardLibrary.GetTask(kv.Value.Id);
                if (task != null)
                {
                    if (kv.Value.ParamType == (int)type)
                    {
                        if (task.ParamChecksList.ContainsKey(paramType) && task.ParamChecksList[paramType].Contains(param))
                        {
                            kv.Value.CurNum += num;
                            items.Add(kv.Value);
                        }
                    }
                }
            }
            SyncTask2Client(items);
            SyncPassCardTasks2DB(items);
        }

        public void AddTypeTaskNum(TaskType type, int[] param, string[] paramType)
        {
            if (param.Count() != paramType.Count())
            {
                return;
            }
            List<PassCardTaskItem> items = new List<PassCardTaskItem>();
            foreach (var kv in taskItemList)
            {
                PassTask task = PassCardLibrary.GetTask(kv.Value.Id);
                if (task != null)
                {
                    bool add = true;
                    if (kv.Value.ParamType == (int)type)
                    {
                        for (int i = 0; i < paramType.Count(); i++)
                        {
                            if (task.ParamChecksList.ContainsKey(paramType[i]) && task.ParamChecksList[paramType[i]].Contains(param[i]))
                            {
                                continue;
                            }
                            else
                            {
                                add = false;
                            }
                        }

                        if (add)
                        {
                            kv.Value.CurNum++;
                            items.Add(kv.Value);
                        }
                    }
                }
            }
            SyncTask2Client(items);
            SyncPassCardTasks2DB(items);
        }

        public void SetNeedSendPanel()
        {
            needPanelInfo = true;
        }

        public void SetNeedDailyRefresh()
        {
            needDailyRefresh = true;
        }

        public void SetNeedWeeklyRefresh()
        {
            needWeeklyRefresh = true;
        }

        public void RefreshDailyPassCard()
        {
            //刷新每日的任务到db 清空每日任务
            foreach (var kv in taskItemList.Where(kv => kv.Value.LoopType == PassTaskLoopType.Daily))
            {
                kv.Value.CurNum = 0;
                kv.Value.Rewarded = false;
            }
            SyncPassCardDailyTasks2DB();
            SendPanelInfo();
        }

        public void RefreshWeeklyPassCard()
        {
            //更新每周的任务清零到db
            foreach (var kv in taskItemList.Where(kv => kv.Value.LoopType == PassTaskLoopType.WeekLy))
            {
                kv.Value.CurNum = 0;
                kv.Value.Rewarded = false;
            }
            SyncPassCardWeeklyTasks2DB();
            SendPanelInfo();
        }

        private void CheckAlign()
        {
            //dbAligning = true;
            try
            {
                //比对内存中的和 配置中的当前的周期内容是否一致  先做简单的剔除
                int period = PassCardLibrary.GetPeriod();
                if (period > CurPeriod)
                {
                    CurPeriod = period;
                    ChangePeriod = true;
                    ClearDBInfo();
                    Owner.RefreshPassCardBuyState();
                }
                var dic = PassCardLibrary.GetCurPeriodTasks();
                if (dic == null)
                {
                    Log.Warn($"Passcard without CurPeriodTasks period {PassCardLibrary.CurPeriod}");
                    return;
                }

                List<int> xmlids = dic.Keys.ToList();
                List<int> taskIds = taskItemList.Keys.ToList();
                List<int> xmlExcept = xmlids.Except(taskIds).ToList();
                List<int> taskExcept = taskIds.Except(xmlids).ToList();

                List<PassCardTaskItem> updates = new List<PassCardTaskItem>();

                if (xmlExcept.Count > 0 || taskExcept.Count > 0)
                {
                    ReloadFromXml();
                    DBReinsertAll();
                }
                else //较强的验证，具体信息比对
                {
                    foreach (var kv in dic)
                    {
                        PassCardTaskItem item = taskItemList[kv.Key];
                        PassTask task = kv.Value;
                        if (item.Id != task.Id || item.ParamType != task.PassTaskType)
                        {
                            taskItemList[kv.Key] = GenerateItem(task);
                            updates.Add(taskItemList[kv.Key]);
                        }
                        item.LoopType = (PassTaskLoopType)task.LoopType;
                        item.ParamNum = task.ParamNum;
                        item.Exp = task.Exp;
                    }

                    if (updates.Count > 0)
                    {
                        SyncPassCardTasks2DB(updates);
                    }
                }
            }
            finally
            {
                //dbAligning = false;
                dbAligned = true;
                SendPanelInfo();
                //GenerateTypeDic();
            }
        }

        //private void GenerateTypeDic()
        //{
        //    foreach (var kv in taskItemList)
        //    {
        //        AddTaskTypeItem(kv.Value.ParamType, kv.Key);
        //    }
        //}

        //private void AddTaskTypeItem(int type, int id)
        //{
        //    List<int> list;
        //    if (taskTypeList.TryGetValue((TaskType)type, out list))
        //    {
        //        list.Add(id);
        //    }
        //    else
        //    {
        //        list = new List<int>();
        //        list.Add(id);
        //        taskTypeList.Add((TaskType)type, list);
        //    }
        //}

        private PassCardTaskItem GenerateItem(PassTask task)
        {
            PassCardTaskItem item = new PassCardTaskItem();
            item.Id = task.Id;
            item.ParamType = task.PassTaskType;
            item.Time = Timestamp.GetUnixTimeStampSeconds(ServerFrame.BaseApi.now);
            item.LoopType = (PassTaskLoopType)task.LoopType;
            //public int CurNum { get; set; }
            //public bool Rewarded { get; set; }
            item.ParamNum = task.ParamNum;
            item.Exp = task.Exp;
            return item;
        }

        private void ReloadFromXml()
        {
            var dic = PassCardLibrary.GetCurPeriodTasks();
            taskItemList.Clear();
            foreach (var kv in dic)
            {
                PassCardTaskItem item = null;
                PassTask task = kv.Value;
                item = GenerateItem(task);
                taskItemList.Add(item.Id, item);
            }
        }

        private void DBReinsertAll()
        {
            QueryDeleteAllPassCardTasks delete = new QueryDeleteAllPassCardTasks(Owner.Uid);
            QueryInsertPassCardTasks tasks = new QueryInsertPassCardTasks(Owner.Uid, taskItemList.Values.ToList());
            Owner.server.GameDBPool.Call(delete, ret =>
            {
                Owner.server.GameDBPool.Call(tasks);
            });
        }

        public void RefreshPassCardPeriod()
        {
            dbAligned = false;
            CheckAlign();
            //ClearDBInfo();
        }

        public void ClearDBInfo()
        {
            this.passcardLevel = 1;
            this.rewardedLevels = "";
            this.superRewardedLevels = "";
            this.Exp = 0;
            int tempExp = Owner.GetCoins(CurrenciesType.PasscardExp);
            Owner.DelCoins(CurrenciesType.PasscardExp, tempExp, ConsumeWay.PassCardClear, "");
            rewardedSet.Clear();
            superRewardedSet.Clear();

            SyncRewardInfo2DB();
            SyncCharacreInfo2DB();
        }

        public void Update()
        {
            //db的通行证任务和配表对齐好了，才能把refresh要求的refresh真正执行
            if (!dbAligned)
            {
                return;
            }
            if (needPanelInfo)
            {
                needPanelInfo = false;
                SendPanelInfo();
            }
            if (needDailyRefresh)
            {
                needDailyRefresh = false;
                RefreshDailyPassCard();
            }
            if (needWeeklyRefresh)
            {
                needWeeklyRefresh = false;
                RefreshWeeklyPassCard();
            }
        }

        private void SyncPassCardDailyTasks2DB()
        {
            List<PassCardTaskItem> items = new List<PassCardTaskItem>();
            foreach (var kv in taskItemList.Where(kv => kv.Value.LoopType == PassTaskLoopType.Daily))
            {
                items.Add(kv.Value);
            }
            if (items.Count > 0)
            {
                SyncPassCardTasks2DB(items);
            }
        }

        private void SyncPassCardWeeklyTasks2DB()
        {
            List<PassCardTaskItem> items = new List<PassCardTaskItem>();
            foreach (var kv in taskItemList.Where(kv => kv.Value.LoopType == PassTaskLoopType.WeekLy))
            {
                items.Add(kv.Value);
            }
            if (items.Count > 0)
            {
                SyncPassCardTasks2DB(items);
            }
        }

        private void SyncPassCardTasks2DB(List<PassCardTaskItem> items)
        {
            if (items.Count <= 0)
            {
                return;
            }
            QueryUpdatePassCardTasks tasks = new QueryUpdatePassCardTasks(Owner.Uid, items);
            Owner.server.GameDBPool.Call(tasks);
        }

        public void AddTaskListItem(List<PassCardTaskItem> list)
        {
            if (list.Count <= 0)
            {
                return;
            }
            taskItemList.Clear();
            foreach (var item in list)
            {
                taskItemList.Add(item.Id, item);
            }
        }

        public bool CheckHaveBoughtThisPeriod()
        {
            if (BoughtedPeriods.Contains(CurPeriod))
            {
                return true;
            }
            return false;
        }

        public PassCardTaskItem GetPasscardTaskItemByType(TaskType type)
        {
            foreach (var kv in taskItemList)
            {
                if (kv.Value.ParamType == (int)type)
                {
                    return kv.Value;
                }
            }
            return null;
        }

        public void LoadReward(QueryLoadPassCardReward reward)
        {
            LoadReward(reward.Passcard_level_reward, reward.Passcard_super_level_reward, reward.Passcard_daily_reward_time);
            //rewardedLevels = reward.passcard_level_reward;
            //rewardedTime = reward.passcard_daily_reward_time;
            //superRewardedLevels = reward.passcard_super_level_reward;

            //rewardedSet = new SortedSet<int>();
            //string[] rewards = rewardedLevels.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            //foreach (var temp in rewards)
            //{
            //    rewardedSet.Add(temp.ToInt());
            //}

            //superRewardedSet = new SortedSet<int>();
            //rewards = superRewardedLevels.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            //foreach (var temp in rewards)
            //{
            //    superRewardedSet.Add(temp.ToInt());
            //}
        }

        public void LoadReward(string passcard_level_reward, string passcard_super_level_reward, DateTime rewardTime)
        {
            rewardedLevels = passcard_level_reward;
            rewardedTime = rewardTime;
            superRewardedLevels = passcard_super_level_reward;

            rewardedSet = new SortedSet<int>();
            string[] rewards = rewardedLevels.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var temp in rewards)
            {
                rewardedSet.Add(temp.ToInt());
            }

            superRewardedSet = new SortedSet<int>();
            rewards = superRewardedLevels.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var temp in rewards)
            {
                superRewardedSet.Add(temp.ToInt());
            }
        }

        public ZMZ_PASSCARD_INFO GetPassCardTransform()
        {
            ZMZ_PASSCARD_INFO info = new ZMZ_PASSCARD_INFO();
            info.PasscardLevel = passcardLevel;
            info.Passcards = passcards;
            info.Exp = Exp;
            info.RewardedLevels = rewardedLevels;
            info.SuperRewardedLevels = superRewardedLevels;
            info.Period = CurPeriod;
            info.RewardedTime = Timestamp.GetUnixTimeStampSeconds(rewardedTime);
            info.Info = new ZMZ_PASSCARD_TASK_INFO();
            foreach (var kv in taskItemList)
            {
                PassCardTaskItem item = kv.Value;
                ZMZ_PASSCARD_TASK temp = new ZMZ_PASSCARD_TASK()
                {
                    Id = item.Id,
                    ParamType = item.ParamType,
                    ParamNum = item.ParamNum,
                    CurNum = item.CurNum,
                    Exp = item.Exp,
                    Rewarded = item.Rewarded,
                    LoopType = (int)item.LoopType,
                    Time = item.Time
                };

                info.Info.Tasks.Add(temp);
            }

            return info;
        }

        public void LoadPasscardTransform(ZMZ_PASSCARD_INFO passcardInfo)
        {
            //个人
            InitPasscard(passcardInfo.Passcards, passcardInfo.PasscardLevel, passcardInfo.Exp, passcardInfo.Period);
            //奖励
            LoadReward(passcardInfo.RewardedLevels, passcardInfo.SuperRewardedLevels, Timestamp.TimeStampToDateTime(passcardInfo.RewardedTime));
            //任务
            List<PassCardTaskItem> list = new List<PassCardTaskItem>();
            foreach (var item in passcardInfo.Info.Tasks)
            {
                PassCardTaskItem temp = new PassCardTaskItem()
                {
                    Id = item.Id,
                    ParamType = item.ParamType,
                    ParamNum = item.ParamNum,
                    CurNum = item.CurNum,
                    Exp = item.Exp,
                    Rewarded = item.Rewarded,
                    LoopType = (PassTaskLoopType)item.LoopType,
                    Time = item.Time
                };
                list.Add(temp);

            }
            AddTaskListItem(list);
            InitTasks(true);
        }
    }
}
