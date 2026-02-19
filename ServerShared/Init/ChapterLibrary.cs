using DataProperty;
using ServerModels;
using System.Linq;
using System.Collections.Generic;

namespace ServerShared
{
    public class ChapterLibrary
    {
        private static Dictionary<int, int> task2PowerLimit = new Dictionary<int, int>();//<taskId, powerlimit>
        private static Dictionary<int, StoryLineModel> storyLineList = new Dictionary<int, StoryLineModel>();
        private static Dictionary<int, StoryLineModel> storyLineIndexByDungeonId = new Dictionary<int, StoryLineModel>();//<DungeonId，model>

        private static Dictionary<int, StoryRewardModel> storyRewardList = new Dictionary<int, StoryRewardModel>();

        private static Dictionary<int, List<StoryLineModel>> chapterStoryReward = new Dictionary<int, List<StoryLineModel>>();//chapterid id
        private static Dictionary<int, List<StoryRewardModel>> chapterBoxReward = new Dictionary<int, List<StoryRewardModel>>();//chapterid id

        public static int MinPower { get; private set; }
        public static int PowerFirstLevel { get; private set; }
        public static int PowerRecover { get; private set; }
        public static int PowerGiveCount { get; private set; }

        public static void Init()
        {
            InitPowerConfig();
            InitStoryLine();
            InitStoryReward();
        }

        public static StoryLineModel GetStoryLine(int id)
        {
            StoryLineModel model;
            storyLineList.TryGetValue(id, out model);
            return model;
        }

        public static StoryLineModel GetStoryLineModelByDungeonId(int dungeonId)
        {
            StoryLineModel model;
            storyLineIndexByDungeonId.TryGetValue(dungeonId, out model);
            return model;
        }

        public static StoryRewardModel GetStoryRewardModel(int id)
        {
            StoryRewardModel model;
            storyRewardList.TryGetValue(id, out model);
            return model;
        }

        public static int GetPowerLimit(int taskId)
        {
            int power;
            task2PowerLimit.TryGetValue(taskId, out power);
            return power;
        }

        public static List<StoryLineModel> GetStoryLineRewardIds(int chapterId)
        {
            List<StoryLineModel> ids;
            chapterStoryReward.TryGetValue(chapterId, out ids);
            return ids;
        }

        public static List<StoryRewardModel> GetBoxRewardIds(int chapterId)
        {
            List<StoryRewardModel> ids;
            chapterBoxReward.TryGetValue(chapterId, out ids);
            return ids;
        }

        private static void InitStoryLine()
        {
            Dictionary<int, StoryLineModel> storyLineList = new Dictionary<int, StoryLineModel>();
            Dictionary<int, StoryLineModel> storyLineIndexByDungeonId = new Dictionary<int, StoryLineModel>();
            Dictionary<int, List<StoryLineModel>> chapterStoryReward = new Dictionary<int, List<StoryLineModel>>();

            DataList dataList = DataListManager.inst.GetDataList("StoryLine");
            foreach (var kv in dataList)
            {
                StoryLineModel model = new StoryLineModel();
                model.BindData(kv.Value);
                storyLineList.Add(model.Id, model);

                if (model.DungeonId > 0)
                {
                    storyLineIndexByDungeonId.Add(model.DungeonId, model);
                }

                if (model.HaveReward)
                {
                    List<StoryLineModel> ids;
                    if (!chapterStoryReward.TryGetValue(model.Chapter, out ids))
                    {
                        ids = new List<StoryLineModel>();
                        chapterStoryReward[model.Chapter] = ids;
                    }
                    ids.Add(model);
                }
            }

            ChapterLibrary.storyLineList = storyLineList;
            ChapterLibrary.chapterStoryReward = chapterStoryReward;
            ChapterLibrary.storyLineIndexByDungeonId = storyLineIndexByDungeonId;
        }

        private static void InitPowerConfig()
        {
            Dictionary<int, int> task2PowerLimit = new Dictionary<int, int>();
            Data data = DataListManager.inst.GetData("PowerConfig", 1);
            PowerFirstLevel = data.GetInt("FirstLevel");
            PowerRecover = data.GetInt("Recover");
            PowerGiveCount = data.GetInt("GiveCount");

            string[] taskIds = data.GetString("TaskId").Split('|');
            string[] powerLimit = data.GetString("PowerLimit").Split('|');
            for (int i = 0; i < taskIds.Length; ++i)
            {
                int power = int.Parse(powerLimit[i]);
                task2PowerLimit.Add(int.Parse(taskIds[i]), power);
            }
            MinPower = task2PowerLimit.Values.Min();
            ChapterLibrary.task2PowerLimit = task2PowerLimit;
        }

        private static void InitStoryReward()
        { 
            Dictionary<int, StoryRewardModel> storyRewardList = new Dictionary<int, StoryRewardModel>();
            Dictionary<int, List<StoryRewardModel>> chapterBoxReward = new Dictionary<int, List<StoryRewardModel>>();
            DataList dataList = DataListManager.inst.GetDataList("StoryReward");
            foreach (var kv in dataList)
            {
                StoryRewardModel model = new StoryRewardModel();
                model.BindData(kv.Value);
                storyRewardList.Add(model.Id, model);

                List<StoryRewardModel> ids;
                if (!chapterBoxReward.TryGetValue(model.Chapter, out ids))
                {
                    ids = new List<StoryRewardModel>();
                    chapterBoxReward[model.Chapter] = ids;
                }
                ids.Add(model);
            }

            ChapterLibrary.storyRewardList = storyRewardList;
            ChapterLibrary.chapterBoxReward = chapterBoxReward;
        }
    }
}
