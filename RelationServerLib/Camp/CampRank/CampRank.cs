using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using RedisUtility;
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
    public class CampRank
    {
        protected CampType campType = CampType.None;
        protected RankType rankType = RankType.None;

        //Dictionary<int, CampRankPlayerBaseInfo> uidPlayerInfoDic = new Dictionary<int, CampRankPlayerBaseInfo>();
        Dictionary<int, RankBaseModel> uidRankInfoDic = new Dictionary<int, RankBaseModel>();

        DateTime LastRefreshTime;

        protected RelationServerApi server;

        RankConfigInfo configInfo;

        TimeSpan RankUpdateTimeSpan;
        //TimeSpan InfoUpdateTimeSpan;
        //bool SyncUpdate;

        public CampRank(RelationServerApi server, CampType campType, RankType rankType)
        {
            this.server = server;
            this.campType = campType;
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

            //加载排行信息uid  score
            //加载排行的playerinfo 
            LoadInitCampRankFromRedis();
        }



        public bool CheckNeedRefresh()
        {
            TimeSpan timeSpan = server.Now() - LastRefreshTime;
            return timeSpan > RankUpdateTimeSpan;
        }

        public void RefreshList(int uid)
        {   
            if (CheckNeedRefresh())
            {
                LoadRankList();
            }
        }

        //public void AddScore(int uid, int score)
        //{
        //    //加到redis
        //    server.Redis.Call(new OperateIncrementCampRankScore(server.MainId, (int)campType, rankType, uid, score, server.Now()));
        //    //
        //    if (!uidPlayerInfoDic.ContainsKey(uid))
        //    {
        //        LoadPlayerInfo(uid);
        //    }

        //    if (CheckNeedRefresh())
        //    {
        //        LoadRankList();
        //    }
        //}


        public void Clear()
        {
            server.GameRedis.Call(new OperateClearCampRank(server.MainId, (int)campType, rankType));
        }

        /// <summary>
        ///这里返回值需要是个protobuf结构，目前先写成这样
        /// </summary>
        /// <param name="page"></param>
        public void PushRankList(int uid, int page)
        {
            PushRankList(uid, page, rankType);
        }

        internal void PushRankList(int uid, int page, RankType pushType)
        {
            if (CheckNeedRefresh())
            {
                RefreshThenPushRankListMsg(uid, page, pushType);
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
                MSG_RZ_GET_RANK_LIST rankMsg = RankManager.PushRankListMsg(rankListModel, pushType);
                client.Write(rankMsg);
            }
        }

        private RankListModel PushRankListInfo(int uid, int page)
        {
            int totalCount = uidRankInfoDic.Count;
            int pCount = 20;
            int begin = 1;
            int end = 20;
            RankConfigInfo config = RankLibrary.GetConfig(rankType);
            if (config != null)
            {
                totalCount = Math.Min(uidRankInfoDic.Count, config.ShowCount);
                pCount = config.CountPerPage;
                begin = 1;
                end = pCount;
            }
            if (page > 0 && (page - 1) * pCount < totalCount)
            {
                begin = (page - 1) * pCount;
                end = Math.Min(page * pCount, totalCount);
            }

            RankListModel rankListModel = new RankListModel();
            rankListModel.Camp = campType;
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

        public void LoadRankList()
        {
            OperateGetCampRankList op = new OperateGetCampRankList(server.MainId, (int)campType, rankType);
            server.GameRedis.Call(op, ret =>
            {
                uidRankInfoDic = op.uidRank;
                if (uidRankInfoDic == null || uidRankInfoDic.Count < 1)
                {
                    Log.Warn($"load rank list fail ,can not find data in redis");
                }
                uidRankInfoDic = uidRankInfoDic.OrderByDescending(v => v.Value.Score).ThenBy(v => v.Value.Time).ToDictionary(o => o.Key, p => p.Value);
                int count = uidRankInfoDic.Count;
                int i = 0;
                foreach(var element in uidRankInfoDic)
                {
                    element.Value.Rank = ++i;
                }
                //for (int i = 0; i < count; i++)
                //{
                //    uidRankInfoDic.ElementAt(i).Value.Rank = i + 1;
                //}
                //检查舒服刷新数据
                server.RPlayerInfoMng.RefreshPlayerList(new List<int>(uidRankInfoDic.Keys));
                LastRefreshTime = server.Now();
            });
        }

        public void RefreshThenPushRankListMsg(int uid, int page, RankType pushType)
        {
            OperateGetCampRankList op = new OperateGetCampRankList(server.MainId, (int)campType, rankType);
            server.GameRedis.Call(op, ret =>
            {
                uidRankInfoDic = op.uidRank;
                if (uidRankInfoDic == null || uidRankInfoDic.Count < 1)
                {
                    Log.Warn($"load rank list fail ,can not find data in redis");
                }
                uidRankInfoDic = uidRankInfoDic.OrderByDescending(v => v.Value.Score).ThenBy(v => v.Value.Time).ToDictionary(o => o.Key, p => p.Value);
                int count = uidRankInfoDic.Count;
                int i = 0;
                foreach (var element in uidRankInfoDic)
                {
                    element.Value.Rank = ++i;
                }
                //for (int i = 0; i < count; i++)
                //{
                //    uidRankInfoDic.ElementAt(i).Value.Rank = i + 1;
                //}

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

        public void LoadInitCampRankFromRedis()
        {
            //uidPlayerInfoDic.Clear();

            OperateGetCampRankList op = new OperateGetCampRankList(server.MainId, (int)campType, rankType);
            server.GameRedis.Call(op, ret =>
            {
                if (op.uidRank == null)
                {
                    Log.Error($"load init server {server.MainId} camp {campType} rank {rankType} info fail,redis can not find data");
                }
                else
                {
                    uidRankInfoDic = op.uidRank;

                    uidRankInfoDic = uidRankInfoDic.OrderByDescending(v => v.Value.Score).ThenBy(v => v.Value.Time).ToDictionary(o => o.Key, p => p.Value);
                    int count = uidRankInfoDic.Count;
                    int i = 0;
                    foreach (var element in uidRankInfoDic)
                    {
                        element.Value.Rank = ++i;
                    }
                    //for (int i = 0; i < count; i++)
                    //{
                    //    uidRankInfoDic.ElementAt(i).Value.Rank = i + 1;
                    //}
                    //检查舒服刷新数据
                    server.RPlayerInfoMng.RefreshPlayerList(new List<int>(uidRankInfoDic.Keys));
                    LastRefreshTime = server.Now();

                }
            });
        }
      
    }
}
