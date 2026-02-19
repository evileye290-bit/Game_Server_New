using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Relation.Protocol.RC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
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
    public class CrossBattleManager
    {
        private RelationServerApi server { get; set; }
        //uid, rank
        private Dictionary<int, ZR_BattlePlayerMsg> uidInfoList = new Dictionary<int, ZR_BattlePlayerMsg>();
        private Dictionary<int, DateTime> uidAddList = new Dictionary<int, DateTime>();
        //private Dictionary<int, int> rankUidList = new Dictionary<int, int>();
        //public Dictionary<int, PlayerRankBaseInfo> LeaderRankList = new Dictionary<int, PlayerRankBaseInfo>();

        //public int TotalCount
        //{
        //    get
        //    {
        //        return rankUidList.Count;
        //    }
        //}
        public CrossBattleManager(RelationServerApi server)
        {
            this.server = server;

            //LoadRankInfoFromRedis();
        }

        public void ClaerLastFinalsPlayers()
        {
            server.GameDBPool.Call(new QueryClearCrossBattleTimeKey());

            MSG_RZ_CLEAR_PLAYER_FINAL clearMsg = new MSG_RZ_CLEAR_PLAYER_FINAL();
            server.ZoneManager.Broadcast(clearMsg);
        }

        public void SyncFinalsPlayerResult(MapField<int, int> dic)
        {
            MSG_RZ_UPDATE_PLAYER_FINAL msg = new MSG_RZ_UPDATE_PLAYER_FINAL();

            foreach (var kv in dic)
            {
                int uid = kv.Key;
                int rank = kv.Value;
                msg.List.Add(uid, rank);

                server.GameDBPool.Call(new QueryUpdaateCrossBattleTimeKey(uid, rank));

                RankRewardInfo reward = CrossBattleLibrary.GetRankRewardInfo(rank);
                if (reward != null)
                {
                    //通知玩家发送信息回来
                    server.EmailMng.SendPersonEmail(uid, reward.EmailId, reward.Rewards);

                    server.TrackingLoggerMng.TrackRankLog(server.MainId, RankType.CrossServer.ToString(), rank, 0, uid, server.Now());
                    //BI
                    server.KomoeEventLogRankFlow(uid, RankType.Contribution, rank, rank, rank, RewardManager.GetRewardDic(reward.Rewards));
                    //发称号卡邮件
                    if (rank == 1)
                    {
                        //发送全服奖励
                        server.GameDBPool.Call(new QueryUpdateCrossBattleServerReward(0, (int)CrossRewardState.None));
                        //通知全服
                        MSG_RZ_CROSS_BATTLE_SERVER_REWARD rewardMsg = new MSG_RZ_CROSS_BATTLE_SERVER_REWARD();
                        server.ZoneManager.Broadcast(rewardMsg);
                    }
                }
            }
            server.ZoneManager.Broadcast(msg);
        }

        /// <summary>
        /// 获取决赛队员
        /// </summary>
        public void LoadFinalsPlayers()
        {
            //加载前8
            OperateGetRankScore op = new OperateGetRankScore(RankType.CrossServer, server.MainId, 0, CrossBattleLibrary.FightPlayerCount - 1);
            server.GameRedis.Call(op, ret =>
            {
                Dictionary<int, RankBaseModel> uidRank = op.uidRank;

                if (uidRank.Count > 0)
                {
                    MSG_RC_GET_FINALS_PLAYER_LIST rankMsg = PushFinalsPlayersMsg(uidRank);
                    server.CrossServer.Write(rankMsg);

                    List<int> uids = new List<int>(uidRank.Keys);
                    //检查舒服刷新数据
                    server.RPlayerInfoMng.RefreshPlayerList(uids);

                    MSG_RZ_GET_CROSS_BATTLE_CHALLENGER_HERO_INFO zoneMsg = new MSG_RZ_GET_CROSS_BATTLE_CHALLENGER_HERO_INFO();
                    zoneMsg.Uids.AddRange(uids);
                    FrontendServer zServer = server.ZoneManager.GetOneServer();
                    if (zServer != null)
                    {
                        zServer.Write(zoneMsg);
                    }

                    //发送通知进入总决赛
                    SendBattleEmail64(uids);
                    //公告
                    BroadcastBattlePlayersAnnouncement(ANNOUNCEMENT_TYPE.CROSS_BATTLE_64, uids);
                }
            });
        }

        public void BroadcastBattlePlayersAnnouncement(ANNOUNCEMENT_TYPE type, List<int> uids)
        {
            List<string> names = new List<string>();
            foreach (var uid in uids)
            {
                RedisPlayerInfo baseInfo = server.RPlayerInfoMng.GetPlayerInfo(uid);
                if (baseInfo != null)
                {
                    names.Add(baseInfo.GetStringValue(HFPlayerInfo.Name));
                }
                else
                {
                    Log.Warn($"lBroadcastCampBattleHoldBoss error: can not find {0} data in server", uid);
                }
            }
            if (names.Count > 0)
            {
                server.ZoneManager.BroadcastAnnouncement(type, names);
            }
        }

        private void SendBattleEmail64(List<int> uids)
        {
            foreach (var uid in uids)
            {
                server.EmailMng.SendPersonEmail(uid, CrossBattleLibrary.BattleEmail64);
            }
        }

        public void NoticePlayerBattleInfo(int timingId, RepeatedField<int> list)
        {
            //邮件通知
            CrossBattleTiming timing = (CrossBattleTiming)timingId;
            int emailId = CrossBattleLibrary.GetBattleEmailId(timing);
            if (emailId > 0)
            {
                foreach (var uid in list)
                {
                    if (uid > 0)
                    {
                        //通知玩家发送信息回来
                        server.EmailMng.SendPersonEmail(uid, emailId);
                    }
                }
            }

            bool isAnnouncement = false;
            ANNOUNCEMENT_TYPE type = ANNOUNCEMENT_TYPE.CROSS_BATTLE_64;
            switch (timing)
            {
                case CrossBattleTiming.ShowTime1:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_BATTLE_32;
                    break;
                case CrossBattleTiming.ShowTime2:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_BATTLE_16;
                    break;
                case CrossBattleTiming.ShowTime3:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_BATTLE_8;
                    break;
                case CrossBattleTiming.ShowTime4:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_BATTLE_4;
                    break;
                case CrossBattleTiming.ShowTime5:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_BATTLE_2;
                    break;
                case CrossBattleTiming.ShowTime6:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_BATTLE_1;
                    break;
                default:
                    break;
            }
            if (isAnnouncement)
            {
                //公告
                BroadcastBattlePlayersAnnouncement(type, list.ToList());
            }
        }

        public void NoticePlayerFirst(int mainId, string name)
        {
            List<string> list = new List<string>();
            list.Add(mainId.ToString());
            list.Add(name);
            //公告
            server.ZoneManager.BroadcastAnnouncement(ANNOUNCEMENT_TYPE.CROSS_BATTLE_1, list);
        }
        public MSG_RC_GET_FINALS_PLAYER_LIST PushFinalsPlayersMsg(Dictionary<int, RankBaseModel> uidRank)
        {
            MSG_RC_GET_FINALS_PLAYER_LIST rankMsg = new MSG_RC_GET_FINALS_PLAYER_LIST();
            rankMsg.MainId = server.MainId;

            RC_BattlePlayerMsg rankInfo;
            foreach (var player in uidRank)
            {
                rankInfo = new RC_BattlePlayerMsg();
                rankInfo.Rank = new RC_RankBaseInfo();
                rankInfo.Rank.Uid = player.Value.Uid;
                rankInfo.Rank.Rank = player.Value.Rank;
                rankInfo.Rank.Score = player.Value.Score;

                RedisPlayerInfo baseInfo = server.RPlayerInfoMng.GetPlayerInfo(player.Value.Uid);
                if (baseInfo != null)
                {
                    RC_HFPlayerBaseInfoItem item;
                    foreach (var kv in baseInfo.DataList)
                    {
                        item = new RC_HFPlayerBaseInfoItem();
                        item.Key = (int)kv.Key;
                        item.Value = kv.Value.ToString();
                        rankInfo.BaseInfo.Add(item);
                    }
                }
                else
                {
                    Log.Warn($"load finals player rank list fail: can not find {0} data in server", player.Value.Uid);
                }
                rankMsg.RankList.Add(rankInfo);
            }
            return rankMsg;
        }

        public void AddPlayerInfoMsg(ZR_BattlePlayerMsg info)
        {
            if (info != null)
            {
                uidInfoList[info.Uid] = info;
                uidAddList[info.Uid] = server.Now();
            }
        }
        public ZR_BattlePlayerMsg GetPlayerInfoMsg(int uid)
        {
            ZR_BattlePlayerMsg msg;
            if (uidInfoList.TryGetValue(uid, out msg))
            {
                DateTime addTime;
                if (uidAddList.TryGetValue(uid, out addTime))
                {
                    if ((server.Now() - addTime).TotalSeconds < 60)
                    {
                        return msg;
                    }
                    else
                    {
                        uidInfoList.Remove(uid);
                        uidAddList.Remove(uid);
                    }
                }
                else
                {
                    uidInfoList.Remove(uid);
                }
            }
            return null;
        }


        public MSG_RZ_CROS_RANK_INFO GetArenaRankInfoMsg(PlayerRankBaseInfo baseInfo)
        {
            MSG_RZ_CROS_RANK_INFO info = new MSG_RZ_CROS_RANK_INFO();
            info.Uid = baseInfo.Uid;
            info.Name = baseInfo.Name;
            info.Icon = baseInfo.Icon;
            info.IconFrame = baseInfo.IconFrame;
            info.ShowDIYIcon = baseInfo.ShowDIYIcon;
            info.Level = baseInfo.Level;
            info.Sex = baseInfo.Sex;
            info.HeroId = baseInfo.HeroId;
            info.GodType = baseInfo.GodType;
            info.BattlePower = baseInfo.BattlePower;
            //info.CrossLevel = baseInfo.CrossLevel;
            //info.CrossStar = baseInfo.CrossStar;
            info.Rank = baseInfo.Rank;
            info.Defensive.AddRange(baseInfo.Defensive);
            return info;
        }

        public void SyncFinalsPlayerTeamId(int uid, int teamId)
        {
            server.GameDBPool.Call(new QueryUpdaateCrossBattleTeamId(uid, teamId));

            MSG_RZ_NOTICE_PLAYER_TEAM_ID msg = new MSG_RZ_NOTICE_PLAYER_TEAM_ID();
            msg.Uid = uid;
            msg.TeanId = teamId;
            server.ZoneManager.Broadcast(msg);
        }

        //public void OnUpdate(double deltaTime)
        //{
        //    //if (CheckUpdateAllRankTip(deltaTime))
        //    //{
        //    //    CrossSeasonInfo seasonInfo = CrossBattleLibrary.GetCrossSeasonInfoByTime(RelationServerApi.now);
        //    //    if (seasonInfo != null)
        //    //    {
        //    //        this.seasonInfo = seasonInfo;

        //    //        if (lastCheckTime < seasonInfo.Start && seasonInfo.Start <= RelationServerApi.now)
        //    //        {
        //    //            //决赛开始锁定排行榜
        //    //            LoadLeaderRankInfoFromRedis();
        //    //        }
        //    //        else if (LeaderRankList.Count == 0 && CheckInfo)
        //    //        {
        //    //            //决赛开始锁定排行榜
        //    //            LoadLeaderRankInfoFromRedis();
        //    //        }
        //    //        lastCheckTime = RelationServerApi.now;
        //    //    }
        //    //    int group = CrossBattleLibrary.GetCrossGroup(server.MainId);

        //    //    LoadRankInfoFromRedis();
        //    //}
        //}

        //public void LoadRankInfoFromRedis()
        //{
        //    //当前赛季信息
        //    if (seasonInfo != null)
        //    {
        //        //获取赛季排行榜
        //        LoadCrossRankInfosByRank(seasonInfo, groupId);
        //    }
        //    else
        //    {
        //        //没有赛季信息
        //        Log.Write("LoadRankInfoFromRedis GetCrossSeasonInfoByTime not find info {0} .", RelationServerApi.now);
        //        return;
        //    }

        //}

        //private void LoadLeaderRankInfoFromRedis()
        //{
        //    CrossSeasonInfo laseSeasonInfo = CrossBattleLibrary.GetCrossSeasonInfo(seasonInfo.Id - 1);
        //    if (laseSeasonInfo != null)
        //    {
        //        //获取赛季排行榜
        //        LoadCrossLeaderInfosByRank(laseSeasonInfo, groupId);
        //    }
        //    else
        //    {
        //        //没有赛季信息
        //        Log.Write("LoadRankInfoFromRedis GetCrossSeasonInfo not find info {0} .", RelationServerApi.now);
        //        return;
        //    }
        //}

        //private void LoadCrossRankInfosByRank(CrossSeasonInfo seasonInfo, int group)
        //{
        //    OperateGetCrossRankInfosByRank operate = new OperateGetCrossRankInfosByRank(seasonInfo.Id, group, 0, CrossBattleLibrary.RankMax - 1);
        //    server.GameRedis.Call(operate, (RedisCallback)(ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            if (operate.Characters == null)
        //            {
        //                return;
        //            }
        //            else
        //            {
        //                Dictionary<int, int> oldRankList = new Dictionary<int, int>();
        //                foreach (var kv in uidRankList)
        //                {
        //                    oldRankList.Add(kv.Key, kv.Value.Rank);
        //                }
        //                uidRankList.Clear();
        //                rankUidList.Clear();
        //                int tempRank = 0;
        //                int i = 0;
        //                foreach (var kv in operate.Characters)
        //                {
        //                    i++;
        //                    ServerModels.PlayerRankBaseInfo item = GetArenaRankInfo(kv.Value, i);
        //                    AddPlayerRankInfo(item);

        //                    if (oldRankList.TryGetValue(item.Uid, out tempRank))
        //                    {
        //                        if (tempRank != i)
        //                        {
        //                            UpdateCrossRank(item.Uid, item.Rank);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //没有，直接通知
        //                        UpdateCrossRank(item.Uid, item.Rank);
        //                    }
        //                    oldRankList.Remove(item.Uid);
        //                }

        //                foreach (var item in oldRankList)
        //                {
        //                    //通知剩余人
        //                    UpdateCrossRank(item.Key, 0);
        //                }

        //                //RankSort();
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            Log.Error("LoadRankInfoFromRedis execute OperateGetCrossRankInfos fail: redis data error!");
        //            return;
        //        }
        //    }));
        //}

        //private void UpdateCrossRank(int pcUid, int rank)
        //{
        //    //排名变更，通知
        //    int mianId = BaseApi.GetMainIdByUid(pcUid);
        //    if (mianId == server.MainId)
        //    {
        //        //没有缓存信息，查看玩家是否在线
        //        Client client = server.ZoneManager.GetClient(pcUid);
        //        if (client != null)
        //        {
        //            //找到玩家说明玩家在线，通知玩家发送信息回来
        //            MSG_RZ_UPDATE_CROSS_RANK msg = new MSG_RZ_UPDATE_CROSS_RANK();
        //            msg.PcUid = pcUid;
        //            msg.Rank = rank;
        //            client.Write(msg);
        //        }
        //    }
        //}

        //private void LoadCrossLeaderInfosByRank(CrossSeasonInfo seasonInfo, int group)
        //{
        //    OperateGetCrossRankInfosByRank operate = new OperateGetCrossRankInfosByRank(seasonInfo.Id, group, 0, 3);
        //    server.GameRedis.Call(operate, (RedisCallback)(ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            if (operate.Characters == null)
        //            {
        //                return;
        //            }
        //            else
        //            {
        //                if (operate.Characters.Count > 0)
        //                {
        //                    LeaderRankList.Clear();
        //                    int i = 0;
        //                    foreach (var kv in operate.Characters)
        //                    {
        //                        i++;
        //                        ServerModels.PlayerRankBaseInfo item = GetArenaRankInfo(kv.Value, i);
        //                        LeaderRankList.Add(item.Uid, item);
        //                    }
        //                }
        //                else
        //                {
        //                    CheckInfo = false;
        //                }
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            Log.Error("LoadRankInfoFromRedis execute OperateGetCrossRankInfos fail: redis data error!");
        //            return;
        //        }
        //    }));
        //}

        //public PlayerRankBaseInfo GetArenaRankInfo(PlayerBaseInfo baseInfo, int rank)
        //{
        //    ServerModels.PlayerRankBaseInfo info = new ServerModels.PlayerRankBaseInfo();
        //    info.Uid = baseInfo.Uid;
        //    info.Name = baseInfo.Name;
        //    info.Level = baseInfo.Level;
        //    info.Sex = baseInfo.Sex;
        //    info.Icon = baseInfo.Icon;
        //    info.IconFrame = baseInfo.IconFrame;
        //    info.ShowDIYIcon = baseInfo.ShowDIYIcon;
        //    info.HeroId = baseInfo.HeroId;
        //    info.GodType = baseInfo.GodType;
        //    info.BattlePower = baseInfo.BattlePower;
        //    //info.CrossLevel = baseInfo.CrossLevel;
        //    //info.CrossStar = baseInfo.CrossStar;
        //    info.SetDefensive(baseInfo.Defensive);
        //    info.Rank = rank;
        //    return info;
        //}



        //public void AddPlayerRankInfo(PlayerRankBaseInfo info)
        //{
        //    if (info.Uid == 0 || info.Rank == 0)
        //    {
        //        Log.Write("AddPlayerRankInfo add uid {0} rank {1} : out of rank ", info.Uid, info.Rank);
        //        return;
        //    }
        //    //排名不重复
        //    if (uidRankList.ContainsKey(info.Uid))
        //    {
        //        Log.Warn("AddPlayerRankInfo add uid {0} rank {1} error: uid has add", info.Uid, info.Rank);
        //    }
        //    uidRankList[info.Uid] = info;
        //    if (rankUidList.ContainsKey(info.Rank))
        //    {
        //        Log.Warn("AddPlayerRankInfo add uid {0} rank {1} error: rank has add", info.Uid, info.Rank);
        //    }
        //    rankUidList[info.Rank] = info.Uid;
        //}

        //public void RankSort()
        //{
        //    rankUidList = rankUidList.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
        //}

        //public PlayerRankBaseInfo GetArenaRankInfoByUid(int uid)
        //{
        //    PlayerRankBaseInfo info;
        //    uidRankList.TryGetValue(uid, out info);
        //    return info;
        //}

        //public List<PlayerRankBaseInfo> GetList(int page)
        //{
        //    List<PlayerRankBaseInfo> list = new List<PlayerRankBaseInfo>();
        //    int totalCount = TotalCount;
        //    int pCount = ArenaLibrary.RankPerPage;
        //    int begin = 1;
        //    int end = pCount;

        //    if (page > 0 && (page - 1) * pCount < totalCount)
        //    {
        //        begin = (page - 1) * pCount;
        //        end = Math.Min(page * pCount, totalCount);
        //    }
        //    for (int i = begin; i < end; i++)
        //    {
        //        if (i < totalCount)
        //        {
        //            int uid = rankUidList.ElementAt(i).Value;
        //            PlayerRankBaseInfo info = GetArenaRankInfoByUid(uid);
        //            if (info != null)
        //            {
        //                list.Add(info);
        //            }
        //        }
        //    }
        //    return list;
        //}

        //double updatedeAllRankTime = 0;
        //private bool CheckUpdateAllRankTip(double deltaTime)
        //{
        //    updatedeAllRankTime += (float)deltaTime;
        //    if (updatedeAllRankTime < CrossBattleLibrary.RankRefreshTime)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        updatedeAllRankTime = 0;
        //        return true;
        //    }
        //}
    }
}
