using EnumerateUtility;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Relation.Protocol.RZ;
using ServerModels;
using Logger;

namespace RelationServerLib
{
    public class CampPrestigeManager : AbstractRankManager
    {
        private CampType camp = CampType.None;

        Dictionary<int, RankClient> rankClients = new Dictionary<int, RankClient>();
        Dictionary<int, RankClient> uidClients = new Dictionary<int, RankClient>();

        protected Dictionary<int, RankBaseModel> uidRankInfoDic = new Dictionary<int, RankBaseModel>();

        public CampPrestigeManager(RelationServerApi api, int zoneId, RankType type, CampType camp) : base(api, zoneId, type)
        {
            this.camp = camp;
            LoadPeriodFromRedis();
            LoadInfoFromRedis();
        }

        public bool CheckNextPeriod()
        {

#if DEBUG
            if (CampLibrary.InDebug)
            {
                return false;
            }
#endif

            if (end < RelationServerApi.now)
            {
                hisBegin = begin;
                hisEnd = end;
                Tuple<DateTime, DateTime> period = RankLibrary.GetNextPeriod(RankType, server.OpenServerTime, PeriodCount + 1);
                begin = period.Item1;
                end = period.Item2;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckNeedReStart()
        {
            if (RelationServerApi.now > begin && NowShowPeriod != PeriodCount)
            {
#if DEBUG
                if (CampLibrary.InDebug)
                {
                    return false;
                }
#endif
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Update()
        {
            if (Loaded)
            {
                if (CheckNextPeriod())
                {
                    RestoreLastPeriodRedis();
                    PeriodCount++;
                }
                if (CheckNeedReStart())
                {
                    ReStartRank();
                }

                DateTime now = RelationServerApi.now;

                //Logger.Log.Debug($"Camp {camp} count {rankClients.Count} Period {PeriodCount} RankType {RankType} begin {begin} end {end}");

                if ((now < begin || now > end) && !CampLibrary.InDebug)
                {
                    return;
                }

                if (CheckUpdateRank())
                {

#if DEBUG
                    Logger.Log.Debug($"Camp {camp} count {rankClients.Count} Period {PeriodCount} RankType {RankType} begin {begin} end {end}");

#endif
                    LoadRankList();
                    //int count;
                    //if (Config.ShowCount == -1)
                    //{
                    //    count = -1;
                    //}
                    //else
                    //{
                    //    count = Config.ShowCount - 1;
                    //}

                    //todo 获取排名
                    //OperateGetCampRank op = new OperateGetCampRank(mainId, (int)camp, 0, count);
                    //server.Redis.Call(op, ret =>
                    //{
                    //    if ((int)ret == 1)
                    //    {
                    //        //UpdateRank(op.entrys);
                    //        if (Config.SyncUpdate || CheckUpdateClients())
                    //        {
                    //            //UpdateClients();
                    //        }
                    //    }
                    //});
                }
            }
        }

        public void LoadRankList()
        {
            OperateGetCampRankList op = new OperateGetCampRankList(server.MainId, (int)camp, RankType);
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

        public void GetList(int page, out MSG_RZ_CAMP_RANK_LIST msg, int pcUid)
        {
            msg = new MSG_RZ_CAMP_RANK_LIST();

            msg.Page = 1;
            msg.TotalCount = (Config.ShowCount > uidRankInfoDic.Count || Config.ShowCount == -1) ? uidRankInfoDic.Count : Config.ShowCount;
            int pCount = Config.CountPerPage;
            int begin = 1, end = pCount;
            if (page > 0 && (page - 1) * pCount < msg.TotalCount)
            {
                begin = (page - 1) * pCount;
                end = page * pCount;
                msg.Page = page;
            }

            RankListModel rankListModel = new RankListModel();
            rankListModel.Camp = camp;
            rankListModel.Type = RankType;

            rankListModel.RankList = GetList(begin, end);

            foreach (var item in rankListModel.RankList)
            {
                MSG_RZ_CAMP_RANK_INFO info = new MSG_RZ_CAMP_RANK_INFO();
                info.Uid = item.RankInfo.Uid;
                info.Rank = item.RankInfo.Rank;

                info.Name = item.BaseInfo.GetStringValue(HFPlayerInfo.Name);
                info.ShowDIYIcon = item.BaseInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
                info.Icon = item.BaseInfo.GetIntValue(HFPlayerInfo.Icon);
                info.IconFrame = item.BaseInfo.GetIntValue(HFPlayerInfo.IconFrame);
                info.Level = item.BaseInfo.GetIntValue(HFPlayerInfo.Level);
                info.HisPrestige = item.BaseInfo.GetIntValue(HFPlayerInfo.HistoryPrestige);
                info.Prestige = item.BaseInfo.GetIntValue(HFPlayerInfo.CampPrestige);
                info.BattlePower = item.BaseInfo.GetIntValue(HFPlayerInfo.BattlePower);
                info.GodType = item.BaseInfo.GetIntValue(HFPlayerInfo.GodType);

                msg.RankInfos.Add(info);

            }

            if (uidRankInfoDic.ContainsKey(pcUid))
            {
                msg.OwnerRank = uidRankInfoDic[pcUid].Rank;
            }
        }

        public void GetList(int page, out MSG_RZ_CAMP_RANK_LIST msg)
        {
            msg = new MSG_RZ_CAMP_RANK_LIST();

            msg.Page = 1;
            msg.TotalCount = Config.ShowCount > uidRankInfoDic.Count ? uidRankInfoDic.Count : Config.ShowCount;
            int pCount = Config.CountPerPage;
            int begin = 1, end = pCount;
            if (page > 0 && (page - 1) * pCount < msg.TotalCount)
            {
                begin = (page - 1) * pCount;
                end = page * pCount;
                msg.Page = page;
            }

            RankListModel rankListModel = new RankListModel();
            rankListModel.Camp = camp;
            rankListModel.Type = RankType;

            rankListModel.RankList = GetList(begin, end);

            foreach (var item in rankListModel.RankList)
            {
                MSG_RZ_CAMP_RANK_INFO info = new MSG_RZ_CAMP_RANK_INFO();
                info.Uid = item.RankInfo.Uid;
                info.Rank = item.RankInfo.Rank;

                info.Name = item.BaseInfo.GetStringValue(HFPlayerInfo.Name);
                info.ShowDIYIcon = item.BaseInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
                info.Icon = item.BaseInfo.GetIntValue(HFPlayerInfo.Icon);
                info.IconFrame = item.BaseInfo.GetIntValue(HFPlayerInfo.IconFrame);
                info.Level = item.BaseInfo.GetIntValue(HFPlayerInfo.Level);
                info.HisPrestige = item.BaseInfo.GetIntValue(HFPlayerInfo.HistoryPrestige);
                info.Prestige = item.BaseInfo.GetIntValue(HFPlayerInfo.CampPrestige);
                info.BattlePower = item.BaseInfo.GetIntValue(HFPlayerInfo.BattlePower);
                info.GodType = item.BaseInfo.GetIntValue(HFPlayerInfo.GodType);

                msg.RankInfos.Add(info);

            }
        }

        public void GetList(out MSG_RZ_CAMP_RANK_LIST msg, int page)
        {
            msg = new MSG_RZ_CAMP_RANK_LIST();
            msg.Page = 1;
            msg.TotalCount = rankClients.Count;
            int pCount = Config.CountPerPage;
            int begin = 1, end = pCount;
            if (page > 0 && (page - 1) * pCount < msg.TotalCount)
            {
                begin = (page - 1) * pCount + 1;
                end = page * pCount;
                msg.Page = page;
            }
            for (int i = begin; i <= end; i++)
            {
                RankClient client = null;
                if (rankClients.TryGetValue(i, out client))
                {
                    MSG_RZ_CAMP_RANK_INFO info = client.GenerateRankInfo();
                    info.Rank = i;
                    msg.RankInfos.Add(info);
                }
            }
            //foreach (var item in rankClients)
            //{
            //    MSG_RZ_CAMP_RANK_INFO info = item.Value.GenerateRankInfo();
            //    info.Rank = item.Key;
            //    msg.RankInfos.Add(info);
            //}
        }

        public void GetPanelList(out MSG_RZ_CAMP_PANEL_LIST msg)
        {
            msg = new MSG_RZ_CAMP_PANEL_LIST();
            for (int i = 1; i <= 3; i++)
            {
                RankClient client = null;
                if (rankClients.TryGetValue(i, out client))
                {
                    MSG_RZ_CAMP_RANK_INFO info = client.GenerateRankInfo();
                    info.Rank = i;
                    msg.RankInfos.Add(info);
                }
            }
        }

        public void GetPanelListFromManager(out MSG_RZ_CAMP_PANEL_LIST msg,int pcUid)
        {
            msg = new MSG_RZ_CAMP_PANEL_LIST();
            int begin = 0;
            int end = 3 > uidRankInfoDic.Count ? uidRankInfoDic.Count : 3;

            RankListModel rankListModel = new RankListModel();

            rankListModel.RankList = GetList(begin, end);

            foreach (var item in rankListModel.RankList)
            {
                MSG_RZ_CAMP_RANK_INFO info = new MSG_RZ_CAMP_RANK_INFO();
                info.Uid = item.RankInfo.Uid;
                info.Rank = item.RankInfo.Rank;

                info.Name = item.BaseInfo.GetStringValue(HFPlayerInfo.Name);
                info.ShowDIYIcon = item.BaseInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
                info.Icon = item.BaseInfo.GetIntValue(HFPlayerInfo.Icon);
                info.IconFrame = item.BaseInfo.GetIntValue(HFPlayerInfo.IconFrame);
                info.Level = item.BaseInfo.GetIntValue(HFPlayerInfo.Level);
                info.HisPrestige = item.BaseInfo.GetIntValue(HFPlayerInfo.HistoryPrestige);
                info.Prestige = item.BaseInfo.GetIntValue(HFPlayerInfo.CampPrestige);
                info.BattlePower = item.BaseInfo.GetIntValue(HFPlayerInfo.BattlePower);
                info.GodType = item.BaseInfo.GetIntValue(HFPlayerInfo.GodType);

                msg.RankInfos.Add(info);
            }

            msg.Period = NowShowPeriod;
            if (uidRankInfoDic.ContainsKey(pcUid))
            {
                msg.OwnerRank = uidRankInfoDic[pcUid].Rank;
            }
        }

        public int GetBattlePower()
        {
            int sum = 0;
            rankClients.ForEach(kv => sum += kv.Value.BattlePower);
            return sum;

        }


        /// <summary>
        /// 废弃
        /// </summary>
        public override void UpdateClients()
        {
            OperateGetCampRankInfoList op = new OperateGetCampRankInfoList(new List<int>(rankUids.Values));

            server.GameRedis.Call(op, ret =>
            {
                if ((int)ret == 1)
                {
                    UpdateRankClientInfos(op.Players);
                }
            });
        }

        public void UpdateRankClientInfos(Dictionary<int, PlayerRankInfo> infos)
        {
            rankClients = new Dictionary<int, RankClient>();
            uidClients = new Dictionary<int, RankClient>();


            //从redis取出的
            foreach (var item in infos)
            {
                RankClient client = new RankClient();
                client.Init(item.Value);
                int rank = 0;
                ulong prestige = 0;
                if (uidRanks.TryGetValue(item.Key, out rank) && rankScores.TryGetValue(rank, out prestige))
                {
                    client.Rank = rank;
                    client.Prestige = (int)prestige;
                    rankClients.Add(client.Rank, client);
                    uidClients.Add(item.Key, client);
                }
            }

        }


        /// <summary>
        /// 重新排名
        /// </summary>
        public override void ReStartRank()
        {
#if DEBUG
            Logger.Log.Debug($"ReStartRank Camp {camp} Period {PeriodCount} RankType {RankType} begin {begin} end {end}");
#endif
            NowShowPeriod = PeriodCount;
            UpdatePeriod2Redis();
            ClearRedis();
            Clear();
            rankClients.Clear();
            uidClients.Clear();
            uidRankInfoDic.Clear();

            //通知zone以便个人去更新
            NotifyZonePeriod();
        }

        public void NotifyZonePeriod()
        {
            foreach (var item in server.ZoneManager.ServerList)
            {
                ((ZoneServer)item.Value).NotifyCampPeriod(NowShowPeriod, RankType, begin, end);
            }
#if DEBUG
            Logger.Log.Debug($"NotifyZonePeriod Camp {camp} Period {PeriodCount} RankType {RankType} begin {begin} end {end}");
#endif
        }

        private void ClearRedis()
        {

            OperateClearPrestigeRank op1 = new OperateClearPrestigeRank(mainId, (int)camp, NowShowPeriod);
            server.GameRedis.Call(op1);

            //清理两个key 复用
            OperateClearCampRank op = new OperateClearCampRank(mainId, (int)camp, RankType);
            server.GameRedis.Call(op);
        }

        private void UpdatePeriod2Redis()
        {
            OperateSetCampRankPeriodInfo op = new OperateSetCampRankPeriodInfo(mainId, (int)camp, PeriodCount, begin, end);
            server.GameRedis.Call(op);

        }

        private void RestoreLastPeriodRedis()
        {
            //把内容整理好放在新的key中
            OperateRestorePrestigeRank op = new OperateRestorePrestigeRank(mainId, (int)camp, PeriodCount,uidRankInfoDic);
            server.GameRedis.Call(op);
        }

        public void LoadInfoFromRedis()
        {
            OperateGetCampRankList op2 = new OperateGetCampRankList(server.MainId, (int)camp, RankType);
            server.GameRedis.Call(op2, ret =>
            {
                if (op2.uidRank == null)
                {
                    Log.Error($"load init server {server.MainId} camp {camp} rank {RankType} info fail,redis can not find data");
                }
                else
                {
                    uidRankInfoDic = op2.uidRank;

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
                    //检查刷新数据
                    server.RPlayerInfoMng.RefreshPlayerList(new List<int>(uidRankInfoDic.Keys));
                    //LastRefreshTime = server.Now();
                }
            });
        }

        protected override void LoadPeriodFromRedis()
        {
            OperateGetCampRankPeriodInfo op = new OperateGetCampRankPeriodInfo(mainId, (int)camp);
            server.GameRedis.Call(op, ret =>
            {
                if (op.info != null)
                {
                    begin = op.info.begin;
                    end = op.info.end;
                    PeriodCount = op.info.Period;
                    NowShowPeriod = PeriodCount;
                }
#if DEBUG
                Logger.Log.Debug($"Camp {camp} count {rankClients.Count} Period {PeriodCount} RankType {RankType} begin {begin} end {end} loaded");
#endif
                Loaded = true;
                NotifyZonePeriod();
            });
        }
    }

    public class RankClient : BaseRankClient
    {
        public int Prestige = 0;

        public MSG_RZ_CAMP_RANK_INFO GenerateRankInfo()
        {
            MSG_RZ_CAMP_RANK_INFO info = new MSG_RZ_CAMP_RANK_INFO();
            info.Uid = Uid;
            info.Name = Name;
            info.ShowDIYIcon = ShowDIYIcon;
            info.Icon = Icon;
            info.GodType = GodType;
            info.IconFrame = IconFrame;
            info.Level = Level;
            info.HisPrestige = HisPrestige;
            info.Family = Family;
            info.Prestige = Prestige;
            info.BattlePower = BattlePower;
            return info;
        }
    }
}
