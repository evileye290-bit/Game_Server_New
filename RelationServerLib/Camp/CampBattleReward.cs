using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public partial class CampBattleReward
    {
        private Dictionary<CampType, Dictionary<RankType, Dictionary<int, RankBaseModel>>> rankList =
            new Dictionary<CampType, Dictionary<RankType, Dictionary<int, RankBaseModel>>>();
        private CampType winCamp { get; set; }
        //private Dictionary<int, int> uidScoreList = new Dictionary<int, int>();
        //private Dictionary<CampType, Dictionary<int, int>> uidScoreList = new Dictionary<CampType, Dictionary<int, int>>();
        //private Dictionary<CampType, Dictionary<int, RankBaseModel>> uidCollectionList = new Dictionary<CampType, Dictionary<int, RankBaseModel>>();
        //private Dictionary<CampType, Dictionary<int, RankBaseModel>> uidFihgtList = new Dictionary<CampType, Dictionary<int, RankBaseModel>>();
        private RelationServerApi server;
        public CampBattleReward(RelationServerApi server)
        {
            this.server = server;
        }

        public void ClearRewardList()
        {
            rankList.Clear();
            //uidCollectionList.Clear();
            //uidFihgtList.Clear();
        }

        public void OnUpdate()
        {
            SendCampBattleFightReward();
        }


        //public Dictionary<RankType, Dictionary<int, RankBaseModel>> GetRankTypeList(CampType camp)
        //{
        //    Dictionary<RankType, Dictionary<int, RankBaseModel>> List;
        //    rankList.TryGetValue(camp, out List);
        //    return List;
        //}

        //public Dictionary<int, RankBaseModel> GetRankList(RankType rank, Dictionary<RankType, Dictionary<int, RankBaseModel>> dic)
        //{
        //    Dictionary<int, RankBaseModel> List;
        //    dic.TryGetValue(rank, out List);
        //    return List;
        //}

        public void InitList()
        {
            ClearRewardList();

            Dictionary<RankType, Dictionary<int, RankBaseModel>> list1 = new Dictionary<RankType, Dictionary<int, RankBaseModel>>();
            list1[RankType.CampBattleScore] = new Dictionary<int, RankBaseModel>();
            list1[RankType.CampBattleCollection] = new Dictionary<int, RankBaseModel>();
            rankList[CampType.TianDou] = list1;

            Dictionary<RankType, Dictionary<int, RankBaseModel>> list2 = new Dictionary<RankType, Dictionary<int, RankBaseModel>>();
            list2[RankType.CampBattleScore] = new Dictionary<int, RankBaseModel>();
            list2[RankType.CampBattleCollection] = new Dictionary<int, RankBaseModel>();
            rankList[CampType.XingLuo] = list2;

            foreach (var camp in rankList)
            {
                LoadRankListByRedis(camp.Key, camp.Value);
            }

            winCamp = server.CampActivityMng.GetWinCamp();
        }

        /// <summary>
        /// 发放奖励时间获取当前积分榜
        /// </summary>
        public void LoadRankListByRedis(CampType camp, Dictionary<RankType, Dictionary<int, RankBaseModel>> dic)
        {
            //Dictionary<RankType, Dictionary<int, RankBaseModel>> dic = GetRankTypeList(camp);
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
                        }
                        uidRankInfoDic = uidRankInfoDic.OrderByDescending(v => v.Value.Score).ThenBy(v => v.Value.Time).ToDictionary(o => o.Key, p => p.Value);

                        int maxCount = 0;
                        switch (rankType)
                        {
                            case RankType.CampBattleScore:
                                maxCount = uidRankInfoDic.Count;
                                break;
                            case RankType.CampBattleCollection:
                                maxCount = CampBattleLibrary.CollectionMax;
                                break;
                            //case RankType.CampBattleFight:
                            //    maxCount = CampBattleLibrary.FightMax;
                            //    break;
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

                //领袖
                dic[RankType.CampLeader] = GetLeaderList(camp);
            }
        }

        private Dictionary<int, RankBaseModel> GetLeaderList(CampType camp)
        {
            //List<PlayerRankBaseInfo> list = server.CampRankMng.GetElectionList(camp, 3);
            List<PlayerRankBaseInfo> list = server.CampRankMng.GetLeaderList(camp);
            Dictionary<int, RankBaseModel> dic = new Dictionary<int, RankBaseModel>();
            foreach (var item in list)
            {
                RankBaseModel rankBase = new RankBaseModel();
                rankBase.Uid = item.Uid;
                rankBase.Rank = item.Rank;
                dic.Add(rankBase.Uid, rankBase);
            }
            return dic;
        }

        public void SendCampBattleFightReward()
        {
            if (rankList.Count > 0)
            {
                foreach (var camp in rankList)
                {
                    BattleResult result = GetCampBattleResult(winCamp, camp.Key);
                    foreach (var rankType in camp.Value)
                    {
                        if (rankType.Value.Count > 0)
                        {
                            RankBaseModel info = rankType.Value.First().Value;
                            SendCampBattleReward(info, rankType.Key, result);
                            rankType.Value.Remove(info.Uid);
                            break;
                        }
                    }
                }
            }
        }

        private BattleResult GetCampBattleResult(CampType winCamp, CampType checkCamp)
        {
            BattleResult result;
            if (winCamp == CampType.None)
            {
                result = BattleResult.Tie;
            }
            else if (checkCamp == winCamp)
            {
                result = BattleResult.Win;
            }
            else
            {
                result = BattleResult.Fail;
            }
            return result;
        }

        private void SendCampBattleReward(RankBaseModel rankInfo, RankType type, BattleResult result)
        {
            List<CampBattleRewardModel> list = new List<CampBattleRewardModel>();

            switch (type)
            {
                case RankType.CampBattleScore:
                    {
                        CampBattleRewardModel info = CampBattleLibrary.GetScoreRewardInfo(rankInfo.Score);
                        if (info != null)
                        {
                            list.Add(info);
                        }
                        CampBattleRewardModel info1 = CampBattleLibrary.GetFightRewardInfo(rankInfo.Rank);
                        if (info1 != null)
                        {
                            list.Add(info1);
                        }

                    }
                   
                    break;
                case RankType.CampBattleCollection:
                    {
                        CampBattleRewardModel info = CampBattleLibrary.GetCollectionRewardInfo(rankInfo.Rank);
                        if (info !=null)
                        {
                            list.Add(info);
                        }
                    }
                    break;
                //case RankType.CampBattleFight:
                //    info = CampBattleLibrary.GetFightRewardInfo(rankInfo.Rank);
                //    break;
                case RankType.CampLeader:
                    {
                        CampBattleRewardModel info = CampBattleLibrary.GetLeaderRewardInfo(rankInfo.Rank);
                        if (info != null)
                        {
                            list.Add(info);
                        };
                    }
                    break;
                default:
                    break;
            }
            if (list.Count==0)
            {
                //Log.Warn("player {0} value {1} semd camop battle reward failed: not find {2} reward info", pcUid, value, type);
                return;
            }
            string biReward = string.Empty;
            foreach (var info in list)
            {
                string rewards = string.Empty;
                switch (result)
                {
                    case BattleResult.Win:
                        rewards = info.RewardsWin;
                        break;
                    case BattleResult.Fail:
                        rewards = info.RewardsLose;
                        break;
                    case BattleResult.Tie:
                        rewards = info.RewardsDeuce;
                        break;
                    default:
                        return;
                }
                //直接发邮件奖励，清理数据库
                //发送邮件
                server.EmailMng.SendPersonEmail(rankInfo.Uid, info.EmailId, rewards);

                biReward += "|" + rewards;
            }

            server.TrackingLoggerMng.TrackRankLog(server.MainId, type.ToString(), rankInfo.Rank, rankInfo.Score, rankInfo.Uid, server.Now());
            //BI
            server.KomoeEventLogRankFlow(rankInfo.Uid, type, rankInfo.Rank, rankInfo.Rank, rankInfo.Score, RewardManager.GetRewardDic(biReward));

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


        public void MergeServerReward()
        {
            InitList();
            //foreach (var camp in rankList)
            //{
            //    BattleResult result = GetCampBattleResult(winCamp, camp.Key);
            //    foreach (var rankType in camp.Value)
            //    {
            //        foreach (var kv in rankType.Value)
            //        {
            //            RankBaseModel info = kv.Value;
            //            SendCampBattleReward(info, rankType.Key, result);
            //        }
            //    }
            //}
            //rankList.Clear();
            Log.Warn("MergeServerReward Camp Battle Reward Success !");
        }
    }

}