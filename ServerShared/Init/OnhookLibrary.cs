using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class OnhookLibrary
    {
        private static Dictionary<int, OnhookModel> onhookList = new Dictionary<int, OnhookModel>();
        private static Dictionary<int, OnhookModel> onhookListIndexByDungeonId = new Dictionary<int, OnhookModel>();
        private static Dictionary<int, int> OpenTaskInfo = new Dictionary<int, int>();
        private static Dictionary<int, int> onhookCards = new Dictionary<int, int>();

        private static Dictionary<int, TimeLimitHookModel> timeLimitOnhookList = new Dictionary<int, TimeLimitHookModel>();


        public static int FirstId { get; private set; }
        public static int MaxId { get; private set; }
        public static int RewardTime { get; private set; }
        public static int MaxRewardTime { get; private set; }
        public static int RandomRewardTime { get; private set; }
        public static int FastRewardTime { get; private set; }
        public static string OpenReward { get; private set; }

        public static void Init()
        {
            InitOnhook();
            InitTimeLimitOnhook();
            InitCongfig();
            InitOnhookCard();
        }

        private static void InitOnhook()
        {
            Dictionary<int, OnhookModel> onhookList = new Dictionary<int, OnhookModel>();
            Dictionary<int, OnhookModel> onhookListIndexByDungeonId = new Dictionary<int, OnhookModel>();
            Dictionary<int, int> OpenTaskInfo = new Dictionary<int, int>();

            OnhookModel model;
            DataList dataList = DataListManager.inst.GetDataList("Onhook");
            foreach (var kv in dataList)
            {
                model = new OnhookModel(kv.Value);
                onhookList.Add(model.Id, model);
                onhookListIndexByDungeonId[model.DungeonId] = model;

                if (FirstId == 0)
                {
                    FirstId = model.DungeonId;
                }

                OpenTaskInfo[model.TaskId] = model.Id;

                MaxId = Math.Max(MaxId, model.Id);
            }
            OnhookLibrary.onhookList = onhookList;
            OnhookLibrary.onhookListIndexByDungeonId = onhookListIndexByDungeonId;
            OnhookLibrary.OpenTaskInfo = OpenTaskInfo;
        }

        private static void InitTimeLimitOnhook()
        {
            Dictionary<int, TimeLimitHookModel> timeLimitOnhookList = new Dictionary<int, TimeLimitHookModel>();

            TimeLimitHookModel model;
            DataList dataList = DataListManager.inst.GetDataList("TimeLimitRewardDrop");
            foreach (var kv in dataList)
            {
                model = new TimeLimitHookModel(kv.Value);
                timeLimitOnhookList[kv.Key] = model;
            }
            OnhookLibrary.timeLimitOnhookList = timeLimitOnhookList;
        }

        private static void InitCongfig()
        {
            Data data = DataListManager.inst.GetData("OnhookConfig", 1);
            RewardTime = data.GetInt("RewardTime");
            MaxRewardTime = data.GetInt("MaxRewardTime");
            RandomRewardTime = data.GetInt("RandomRewardTime");
            FastRewardTime = data.GetInt("FastRewardTime");
            OpenReward = data.GetString("OpenReward");         
        }

        private static void InitOnhookCard()
        {
            Dictionary<int, int> onhookCards = new Dictionary<int, int>();
            DataList dataList = DataListManager.inst.GetDataList("OnhookCard");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!onhookCards.ContainsKey(data.ID))
                {
                    onhookCards.Add(data.ID, data.GetInt("TimeHourType"));
                }
            }
            OnhookLibrary.onhookCards = onhookCards;
        }

        public static bool OpenNew(int finishedTaskId, out int rewardId)
        {
            return OpenTaskInfo.TryGetValue(finishedTaskId, out rewardId);
        }

        public static int CheckNewId(int currMainTaskId)
        {
            int id = 0;
            foreach (var kv in OpenTaskInfo)
            {
                if (kv.Key > currMainTaskId) break;

                id = kv.Value;
            }
            return id;
        }

        public static OnhookModel GetOnhookModel(int id)
        {
            OnhookModel model;
            onhookList.TryGetValue(id, out model);
            return model;
        }

        public static OnhookModel GetFirstOnhookModel()
        {
            return onhookList.First().Value; ;
        }

        public static int GetOnhookCardHourType(int itemId)
        {
            int hour;
            onhookCards.TryGetValue(itemId, out hour);
            return hour;
        }

        public static TimeLimitHookModel GeTimeLimitHookModel(DateTime time)
        {
            return timeLimitOnhookList.Values.Where(x => x.StartTime <= time && time < x.EndTime).FirstOrDefault();
        }
    }
}
