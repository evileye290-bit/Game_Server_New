using DataProperty;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class TeamLibrary
    {
        private static Dictionary<int, int> reviveCD = new Dictionary<int, int>();
        public static int EnergyCanMaxNum { get; private set; }
        public static int EnergyMaxNum { get; private set; }
        public static int EnergyAddNum { get; private set; }
        public static int HelpCount { get; private set; }//请求发送给玩家数量
        public static int HelpLevelRange { get; private set; }//等级范围
        public static int RepeatLevelRange { get; private set; }//重复等级范围
        public static int RepeatSendTime { get; private set; }//第二次发送时间
        public static int RobortTime { get; private set; }//补充机器人时间
        public static int HelpCDTime { get; private set; }//发送请求cd时间
        public static int VerifyQuitTeam { get; private set; }//确认退出队伍时间
        

        public static float EnergyMaxAdd { get; private set; }
        public static float EnergyCureRatio { get; private set; }
        public static float EnergyHitRatio { get; private set; }
        public static float EnergyOnHitRatio { get; private set; }
        public static int FleeCountLimit { get; private set; }

        public static List<float> ResearchRanges { get; private set; }

        public static string ResearchRange { get; private set; }
        public static int TeamMemberCountLimit { get; private set; }
        
        public static void Init()
        {
            ResearchRanges = new List<float>();
            Data data = DataListManager.inst.GetData("TeamConfig", 1);

            EnergyCanMaxNum = data.GetInt("EnergyCanMaxNum");
            EnergyMaxNum = data.GetInt("EnergyMaxNum");
            EnergyAddNum = data.GetInt("EnergyAddNum");
            HelpLevelRange= data.GetInt("HelpLevelRange");
            HelpCount = data.GetInt("HelpCount");
            RepeatLevelRange = data.GetInt("RepeatLevelRange");
            RepeatSendTime = data.GetInt("RepeatSendTime");
            RobortTime = data.GetInt("RobortTime");
            HelpCDTime = data.GetInt("HelpCDTime");
            VerifyQuitTeam = data.GetInt("VerifyQuitTeam");
            TeamMemberCountLimit = data.GetInt("TeamMemberCountLimit");

            BuildReviveCD(data.GetString("ReviveTime"));

            ResearchRange = data.GetString("ResearchRange");
            ResearchRanges.Clear();
            string[] ranges = ResearchRange.Split('|');
            foreach(var item in ranges)
            {
                ResearchRanges.Add(float.Parse(item));
            }

            Data battleData= DataListManager.inst.GetData("BattleConfig", 1);
            EnergyMaxAdd = battleData.GetFloat("EnergyMaxAdd");
            EnergyCureRatio = battleData.GetFloat("EnergyCureRatio");
            EnergyHitRatio = battleData.GetFloat("EnergyHitRatio");
            EnergyOnHitRatio = battleData.GetFloat("EnergyOnHitRatio");
            FleeCountLimit = battleData.GetInt("FleeCountLimit");
        }

        private static void BuildReviveCD(string cdStr)
        {
            //reviveCD.Clear();
            Dictionary<int, int> reviveCD = new Dictionary<int, int>();

            if (string.IsNullOrEmpty(cdStr))
            {
                return;
            }
            foreach (string item in cdStr.Split('|'))
            {
                //取出单个设置
                string[] resource = item.Split(':');
                if (resource.Length != 2)
                {
                    continue;
                }
                reviveCD.Add(int.Parse(resource[0]), int.Parse(resource[1]));
            }
            reviveCD = reviveCD.OrderByDescending(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

            TeamLibrary.reviveCD = reviveCD;
        }

        public static int GetReviveCD(int reviveCount)
        {
            foreach (var kv in reviveCD)
            {
                if (reviveCount >= kv.Key)
                {
                    return kv.Value;
                }
            }
            return 30;
        }
    }
}
