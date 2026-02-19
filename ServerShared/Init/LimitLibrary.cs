using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ServerShared
{
    public static class LimitLibrary
    {
        private static Dictionary<LimitType, LimitData> LimitDatas = new Dictionary<LimitType, LimitData>();

        private static Dictionary<int, List<LimitType>> LevelLimits = new Dictionary<int, List<LimitType>>();

        private static Dictionary<int, List<LimitType>> TaskIdLimits = new Dictionary<int, List<LimitType>>();

        private static Dictionary<int, int> BranchTaskIds = new Dictionary<int, int>();

        //private static Dictionary<int, List<LimitType>> HeroIdLimits = new Dictionary<int, List<LimitType>>();
        //private static Dictionary<int, List<LimitType>> LadderLevelLimits = new Dictionary<int, List<LimitType>>();
        //private static Dictionary<int, List<LimitType>> StoryLimits = new Dictionary<int, List<LimitType>>();

        public static void BindDatas()
        {
            //LimitDatas.Clear();
            //LevelLimits.Clear();
            //TaskIdLimits.Clear();
            //BranchTaskIds.Clear();

            //HeroIdLimits = new Dictionary<int, List<LimitType>>();
            //LadderLevelLimits = new Dictionary<int, List<LimitType>>();
            //StoryLimits = new Dictionary<int, List<LimitType>>();

            BindLimitData();
        }

        private static void BindLimitData()
        {
            Dictionary<LimitType, LimitData> LimitDatas = new Dictionary<LimitType, LimitData>();
            Dictionary<int, List<LimitType>> LevelLimits = new Dictionary<int, List<LimitType>>();
            Dictionary<int, List<LimitType>> TaskIdLimits = new Dictionary<int, List<LimitType>>();
            Dictionary<int, int> BranchTaskIds = new Dictionary<int, int>();

            LimitData info = new LimitData();
            DataList dataList = DataListManager.inst.GetDataList("Limit");
            try
            {
                Type type = typeof(LimitType);
                foreach (var item in dataList)
                {
                    Data data = item.Value;
                    info = new LimitData();
                    info.Id = data.ID;

                    if (Enum.IsDefined(type, info.Id))
                    {
                        info.Type = (LimitType)info.Id;
                        info.Level = data.GetInt("level");
                        //info.Story = data.GetInt("story");
                        string taskIdString = data.GetString("mainTaskId");
                        string[] taskList = StringSplit.GetArray("|", taskIdString);
                        foreach (var task in taskList)
                        {
                            int taskId = int.Parse(task);
                            info.TaskIds.Add(taskId);
                            AddTaskIdLimit(TaskIdLimits, taskId, info.Type);
                        }

                        string btanchTaskIdsString = data.GetString("branchTaskId");
                        string[] btanchTaskIds = StringSplit.GetArray("|", btanchTaskIdsString);
                        foreach (var task in btanchTaskIds)
                        {
                            int taskId = int.Parse(task);
                            info.BranchTaskIds.Add(taskId);
                            AddTaskIdLimit(TaskIdLimits, taskId, info.Type);
                            BranchTaskIds[taskId] = 0;
                        }

                        if (info.Level > 1)
                        {
                            AddLevelLimit(LevelLimits, info.Level, info.Type);
                        }

                        LimitDatas.Add(info.Type, info);

                        //info.LadderLevel = data.GetInt("ladderLevel");
                        //string heroIdString = data.GetString("heroId");
                        //string[] heroList = StringSplit.GetArray("|", heroIdString);
                        //foreach (var hero in heroList)
                        //{
                        //    int heroId = int.Parse(hero);
                        //    info.HeroIds.Add(heroId);
                        //    AddHeroIdLimit(heroId, info.Type);
                        //}

                        //if (info.LadderLevel > 0)
                        //{
                        //    AddLadderLevelLimit(info.LadderLevel, info.Type);
                        //}

                        //if (info.Story > 0)
                        //{
                        //    AddStoryLimit(info.Story, info.Type);
                        //}

                    }
                }
            }
            catch (Exception e)
            {
                Log.Alert("[Error] BindLimitData : " + e.ToString());
            }

            LimitLibrary.LimitDatas = LimitDatas;
            LimitLibrary.LevelLimits = LevelLimits;
            LimitLibrary.TaskIdLimits = TaskIdLimits;
            LimitLibrary.BranchTaskIds = BranchTaskIds;
        }

        public static LimitData GetLimitData(LimitType type)
        {
            LimitData list;
            LimitDatas.TryGetValue(type, out list);
            return list;
        }

        private static void AddLevelLimit(Dictionary<int, List<LimitType>> LevelLimits, int level, LimitType id)
        {
            List<LimitType> list;
            if (LevelLimits.TryGetValue(level, out list))
            {
                list.Add(id);
            }
            else
            {
                list = new List<LimitType>();
                list.Add(id);
                LevelLimits.Add(level, list);
            }
        }

        public static List<LimitType> GetLevelLimitList(int level)
        {
            List<LimitType> list;
            LevelLimits.TryGetValue(level, out list);
            return list;
        }

        private static void AddTaskIdLimit(Dictionary<int, List<LimitType>> TaskIdLimits, int taskId, LimitType id)
        {
            List<LimitType> list;
            if (TaskIdLimits.TryGetValue(taskId, out list))
            {
                list.Add(id);
            }
            else
            {
                list = new List<LimitType>();
                list.Add(id);
                TaskIdLimits.Add(taskId, list);
            }
        }

        public static List<LimitType> GetTaskIdLimitList(int taskId)
        {
            List<LimitType> list;
            TaskIdLimits.TryGetValue(taskId, out list);
            return list;
        }

        public static bool CheckBranchTaskId(int taskId)
        {
            return BranchTaskIds.ContainsKey(taskId);
        }

        //private static void AddHeroIdLimit(int heroId, LimitType id)
        //{
        //    List<LimitType> list;
        //    if (HeroIdLimits.TryGetValue(heroId, out list))
        //    {
        //        list.Add(id);
        //    }
        //    else
        //    {
        //        list = new List<LimitType>();
        //        list.Add(id);
        //        HeroIdLimits.Add(heroId, list);
        //    }
        //}

        //public static List<LimitType> GetHeroIdLimitList(int heroId)
        //{
        //    List<LimitType> list;
        //    HeroIdLimits.TryGetValue(heroId, out list);
        //    return list;
        //}



        //private static void AddLadderLevelLimit(int ladderlevel, LimitType id)
        //{
        //    List<LimitType> list;
        //    if (LadderLevelLimits.TryGetValue(ladderlevel, out list))
        //    {
        //        list.Add(id);
        //    }
        //    else
        //    {
        //        list = new List<LimitType>();
        //        list.Add(id);
        //        LadderLevelLimits.Add(ladderlevel, list);
        //    }
        //}

        //public static List<LimitType> GetLadderLevelLimitList(int ladderlevel)
        //{
        //    List<LimitType> list;
        //    LadderLevelLimits.TryGetValue(ladderlevel, out list);
        //    return list;
        //}

        //private static void AddStoryLimit(int story, LimitType id)
        //{
        //    List<LimitType> list;
        //    if (StoryLimits.TryGetValue(story, out list))
        //    {
        //        list.Add(id);
        //    }
        //    else
        //    {
        //        list = new List<LimitType>();
        //        list.Add(id);
        //        StoryLimits.Add(story, list);
        //    }
        //}

        //public static List<LimitType> GetStoryLimitList(int story)
        //{
        //    List<LimitType> list;
        //    StoryLimits.TryGetValue(story, out list);
        //    return list;
        //}


    }
}
