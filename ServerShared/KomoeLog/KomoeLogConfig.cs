using DataProperty;
using System.Collections.Generic;

namespace ServerShared
{
    public class KomoeLogConfig
    {
        public static string GameBaseId { get; set; }
        //Google：6360 ios：6361官网：6362
        public static int GameId { get; set; }
        public static string Platform { get; set; }

        public static float RankTime { get; set; }
        public static void Init()
        {
            InitConfgig();
        }

        private static void InitConfgig()
        {
            DataList dataList = DataListManager.inst.GetDataList("KomoeLogConfig");
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                GameBaseId = data.GetString("gameBaseId");
                GameId = data.GetInt("gameId");
                Platform = data.GetString("platform");
                RankTime = data.GetFloat("RankTime");
                if (RankTime == 0)
                {
                    RankTime = 2;
                }
            }
        }
    }
}
