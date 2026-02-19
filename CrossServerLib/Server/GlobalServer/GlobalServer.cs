using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Global.Protocol.GA;
using Message.Global.Protocol.GCross;
using Message.IdGenerator;
using MessagePacker;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace CrossServerLib
{
    public partial class GlobalServer : BaseGlobalServer
    {
        private CrossServerApi Api
        { get { return (CrossServerApi)api; } }
        public GlobalServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GCross_UPDATE_XML>.Value, OnResponse_UpdateXml);
            AddResponser(Id<MSG_GCross_MERGE_SERVER_REWARD>.Value, OnResponse_MergeServer);
            //ResponserEnd
        }

        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            MSG_GCross_UPDATE_XML msg = ProtobufHelper.Deserialize<MSG_GCross_UPDATE_XML>(stream);
            if (msg.Type == 1)
            {
                //Api.UpdateServerXml();
            }
            else
            {
                Api.UpdateXml();
            }
            Log.Write("GM update xml");
        }

        private void OnResponse_MergeServer(MemoryStream stream, int uid = 0)
        {
            MSG_GCross_MERGE_SERVER_REWARD msg = ProtobufHelper.Deserialize<MSG_GCross_MERGE_SERVER_REWARD>(stream);

            HashSet<int> groupList = new HashSet<int>();
            for (int i = msg.StartServerId; i <= msg.EndServerId; i++)
            {
                groupList.Add(CrossBattleLibrary.GetGroupId(i));
            }

            foreach (var groupId in groupList)
            {
                CrossBossRankManager manager = Api.RankMng.GetCrossBossRankManager(groupId);
                if(manager == null || manager.ChapterList.Count==0) continue;

                int chapter = 0;
                foreach (var kvp in manager.ChapterList.OrderByDescending(x => x.Key))
                {
                    if (kvp.Value.GetUidRankInfos().Count > 0)
                    {
                        chapter = kvp.Key;
                        break;
                    }
                }
                int siteId = CrossBossLibrary.GetBossDungeonId(chapter);

                //通关奖励
                //公告
                //Api.RelationManager.BroadcastAnnouncement(ANNOUNCEMENT_TYPE.CROSS_BOSS_PASS, MainId, siteInfo.Id);

                //获取这个BOSS排行榜发奖励
                OperateGetCrossRankScore totalOp = new OperateGetCrossRankScore(RankType.CrossBoss, groupId, chapter, 0, -1);
                Api.CrossRedis.Call(totalOp, totalRet =>
                {
                    if (totalOp.uidRank.Count > 0)
                    {
                        List<string> rankInfoList = new List<string>();
                        int randMainId = 0;

                        foreach (var rankItem in totalOp.uidRank)
                        {
                            JsonPlayerInfo rankPlayerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, rankItem.Value.Uid);
                            if (rankPlayerInfo != null)
                            {
                                if (rankItem.Value.Rank == 1)
                                {
                                    MSG_CorssR_CROSS_BOSS_RANK_REWARD firstMsg = new MSG_CorssR_CROSS_BOSS_RANK_REWARD();
                                    firstMsg.DungeonId = siteId;
                                    Api.RelationManager.WriteToRelation(firstMsg, rankPlayerInfo.MainId);
                                }

                                CampBuildRankRewardData data = CrossBossLibrary.GetRankRewardInfo(chapter, rankItem.Value.Rank);
                                if (data == null)
                                {
                                    break;
                                }

                                MSG_CorssR_SEND_FINALS_REWARD rankMsg = new MSG_CorssR_SEND_FINALS_REWARD();
                                rankMsg.MainId = rankPlayerInfo.MainId;
                                rankMsg.Uid = rankPlayerInfo.Uid;
                                rankMsg.Reward = data.Rewards;
                                rankMsg.EmailId = data.EmailId;
                                rankMsg.Param = $"{CommonConst.RANK}:{rankItem.Value.Rank}";
                                Api.RelationManager.WriteToRelation(rankMsg, rankMsg.MainId);

                                Api.TrackingLoggerMng.RecordSendEmailRewardLog(rankMsg.Uid, rankMsg.EmailId, rankMsg.Reward, rankMsg.Param, rankMsg.MainId, Api.Now());
                                Api.TrackingLoggerMng.TrackRankEmailLog(groupId, chapter, RankType.CrossBoss.ToString(), rankItem.Value.Uid, rankItem.Value.Score, data.EmailId, rankItem.Value.Rank, Api.Now());

                                if (rankItem.Value.Rank < 100)
                                {
                                    rankInfoList.Add(rankItem.Value.Rank + "_" + rankItem.Value.Uid + "_" + rankItem.Value.Score);
                                    randMainId = rankPlayerInfo.MainId;
                                }
                                //BI
                                Api.KomoeEventLogRankFlow(rankItem.Value.Uid, rankPlayerInfo.MainId, RankType.CrossBoss, rankItem.Value.Rank, rankItem.Value.Rank, rankItem.Value.Score, RewardManager.GetRewardDic(data.Rewards));
                            }
                        }

                        //通知玩家自己领取
                        Api.RelationManager.BroadcastToGroupRelation(new MSG_CorssR_CROSS_BOSS_PASS_REWARD() { DungeonId = siteId }, groupId);

                        Api.RelationManager.SendRankInfoToRelation("crossBoss", rankInfoList, randMainId);
                    }
                });
            }

        }
    }
}