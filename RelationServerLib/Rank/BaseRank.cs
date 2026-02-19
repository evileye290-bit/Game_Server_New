using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class BaseRank
    {
        protected RankType rankType;

        DateTime LastRefreshTime;
        protected RelationServerApi server;
        RankConfigInfo configInfo;

        protected Dictionary<int, RankBaseModel> uidRankInfoDic = new Dictionary<int, RankBaseModel>();


        TimeSpan RankUpdateTimeSpan;
        //Dictionary<int, int> uidScoreDic = new Dictionary<int, int>();
        //Dictionary<int, int> uidRankDic = new Dictionary<int, int>();
        public BaseRank(RelationServerApi server)
        {
            this.server = server;
        }

        public BaseRank(RelationServerApi server, RankType rankType) : this(server)
        {
            this.rankType = rankType;
        }

        public void Init()
        {
            configInfo = RankLibrary.GetConfig(rankType);
            if (configInfo == null)
            {
                Log.Error($"rank {rankType} init fail,can not find xml data ");
                return;
            }
            RankUpdateTimeSpan = configInfo.RankUpdateTimeSpan;
            LoadInitBattlePowerRankFromRedis();
        }


        public bool CheckNeedRefresh()
        {
            TimeSpan timeSpan = server.Now() - LastRefreshTime;
            return timeSpan > RankUpdateTimeSpan;
        }

        public void Clear()
        {
            server.GameRedis.Call(new OperateClearRank(rankType, server.MainId));
        }

        protected void LoadInitBattlePowerRankFromRedis()
        {
            if (configInfo == null)
            {

                return;
            }
            OperateGetRankScore op = new OperateGetRankScore(rankType, server.MainId, 0, configInfo.ShowCount - 1);
            server.GameRedis.Call(op, ret =>
            {
                uidRankInfoDic = op.uidRank;
                LastRefreshTime = server.Now();
                server.RPlayerInfoMng.RefreshPlayerList(new List<int>(uidRankInfoDic.Keys));
            });
        }

        private RankListModel PushRankListInfo(int uid, int page)
        {
            CheckKomoeLog();

            int totalCount = uidRankInfoDic.Count;
            int pCount = 20;
            int begin = 1;
            int end = 20;
            if (configInfo != null)
            {
                totalCount = Math.Min(uidRankInfoDic.Count, configInfo.ShowCount);
                pCount = configInfo.CountPerPage;
                begin = 1;
                end = pCount;
            }
            if (page > 0 && (page - 1) * pCount < totalCount)
            {
                begin = (page - 1) * pCount;
                end = Math.Min(page * pCount, totalCount);
            }

            RankListModel rankListModel = new RankListModel();
            //rankListModel.Camp = campType;
            rankListModel.Type = rankType;
            rankListModel.Page = page;
            rankListModel.TotalCount = totalCount;
            rankListModel.RankList = GetList(begin, end);
            rankListModel.OwnerInfo = GetRankOwnerInfo(uid);
            return rankListModel;
        }

        private RankBaseModel GetRankOwnerInfo(int uid)
        {
            RankBaseModel ownerInfo = GetRankBaseInfo(uid);
            if (ownerInfo == null)
            {
                //没有排行信息
                ownerInfo = new RankBaseModel();
                ownerInfo.Uid = uid;
                ownerInfo.Score = 0;
                ownerInfo.Time = server.Now();
            }
            return ownerInfo;
        }

        public RankBaseModel GetRankBaseInfo(int uid)
        {
            RankBaseModel info;
            uidRankInfoDic.TryGetValue(uid, out info);
            return info;
        }

        public List<PlayerRankModel> GetList(int begin, int end)
        {
            List<PlayerRankModel> rankList = new List<PlayerRankModel>();
            PlayerRankModel rankInfo;
            for (int i = begin; i < end; i++)
            {
                if (uidRankInfoDic.Count > i)
                {
                    rankInfo = new PlayerRankModel();
                    rankInfo.RankInfo = uidRankInfoDic.ElementAt(i).Value;

                    RedisPlayerInfo baseInfo = server.RPlayerInfoMng.GetPlayerInfo(rankInfo.RankInfo.Uid);
                    if (baseInfo != null)
                    {
                        rankInfo.BaseInfo = baseInfo;
                        rankList.Add(rankInfo);
                    }
                    else
                    {
                        Log.Warn($"load rank list error: can not find {0} data in server", rankInfo.RankInfo.Uid);
                    }
                }
            }
            return rankList;
        }


        internal void PushRankList(int uid, int page)
        {
            if (CheckNeedRefresh())
            {
                RefreshThenPushRankListMsg(uid, page, rankType);
            }
            else
            {
                Client client = server.ZoneManager.GetClient(uid);
                if (client == null)
                {
                    Log.Warn($"player {uid} PushRankListMsg failed: not find client ");
                    return;
                }

                RankListModel rankListModel = PushRankListInfo(uid, page);

                if (rankListModel == null)
                {
                    Log.Warn($"player {uid} PushRankListMsg failed: not find info ");
                    return;
                }
                MSG_RZ_GET_RANK_LIST rankMsg = RankManager.PushRankListMsg(rankListModel, rankType);
                client.Write(rankMsg);
            }
        }

        private void RefreshThenPushRankListMsg(int uid, int page, RankType pushType)
        {
            OperateGetRankScore op = new OperateGetRankScore(rankType, server.MainId, 0, configInfo.ShowCount - 1);
            server.GameRedis.Call(op, ret =>
            {
                uidRankInfoDic = op.uidRank;

                if (uidRankInfoDic == null || uidRankInfoDic.Count < 1)
                {
                    Log.Warn($"load rank list fail ,can not find data in redis");
                }

                Client client = server.ZoneManager.GetClient(uid);
                if (client == null)
                {
                    Log.Warn($"player {uid} RefreshThenPushRankListMsg failed: not find client ");
                    return;
                }

                RankListModel rankListModel = PushRankListInfo(uid, page);
                if (rankListModel == null)
                {
                    Log.Warn($"player {uid} RefreshThenPushRankListMsg failed: not find info ");
                    return;
                }
                MSG_RZ_GET_RANK_LIST rankMsg = RankManager.PushRankListMsg(rankListModel, pushType);
                client.Write(rankMsg);

                //检查舒服刷新数据
                server.RPlayerInfoMng.RefreshPlayerList(new List<int>(uidRankInfoDic.Keys));
                LastRefreshTime = server.Now();
            });
        }

        public virtual void ChangeScore(int uid, int score)
        {
            Dictionary<int, RankBaseModel> oldRankList = new Dictionary<int, RankBaseModel>();
            foreach (var kv in uidRankInfoDic)
            {
                oldRankList.Add(kv.Key, kv.Value);
            }

            //新
            RankBaseModel rankItem = new RankBaseModel();
            rankItem.Uid = uid;
            rankItem.Rank = 0;
            rankItem.Score = score;
            oldRankList[rankItem.Uid] = rankItem;
            //排序
            oldRankList = oldRankList.OrderByDescending(o => o.Value.Score).ToDictionary(o => o.Key, p => p.Value);

            Dictionary<int, RankBaseModel> newRankList = new Dictionary<int, RankBaseModel>();
            RankBaseModel tempRank;
            int i = 0;
            foreach (var kv in oldRankList)
            {
                i++;
                if (i <= configInfo.ShowCount)
                {
                    if (uidRankInfoDic.TryGetValue(kv.Value.Uid, out tempRank))
                    {
                        //说明当前有
                        if (tempRank.Rank != i)
                        {
                            //排名变更
                            UpdateCrossRank(kv.Value, i);
                        }
                        else
                        {
                            if (tempRank.Uid == uid)
                            {
                                //排名变更
                                UpdateCrossRank(kv.Value, i);
                            }
                        }
                    }
                    else
                    {
                        //没有，直接通知
                        UpdateCrossRank(kv.Value, i);
                    }
                    newRankList.Add(kv.Value.Uid, kv.Value);
                }
                else
                {
                    //出榜了
                    //kv.Value.Rank = 0;
                    UpdateCrossRank(kv.Value, 0);
                }
            }
            uidRankInfoDic = newRankList;


            BILoggerRecordLog(uid, score, rankItem);
        }
        private void BILoggerRecordLog(int uid, int addValue, RankBaseModel ownerInfo)
        {
            //if (GameConfig.TrackingLogSwitch)
            {
                int rank = 0;
                int tatalValue = ownerInfo.Score;
                ownerInfo = GetRankBaseInfo(uid);
                if (ownerInfo != null)
                {
                    rank = ownerInfo.Rank;
                }
                server.TrackingLoggerMng.RecordRealtionRankLog(uid, addValue, tatalValue, rank, rankType.ToString(), server.MainId, 0, server.Now());
            }
        }

        protected virtual void UpdateCrossRank(RankBaseModel tempRank, int rank)
        {
            tempRank.Rank = rank;

            server.GameDBPool.Call(new QueryUpdateCrossBattleRank(tempRank.Uid, rank));

            ////没有缓存信息，查看玩家是否在线
            //Client client = server.ZoneManager.GetClient(pcUid);
            //if (client != null)
            //{
            //找到玩家说明玩家在线，通知玩家发送信息回来
            MSG_RZ_UPDATE_CROSS_RANK msg = new MSG_RZ_UPDATE_CROSS_RANK();
            msg.PcUid = tempRank.Uid;
            msg.Rank = rank;
            server.ZoneManager.Broadcast(msg);
            //}
        }


        private DateTime nextTime { get; set; }
        public void CheckKomoeLog()
        {
            if (server.Now() > nextTime)
            {
                Task t = new Task(() =>
                {
                    Dictionary<int, RankBaseModel> dic = new Dictionary<int, RankBaseModel>(uidRankInfoDic);
                    foreach (var item in dic)
                    {
                        //BI
                        server.KomoeUserLogUserListSnapshot(item.Value.Uid, rankType, item.Value.Rank, item.Value.Score);
                    }
                });
                t.Start();
                nextTime = nextTime.AddHours(KomoeLogConfig.RankTime);
            }
        }
    }
}