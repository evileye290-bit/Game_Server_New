using CommonUtility;
using EnumerateUtility;
using Message.Corss.Protocol.CorssR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace CrossServerLib
{
    public class RouletteManager
    {
        private CrossServerApi server { get; set; }
        private int period = 1;

        public RouletteManager(CrossServerApi server)
        {
            this.server = server;
            UpdatePeriod();
        }

        public void Clear()
        {
            server.RankMng.ClearIsRouletteRank();

            MSG_CorssR_CLEAR_VALUE msg = new MSG_CorssR_CLEAR_VALUE();
            msg.GiftType = (int)RechargeGiftType.Roulette;
            server.RelationManager.Broadcast(msg);
        }

        public void UpdatePeriod()
        {
            RechargeGiftModel model;
            RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.Roulette, server.Now(), out model);
            if (model != null)
            {
                period = model.SubType;
            }
        }

        public void SendReward()
        {
            foreach (var kv in CrossBattleLibrary.GroupList)
            {
                int groupId = kv.Key;
                RouletteRank rouletteRank = server.RankMng.GetRouletteRank(groupId);
                if (rouletteRank != null)
                {
                    rouletteRank.LoadInitRankFromRedis(() => 
                    {
                        List<string> rankInfoList = new List<string>();
                        int randMainId = 0;

                        Dictionary<int, RankBaseModel> uidRankInfoDic = rouletteRank.GetRankInfoList();
                        foreach (var rankItem in uidRankInfoDic)
                        {
                            JsonPlayerInfo rankPlayerInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, rankItem.Value.Uid);
                            if (rankPlayerInfo != null)
                            {
                                CampBuildRankRewardData data = RouletteLibrary.GetRankRewardInfo(period, rankItem.Value.Rank);
                                if (data == null)
                                {
                                    break;
                                }
                                else
                                {
                                    MSG_CorssR_SEND_FINALS_REWARD rankMsg = new MSG_CorssR_SEND_FINALS_REWARD();
                                    rankMsg.MainId = rankPlayerInfo.MainId;
                                    rankMsg.Uid = rankPlayerInfo.Uid;
                                    rankMsg.Reward = data.Rewards;
                                    rankMsg.EmailId = data.EmailId;
                                    rankMsg.Param = $"{CommonConst.RANK}:{rankItem.Value.Rank}";
                                    server.RelationManager.WriteToRelation(rankMsg, rankMsg.MainId);

                                    server.TrackingLoggerMng.RecordSendEmailRewardLog(rankMsg.Uid, rankMsg.EmailId, rankMsg.Reward, rankMsg.Param, rankMsg.MainId, server.Now());
                                    server.TrackingLoggerMng.TrackRankEmailLog(groupId, 0, RankType.Roulette.ToString(), rankItem.Value.Uid, rankItem.Value.Score, data.EmailId, rankItem.Value.Rank, server.Now());

                                    if (rankItem.Value.Rank < 100)
                                    {
                                        rankInfoList.Add(rankItem.Value.Rank + "_" + rankItem.Value.Uid + "_" + rankItem.Value.Score);
                                        randMainId = rankPlayerInfo.MainId;
                                    }
                                }
                            }
                        }
                        server.RelationManager.SendRankInfoToRelation("roulette", rankInfoList, randMainId);
                    });
                }
            }
        }
    }
}
