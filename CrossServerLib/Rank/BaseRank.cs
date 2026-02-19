using EnumerateUtility;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrossServerLib
{
    public class BaseRank
    {
        protected RankType rankType;
        protected int groupId;
        protected int paramId;
        protected int rank;

        DateTime LastRefreshTime;
        protected CrossServerApi server;
        RankConfigInfo configInfo;

        protected Dictionary<int, RankBaseModel> uidRankInfoDic = new Dictionary<int, RankBaseModel>();
        protected Dictionary<int, int> rankUidDic = new Dictionary<int, int>();
        TimeSpan RankUpdateTimeSpan;

        //public RankBaseModel FirstRank;
        public BaseRank(CrossServerApi server, RankType rankType)
        {
            this.rankType = rankType;
            this.server = server;

            nextTime = server.Now().AddHours(KomoeLogConfig.RankTime);
        }
        public Dictionary<int, RankBaseModel> GetUidRankInfos()
        {
            return uidRankInfoDic;
        }

        public void Init(int groupId, int paramId)
        {
            this.groupId = groupId;
            this.paramId = paramId;
            configInfo = RankLibrary.GetConfig(rankType);
            if (configInfo == null)
            {
                Log.Error($"rank {rankType} init fail,can not find xml data ");
                return;
            }
            RankUpdateTimeSpan = configInfo.RankUpdateTimeSpan;
        }

        public RankBaseModel GetFirst()
        {
            if (CheckNeedRefresh())
            {
                LoadInitRankFromRedis();
            }
            int uid = GetRankUid(1);
            if (uid > 0)
            {
                return GetRankBaseInfo(uid);
            }
            else
            {
                return null;
            }
        }

        public bool CheckNeedRefresh()
        {
            TimeSpan timeSpan = server.Now() - LastRefreshTime;
            return timeSpan > RankUpdateTimeSpan;
        }

        public void Clear()
        {
            //FirstRank = null;
            uidRankInfoDic.Clear();
            server.CrossRedis.Call(new OperateClearCrossRank(rankType, groupId, paramId));
        }

        public void LoadInitRankFromRedis(Action callback = null)
        {
            if (configInfo == null)
            {
                return;
            }
            OperateGetCrossRankScore op = new OperateGetCrossRankScore(rankType, groupId, paramId, 0, -1);
            server.CrossRedis.Call(op, ret =>
            {
                uidRankInfoDic = op.uidRank;
                rankUidDic = op.rankUidDic;
                LastRefreshTime = server.Now();
                //server.PlayerInfoMng.HiddenWeaponMng.RefreshPlayerList(new List<int>(uidRankInfoDic.Keys));

                //FirstRank = GetInitFirst();

                callback?.Invoke();
            });
        }

        private RankListModel PushRankListInfo(int uid, int page)
        {
            int totalCount = rankUidDic.Count;
            int pCount = 20;
            int begin = 0;
            int end = 20;
            if (configInfo != null)
            {
                totalCount = Math.Min(rankUidDic.Count, configInfo.ShowCount);
                pCount = configInfo.CountPerPage;
                begin = 0;
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

        public RankBaseModel GetRankOwnerInfo(int uid)
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
            for (int i = begin + 1; i <= end; i++)
            {
                int pcUid = GetRankUid(i);
                if (pcUid > 0)
                {
                    RankBaseModel info = GetRankBaseInfo(pcUid);
                    if (info != null)
                    {
                        rankInfo = new PlayerRankModel();
                        rankInfo.RankInfo = info;

                        JsonPlayerInfo baseInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, rankInfo.RankInfo.Uid);
                        if (baseInfo != null)
                        {
                            rankInfo.JsonInfo = baseInfo;
                            rankList.Add(rankInfo);
                        }
                        else
                        {
                            Log.Warn($"load rank list error: can not find {0} data in server", rankInfo.RankInfo.Uid);
                        }
                    }
                    else
                    {
                        Log.Warn($"ChangeScore failed: not find info player {pcUid} ");
                    }
                }
                else
                {
                    break;
                }
            }
            return rankList;
        }


        internal void PushRankList(int uid, int mainId, int page)
        {
            if (CheckNeedRefresh())
            {
                RefreshThenPushRankListMsg(uid, mainId, page);
            }
            else
            {
                RankListModel rankListModel = PushRankListInfo(uid, page);

                if (rankListModel == null)
                {
                    Log.Warn($"player {uid} PushRankListMsg failed: not find info ");
                    return;
                }
                MSG_RZ_GET_RANK_LIST rankMsg = PushRankListMsg(rankListModel, GetRankType());
                server.RelationManager.WriteToRelation(rankMsg, mainId, uid);
            }
        }

        public static MSG_RZ_GET_RANK_LIST PushRankListMsg(RankListModel Info, RankType pushType)
        {
            MSG_RZ_GET_RANK_LIST rankMsg = new MSG_RZ_GET_RANK_LIST();
            rankMsg.RankType = (int)pushType;
            rankMsg.Page = Info.Page;
            rankMsg.Camp = (int)Info.Camp;
            rankMsg.Count = Info.TotalCount;

            foreach (var player in Info.RankList)
            {
                PlayerRankMsg info = GetRankPlayerMsgInfo(player);
                rankMsg.RankList.Add(info);
            }
            if (Info.OwnerInfo != null)
            {
                rankMsg.Info = GetRankBaseInfo(Info.OwnerInfo);
            }
            return rankMsg;
        }

        private static PlayerRankMsg GetRankPlayerMsgInfo(PlayerRankModel player)
        {
            PlayerRankMsg info = new PlayerRankMsg();
            if (player.BaseInfo != null)
            {
                info.BaseInfo.AddRange(GetRankPlayerBaseInfoItem(player.BaseInfo));
            }
            if (player.JsonInfo != null)
            {
                info.BaseInfo.AddRange(GetRankPlayerBaseInfoItem(player.JsonInfo));
            }

            if (player.RankInfo != null)
            {
                info.Rank = GetRankBaseInfo(player.RankInfo);
            }
            return info;
        }

        public static List<HFPlayerBaseInfoItem> GetRankPlayerBaseInfoItem(RedisPlayerInfo player)
        {
            List<HFPlayerBaseInfoItem> list = new List<HFPlayerBaseInfoItem>();
            if (player != null)
            {
                HFPlayerBaseInfoItem item;
                foreach (var kv in player.DataList)
                {
                    item = new HFPlayerBaseInfoItem();
                    item.Key = (int)kv.Key;
                    item.Value = kv.Value.ToString();
                    list.Add(item);
                }
            }
            return list;
        }
        public static List<HFPlayerBaseInfoItem> GetRankPlayerBaseInfoItem(JsonPlayerInfo player)
        {
            List<HFPlayerBaseInfoItem> list = new List<HFPlayerBaseInfoItem>();
            list.Add(GetPlayerInfoMsg(HFPlayerInfo.Uid, player.Uid.ToString()));
            list.Add(GetPlayerInfoMsg(HFPlayerInfo.Name, player.Name));
            list.Add(GetPlayerInfoMsg(HFPlayerInfo.MainId, player.MainId.ToString()));
            list.Add(GetPlayerInfoMsg(HFPlayerInfo.HeroId, player.HeroId.ToString()));
            list.Add(GetPlayerInfoMsg(HFPlayerInfo.BattlePower, player.BattlePower.ToString()));
            list.Add(GetPlayerInfoMsg(HFPlayerInfo.GodType, player.GodType.ToString()));
            list.Add(GetPlayerInfoMsg(HFPlayerInfo.Icon, player.Icon.ToString()));
            return list;
        }
        private static HFPlayerBaseInfoItem GetPlayerInfoMsg(HFPlayerInfo key, string value)
        {
            HFPlayerBaseInfoItem item = new HFPlayerBaseInfoItem();
            item.Key = (int)key;
            item.Value = value;
            return item;
        }

        private static RankBaseInfo GetRankBaseInfo(RankBaseModel player)
        {
            RankBaseInfo rank = new RankBaseInfo();
            rank.Uid = player.Uid;
            rank.Rank = player.Rank;
            rank.Score = player.Score;
            return rank;
        }



        private void RefreshThenPushRankListMsg(int uid, int mainId, int page)
        {
            OperateGetCrossRankScore op = new OperateGetCrossRankScore(rankType, groupId, paramId, 0, -1);
            server.CrossRedis.Call(op, ret =>
            {
                uidRankInfoDic = op.uidRank;
                rankUidDic = op.rankUidDic;

                if (uidRankInfoDic == null || uidRankInfoDic.Count < 1)
                {
                    Log.Warn($"load rank list fail ,can not find data in redis");
                }

                RankListModel rankListModel = PushRankListInfo(uid, page);
                if (rankListModel == null)
                {
                    Log.Warn($"player {uid} RefreshThenPushRankListMsg failed: not find info ");
                    return;
                }

                MSG_RZ_GET_RANK_LIST rankMsg = PushRankListMsg(rankListModel, GetRankType());
                server.RelationManager.WriteToRelation(rankMsg, mainId, uid);

                ////检查舒服刷新数据
                //server.RPlayerInfoMng.RefreshPlayerList(new List<int>(uidRankInfoDic.Keys));
                LastRefreshTime = server.Now();

                //FirstRank = GetInitFirst();
            });
        }

        protected virtual RankType GetRankType()
        {
            return rankType;
        }

        private void UpdateCrossRank(RankBaseModel info, int rank)
        {
            int start = rank;
            int end = rankUidDic.Count;
            if (info.Rank > 0)
            {
                //说明本身有排名
                end = info.Rank - 1;
            }

            info.Rank = rank;
            uidRankInfoDic[info.Uid] = info;

            Dictionary<int, int> oldRankUidDic = new Dictionary<int, int>(rankUidDic);

            rankUidDic[rank] = info.Uid;

            if (oldRankUidDic.Count > 0)
            {
                int pcUid;
                for (int i = start; i <= end; i++)
                {
                    if (oldRankUidDic.TryGetValue(i, out pcUid))
                    {
                        if (pcUid != info.Uid)
                        {
                            RankBaseModel rankInfo = GetRankBaseInfo(pcUid);
                            if (rankInfo != null)
                            {
                                rankInfo.Rank = i + 1;
                                rankUidDic[rankInfo.Rank] = rankInfo.Uid;
                            }
                            else
                            {
                                Log.Warn($"UpdateCrossRank failed: not find info player {pcUid} ");
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private int GetRankUid(int rank)
        {
            int uid;
            rankUidDic.TryGetValue(rank, out uid);
            return uid;
        }

        public RankBaseModel ChangeScore(int uid, int score)
        {
            RankBaseModel ownerInfo = GetRankBaseInfo(uid);
            if (ownerInfo == null)
            {
                //新
                ownerInfo = new RankBaseModel();
                ownerInfo.Uid = uid;
                ownerInfo.Rank = 0;
                ownerInfo.Score = score;
                uidRankInfoDic[uid] = ownerInfo;
            }
            else
            {
                ownerInfo.Score = score;
            }

            int start = rankUidDic.Count;
            if (ownerInfo.Rank > 0)
            {
                start = ownerInfo.Rank;
            }

            if (start > 0)
            {
                //排序
                int rank = 0;
                for (int i = start; i > 0; i--)
                {
                    int pcUid = GetRankUid(i);
                    if (pcUid > 0)
                    {
                        if (pcUid != uid)
                        {
                            RankBaseModel info = GetRankBaseInfo(pcUid);
                            if (info != null)
                            {
                                if (score > info.Score)
                                {
                                    //说明分高,跟新自己的
                                    UpdateCrossRank(ownerInfo, i);
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                Log.Warn($"ChangeScore failed: not find info player {pcUid} ");
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        rank = i;
                        break;
                    }
                }
                if (rank > 0)
                {
                    UpdateCrossRank(ownerInfo, rank);
                }
            }
            else
            {
                UpdateCrossRank(ownerInfo, 1);
            }
            ////排序
            //int rank = 0;
            //for (int i = 1; i <= configInfo.ShowCount; i++)
            //{
            //    int pcUid = GetRankUid(i);
            //    if (pcUid > 0)
            //    {
            //        if (pcUid != uid)
            //        {
            //            RankBaseModel info = GetRankBaseInfo(pcUid);
            //            if (info != null)
            //            {
            //                if (score > info.Score)
            //                {
            //                    //说明分高,跟新自己的
            //                    UpdateCrossRank(ownerInfo, i);
            //                    break;
            //                }
            //                else
            //                {
            //                    continue;
            //                }
            //            }
            //            else
            //            {
            //                Log.Warn($"ChangeScore failed: not find info player {pcUid} ");
            //            }
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //    else
            //    {
            //        rank = i;
            //        break;
            //    }
            //}
            //if (rank > 0)
            //{
            //    UpdateCrossRank(ownerInfo, rank);
            //}
            CheckKomoeLog();

            return ownerInfo;
        }

        public int GetNewRankInfo(int uid, int mainId)
        {
            int rank = -1;
            if (CheckNeedRefresh())
            {
                RefreshRankInfo(uid, mainId);
            }
            else
            {
               rank = GetUidRank(uid);
            }
            return rank;
        }

        private void RefreshRankInfo(int uid, int mainId)
        {
            OperateGetCrossRankScore op = new OperateGetCrossRankScore(rankType, groupId, paramId, 0, -1);
            server.CrossRedis.Call(op, ret =>
            {
                uidRankInfoDic = op.uidRank;
                rankUidDic = op.rankUidDic;

                if (uidRankInfoDic == null || uidRankInfoDic.Count < 1)
                {
                    Log.Warn($"refresh rank fail ,can not find data in redis");
                }
                LastRefreshTime = server.Now();

                int rank = GetUidRank(uid);
                MSG_CorssR_GET_RANK_REWARD response = new MSG_CorssR_GET_RANK_REWARD();
                response.Uid = uid;
                response.RankType = (int)rankType;
                response.Rank = rank;
                server.RelationManager.WriteToRelation(response, mainId, uid);
            });
        }

        public int GetUidRank(int uid)
        {
            RankBaseModel rank;
            uidRankInfoDic.TryGetValue(uid, out rank);
            if (rank != null)
            {
                return rank.Rank;
            }
            return 0;
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
                        JsonPlayerInfo baseInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, item.Value.Uid);
                        if (baseInfo != null)
                        {
                            server.KomoeUserLogUserListSnapshot(item.Value.Uid, baseInfo.MainId, rankType, item.Value.Rank, item.Value.Score, groupId);
                        }
                    }
                });
                t.Start();
                nextTime = server.Now().AddHours(KomoeLogConfig.RankTime);
            }
        }
    }
}