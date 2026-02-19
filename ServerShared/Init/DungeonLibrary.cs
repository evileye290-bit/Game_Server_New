using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class DungeonLibrary
    {
        private static Dictionary<int, DungeonModel> dungeonList = new Dictionary<int, DungeonModel>();

        /// <summary>
        /// 跳过战斗加速计算倍率
        /// </summary>
        public static int SkipBattleSpeedUp { get; private set; }
        /// <summary>
        /// 跳过战斗每帧步进时间
        /// 单位：s
        /// </summary>
        public static float SpeedUpPerFpsAddTime { get; private set; }

        public static void Init()
        {
            //dungeonList.Clear();
            Dictionary<int, DungeonModel> dungeonList = new Dictionary<int, DungeonModel>();

            DataList dataList = DataListManager.inst.GetDataList("Dungeon");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                dungeonList.Add(item.Key, new DungeonModel(data));
            }

            Data dungeonConfig = DataListManager.inst.GetData("DungeonConfig", 1);
            SkipBattleSpeedUp = dungeonConfig.GetInt("SkipBattleSpeedUp");
            SpeedUpPerFpsAddTime = dungeonConfig.GetFloat("SpeedUpPerFpsAddTime") * 0.001f;
            DungeonLibrary.dungeonList = dungeonList;
        }

        public static DungeonModel GetDungeon(int id)
        {
            DungeonModel dungeon = null;
            dungeonList.TryGetValue(id, out dungeon);
            return dungeon;
        }

    }
}
