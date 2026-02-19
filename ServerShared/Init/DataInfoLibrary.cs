using DataProperty;

namespace ServerShared
{
    static public class DataInfoLibrary
    {
        public static int BaseInfoUpdateTime;
        public static int DetailedUpdateTime;

        public static void Init()
        {
            InitGameCongfig();
        }
        private static void InitGameCongfig()
        {
            DataList gameConfig = DataListManager.inst.GetDataList("DataInfoConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                BaseInfoUpdateTime = data.GetInt("BaseInfoUpdateTime");
                DetailedUpdateTime = data.GetInt("DetailedUpdateTime");
            }
        }

    }

}
