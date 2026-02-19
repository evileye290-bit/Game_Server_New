using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;
using Google.Protobuf.WellKnownTypes;
using Logger;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;

namespace RelationServerLib
{
    public partial class CampBuild
    {
        private Dictionary<CampType, Dictionary<RankType, Dictionary<int, RankBaseModel>>> rankList =
    new Dictionary<CampType, Dictionary<RankType, Dictionary<int, RankBaseModel>>>();


        private void ClearRewardList()
        {
            rankList.Clear();
        }

        public void SendRewardUpdate()
        {
            SendCampBuildValueReward();
        }

        public Dictionary<RankType, Dictionary<int, RankBaseModel>> GetRankTypeList(CampType camp)
        {
            Dictionary<RankType, Dictionary<int, RankBaseModel>> List;
            rankList.TryGetValue(camp, out List);
            return List;
        }

        public Dictionary<int, RankBaseModel> GetRankList(RankType rank, Dictionary<RankType, Dictionary<int, RankBaseModel>> dic)
        {
            Dictionary<int, RankBaseModel> List;
            dic.TryGetValue(rank, out List);
            return List;
        }

        public void InitRankList()
        {
            ClearRewardList();

            Dictionary<RankType, Dictionary<int, RankBaseModel>> list1 = new Dictionary<RankType, Dictionary<int, RankBaseModel>>();
            list1[RankType.CampBuild] = new Dictionary<int, RankBaseModel>();
            rankList[CampType.TianDou] = list1;

            Dictionary<RankType, Dictionary<int, RankBaseModel>> list2 = new Dictionary<RankType, Dictionary<int, RankBaseModel>>();
            list2[RankType.CampBuild] = new Dictionary<int, RankBaseModel>();
            rankList[CampType.XingLuo] = list2;

            foreach (var camp in rankList.Keys)
            {
                LoadRankListByRedis(camp);
            }
        }


        /// <summary>
        /// 发放奖励时间获取当前积分榜
        /// </summary>
        public void LoadRankListByRedis(CampType camp)
        {
            Dictionary<RankType, Dictionary<int, RankBaseModel>> dic = GetRankTypeList(camp);
            if (dic != null)
            {
                foreach (var rankType in dic.Keys)
                {
                    OperateGetCampRankList op = new OperateGetCampRankList(server.MainId, (int)camp, rankType);
                    server.GameRedis.Call(op, ret =>
                    {
                        Dictionary<int, RankBaseModel> uidRankInfoDic = op.uidRank;
                        if (uidRankInfoDic == null || uidRankInfoDic.Count < 1)
                        {
                            Log.Warn($"load rank list fail ,can not find data in redis");
                            return;
                        }
                        uidRankInfoDic = uidRankInfoDic.OrderByDescending(v => v.Value.Score).ThenBy(v => v.Value.Time).ToDictionary(o => o.Key, p => p.Value);

                        int maxCount = 0;
                        switch (rankType)
                        {
                            case RankType.CampBuild:
                                maxCount = RankLibrary.GetConfig(RankType.CampBuild).ShowCount;
                                break;
                            default:
                                break;
                        }
                        int i = 0;
                        Dictionary<int, RankBaseModel> newLsit = new Dictionary<int, RankBaseModel>();
                        foreach (var item in uidRankInfoDic)
                        {
                            if (maxCount > 0)
                            {
                                i++;
                                item.Value.Rank = i;
                                newLsit.Add(item.Key, item.Value);
                                maxCount--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        dic[rankType] = newLsit;
                    });
                }
            }
        }

        public void SendCampBuildValueReward()
        {
            if (rankList.Count > 0)
            {
                foreach (var camp in rankList)
                {
                    foreach (var rankType in camp.Value)
                    {
                        if (rankType.Value.Count>0)
                        {
                            RankBaseModel info = rankType.Value.First().Value;
                            SendCampBuildValueReward(info);
                            rankType.Value.Remove(info.Uid);
                            break;
                        }
                    }
                }
            }
        }

        public void MergeServerReward()
        {
            InitRankReward();
            //foreach (var camp in rankList)
            //{
            //    foreach (var rankType in camp.Value)
            //    {
            //        foreach (var kv in rankType.Value)
            //        {
            //            SendCampBuildValueReward(kv.Value);
            //        }
            //    }
            //}
            Log.Warn("MergeServerReward Camp Build Reward Success !");
        }

        private void SendCampBuildValueReward(RankBaseModel rankInfo)
        {
            CampBuildRankRewardData data = CampBuildLibrary.GetCampBuildRankRewardInfo(nowShowPhaseNum, rankInfo.Rank);
            if (data == null)
            {
                //Log.Warn("player {0} value {1} semd camop battle reward failed: not find {2} reward info", pcUid, value, type);
                return;
            }

            string rewards = string.Empty;
            rewards = data.Rewards;
            //直接发邮件奖励，清理数据库
            //发送邮件
            server.EmailMng.SendPersonEmail(rankInfo.Uid, data.EmailId, rewards);
            server.TrackingLoggerMng.TrackRankLog(server.MainId, RankType.CampBuild.ToString(), rankInfo.Rank, rankInfo.Score, rankInfo.Uid, server.Now());

            //BI
            server.KomoeEventLogRankFlow(rankInfo.Uid, RankType.CampBuild, rankInfo.Rank, rankInfo.Rank, rankInfo.Score, RewardManager.GetRewardDic(rewards));
            //Client client = server.ZoneManager.GetClient(pcUid);
            //if (client != null)
            //{
            //    //直接发邮件奖励，清理数据库
            //    //发送邮件
            //    server.EmailMng.SendPersonEmail(pcUid, info.EmailId, info.Rewards);
            //}
            //else
            //{
            //    //不在线
            //    //记录已经发送奖励
            //server.GameDBPool.Call(new QueryUpdateCampBattleScoreReward(pcUid, info.Id));

            //    MSG_RZ_SAVE_EMAI msg = new MSG_RZ_SAVE_EMAI();
            //    msg.PcUid = pcUid;
            //    msg.Id = info.Id;
            //    msg.EmailId = info.EmailId;
            //    msg.Reward = info.Rewards;
            //    msg.EmailType = (int)type;
            //    server.ZoneManager.Broadcast(msg);
            //}
        }



    }
}
