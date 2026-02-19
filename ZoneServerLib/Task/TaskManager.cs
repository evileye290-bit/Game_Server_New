using CommonUtility;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib.Task
{
    public class TaskManager
    {
        //自己
        public PlayerChar Oneself { get; set; }
        public int CurrMaxMainTaskId { get; set; }
        /// <summary>
        /// 所有任务
        /// </summary>
        Dictionary<int, TaskItem> taskItemList = new Dictionary<int, TaskItem>();
        /// <summary>
        /// 维护任务类型关系表
        /// </summary>
        Dictionary<int, List<int>> taskTypeList = new Dictionary<int, List<int>>();

        public TaskManager(PlayerChar oneself)
        {
            Oneself = oneself;
            taskItemList.Clear();
            taskTypeList.Clear();
        }

        public Dictionary<int, TaskItem> GetTaskList()
        {
            return taskItemList;
        }


        public TaskItem AddTaskListItem(int id, int paramType, int curNum, int paramNum, int time, int tagType)
        {
            TaskItem item = new TaskItem();
            item.Id = id;
            item.ParamType = paramType;
            item.ParamNum = paramNum;
            item.CurNum = curNum;
            item.Time = time;
            item.MainType = tagType;

            if (AddTaskListItem(item))
            {
                return item;
            }
            else
            {
                return null;
            }
        }

        public bool AddTaskListItem(TaskItem item)
        {
            //if (item.TagType == (int)TaskTagType.Main)
            //{
            //    MainTaskId = item.Id;
            //}
            TaskItem tempTask;
            if (!taskItemList.TryGetValue(item.Id, out tempTask))
            {
                taskItemList.Add(item.Id, item);
                AddTaskTypeItem(item.ParamType, item.Id);
                return true;
            }
            else
            {
                //有多余任务需要删除
                if (tempTask.CurNum < item.CurNum)
                {
                    //TODO:保留数大的，删除数小的，清除掉 tempTask 添加新的task
                    //不用跟新type list，因为是同类型和id号
                    taskItemList.Remove(item.Id);

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

        public void AddTaskTypeItem(int type, int id)
        {
            List<int> list;
            if (taskTypeList.TryGetValue(type, out list))
            {
                list.Add(id);
            }
            else
            {
                list = new List<int>();
                list.Add(id);
                taskTypeList.Add(type, list);
            }
        }

        //public TaskItem AddNewTask(int id)
        //{
        //    TaskInfo info = TaskLibrary.GetTaskInfoById(id);
        //    if (info != null)
        //    {
        //        return AddNewTask(info);
        //    }
        //    else
        //    {
        //        //Error
        //        return null;
        //        //Log.ErrorLine("player {0} add new task {1} not find task info.", Owner.Uid, id);
        //    }
        //}

        public TaskItem AddNewTask(TaskInfo info)
        {
            int paramType = info.ParamType;
            int curNum = 0;
            int paramNum = info.GetParamIntValue(TaskParamType.NUM);
            int Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            //对个别人物类型特殊处理
            TaskType type = (TaskType)info.ParamType;
            switch (type)
            {
                case TaskType.Handin:
                    {
                        //检查背包中物品数量是否足够
                        if (info.CheckParamKey(TaskParamType.CONSUMABLE))
                        {
                            int itemTypeId = info.GetParamIntValue(TaskParamType.CONSUMABLE);
                            BaseItem item = Oneself.BagManager.GetItem(MainType.Consumable, itemTypeId);
                            if (item != null && item.PileNum > 0)
                            {
                                curNum = item.PileNum;
                            }
                        }
                        else
                        {
                            Log.Warn("player {0} init task id {1}  cur num not find  ParamKey type consumable ", Oneself.Uid, info.Id);
                        }
                    }
                    break;
                case TaskType.PlayerLevel:
                    {
                        curNum = Oneself.Level;
                    }
                    break;
                case TaskType.BrotherNum:
                    {
                        curNum = Oneself.BrotherCount;
                    }
                    break;
                case TaskType.OwnSoulRingForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        int num = Oneself.BagManager.SoulRingBag.GetSoulRingCountByLevel(level);
                        curNum = num;
                    }
                    break;
                case TaskType.SoulSkillLevelCountForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        int num = Oneself.HeroMng.GetSoulSkillCountByLevel(level);
                        curNum = num;
                    }
                    break;
                case TaskType.OwnSoulRingForYear:
                    {
                        int year = info.GetParamIntValue(TaskParamType.VALUE);
                        if (year > 0)
                        {
                            //取出所有魂环，检查等级
                            Dictionary<ulong, BaseItem> items = Oneself.BagManager.SoulRingBag.GetAllItems();
                            foreach (var kv in items)
                            {
                                SoulRingItem item = kv.Value as SoulRingItem;
                                if (item.Year >= year)
                                {
                                    curNum++;
                                }
                            }
                        }
                    }
                    break;
                case TaskType.EquipSoulRingForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        if (level > 0)
                        {
                            //取出所有魂环，检查等级
                            Dictionary<int, Dictionary<int, SoulRingItem>> heroItems = Oneself.SoulRingManager.GetAllEquipedSoulRings();
                            foreach (var items in heroItems)
                            {
                                foreach (var kv in items.Value)
                                {
                                    SoulRingItem item = kv.Value;
                                    if (item.Level >= level)
                                    {
                                        curNum++;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case TaskType.HeroTitleLevelCount:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        if (level > 0)
                        {
                            //取出所有装备，检查等级
                            Dictionary<int, HeroInfo> dic = Oneself.HeroMng.GetHeroInfoList();
                            foreach (var kv in dic)
                            {
                                if (kv.Value.TitleLevel >= level)
                                {
                                    curNum++;
                                }
                            }
                        }
                    }
                    break;
                case TaskType.HeroEquipCount:
                    {
                        int num = Oneself.HeroMng.GetEquipHeroCount();
                        curNum = num;
                    }
                    break;
                case TaskType.FollowHeroCount:
                    {
                        if (Oneself.FollowerId > 0)
                        {
                            curNum = 1;
                        }
                    }
                    break;
                case TaskType.EquipSoulRing:
                    {
                        int num = Oneself.SoulRingManager.GetEquipedCount();
                        curNum = num;
                    }
                    break;
                case TaskType.EquipSoulRingForYear:
                    {
                        int year = info.GetParamIntValue(TaskParamType.VALUE);
                        int num = Oneself.SoulRingManager.GetEquipedCount(year);
                        curNum = num;
                    }
                    break;
                case TaskType.EquipHeroSoulRing:
                    {
                        int num = Oneself.GetEquipedHeroSoulRings();
                        curNum = num;
                    }
                    break;
                case TaskType.EquipSoulBone:
                    {
                        int num = Oneself.SoulboneMng.GetEquipedCount();
                        curNum = num;
                    }
                    break;
                case TaskType.EquipSoulBoneBySlot:
                    {
                        int slot = info.GetParamIntValue(TaskParamType.SLOT);
                        int num = Oneself.SoulboneMng.GetEquipedCountBySlot(slot);
                        curNum = num;
                    }
                    break;
                case TaskType.EquipEquipment:
                    {
                        int num = Oneself.EquipmentManager.GetEquipedCount();
                        curNum = num;
                    }
                    break;
                case TaskType.TowerStage:
                    {
                        int num = Oneself.TowerManager.NodeId;
                        curNum = num;
                    }
                    break;
                case TaskType.SecretAreaStage:
                    {
                        int num = Oneself.SecretAreaManager.Id;
                        curNum = num;
                    }
                    break;
                case TaskType.EquipEquipmentBySlot:
                    {
                        int slot = info.GetParamIntValue(TaskParamType.SLOT);
                        int num = Oneself.EquipmentManager.GetEquipedCountBySlot(slot);
                        curNum = num;
                    }
                    break;
                case TaskType.EquipSoulRingBySlot:
                    {
                        int slot = info.GetParamIntValue(TaskParamType.SLOT);
                        int num = Oneself.SoulRingManager.GetEquipedCountBySlot(slot);
                        curNum = num;
                    }
                    break;
                case TaskType.EquipmentUpgradeForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        if (level > 0)
                        {
                            //取出所有装备，检查等级
                            Dictionary<int, Dictionary<int, Slot>> dic = Oneself.EquipmentManager.GetSlotDic();
                            foreach (var kv in dic)
                            {
                                foreach (var slot in kv.Value)
                                {
                                    if (slot.Value.EquipLevel >= level)
                                    {
                                        curNum++;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case TaskType.EquipmentInjectForNum:
                    {
                        int num = Oneself.EquipmentManager.GetFullInjectCount();
                        curNum = num;
                    }
                    break;
                case TaskType.HuntingResearchForValue:
                    {
                        int num = Oneself.HuntingManager.Research;
                        curNum = num;
                    }
                    break;
                case TaskType.EquipmentJewel:
                    {
                        int num = Oneself.EquipmentManager.GetAllJewelCount();
                        curNum = num;
                    }
                    break;
                case TaskType.EquipmentJewelForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        if (level > 0)
                        {
                            int num = Oneself.EquipmentManager.GetJewelCountByLevel(level);
                            curNum = num;
                        }
                    }
                    break;
                case TaskType.PushFigureComplete:
                    {
                        int dungeonId = info.GetParamIntValue(TaskParamType.ID);
                        switch (Oneself.pushFigureManager.PushFigureStatus)
                        {
                            case PushFigureStatus.NotOpen:
                            case PushFigureStatus.Opening:
                                if (dungeonId < Oneself.pushFigureManager.Id)
                                {
                                    //不是指定的副本
                                    curNum++;
                                }
                                break;
                            case PushFigureStatus.Finished:
                                if (dungeonId <= Oneself.pushFigureManager.Id)
                                {
                                    //不是指定的副本
                                    curNum++;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case TaskType.HeroLevelCount:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        if (level > 0)
                        {
                            //取出所有装备，检查等级
                            Dictionary<int, HeroInfo> dic = Oneself.HeroMng.GetHeroInfoList();
                            foreach (var kv in dic)
                            {
                                if (kv.Value.Level >= level)
                                {
                                    curNum++;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            Oneself.RecordTaskLog(info.Id);
            //任务BI
            Oneself.BIRecordRecordTaskLog(type, info.Id, 1);
            //BI 任务
            Oneself.KomoeEventLogMissionFlow("任务", info.Id, 1, 1, Oneself.GetCoins(CurrenciesType.exp), Oneself.HeroMng.CalcBattlePower());

            return AddTaskListItem(info.Id, paramType, curNum, paramNum, Time, info.MainType);
        }

        public CheckTaskErrorType CheckTaskAdd(TaskItem task, TaskInfo info, object obj)
        {

            TaskType type = (TaskType)info.ParamType;
            switch (type)
            {
                case TaskType.Collect:
                    {
                        for (int i = 1; i <= task.ParamNum; i++)
                        {
                            int zoneGoodsId = info.GetParamIntValue(TaskParamType.GOODS + i);
                            if (zoneGoodsId == (int)obj)
                            {
                                return CheckTaskErrorType.Success;
                            }
                        }
                        //采集物错误
                        return CheckTaskErrorType.NoChange;
                    }
                case TaskType.Handin:
                    {
                        int itemId = info.GetParamIntValue(TaskParamType.CONSUMABLE);
                        if (itemId != (int)obj)
                        {
                            //道具不符
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.PushFigureComplete:
                    {
                        int id = info.GetParamIntValue(TaskParamType.ID);
                        if (id != (int)obj)
                        {
                            //不是指定的副本
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.CompleteOneDungeon:
                    {
                        int dungeonId = info.GetParamIntValue(TaskParamType.DUNGEON);
                        if (dungeonId != (int)obj)
                        {
                            //不是指定的副本
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.CompleteDungeonList:
                    {
                        List<int> dungeonIds = info.GetParamIntList(TaskParamType.DUNGEON_LIST);
                        if (!dungeonIds.Contains((int)obj))
                        {
                            //不是指定的副本
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.CompleteDungeonTypes:
                    {
                        List<int> types = info.GetParamIntList(TaskParamType.DUNGEON_TYPES);
                        if (!types.Contains((int)obj))
                        {
                            //不是指定的副本
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.DrawHeroCardSingle:
                case TaskType.DrawHeroCardContinuous:
                case TaskType.DrawHeroCardNum:
                case TaskType.ItemResolve:
                case TaskType.ShopBuyItem:
                case TaskType.CompleteDungeons:
                    {
                        int dungeonType = info.GetParamIntValue(TaskParamType.TYPE);
                        if (dungeonType != (int)obj)
                        {
                            //不是指定副本类型
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;            
                case TaskType.EquipEquipmentLevelCount:
                case TaskType.HeroLevelCount:
                case TaskType.EquipSoulRingForLevel:
                case TaskType.HeroTitleLevelCount:
                case TaskType.EquipmentUpgradeForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        if (level != (int)obj)
                        {
                            //不是指定等级
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.EquipSoulRingForYear:
                    {
                        int year = info.GetParamIntValue(TaskParamType.VALUE);
                        int num = Oneself.SoulRingManager.GetEquipedCount(year);

                        if (task.CurNum != num)
                        {
                            //需要特殊处理
                            task.CurNum = num - 1;
                        }
                        else
                        {
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.OwnSoulRingForYear:
                    {
                        int year = info.GetParamIntValue(TaskParamType.VALUE);
                        if (year > (int)obj)
                        {
                            //不是指定年份
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.EquipSoulBoneBySlot:
                case TaskType.EquipEquipmentBySlot:
                case TaskType.EquipSoulRingBySlot:
                    {
                        int slot = info.GetParamIntValue(TaskParamType.SLOT);
                        if (slot != (int)obj)
                        {
                            //不是指定等级
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.OwnSoulRingForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        int num = Oneself.BagManager.SoulRingBag.GetSoulRingCountByLevel(level);
                        if (task.CurNum == num)
                        {
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.SoulSkillLevelCountForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        int num = Oneself.HeroMng.GetSoulSkillCountByLevel(level);
                        if (task.CurNum == num)
                        {
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.EquipmentJewelForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        if (level > (int)obj)
                        {
                            //不是指定等级
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.ItemForge:
                    {
                        List<int> list = obj as List<int>;

                        int itemType = info.GetParamIntValue(TaskParamType.TYPE);
                        if (itemType != list[0])
                        {
                            //不是指定副本类型
                            return CheckTaskErrorType.NoChange;
                        }

                        int subType = info.GetParamIntValue(TaskParamType.SUB_TYPE);
                        if (subType != list[1])
                        {
                            //不是指定等级
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.UseConsumable:
                    {
                        int subType = info.GetParamIntValue(TaskParamType.SUB_TYPE);
                        if (subType != (int)obj)
                        {
                            //不是指定等级
                            return CheckTaskErrorType.NoChange;
                        }
                    }
                    break;
                case TaskType.Select:
                default:
                    break;
            }
            //含有概率增加的进行处理
            if (info.CheckParamKey(TaskParamType.FINISH_PRO))
            {
                int finishPro = info.GetParamIntValue(TaskParamType.FINISH_PRO);
                int pro = NewRAND.Next(1, 10000);
                if (pro > finishPro)
                {
                    return CheckTaskErrorType.NoChange;
                }
            }
            return CheckTaskErrorType.Success;
        }

        public int ChangeTaskAddNum(TaskInfo info, int num)
        {
            switch ((TaskType)info.ParamType)
            {
                case TaskType.EquipmentJewelForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        if (level > 0)
                        {
                            num = Oneself.EquipmentManager.GetJewelCountByLevel(level);
                        }
                    }
                    break;
                case TaskType.OwnSoulRingForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        num = Oneself.BagManager.SoulRingBag.GetSoulRingCountByLevel(level);
                    }
                    break;
                case TaskType.SoulSkillLevelCountForLevel:
                    {
                        int level = info.GetParamIntValue(TaskParamType.LEVEL);
                        num = Oneself.HeroMng.GetSoulSkillCountByLevel(level);
                    }
                    break;
                default:
                    break;
            }
            return num;
        }

        public void UpdateTask(TaskItem task)
        {
            taskItemList[task.Id] = task;
        }

        public Dictionary<int, int> RemoveTaskType(int tagType)
        {
            Dictionary<int, int> list = GetEmailTaskIds(tagType);

            foreach (var item in list)
            {
                RemoveTaskTypeItem(item.Value, item.Key);
            }
            return list;
        }
        public bool RemoveTaskTypeItem(int type, int id)
        {
            taskItemList.Remove(id);
            List<int> list;
            if (taskTypeList.TryGetValue(type, out list))
            {
                list.Remove(id);
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<int> GetTaskItemForType(int type)
        {
            List<int> list;
            taskTypeList.TryGetValue(type, out list);
            return list;
        }
        public TaskItem GetTaskItemForId(int id)
        {
            TaskItem item;
            taskItemList.TryGetValue(id, out item);
            return item;
        }

        public Dictionary<int, int> GetEmailTaskIds(int tagType)
        {
            Dictionary<int, int> list = new Dictionary<int, int>();
            foreach (var item in taskItemList)
            {
                if (item.Value.MainType == tagType)
                {
                    list.Add(item.Value.Id, item.Value.ParamType);
                }
            }

            return list;
        }

        public bool SetTaskNum(TaskItem task, int num, bool isAdd)
        {
            int oldValue = task.CurNum;
            //成功
            int value = num;
            if (isAdd)
            {
                value += task.CurNum;
            }

            //判断值不能比完成数大
            if (value > task.ParamNum)
            {
                value = task.ParamNum;
            }
            //更新值
            task.CurNum = value;

            if (oldValue == value)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CheckTaskComplete(TaskItem task)
        {
            if (task.CurNum < task.ParamNum)
            {
                //任务未完成
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetCurrFinishedMaxMainTaskId(int taskId)
        {
            if (CurrMaxMainTaskId < taskId)
            {
                CurrMaxMainTaskId = taskId;
            }
        }

        public bool CheckDungeonTask(int dungeonId)
        {
            List<int> dungeonTask = new List<int>();
            dungeonTask.Add((int)TaskType.CompleteOneDungeon);
            dungeonTask.Add((int)TaskType.CompleteDungeonList);

            foreach (var kv in taskItemList)
            {
                if (!dungeonTask.Contains(kv.Value.ParamType))
                {
                    continue;
                }

                TaskInfo info = TaskLibrary.GetTaskInfoById(kv.Key);
                if (info == null)
                {
                    return false;
                }

                if (info.GetParamIntValue(TaskParamType.DUNGEON) == dungeonId)
                {
                    if (info.GetParamIntValue(TaskParamType.NUM) != kv.Value.CurNum)
                    {
                        return true;
                    }
                }

                List<int> dungoenList = info.GetParamIntList(TaskParamType.DUNGEON_LIST);

                if (dungoenList != null && dungoenList.Contains(dungeonId))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckOneDungeonTask(int dungeonId)
        {
            List<int> dungeonTask = new List<int>();
            dungeonTask.Add((int)TaskType.CompleteOneDungeon);
            dungeonTask.Add((int)TaskType.CompleteDungeonList);

            foreach (var kv in taskItemList)
            {
                if (!dungeonTask.Contains(kv.Value.ParamType))
                {
                    continue;
                }

                TaskInfo info = TaskLibrary.GetTaskInfoById(kv.Key);
                if (info == null)
                {
                    return false;
                }

                if (info.GetParamIntValue(TaskParamType.DUNGEON) == dungeonId)
                {
                    if (info.GetParamIntValue(TaskParamType.NUM) != kv.Value.CurNum)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
