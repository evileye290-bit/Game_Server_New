using DataProperty;
using Logger;

namespace ServerShared
{
    static public class LiftConfig
    {
 
        public static float DownPeriod;
        public static float GoingUpPeriod;
        public static float UpPeriod;
        public static float GoingDownPeriod;
        public static float LiftLength;
        public static float LiftWedth;
        public static int NpcId;

        public static void InitLiftCongfig()
        {
            Data data = DataListManager.inst.GetData("LiftConfig", 1);
            if (data == null)
            {
                Log.Warn("init lift config failed: data is null");
                return;
            }
            DownPeriod = data.GetInt("DownPeriod");
            GoingUpPeriod = data.GetInt("GoingUpPeriod");
            UpPeriod = data.GetInt("UpPeriod");
            GoingDownPeriod = data.GetInt("GoingDownPeriod");
            LiftLength = data.GetInt("LiftLength");
            LiftWedth = data.GetInt("LiftWedth");
            NpcId = data.GetInt("NpcId");
        }

    }
}
