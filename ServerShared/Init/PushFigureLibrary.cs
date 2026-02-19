using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public static class PushFigureLibrary
    {
        private static Dictionary<int, PushFigureModel> pushFigureList = new Dictionary<int, PushFigureModel>();
        private static Dictionary<int, PushFigureModel> pushFigureByDungeonId = new Dictionary<int, PushFigureModel>();
        private static Dictionary<int, int> taskFinishOpenId = new Dictionary<int, int>();

        public static int FirstId { get; private set; }
        public static int LastId { get; private set; }

        public static void Init()
        {
            //pushFigureList.Clear();

            InitPushFigureData();
        }

        private static void InitPushFigureData()
        {
            Dictionary<int, PushFigureModel> pushFigureByDungeonId = new Dictionary<int, PushFigureModel>();
            Dictionary<int, PushFigureModel> pushFigureList = new Dictionary<int, PushFigureModel>();
            Dictionary<int, int> taskFinishOpenId = new Dictionary<int, int>();
            DataList dataes = DataListManager.inst.GetDataList("PushFigure");
            foreach (var kv in dataes)
            {
                PushFigureModel model = new PushFigureModel(kv.Value);
                pushFigureList.Add(kv.Key, model);

                if (model.TaskLimit > 0)
                {
                    taskFinishOpenId[model.TaskLimit] = model.Id;
                }

                if (model.DungeonId > 0)
                {
                    pushFigureByDungeonId[model.DungeonId] = model;
                }
            }

            FirstId = pushFigureList.Keys.Min();
            LastId = pushFigureList.Keys.Max();
            PushFigureLibrary.pushFigureList = pushFigureList;
            PushFigureLibrary.pushFigureByDungeonId = pushFigureByDungeonId;
            PushFigureLibrary.taskFinishOpenId = taskFinishOpenId;
        }

        public static PushFigureModel GetPushFigureModel(int id)
        {
            PushFigureModel model;
            pushFigureList.TryGetValue(id, out model);
            return model;
        }
        public static PushFigureModel GetPushFigureModelBuDungeonId(int dungeonId)
        {
            PushFigureModel model;
            pushFigureByDungeonId.TryGetValue(dungeonId, out model);
            return model;
        }

        public static bool CheckOpenNew(int taskId, out int newId)
        {
            return taskFinishOpenId.TryGetValue(taskId, out newId);
        }
    }
}
