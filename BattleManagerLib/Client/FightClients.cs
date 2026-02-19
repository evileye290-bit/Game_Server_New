using BattleManagerServerLib;
using CommonUtility;
using System;

namespace BattleServerLib.Client
{
    public class FightClients
    {
        public BattleClient Client1 { get; set; }
        public BattleClient Client2 { get; set; }

        public int RoomId { get; set; }

        public int FindZoneCount { get; set; }

        public DateTime JoinTime { get; set; }
        public int RankingLevel { get; set; }
        public int RankingValue { get; set; }

        public int LadderLevel { get; set; }
        public int TempValue { get; set; }

        public int GameLevelId { get; set; }
        public bool IsTeam { get; set; }
        public void SetRankingInfo()
        {
            JoinTime = BattleManagerServerApi.now;

            RankingValue = (Client1.RankingValue + Client2.RankingValue) / 2;
            LadderLevel = (Client1.LadderLevel + Client2.LadderLevel) / 2;
            RankingLevel = RankingValue / CommonConst.BATTLE_LEVEL_BASE;

        }

        private double GetWriteTime()
        {
            return (BattleManagerServerApi.now - JoinTime).TotalSeconds;
        }
     
    }
}
