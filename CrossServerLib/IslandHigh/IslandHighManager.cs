using CommonUtility;
using EnumerateUtility;
using Message.Corss.Protocol.CorssR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace CrossServerLib
{
    public class IslandHighManager
    {
        private CrossServerApi server { get; set; }
        private int period = 1;

        public IslandHighManager(CrossServerApi server)
        {
            this.server = server;
            UpdatePeriod();
        }

        public void Clear()
        {
            server.RankMng.ClearIslandHighRank();
            server.RankMng.ClearIslandHighLastStageRank();

            MSG_CorssR_CLEAR_VALUE msg = new MSG_CorssR_CLEAR_VALUE();
            msg.GiftType = (int)RechargeGiftType.IslandHigh;
            server.RelationManager.Broadcast(msg);
        }

        public void UpdatePeriod()
        {
            RechargeGiftModel model;
            RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.IslandHigh, server.Now(), out model);
            if (model != null)
            {
                period = model.SubType;
            }
        }

        public void SendReward(int stage)
        {
            foreach (var kv in CrossBattleLibrary.GroupList)
            {
                int groupId = kv.Key;
                IslandHighRank highRank = server.RankMng.GetIslandHighRank(groupId);
                IslandHighLastStageRank lastStageRank = server.RankMng.GetIslandHighLastStageRank(groupId);
                if (lastStageRank == null)
                {
                    server.RankMng.AddIslandHighLastStageRank(groupId);
                    lastStageRank = server.RankMng.GetIslandHighLastStageRank(groupId);
                }

                if (highRank != null)
                {
                    highRank.LoadInitRankFromRedis(() => 
                    { 
                        Dictionary<int, RankBaseModel> uidRankInfoDic = highRank.GetRankInfoList();

                        lastStageRank.UpdateRankInfoList(uidRankInfoDic);

                        IslandHighRankRewardModel data = IslandHighLibrary.GetRankStageRewardInfo(period, stage);
                        if (data != null)
                        {
                            int randMainId = 0;
                            List<string> rankInfoList = new List<string>();

                            foreach (var rankItem in uidRankInfoDic.Where(x => x.Value.Rank <= data.Rank))
                            {
                                JsonPlayerInfo rankPlayerInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, rankItem.Value.Uid);
                                if (rankPlayerInfo != null)
                                {
                                    if (rankItem.Value.Rank > data.Rank) break;

                                    MSG_CorssR_SEND_FINALS_REWARD rankMsg = new MSG_CorssR_SEND_FINALS_REWARD();
                                    rankMsg.MainId = rankPlayerInfo.MainId;
                                    rankMsg.Uid = rankPlayerInfo.Uid;
                                    rankMsg.Reward = data.Rewards;
                                    rankMsg.EmailId = data.EmailId;
                                    rankMsg.Param = $"{CommonConst.RANK}:{rankItem.Value.Rank}";
                                    server.RelationManager.WriteToRelation(rankMsg, rankMsg.MainId);

                                    server.TrackingLoggerMng.RecordSendEmailRewardLog(rankMsg.Uid, rankMsg.EmailId, rankMsg.Reward, rankMsg.Param, rankMsg.MainId, server.Now());
                                    server.TrackingLoggerMng.TrackRankEmailLog(groupId, 0, RankType.IslandHigh.ToString(), rankItem.Value.Uid, rankItem.Value.Score, data.EmailId, rankItem.Value.Rank, server.Now());


                                    rankInfoList.Add(rankItem.Value.Rank + "_" + rankItem.Value.Uid + "_" + rankItem.Value.Score);
                                    randMainId = rankPlayerInfo.MainId;

                                    //BI
                                    server.KomoeEventLogRankFlow(rankItem.Value.Uid, rankPlayerInfo.MainId, RankType.IslandHigh, rankItem.Value.Rank, rankItem.Value.Rank, rankItem.Value.Score, RewardManager.GetRewardDic(data.Rewards));
                                }
                            }

                            server.RelationManager.SendRankInfoToRelation("islandHigh", rankInfoList, randMainId, stage);

                        }
                    });
                }
            }
        }

        public void SendFinalReward()
        {
            foreach (var kv in CrossBattleLibrary.GroupList)
            {
                int groupId = kv.Key;
                IslandHighRank highRank = server.RankMng.GetIslandHighRank(groupId);
                if (highRank != null)
                {
                    highRank.LoadInitRankFromRedis(() => 
                    {
                        List<string> rankInfoList = new List<string>();
                        int randMainId = 0;

                        Dictionary<int, RankBaseModel> uidRankInfoDic = highRank.GetRankInfoList();
                        foreach (var rankItem in uidRankInfoDic)
                        {
                            JsonPlayerInfo rankPlayerInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, rankItem.Value.Uid);
                            if (rankPlayerInfo != null)
                            {
                                CampBuildRankRewardData data = IslandHighLibrary.GetRankFinalRewardInfo(period, rankItem.Value.Rank);
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
                                    server.TrackingLoggerMng.TrackRankEmailLog(groupId, 0, RankType.IslandHigh.ToString(), rankItem.Value.Uid, rankItem.Value.Score, data.EmailId, rankItem.Value.Rank, server.Now());

                                    if (rankItem.Value.Rank < 100)
                                    {
                                        rankInfoList.Add(rankItem.Value.Rank + "_" + rankItem.Value.Uid + "_" + rankItem.Value.Score);
                                        randMainId = rankPlayerInfo.MainId;
                                    }
                                }

                                //BI
                                server.KomoeEventLogRankFlow(rankItem.Value.Uid, rankPlayerInfo.MainId, RankType.IslandHigh, rankItem.Value.Rank, rankItem.Value.Rank, rankItem.Value.Score, RewardManager.GetRewardDic(data.Rewards));
                            }
                        }
                        server.RelationManager.SendRankInfoToRelation("islandHigh", rankInfoList, randMainId);
                    });
                }
            }
        }      
    }
}
