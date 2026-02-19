using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Corss.Protocol.CorssR;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossServerLib
{
    public class CanoeManager
    {
        private CrossServerApi server { get; set; }

        public CanoeManager(CrossServerApi server)
        {
            this.server = server;
        }

        public RankBaseModel GetFirstValue(int groupId)
        {
            CanoeRank rank = server.RankMng.GetCanoeRank(groupId);
            if (rank != null)
            {
                //获取rank
                return rank.GetFirst();
            }
            else
            {
                return null;
            }
        }

        //private RankBaseModel GetPlayerValue(int groupId, int uid)
        //{
        //    CanoeRank rank = server.RankMng.GetCanoeRank(groupId);
        //    if (rank != null)
        //    {
        //        //获取rank
        //        return rank.GetRankBaseInfo(uid);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public void UpdatePlayerValue(int groupId, int uid, int value)
        {
            CanoeRank canoeRank = server.RankMng.GetCanoeRank(groupId);
            if (canoeRank == null)
            {
                server.RankMng.AddCanoeRank(groupId);
                canoeRank = server.RankMng.GetCanoeRank(groupId);
            }
            canoeRank.UpdateScore(uid, value);
        }

        public void Clear()
        {
            server.RankMng.ClearCanoeRank();

            MSG_CorssR_CLEAR_VALUE msg = new MSG_CorssR_CLEAR_VALUE();
            msg.GiftType = (int)RechargeGiftType.Canoe;
            server.RelationManager.Broadcast(msg);
        }

        public void SendReward()
        {
            foreach (var kv in CrossBattleLibrary.GroupList)
            {
                int groupId = kv.Key;
                CanoeRank canoeRank = server.RankMng.GetCanoeRank(groupId);
                if (canoeRank != null)
                {
                    canoeRank.LoadInitRankFromRedis(() => 
                    {
                        List<string> rankInfoList = new List<string>();
                        int randMainId = 0;

                        Dictionary<int, RankBaseModel> uidRankInfoDic = canoeRank.GetRankInfoList();
                        foreach (var rankItem in uidRankInfoDic)
                        {
                            JsonPlayerInfo rankPlayerInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, rankItem.Value.Uid);
                            if (rankPlayerInfo != null)
                            {
                                RankRewardInfo rewardInfo = CanoeLibrary.GetRankRewardInfo(rankItem.Value.Rank);
                                if (rewardInfo == null)
                                {
                                    break;
                                }
                                else
                                {
                                    MSG_CorssR_SEND_FINALS_REWARD rankMsg = new MSG_CorssR_SEND_FINALS_REWARD();
                                    rankMsg.MainId = rankPlayerInfo.MainId;
                                    rankMsg.Uid = rankPlayerInfo.Uid;
                                    rankMsg.Reward = rewardInfo.Rewards;
                                    rankMsg.EmailId = rewardInfo.EmailId;
                                    rankMsg.Param = $"{CommonConst.RANK}:{rankItem.Value.Rank}";
                                    server.RelationManager.WriteToRelation(rankMsg, rankMsg.MainId);

                                    server.TrackingLoggerMng.RecordSendEmailRewardLog(rankMsg.Uid, rankMsg.EmailId, rankMsg.Reward, rankMsg.Param, rankMsg.MainId, server.Now());
                                    server.TrackingLoggerMng.TrackRankEmailLog(groupId, 0, RankType.Canoe.ToString(), rankItem.Value.Uid, rankItem.Value.Score, rewardInfo.EmailId, rankItem.Value.Rank, server.Now());

                                    if (rankItem.Value.Rank < 100)
                                    {
                                        rankInfoList.Add(rankItem.Value.Rank + "_" + rankItem.Value.Uid + "_" + rankItem.Value.Score);
                                        randMainId = rankPlayerInfo.MainId;
                                    }
                                }
                            }
                        }
                        server.RelationManager.SendRankInfoToRelation("canoe", rankInfoList, randMainId);
                    });
                }
            }
        }
    }
}
