using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class RankManager
    {
        private RelationServerApi server { get; set; }

        public BattlePowerRank BattlePowerRank;
        public HuntingRank HuntingRank;
        public SecretAreaRank SecretAreaRank;
        public PushFigureRank PushFigureRank;
        public ContributionRank ContributionRank;
        public CrossBattleRank CrossBattleRank;
        public ThemeBossRank ThemeBossRank;
        public CrossChallengeRank CrossChallengeRank;

        public RankReward RankReward;

        public RankManager(RelationServerApi server)
        {
            this.server = server;
        }

        public void InitRanks()
        {
            BattlePowerRank = new BattlePowerRank(server);
            BattlePowerRank.Init();

            HuntingRank = new HuntingRank(server);
            HuntingRank.Init();

            SecretAreaRank = new SecretAreaRank(server);
            SecretAreaRank.Init();

            PushFigureRank = new PushFigureRank(server);
            PushFigureRank.Init();

            ContributionRank = new ContributionRank(server);
            ContributionRank.Init();

            CrossBattleRank = new CrossBattleRank(server);
            CrossBattleRank.Init();

            ThemeBossRank = new ThemeBossRank(server);
            ThemeBossRank.Init();


            CrossChallengeRank = new CrossChallengeRank(server);
            CrossChallengeRank.Init();

            RankReward = new RankReward(server);
            RankReward.Init();
        }

        public static MSG_RZ_GET_RANK_LIST PushRankListMsg(RankListModel Info, RankType pushType)
        {
            //if (Info == null)
            //{
            //    Log.Warn($"player {uid} PushRankListMsg failed: not find info ");
            //    return;
            //}

            //Client client = server.ZoneManager.GetClient(uid);
            //if (client == null)
            //{
            //    Log.Warn($"player {uid} PushRankListMsg failed: not find client ");
            //    return;
            //}

            MSG_RZ_GET_RANK_LIST rankMsg = new MSG_RZ_GET_RANK_LIST();
            rankMsg.RankType = (int)pushType;
            rankMsg.Page = Info.Page;
            rankMsg.Camp = (int)Info.Camp;
            rankMsg.Count = Info.TotalCount;

            foreach (var player in Info.RankList)
            {
                PlayerRankMsg info = GetRankPlayerMsgInfo(player);
                rankMsg.RankList.Add(info);
            }
            if (Info.OwnerInfo != null)
            {
                rankMsg.Info = GetRankBaseInfo(Info.OwnerInfo);
            }
            return rankMsg;
            //client.Write(rankMsg);
        }

        private static PlayerRankMsg GetRankPlayerMsgInfo(PlayerRankModel player)
        {
            PlayerRankMsg info = new PlayerRankMsg();
            if (player.BaseInfo != null)
            {
                info.BaseInfo.AddRange(GetRankPlayerBaseInfoItem(player.BaseInfo));
            }

            if (player.RankInfo != null)
            {
                info.Rank = GetRankBaseInfo(player.RankInfo);
            }
            return info;
        }

        public static List<HFPlayerBaseInfoItem> GetRankPlayerBaseInfoItem(RedisPlayerInfo player)
        {
            List<HFPlayerBaseInfoItem> list = new List<HFPlayerBaseInfoItem>();
            HFPlayerBaseInfoItem item;
            foreach (var kv in player.DataList)
            {
                item = new HFPlayerBaseInfoItem();
                item.Key = (int)kv.Key;
                item.Value = kv.Value.ToString();
                list.Add(item);
            }
            return list;
        }

        private static RankBaseInfo GetRankBaseInfo(RankBaseModel player)
        {
            RankBaseInfo rank = new RankBaseInfo();
            rank.Uid = player.Uid;
            rank.Rank = player.Rank;
            rank.Score = player.Score;
            return rank;
        }

        public void BIRecordRankLog(string rankType, int stage, List<string> rankInfoList)
        {
            int group = CrossBattleLibrary.GetGroupId(server.MainId);
            List<int> groupServers = CrossBattleLibrary.GetGroupServers(group);
            string[] serverIdArr = null;
            if (groupServers != null)
            {
                serverIdArr = new string[groupServers.Count];
                for (int i = 0; i < groupServers.Count; i++)
                {
                    serverIdArr[i] = groupServers[i].ToString();
                }
            }

            string[] rankInfoArr = new string[rankInfoList.Count];
            for (int i = 0; i < rankInfoList.Count; i++)
            {
                rankInfoArr[i] = rankInfoList[i];
            }

            // LOG 记录开关
            //if (!GameConfig.TrackingLogSwitch)
            //{
            //    return;
            //}
            server.BILoggerMng.RankTaLog(server.MainId, serverIdArr, rankType, stage, rankInfoArr);
        }
    }
}
