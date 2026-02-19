using CommonUtility;
using EnumerateUtility;
using Message.Corss.Protocol.CorssR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace CrossServerLib
{
    public class GardenManager
    {
        private CrossServerApi server { get; set; }
        private int period = 1;
        public int Period => period;

        public GardenManager(CrossServerApi server)
        {
            this.server = server;
            UpdatePeriod();
        }

        public void Clear()
        {
            server.RankMng.ClearGardenRank();

            MSG_CorssR_CLEAR_VALUE msg = new MSG_CorssR_CLEAR_VALUE();
            msg.GiftType = (int)RechargeGiftType.Garden;
            server.RelationManager.Broadcast(msg);
        }

        public void UpdatePeriod()
        {
            RechargeGiftModel model;
            RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.Garden, server.Now(), out model);
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
                GardenRank gardenRank = server.RankMng.GetGardenRank(groupId);
                if (gardenRank != null)
                {
                    gardenRank.LoadInitRankFromRedis(() => 
                    {
                        List<string> rankInfoList = new List<string>();
                        int randMainId = 0;

                        Dictionary<int, RankBaseModel> uidRankInfoDic = gardenRank.GetRankInfoList();
                        foreach (var rankItem in uidRankInfoDic)
                        {
                            JsonPlayerInfo rankPlayerInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, rankItem.Value.Uid);
                            if (rankPlayerInfo != null)
                            {
                                CampBuildRankRewardData data = RechargeLibrary.GetRankRewardInfo(Period, rankItem.Value.Rank);
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
                                    server.TrackingLoggerMng.TrackRankEmailLog(groupId, 0, RankType.Garden.ToString(), rankItem.Value.Uid, rankItem.Value.Score, data.EmailId, rankItem.Value.Rank, server.Now());
                                //BI
                                server.KomoeEventLogRankFlow(rankItem.Value.Uid, rankPlayerInfo.MainId, RankType.DivineLove, rankItem.Value.Rank, rankItem.Value.Rank, rankItem.Value.Score, RewardManager.GetRewardDic(data.Rewards));

                                    if (rankItem.Value.Rank < 100)
                                    {
                                        rankInfoList.Add(rankItem.Value.Rank + "_" + rankItem.Value.Uid + "_" + rankItem.Value.Score);
                                        randMainId = rankPlayerInfo.MainId;
                                    }
                                }
                            }
                        }
                        server.RelationManager.SendRankInfoToRelation("garden", rankInfoList, randMainId);
                    });
                }
            }
        }
    }
}
