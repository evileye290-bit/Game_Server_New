using DBUtility;
using EnumerateUtility;
using Logger;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationServerLib
{
    //public class CampElectionManager : AbstractRankManager
    public class CampElectionManager
    {
        private CampType camp = CampType.None;
        private RankType rankType = RankType.CampBattlePower;
        private int mainId;
        bool Loaded = false;
        RelationServerApi server;
        //Dictionary<int, ElectionClient> rankClients = new Dictionary<int, ElectionClient>();
        //Dictionary<int, ElectionClient> uidClients = new Dictionary<int, ElectionClient>();

        protected Dictionary<int, RankBaseModel> uidRankInfoDic = new Dictionary<int, RankBaseModel>();
        List<WorshipRedisInfo> curWorships = new List<WorshipRedisInfo>();

        public List<WorshipRedisInfo> GetCurWorships()
        {
            return curWorships;
        }

        //public CampElectionManager(RelationServerApi api, int zoneId, RankType type, CampType camp) : base(api, zoneId, type)
        public CampElectionManager(RelationServerApi api, int zoneId, RankType rankType, CampType camp)
        {
            server = api;
            mainId = zoneId;
            this.camp = camp;
            this.rankType = rankType;
            LoadFromRedis();
        }

        //public bool NextPeriod()
        //{
        //    if (end < DateTime.Now)
        //    {
        //        Tuple<DateTime, DateTime> period = RankLibrary.GetNextPeriod(RankType);
        //        begin = period.Item1;
        //        end = period.Item2;
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public DateTime lastAllowedUpdate = DateTime.Now;

        //        public bool CheckNextPeriod()
        //        {
        //#if DEBUG
        //            if (CampLibrary.InDebug)
        //            {
        //                return false;
        //            }
        //#endif
        //            if (end < RelationServerApi.now)
        //            {
        //                hisBegin = begin;
        //                hisEnd = end;
        //                Tuple<DateTime, DateTime> period = RankLibrary.GetNextPeriod(RankType, server.OpenServerTime, PeriodCount + 1);
        //                begin = period.Item1;
        //                end = period.Item2;
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }

        //        public bool CheckNeedReStart()
        //        {
        //            if (RelationServerApi.now > begin)// && NowShowPeriod != PeriodCount)
        //            {
        //#if DEBUG
        //                if (CampLibrary.InDebug)
        //                {
        //                    return false;
        //                }
        //#endif
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }

        //        public void Update()
        //        {
        //            if (Loaded)
        //            {
        //                //if (CheckNextPeriod())
        //                //{
        //                //    PeriodCount++;
        //                //}
        //                if (CheckNeedReStart())
        //                {
        //                    ReStartRank();
        //                }

        //                //Logger.Log.Debug($"Camp {camp} count {rankClients.Count} Period {PeriodCount} RankType {RankType} begin {begin} end {end}");

        //                DateTime now = RelationServerApi.now;
        //                if ((now < begin || now > end) && !CampLibrary.InDebug)
        //                {
        //                    return;
        //                }

        //                if (CheckUpdateRank())
        //                {
        ////#if DEBUG

        ////                    Logger.Log.Debug($"Camp {camp} count {uidRankInfoDic.Count} Period {PeriodCount} RankType {RankType} begin {begin} end {end}");
        ////#endif
        //                    LoadRankList();
        //                    //int count;
        //                    //if (Config.ShowCount == -1)
        //                    //{
        //                    //    count = -1;
        //                    //}
        //                    //else
        //                    //{
        //                    //    count = Config.ShowCount - 1;
        //                    //}
        //                    //OperateGetCampElectionRank op = new OperateGetCampElectionRank(mainId, (int)camp, 0, count);
        //                    //server.Redis.Call(op, ret =>
        //                    //{
        //                    //    if ((int)ret == 1)
        //                    //    {
        //                    //        UpdateRank(op.entrys);

        //                    //        if (Config.SyncUpdate || CheckUpdateClients())
        //                    //        {
        //                    //            UpdateClients();
        //                    //        }
        //                    //        else if (rankUids.Count > historyRankUids.Count)
        //                    //        {
        //                    //            CheckAlignClients();
        //                    //        }
        //                    //    }
        //                    //});
        //                }
        //            }
        //        }

        private static TimeSpan updateRankLeastTime = TimeSpan.FromSeconds(0.025);

        public bool NeedSync { get; internal set; }

        public void TryUpdateRankList()
        {
            if ((server.Now() - lastAllowedUpdate) > updateRankLeastTime)
            {
                lastAllowedUpdate = server.Now();
                LoadRankList();
            }
        }

        public void LoadRankList()
        {
            OperateGetCampRankList op = new OperateGetCampRankList(server.MainId, (int)camp, rankType);
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
                //检查舒服刷新数据
                server.RPlayerInfoMng.RefreshPlayerList(new List<int>(uidRankInfoDic.Keys));
                //LastRefreshTime = server.Now();
            });
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



        //public void GetList(int page, out MSG_RZ_CAMP_ELECTION_LIST msg)
        //{
        //    msg = new MSG_RZ_CAMP_ELECTION_LIST();

        //    msg.Page = 1;
        //    msg.TotalCount = (Config.ShowCount > uidRankInfoDic.Count||Config.ShowCount==-1) ? uidRankInfoDic.Count : Config.ShowCount;
        //    int pCount = Config.CountPerPage;
        //    int begin = 1, end = pCount;
        //    if (page > 0 && (page - 1) * pCount < msg.TotalCount)
        //    {
        //        begin = (page - 1) * pCount;
        //        end = page * pCount;
        //        msg.Page = page;
        //    }

        //    RankListModel rankListModel = new RankListModel();
        //    rankListModel.Camp = camp;
        //    rankListModel.Type = RankType;

        //    rankListModel.RankList = GetList(begin, end);

        //    foreach (var item in rankListModel.RankList)
        //    {
        //        MSG_RZ_ELECTION_INFO info = new MSG_RZ_ELECTION_INFO();
        //        info.Uid = item.RankInfo.Uid;
        //        info.Rank = item.RankInfo.Rank;

        //        info.Name = item.BaseInfo.GetStringValue(HFPlayerInfo.Name);
        //        info.ShowDIYIcon = item.BaseInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
        //        info.Icon = item.BaseInfo.GetIntValue(HFPlayerInfo.Icon);
        //        info.IconFrame = item.BaseInfo.GetIntValue(HFPlayerInfo.IconFrame);
        //        info.Level = item.BaseInfo.GetIntValue(HFPlayerInfo.Level);
        //        info.HisPrestige = item.BaseInfo.GetIntValue(HFPlayerInfo.HistoryPrestige);
        //        //info.Prestige = item.BaseInfo.GetIntValue(HFPlayerInfo.CampPrestige);
        //        info.BattlePower = item.BaseInfo.GetIntValue(HFPlayerInfo.BattlePower);
        //        info.TicketScore = item.RankInfo.Score;

        //        msg.ElectionInfos.Add(info);

        //    }
        //}

        //public void GetList(out MSG_RZ_CAMP_ELECTION_LIST msg, int page)
        //{
        //    msg = new MSG_RZ_CAMP_ELECTION_LIST();
        //    msg.Page = 1;
        //    msg.TotalCount = uidRankInfoDic.Count;
        //    int pCount = Config.CountPerPage;
        //    int begin = 1, end = pCount;
        //    if (page > 0 && (page - 1) * pCount < msg.TotalCount)
        //    {
        //        begin = (page - 1) * pCount + 1;
        //        end = page * pCount;
        //        msg.Page = page;
        //    }
        //    if (RelationServerApi.now < this.begin)
        //    {
        //        begin = 1;
        //        end = 3;
        //        msg.Page = 1;
        //        msg.TotalCount = 3;
        //    }
        //    for (int i = begin; i <= end; i++)
        //    {
        //        ElectionClient client = null;
        //        //if (rankClients.TryGetValue(i, out client))
        //        //{
        //        //    MSG_RZ_ELECTION_INFO info = client.GenerateElectionInfo();
        //        //    info.Rank = i;
        //        //    info.TicketScore = (int)rankScores[i];
        //        //    msg.ElectionInfos.Add(info);
        //        //}
        //    }
        //    //foreach (var item in rankClients)
        //    //{
        //    //    MSG_RZ_ELECTION_INFO info = item.Value.GenerateElectionInfo();
        //    //    info.Rank = item.Key;
        //    //    info.TicketScore = (int)rankScores[item.Key];
        //    //    msg.ElectionInfos.Add(info);
        //    //}
        //}

        public List<PlayerRankBaseInfo> GetElectionList4Manager(int count)
        {

            List<PlayerRankBaseInfo> list = new List<PlayerRankBaseInfo>();
            int totalCount = uidRankInfoDic.Count;
            int pCount = CampLibrary.BattlePerPage;
            int begin = 0;

            int end = Math.Min(count, totalCount);

            List<PlayerRankModel> rankInfos = GetList(begin, end);

            foreach (var item in rankInfos)
            {
                PlayerRankBaseInfo info = new PlayerRankBaseInfo();

                info.Uid = item.RankInfo.Uid;
                info.Rank = item.RankInfo.Rank;
                info.Name = item.BaseInfo.GetStringValue(HFPlayerInfo.Name);
                info.Icon = item.BaseInfo.GetIntValue(HFPlayerInfo.Icon);
                info.GodType = item.BaseInfo.GetIntValue(HFPlayerInfo.GodType);
                info.Level = item.BaseInfo.GetIntValue(HFPlayerInfo.Level);

                list.Add(info);
            }

            return list;
        }

        public List<PlayerRankBaseInfo> GetLeaderList()
        {
            List<PlayerRankBaseInfo> rankList = new List<PlayerRankBaseInfo>();
            List<PlayerRankModel> rankInfos = new List<PlayerRankModel>();

            foreach (var item in curWorships)
            {
                PlayerRankModel rankInfo = new PlayerRankModel();

                RedisPlayerInfo baseInfo = server.RPlayerInfoMng.GetPlayerInfo(item.Uid);
                if (baseInfo != null)
                {
                    rankInfo.BaseInfo = baseInfo;

                    rankInfo.RankInfo = new RankBaseModel()
                    {
                        Uid = item.Uid,
                        Rank = item.Rank
                    };
                    rankInfo.RankInfo.Score = baseInfo.GetIntValue(HFPlayerInfo.BattlePower);
                    rankInfos.Add(rankInfo);


                    PlayerRankBaseInfo info = new PlayerRankBaseInfo();

                    info.Uid = item.Uid;
                    info.Rank = item.Rank;
                    info.Name = baseInfo.GetStringValue(HFPlayerInfo.Name);
                    info.Icon = baseInfo.GetIntValue(HFPlayerInfo.Icon);
                    info.GodType = baseInfo.GetIntValue(HFPlayerInfo.GodType);
                    info.Level = baseInfo.GetIntValue(HFPlayerInfo.Level);
                    info.ShowValue = baseInfo.GetIntValue(HFPlayerInfo.BattlePower);
                    rankList.Add(info);
                }
                else
                {
                    Log.Warn($"load rank list error: can not find {0} data in server", rankInfo.RankInfo.Uid);
                }
            }

            return rankList;
        }

        //public List<PlayerRankBaseInfo> GetElectionList(int count)
        //{

        //    List<PlayerRankBaseInfo> list = new List<PlayerRankBaseInfo>();
        //    int totalCount = uidRankInfoDic.Count;
        //    int pCount = CampLibrary.BattlePerPage;
        //    int begin = 1;
        //    int end = count;

        //    end = Math.Min(count, totalCount);
        //    for (int i = begin; i < end; i++)
        //    {
        //        if (i < totalCount)
        //        {
        //            //ElectionClient client = null;
        //            //if (rankClients.TryGetValue(i, out client))
        //            //{
        //            //    //MSG_RZ_ELECTION_INFO info = client.GenerateElectionInfo();
        //            //    //info.Rank = i;
        //            //    //info.TicketScore = (int)rankScores[i];
        //            //    list.Add(client.GetRankBaseInfo());
        //            //}
        //        }
        //    }
        //    return list;
        //}

        //public override void UpdateClients()
        //{
        //    OperateGetCampRankInfoList op = new OperateGetCampRankInfoList(new List<int>(rankUids.Values));
        //    server.Redis.Call(op, ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            UpdateRankClientInfos(op.Players);
        //        }
        //    });
        //}

        //public void CheckAlignClients()
        //{
        //    OperateGetCampRankInfoList op = new OperateGetCampRankInfoList(updateUids);

        //    server.Redis.Call(op, ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            AlignRankClientInfos(op.Players);
        //        }
        //    });
        //}

        //public void UpdateRankClientInfos(Dictionary<int, PlayerRankInfo> infos)
        //{
        //    //rankClients = new Dictionary<int, ElectionClient>();
        //    //uidClients = new Dictionary<int, ElectionClient>();

        //    //foreach (var item in infos)
        //    //{
        //    //    ElectionClient client = new ElectionClient();
        //    //    client.Init(item.Value);
        //    //    int rank = 0;
        //    //    if (uidRanks.TryGetValue(item.Key, out rank))
        //    //    {
        //    //        client.Rank = rank;
        //    //        rankClients.Add(client.Rank, client);
        //    //        uidClients.Add(item.Key, client);
        //    //    }
        //    //}
        //}

        //public void AlignRankClientInfos(Dictionary<int, PlayerRankInfo> infos)
        //{
        //    ////Dictionary<int, RankClient> historyRankClients = rankClients;
        //    //Dictionary<int, ElectionClient> historyUidClients = uidClients;

        //    //rankClients = new Dictionary<int, ElectionClient>();
        //    //uidClients = new Dictionary<int, ElectionClient>();

        //    ////reusedUids
        //    //foreach (var item in reusedUids)
        //    //{
        //    //    ElectionClient client = null;
        //    //    int rank = 0;
        //    //    if (historyUidClients.TryGetValue(item, out client) && uidRanks.TryGetValue(item, out rank))
        //    //    {
        //    //        client.Rank = rank;
        //    //        rankClients.Add(client.Rank, client);
        //    //        uidClients.Add(item, client);
        //    //    }
        //    //}

        //    ////redis
        //    //foreach (var item in infos)
        //    //{
        //    //    ElectionClient client = new ElectionClient();
        //    //    client.Init(item.Value);
        //    //    int rank = 0;
        //    //    if (uidRanks.TryGetValue(item.Key, out rank))
        //    //    {
        //    //        client.Rank = rank;
        //    //        rankClients.Add(client.Rank, client);
        //    //        uidClients.Add(item.Key, client);
        //    //    }
        //    //}
        //}

        /// <summary>
        /// 重新排名
        /// </summary>
        public void ReStartRank()
        {
#if DEBUG
            //Logger.Log.Debug($"ReStartRank Camp {camp} RankType {rankType} begin {begin} end {end}");
            Logger.Log.Debug($"ReStartRank Camp {camp} RankType {rankType} ");
#endif
            //NowShowPeriod = PeriodCount;

            LoadWorshipInfoToRedis();
            //UpdatePeriod2Redis();
            //ClearRedis();
            //rankClients.Clear();
            //uidClients.Clear();
            //通知zone
            // NotifyZone();
        }



        //public void ClearRedis()
        //{
        //    //
        //    //清理两个key 复用
        //    OperateClearCampRank op1 = new OperateClearCampRank(mainId, (int)camp, RankType);
        //    server.Redis.Call(op1);

        //    //OperateClearElectionRank op = new OperateClearElectionRank(mainId, (int)camp);
        //    //server.Redis.Call(op);
        //}

        //private void UpdatePeriod2Redis()
        //{
        //    //OperateSetCampPeriod spop = new OperateSetCampPeriod(mainId, (int)camp, PeriodCount);
        //    //server.Redis.Call(spop);
        //    //OperateSetCampElectionRankPeriodInfo op = new OperateSetCampElectionRankPeriodInfo(mainId, (int)camp, PeriodCount, begin, end);
        //    //server.Redis.Call(op);
        //    ////获取前三名信息然后存入redis
        //    LoadWorshipInfoToRedis();
        //}

        private void LoadWorshipInfoToRedis()
        {
            List<AbstractDBQuery> querys = new List<AbstractDBQuery>();
            OperateGetCampRankScore operateGetRankScore = new OperateGetCampRankScore(RankType.CampBattlePower, camp, mainId, 0, 2);
            server.GameRedis.Call(operateGetRankScore, ret =>
            {
                foreach (var item in operateGetRankScore.uidRank)
                {
                    RankBaseModel temp = item.Value;
                    QueryLoadWorshipBasic basic = new QueryLoadWorshipBasic(temp.Uid, temp.Rank, temp.Score);
                    querys.Add(basic);

                    server.TrackingLoggerMng.TrackRankLog(server.MainId, RankType.CampBattlePower.ToString() + "-" + camp, temp.Rank, temp.Score, temp.Uid, server.Now());

                    //BI
                    server.KomoeEventLogRankFlow(temp.Uid, RankType.CampBattlePower, temp.Rank, temp.Rank, temp.Score, RewardManager.GetRewardDic(""));
                }

                DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);
                server.GameDBPool.Call(dBQuerysWithoutTransaction, dbret =>
                {
                    curWorships.Clear();

                    foreach (var item1 in querys)
                    {
                        QueryLoadWorshipBasic temp1 = item1 as QueryLoadWorshipBasic;
                        WorshipRedisInfo info = new WorshipRedisInfo();
                        info.Uid = temp1.uid;
                        info.Rank = temp1.Rank;
                        info.Name = temp1.CharName;
                        info.ModelId = temp1.FashionId;
                        info.HeroId = temp1.HeroId;
                        info.Level = temp1.Level;
                        info.Icon = temp1.Icon;
                        info.BattlePower = temp1.Score;
                        info.GodType = temp1.GodType;
                        curWorships.Add(info);
                    }

                    server.GameRedis.Call(new OperateSetWorshipInfo(mainId, (int)camp, curWorships), ret2 =>
                    {
                        NeedSync = true;
                    });
                });
            });

        }

        //private void LoadDBWorshipInfoToRedis()
        //{
        //    List<AbstractDBQuery> querys = new List<AbstractDBQuery>();
        //    //QueryLoadWorshipBasic basic1 = new QueryLoadWorshipBasic();
        //    for (int i = 1; i <= 3; i++)
        //    {
        //        RankBaseModel client = null;
        //        if (uidRankInfoDic .Count>=i)
        //        {
        //            client = uidRankInfoDic.ElementAt(i - 1).Value;
        //            QueryLoadWorshipBasic basic = new QueryLoadWorshipBasic(client.Uid, i);
        //            querys.Add(basic);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    if (querys.Count <= 0)
        //    {
        //        return;
        //    }

        //    DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);
        //    server.GameDBPool.Call(dBQuerysWithoutTransaction, ret =>
        //    {
        //        curWorships.Clear();
        //        foreach (var item in querys)
        //        {
        //            QueryLoadWorshipBasic temp = item as QueryLoadWorshipBasic;
        //            WorshipRedisInfo info = new WorshipRedisInfo();
        //            info.Uid = temp.uid;
        //            info.Rank = temp.Rank;
        //            info.Name = temp.CharName;
        //            info.ModelId = temp.FashionId;
        //            info.HeroId = temp.HeroId;
        //            curWorships.Add(info);
        //        }

        //        server.Redis.Call(new OperateSetWorshipInfo(mainId, (int)camp, curWorships),ret2=>
        //        {
        //            NotifyZone();
        //        });
        //    });
        //}

        //protected override void LoadPeriodFromRedis()
        protected void LoadFromRedis()
        {
            //            OperateGetCampElectionRankPeriodInfo op = new OperateGetCampElectionRankPeriodInfo(mainId, (int)camp);
            //            server.Redis.Call(op, ret =>
            //            {
            //                if (op.info != null)
            //                {
            //                    begin = op.info.begin;
            //                    end = op.info.end;
            //                    PeriodCount = op.info.Period;
            //                    NowShowPeriod = PeriodCount;
            //                }
            //                //加载旧数据
            //                //int count;
            //                //if (Config.ShowCount == -1)
            //                //{
            //                //    count = -1;
            //                //}
            //                //else
            //                //{
            //                //    count = Config.ShowCount - 1;
            //                //}
            //                //OperateGetCampElectionRank opr = new OperateGetCampElectionRank(mainId, (int)camp, 0, count);
            //                //server.Redis.Call(opr, ret2 =>
            //                //{
            //                //    if ((int)ret2 == 1)
            //                //    {
            //                //        UpdateRank(opr.entrys);
            //                //        UpdateClients();
            //                //    }
            //                //});
            //                //加载完成
            //                LoadRankList();
            //                Loaded = true;
            //#if DEBUG
            //                Logger.Log.Debug($"Camp {camp} count {uidRankInfoDic.Count} Period {PeriodCount} RankType {RankType} begin {begin} end {end} loaded");
            //#endif
            //                NotifyZone();
            //            });


            OperateGetWorshipInfo info = new OperateGetWorshipInfo(mainId, (int)camp);
            server.GameRedis.Call(info, ret =>
            {
                int count = info.Infos.Count;
                if (count > 0)
                {
                    curWorships = info.Infos;

                    foreach (var item in curWorships)
                    {
                        server.RPlayerInfoMng.GetPlayerInfo(item.Uid);
                    }

                    Loaded = true;
                }
            });
        }
    }

    //public class ElectionClient
    //{
    //    public int Uid = 0;
    //    public int MainId = 0;
    //    public int Rank = 0;

    //    public string Name = string.Empty;

    //    public int Icon = 0;
    //    public bool ShowDIYIcon = false;
    //    public int IconFrame = 0;

    //    public int Level = 0;

    //    public bool IsOnline = false;
    //    public int LastLogoutTime = 0;

    //    public int CampId = 0;

    //    public int HisPrestige = 0;

    //    public int Family = 0;

    //    public int BattlePower = 0;

    //    public void Init(PlayerRankInfo info)
    //    {
    //        Uid = info.Uid;
    //        MainId = info.MainId;
    //        Name = info.Name;
    //        Icon = info.Icon;
    //        ShowDIYIcon = info.ShowDIYIcon;
    //        IconFrame = info.IconFrame;
    //        Level = info.Level;
    //        IsOnline = info.IsOnline;
    //        LastLogoutTime = info.LastLogoutTime;
    //        CampId = info.CampId;
    //        HisPrestige = info.HisPrestige;
    //        BattlePower = info.BattlePower;
    //    }

    //    public MSG_RZ_ELECTION_INFO GenerateElectionInfo()
    //    {
    //        MSG_RZ_ELECTION_INFO info = new MSG_RZ_ELECTION_INFO();
    //        info.Uid = Uid;
    //        info.Name = Name;
    //        info.ShowDIYIcon = ShowDIYIcon;
    //        info.Icon = Icon;
    //        info.IconFrame = IconFrame;
    //        info.Level = Level;
    //        info.HisPrestige = HisPrestige;
    //        info.Family = Family;
    //        info.BattlePower =BattlePower;
    //        return info;
    //    }

    //    public PlayerRankBaseInfo GetRankBaseInfo()
    //    {
    //        PlayerRankBaseInfo info = new PlayerRankBaseInfo();

    //        info.Uid = Uid;
    //        info.Rank = Rank;
    //        info.Name = Name;
    //        info.Icon = Icon;
    //        info.Level = Level;

    //        return info;
    //    }
    //}

}
