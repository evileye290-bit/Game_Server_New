using System;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using ServerFrame;
using Timestamp = CommonUtility.Timestamp;

namespace ZoneServerLib
{
    public class DriftExploreManager
    {
        private PlayerChar owner;
        private DriftExploreInfo driftExplore;
        private List<DriftExploreTaskInfo> dbTaskList = new List<DriftExploreTaskInfo>();
        private int CurPeriod { get; set; }

        public DriftExploreInfo DriftExplore
        {
            get { return driftExplore; }
        }

        private Dictionary<int, DriftExploreTaskInfo> driftExploreTasks;

        public Dictionary<int, DriftExploreTaskInfo> DriftExploreTasks
        {
            get { return driftExploreTasks; }
        }

        public DriftExploreManager(PlayerChar player)
        {
            owner = player;
        }

        public MSG_ZMZ_DRIFT_EXPLORE_TASK_INFO GenerateDriftExploreTaskTransformMsg()
        {
            MSG_ZMZ_DRIFT_EXPLORE_TASK_INFO msg = new MSG_ZMZ_DRIFT_EXPLORE_TASK_INFO();
            msg.Info = GenerateDriftExploreTaskFinishTransformMsg();
            driftExploreTasks.Values.ForEach(x => msg.TaskList.Add(GetDriftExploreTaskTransformMsg(x)));

            return msg;
        }

        private ZMZ_DRIFT_EXPLORE_INFO GenerateDriftExploreTaskFinishTransformMsg()
        {
            ZMZ_DRIFT_EXPLORE_INFO msg = new ZMZ_DRIFT_EXPLORE_INFO()
            {
                CurNum = driftExplore.CurNum,
                RefreshTime = Timestamp.GetUnixTimeStampSeconds(driftExplore.RefreshTime),
                Rewarded = driftExplore.Rewarded,
                Period = driftExplore.Period,
            };
            return msg;
        }

        private ZMZ_DRIFT_EXPLORE_TASK_ITEM GetDriftExploreTaskTransformMsg(DriftExploreTaskInfo taskInfo)
        {
            ZMZ_DRIFT_EXPLORE_TASK_ITEM msg = new ZMZ_DRIFT_EXPLORE_TASK_ITEM()
            {
                Id = taskInfo.Id,
                TaskType = taskInfo.TaskType,
                CurNum = taskInfo.CurNum,
                ParamNum = taskInfo.ParamNum,
                Rewarded = taskInfo.Rewarded,
            };
            return msg;
        }

        public void LoadDriftExploreTaskTransformMsg(MSG_ZMZ_DRIFT_EXPLORE_TASK_INFO msg)
        {
            LoadDriftExploreTransformMsg(msg.Info);
            LoadDriftExploreTasksTransformMsg(msg.TaskList);
        }

        private void LoadDriftExploreTransformMsg(ZMZ_DRIFT_EXPLORE_INFO msg)
        {
            CurPeriod = msg.Period;
            driftExplore = new DriftExploreInfo();
            driftExplore.CurNum = msg.CurNum;
            driftExplore.RefreshTime = Timestamp.TimeStampToDateTime(msg.RefreshTime);
            driftExplore.Rewarded = msg.Rewarded;
            driftExplore.Period = msg.Period;
        }

        private void LoadDriftExploreTasksTransformMsg(RepeatedField<ZMZ_DRIFT_EXPLORE_TASK_ITEM> taskList)
        {
            driftExploreTasks = new Dictionary<int, DriftExploreTaskInfo>();
            foreach (var item in taskList)
            {
                driftExploreTasks.Add(item.Id, new DriftExploreTaskInfo()
                {
                    Id = item.Id,
                    TaskType = item.TaskType,
                    CurNum = item.CurNum,
                    ParamNum = item.ParamNum,
                    Rewarded = item.Rewarded,
                });
            }
        }

        public void Init(DriftExploreInfo driftExplore, List<DriftExploreTaskInfo> taskList)
        {
            dbTaskList = taskList;
            InitDriftExploreInfo(driftExplore);
            InitDriftExploreTasksInfo();
        }

        private void InitDriftExploreInfo(DriftExploreInfo driftExplore)
        {
            this.driftExplore = driftExplore;
        }

        private void InitDriftExploreTasksInfo()
        {
            if (GetCurWeekMonday() > driftExplore.RefreshTime ||
                driftExplore.Period == 0 || driftExplore.Period > DriftExploreLibrary.MaxPeriod)
            {
                RefreshDriftExploreTasksInfo();
            }
            else
            {
                CurPeriod = driftExplore.Period;
                driftExploreTasks = new Dictionary<int, DriftExploreTaskInfo>();
                foreach (var task in dbTaskList)
                {
                    driftExploreTasks.Add(task.Id, task);
                }
            }
        }

        public void RefreshDriftExploreTasksInfo()
        {
            //更新每周的任务清零到db
            RefreshDriftExploreTaskInfo();
            RefreshDriftExploreInfo();
        }

        private void RefreshDriftExploreTaskInfo()
        {
            driftExploreTasks = new Dictionary<int, DriftExploreTaskInfo>();

            RechargeGiftModel model;
            if (!RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.DriftExplore, BaseApi.now, out model)) return;
            
            Dictionary<int, DriftExploreTaskModel> curTasksModels =
                DriftExploreLibrary.GetPeriodDriftExploreTasks(RefreshCurPeriod(model));
            if (curTasksModels == null)
            {
                return;
            }

            List<DriftExploreTaskInfo> updateList = new List<DriftExploreTaskInfo>();
            List<DriftExploreTaskInfo> addList = new List<DriftExploreTaskInfo>();
            List<DriftExploreTaskInfo> deleteList = new List<DriftExploreTaskInfo>();

            foreach (var task in dbTaskList)
            {
                //check delete
                DriftExploreTaskModel taskModel;
                curTasksModels.TryGetValue(task.Id, out taskModel);
                if (taskModel == null)
                {
                    deleteList.Add(task);
                }
                else
                {
                    driftExploreTasks.Add(task.Id, task);
                }
            }

            DriftExploreTaskInfo taskInfo;
            foreach (var kv in curTasksModels)
            {
                if (driftExploreTasks.TryGetValue(kv.Key, out taskInfo))
                {
                    //check update
                    CheckUpdateDriftExploreTaskParamInfo(taskInfo, kv.Value, updateList);
                    taskInfo.LoadXmlData(kv.Value);
                }
                else
                {
                    //insert
                    AddNewDriftExploreTask(kv.Value, addList);
                }
            }

            SyncUpdateDriftExploreTasks2DB(updateList);
            SyncInsertDriftExploreTasks2DB(addList);
            DeleteDriftExploreTasks(deleteList);
        }

        private void RefreshDriftExploreInfo()
        {
            driftExplore.Rewarded = false;
            driftExplore.CurNum = 0;
            driftExplore.RefreshTime = DateTime.Now;
            driftExplore.Period = CurPeriod;

            SyncUpdateDriftExplore2DB();
        }


        public void AddTypeTaskNum(TaskType type, double num, bool replace, object obj = null)
        {
            List<DriftExploreTaskInfo> tasks = new List<DriftExploreTaskInfo>();
            foreach (var kv in driftExploreTasks)
            {
                DriftExploreTaskModel task = DriftExploreLibrary.GetTaskModel(kv.Value.Id, CurPeriod);
                if (task == null
                    || kv.Value.Rewarded
                    || kv.Value.TaskType != (int)type
                    || !CheckTaskCondition(kv.Value, task, obj))
                {
                    continue;
                }

                switch (type)
                {
                    case TaskType.SoulSkillLevelCountForLevel:
                    {
                        int level = task.GetParamIntValue(TaskParamType.LEVEL);
                        num = owner.HeroMng.GetSoulSkillCountByLevel(level);
                    }
                        break;
                    default:
                        break;
                }

                if (!replace)
                {
                    decimal temp = Convert.ToDecimal(kv.Value.CurNum) + Convert.ToDecimal(num);
                    kv.Value.CurNum = Convert.ToDouble(string.Format("{0:N2}", temp));
                }
                else
                {
                    decimal temp = kv.Value.CurNum > num
                        ? Convert.ToDecimal(kv.Value.CurNum)
                        : Convert.ToDecimal(num);
                    
                    kv.Value.CurNum = Convert.ToDouble(string.Format("{0:N2}", temp));
                }

                tasks.Add(kv.Value);
            }

            SyncDriftExploreTask2Client(tasks);
            SyncUpdateDriftExploreTasks2DB(tasks);
        }

        public bool CheckTaskCondition(DriftExploreTaskInfo task, DriftExploreTaskModel info, object obj = null)
        {
            if (task.CurNum >= info.ParamNum) return false;

            TaskType type = (TaskType)task.TaskType;
            switch (type)
            {
                case TaskType.HeroLevelCount:
                case TaskType.PetCountForLevel:
                {
                    int level = info.GetParamIntValue(TaskParamType.LEVEL);
                    if (level > (int)obj)
                    {
                        return false;
                    }
                }
                    break;
                case TaskType.PetSkillBaptizeForQuality:
                case TaskType.EquipCountForQuality:
                case TaskType.PoolUseQualityItem:
                {
                    int quality = info.GetParamIntValue(TaskParamType.QUALITY);
                    if (quality > (int)obj)
                    {
                        return false;
                    }
                }
                    break;
                case TaskType.ShopBuyItem:
                case TaskType.CompleteDungeons:
                case TaskType.DrawHeroCardNum:
                case TaskType.ItemResolve:
                {
                    int dungeonType = info.GetParamIntValue(TaskParamType.TYPE);
                    if (dungeonType != (int)obj)
                    {
                        return false;
                    }
                }
                    break;
                case TaskType.FinishTask:
                case TaskType.ItemForgeForId:
                case TaskType.PushFigureComplete:
                {
                    int id = info.GetParamIntValue(TaskParamType.ID);
                    if (id != (int)obj)
                    {
                        return false;
                    }
                }
                    break;
                case TaskType.EquipSoulRingForYear:
                {
                    int year = info.GetParamIntValue(TaskParamType.VALUE);
                    int num = owner.SoulRingManager.GetEquipedCount(year);

                    if (task.CurNum != num)
                    {
                        //需要特殊处理
                        task.CurNum = num - 1;
                    }
                    else
                    {
                        return false;
                    }
                }
                    break;
                case TaskType.CompleteDungeonTypes:
                {
                    List<int> types = info.GetParamIntList(TaskParamType.DUNGEON_TYPES);
                    if (!types.Contains((int)obj))
                    {
                        //不是指定的副本
                        return false;
                    }
                }
                    break;
                case TaskType.UseConsumable:
                {
                    int subType = info.GetParamIntValue(TaskParamType.SUB_TYPE);
                    if (subType != (int)obj)
                    {
                        //不是指定等级
                        return false;
                    }
                }
                    break;
                case TaskType.EquipSoulRingBySlot:
                {
                    int slot = info.GetParamIntValue(TaskParamType.SLOT);
                    if (slot != (int)obj)
                    {
                        //不是指定等级
                        return false;
                    }
                }
                    break;
                default:
                    break;
            }

            return true;
        }

        private void SyncDriftExploreTask2Client(List<DriftExploreTaskInfo> tasks)
        {
            if (tasks.Count > 0)
            {
                MSG_ZGC_UPDATE_DRIFT_EXPLORE_TASK_INFO msg = new MSG_ZGC_UPDATE_DRIFT_EXPLORE_TASK_INFO();
                foreach (var task in tasks)
                {
                    msg.ChangedTasks.Add(GenerateDriftExploreTaskMsg(task));
                }

                owner.Write(msg);
            }
        }

        private ZGC_DRIFT_EXPLORE_TASK GenerateDriftExploreTaskMsg(DriftExploreTaskInfo task)
        {
            ZGC_DRIFT_EXPLORE_TASK msg = new ZGC_DRIFT_EXPLORE_TASK()
            {
                TaskId = task.Id,
                CurNum = task.CurNum,
                Rewarded = task.Rewarded,
            };
            return msg;
        }

        private void CheckUpdateDriftExploreTaskParamInfo(DriftExploreTaskInfo taskInfo, DriftExploreTaskModel taskModel,
            List<DriftExploreTaskInfo> updateList)
        {
            taskInfo.TaskType = taskModel.TaskType;
            taskInfo.CurNum = 0;
            taskInfo.Rewarded = false;
            updateList.Add(taskInfo);
        }

        private void AddNewDriftExploreTask(DriftExploreTaskModel taskModel, List<DriftExploreTaskInfo> addList)
        {
            DriftExploreTaskInfo taskInfo = GenerateDriftExploreTaskInfo(taskModel);
            driftExploreTasks.Add(taskInfo.Id, taskInfo);
            addList.Add(taskInfo);
        }

        private DriftExploreTaskInfo GenerateDriftExploreTaskInfo(DriftExploreTaskModel taskModel)
        {
            DriftExploreTaskInfo taskInfo = new DriftExploreTaskInfo()
            {
                Id = taskModel.Id,
                TaskType = taskModel.TaskType
            };
            return taskInfo;
        }

        public bool CheckParentRewarded(DriftExploreTaskInfo task)
        {
            DriftExploreTaskModel taskModel = DriftExploreLibrary.GetDriftExploreTaskByTaskId(task.Id, CurPeriod);
            DriftExploreTaskInfo taskInfo;
            if (taskModel.ParentId == 0)
            {
                return true;
            }

            driftExploreTasks.TryGetValue(taskModel.ParentId, out taskInfo);

            return taskInfo.Rewarded;
        }

        public void GetDriftExploreReward(DriftExploreTaskInfo task)
        {
            task.Rewarded = true;
            //统计任务完成数量
            driftExplore.CurNum++;

            SyncUpdateDriftExploreTasks2DB(new List<DriftExploreTaskInfo>() { task });
            SyncDbUpdateDriftExploreInfo();
            SendDriftExploreInfo();
        }

        public void RefreshDriftExplore()
        {
            dbTaskList.Clear();
            foreach (var task in driftExploreTasks)
            {
                dbTaskList.Add(task.Value);
            }

            RefreshDriftExploreTasksInfo();
        }

        public void SendDriftExploreTasksInfo()
        {
            int endTime = Timestamp.GetUnixTimeStampSeconds(GetCurWeekMonday()) + (86400 * 7);

            MSG_ZGC_INIT_DRIFT_EXPLORE_INFO msg = new MSG_ZGC_INIT_DRIFT_EXPLORE_INFO()
            {
                Time = endTime,
                Period = driftExplore.Period,
            };

            foreach (var item in driftExploreTasks.Values)
            {
                msg.Tasks.Add(GenerateDriftExploreTaskMsg(item));

            }

            owner.Write(msg);
        }

        public bool CheckGotDriftExploreReward()
        {
            return driftExplore.Rewarded;
        }

        public bool CheckDriftExploreNum(int limit)
        {
            return driftExplore.CurNum >= limit;
        }

        public void UpdateDriftExploreReward()
        {
            driftExplore.Rewarded = true;
            SyncDbUpdateDriftExploreInfo();
            SendDriftExploreInfo();
        }

        public DateTime GetCurWeekMonday(DateTime date = default)
        {
            if (date == default)
            {
                date = DateTime.Now;
            }

            // DateTime date = DateTime.Now;
            DateTime firstDate = DateTime.Now;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    firstDate = date;
                    break;
                case DayOfWeek.Tuesday:
                    firstDate = date.AddDays(-1);
                    break;
                case DayOfWeek.Wednesday:
                    firstDate = date.AddDays(-2);
                    break;
                case DayOfWeek.Thursday:
                    firstDate = date.AddDays(-3);
                    break;
                case DayOfWeek.Friday:
                    firstDate = date.AddDays(-4);
                    break;
                case DayOfWeek.Saturday:
                    firstDate = date.AddDays(-5);
                    break;
                case DayOfWeek.Sunday:
                    firstDate = date.AddDays(-6);
                    break;
            }

            return firstDate.Date;
        }

        private void SyncUpdateDriftExplore2DB()
        {
            owner.server.GameDBPool.Call(new QueryUpdateDriftExploreInfo(owner.Uid, driftExplore));
        }

        private void SyncUpdateDriftExploreTasks2DB(List<DriftExploreTaskInfo> tasks)
        {
            if (tasks.Count <= 0)
            {
                return;
            }

            owner.server.GameDBPool.Call(new QueryUpdateDriftExploreTasks(owner.Uid, tasks));
        }

        private void SyncInsertDriftExploreTasks2DB(List<DriftExploreTaskInfo> tasks)
        {
            if (tasks.Count <= 0)
            {
                return;
            }

            owner.server.GameDBPool.Call(new QueryInsertDriftExploreTasks(owner.Uid, tasks));
        }

        private void DeleteDriftExploreTasks(List<DriftExploreTaskInfo> taskList)
        {
            List<int> deleteList = new List<int>();
            foreach (var item in taskList)
            {
                deleteList.Add(item.Id);
            }

            SyncDbDeleteDriftExploreTasks(deleteList);
        }

        private void SyncDbDeleteDriftExploreTasks(List<int> tasks)
        {
            if (tasks.Count <= 0)
            {
                return;
            }

            owner.server.GameDBPool.Call(new QueryDeleteDriftExploreTasks(owner.Uid, tasks));
        }

        private void SyncDbUpdateDriftExploreInfo()
        {
            owner.server.GameDBPool.Call(new QueryUpdateDriftExploreInfo(owner.Uid, driftExplore));
        }

        public void SendDriftExploreInfo()
        {
            MSG_ZGC_UPDATE_DRIFT_EXPLORE_INFO msg = new MSG_ZGC_UPDATE_DRIFT_EXPLORE_INFO()
            {
                CurNum = driftExplore.CurNum,
                Rewarded = driftExplore.Rewarded
            };

            owner.Write(msg);
        }

        public int RefreshCurPeriod(RechargeGiftModel model)
        {
            CurPeriod = model.SubType;
            return CurPeriod;
        }

        public DriftExploreConfigModel GetDriftExploreConfig()
        {
            return DriftExploreLibrary.GetDriftExploreConfig(CurPeriod);
        }

        public DriftExploreTaskModel GetDriftExploreTaskByTaskId(int taskId)
        {
            return DriftExploreLibrary.GetDriftExploreTaskByTaskId(taskId, CurPeriod);
        }
        public void SendDriftExploreAllInfo()
        {
            SendDriftExploreTasksInfo();
            SendDriftExploreInfo();
        }

    }
}