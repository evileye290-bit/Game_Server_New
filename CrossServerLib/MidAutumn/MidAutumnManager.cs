using CommonUtility;
using EnumerateUtility;
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
    public class MidAutumnManager
    {
        private CrossServerApi server { get; set; }
        private int period = 1;

        public MidAutumnManager(CrossServerApi server)
        {
            this.server = server;
            UpdatePeriod();
        }

        public void UpdatePeriod()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.MidAutumn, server.Now(), out model))
            {
                period = model.SubType;
            }
        }

        public void UpdatePlayerValue(int groupId, int uid, int value)
        {
            MidAutumnRank rank = server.RankMng.GetMidAutumnRank(groupId);
            if (rank == null)
            {
                server.RankMng.AddMidAutumnRank(groupId);
                rank = server.RankMng.GetMidAutumnRank(groupId);
            }
            rank.UpdateScore(uid, value);
        }

        public void Clear()
        {
            server.RankMng.ClearMidAutumnRank();
            UpdatePeriod();
        }

        public void SendReward()
        {
            //排行榜被屏蔽的活动不发放排行奖励
            MidAutumnConfig config = MidAutumnLibrary.GetConfig(period);
            if (config != null && !config.ShowRank)
            {
                return;
            }

            foreach (var kv in CrossBattleLibrary.GroupList)
            {
                int groupId = kv.Key;
                MidAutumnRank rank = server.RankMng.GetMidAutumnRank(groupId);
                if (rank != null)
                {
                    rank.LoadInitRankFromRedis(() => 
                    {
                        List<string> rankInfoList = new List<string>();
                        int randMainId = 0;

                        Dictionary<int, RankBaseModel> uidRankInfoDic = rank.GetRankInfoList();
                        foreach (var rankItem in uidRankInfoDic)
                        {
                            JsonPlayerInfo rankPlayerInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, rankItem.Value.Uid);
                            if (rankPlayerInfo != null)
                            {
                                RankRewardInfo rewardInfo = MidAutumnLibrary.GetRankRewardInfo(period, rankItem.Value.Rank);
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
                                    server.TrackingLoggerMng.TrackRankEmailLog(groupId, 0, RankType.MidAutumn.ToString(), rankItem.Value.Uid, rankItem.Value.Score, rewardInfo.EmailId, rankItem.Value.Rank, server.Now());

                                    if (rankItem.Value.Rank <= 100)
                                    {
                                        rankInfoList.Add(rankItem.Value.Rank + "_" + rankItem.Value.Uid + "_" + rankItem.Value.Score);
                                        randMainId = rankPlayerInfo.MainId;
                                    }
                                }
                            }
                        }
                        server.RelationManager.SendRankInfoToRelation("mid_autumn", rankInfoList, randMainId);
                    });
                }
            }
        }
    }
}
