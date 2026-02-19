using DataProperty;
using Logger;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;
using ServerModels;

namespace ServerShared
{
    public class FamilyBarTime
    {
        public DayOfWeek Day;
        public int Hour;
        public int Min;
    }

    static public class FamilyLibrary
    {
        // key 开始时间 value 结束时间
        public static List<FamilyBarTime> StartTimeList = new List<FamilyBarTime>();
        public static int UpdatePeriod = 10;
        public static int DungeonId;
        public static int Duration = 20;
        public static Dictionary<int, int> RandomRewards = new Dictionary<int, int>();
        public static int RewardRate = 0;

        public static TimeModel SelectionStart { get; set; }
        public static TimeModel SelectionEnd { get; set; }
        public static TimeModel SemiFinalsPrepareStart { get; set; }
        public static TimeModel SemiFinalsPrepareEnd { get; set; }
        public static TimeModel FinalsPrepareStart { get; set; }
        public static TimeModel FinalsPrepareEnd { get; set; }

        public static TimeModel SemiFinalsStart { get; set; }
        public static TimeModel SemiFinalsEnd { get; set; }

        public static TimeModel FinalsStart { get; set; }
        public static TimeModel FinalsEnd { get; set; }

        public static int WarSource = 10;
        public static int SemiFinalDungeonId1 = 5001;
        public static int SemiFinalDungeonId2 = 5002;
        public static int FinalDungeonId = 5003;
        public static Vec2 CollectPosition;
        public static float CollectPositionLength;
        public static float CountCollectCD = 10;
        public static float SourceCollectCD = 10;

        public static int RestoreLifeBuffPro = 2500;
        public static int AttackBuffPro = 5000;
        public static int ScoreBuffPro = 7500;
        public static float RestoreLifeBuffTime = 3;
        public static float RestoreLifeBuffMaxTime = 30;
        public static int RestoreLifeBuffNum = 1000;
        public static float AttackBuffTime = 3;
        public static float AttackBuffMaxTime = 30;
        public static int AttackBuffNum = 1000;
        public static float ScoreBuffTime = 3;
        public static float ScoreBuffMaxTime = 30;
        public static int ScoreBuffNum = 1000;


        public static void Init()
        {

        }


    }

}
