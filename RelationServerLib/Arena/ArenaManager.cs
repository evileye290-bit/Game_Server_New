using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RelationServerLib
{
    public class ArenaManager
    {
        private RelationServerApi server { get; set; }
        //uid, rank
        private Dictionary<int, RankBaseModel> uidRankList = new Dictionary<int, RankBaseModel>();
        private Dictionary<int, int> rankUidList = new Dictionary<int, int>();
        private Dictionary<int, int> rewardRankUidList = new Dictionary<int, int>();

        public int TotalCount
        {
            get
            {
                return rankUidList.Count;
            }
        }
        public ArenaManager(RelationServerApi server)
        {
            this.server = server;
            nextTime = server.Now().AddHours(2);

            LoadRankInfoFromDB();

        }

        public void Update()
        {
            SendDailyReward();
        }

        public void LoadRankInfoFromDB()
        {
            QueryLoadArenaRank queryRank = new QueryLoadArenaRank();
            server.GameDBPool.Call(queryRank, ret =>
            {
                uidRankList.Clear();
                rankUidList.Clear();
                List<int> uids = new List<int>();
                List<RankBaseModel> errorList = new List<RankBaseModel>();
                foreach (var item in queryRank.Ranks)
                {
                    uids.Add(item.Uid);
                    //判断uid是否重复
                    if (!uidRankList.ContainsKey(item.Uid))
                    {
                        //判断排名是否重复
                        if (!rankUidList.ContainsKey(item.Rank))
                        {
                            //排名不重复
                            AddPlayerRankInfo(item.Uid, item.Rank);
                        }
                        else
                        {
                            Log.Warn("LoadRankInfoFromDB add rank {0} error: is in the list uid is {1}", item.Rank, item.Uid);
                            errorList.Add(item);
                        }
                    }
                    else
                    {
                        Log.Warn("LoadRankInfoFromDB add uid {0} error: is in the list", item.Uid);
                    }
                }
                server.RPlayerInfoMng.RefreshPlayerList(uids);

                foreach (var item in errorList)
                {
                    for (int i = item.Rank + 1; i <= ArenaLibrary.RankMax; i++)
                    {
                        if (!rankUidList.ContainsKey(item.Rank))
                        {
                            Log.Write("LoadRankInfoFromDB add rank uid {0} change rank from {1} to {2}.", item.Uid, item.Rank, i);
                            //排名不重复
                            item.Rank = i;
                            //item.HistoryRank = Math.Max(item.HistoryRank, i);
                            AddPlayerRankInfo(item);
                            //保存DB
                            server.GameDBPool.Call(new QueryUpdateArenaRank(item.Uid, item.Rank));
                        }
                        else
                        {
                            //已经被使用，继续查看下一个排名
                            continue;
                        }
                    }
                }

                RankSort();
            });
        }

        public void AddPlayerRankInfo(int uid, int rank)
        {
            RankBaseModel info = new RankBaseModel();
            info.Uid = uid;
            info.Rank = rank;
            //排名不重复
            AddPlayerRankInfo(info);
        }

        public void AddPlayerRankInfo(RankBaseModel info)
        {
            if (info.Uid == 0 || info.Rank == 0)
            {
                Log.Write("AddPlayerRankInfo add uid {0} rank {1} : out of rank ", info.Uid, info.Rank);
                return;
            }
            //排名不重复
            if (uidRankList.ContainsKey(info.Uid))
            {
                Log.Warn("AddPlayerRankInfo add uid {0} rank {1} error: uid has add", info.Uid, info.Rank);
            }
            uidRankList[info.Uid] = info;
            if (rankUidList.ContainsKey(info.Rank))
            {
                Log.Warn("AddPlayerRankInfo add uid {0} rank {1} error: rank has add", info.Uid, info.Rank);
            }
            rankUidList[info.Rank] = info.Uid;
        }

        public void RemovePlayerRankInfo(int uid, int rank)
        {
            PlayerRankBaseInfo info = new PlayerRankBaseInfo();
            info.Uid = uid;
            info.Rank = rank;
            //排名不重复
            RemovePlayerRankInfo(info);
        }

        public void RemovePlayerRankInfo(PlayerRankBaseInfo info)
        {
            if (info.Uid == 0 || info.Rank == 0)
            {
                Log.Write("RemovePlayerRankInfo remove uid {0} rank {1} : out of rank ", info.Uid, info.Rank);
                return;
            }
            if (!uidRankList.Remove(info.Uid))
            {
                Log.Warn("RemovePlayerRankInfo remove uid {0} rank {1} error: not find uid ", info.Uid, info.Rank);
            }
            if (!rankUidList.Remove(info.Rank))
            {
                Log.Warn("RemovePlayerRankInfo remove uid {0} rank {1} error: not find  rank", info.Uid, info.Rank);
            }
        }

        public void ExchangePlayerAndChallengerRank(int pcUid, int playerRank, int challengerUid, int challengerRank)
        {
            RemovePlayerRankInfo(pcUid, playerRank);
            RemovePlayerRankInfo(challengerUid, challengerRank);

            AddPlayerRankInfo(pcUid, challengerRank);
            AddPlayerRankInfo(challengerUid, playerRank);
        }

        public void RankSort()
        {
            rankUidList = rankUidList.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);    
        }

        public bool CanExchangeRank(int challengerRank, int playerRank)
        {
            //更新日志
            CheckKomoeLog();

            if (challengerRank <= 0)
            {
                //被挑战的人已经跌出排名，玩家不用替换了
                return false;
            }
            else
            {
                //挑战者在排名内
                if (playerRank <= 0)
                {
                    //说明玩家在排名外，直接替换
                    return true;
                }
                else
                {
                    //玩家在排名内
                    if (playerRank > challengerRank)
                    {
                        //说明玩家排名在对手之后，进行调换
                        return true;
                    }
                    else
                    {
                        //说明玩家排名在对手前，不需要调换
                        return false;
                    }
                }
            }
        }


        public RankBaseModel GetArenaRankInfoByUid(int uid)
        {
            RankBaseModel info;
            uidRankList.TryGetValue(uid, out info);
            server.RPlayerInfoMng.CheckUpdatePlayerInfo(uid);
            return info;
        }

        public RankBaseModel GetArenaRankInfoByRank(int rank)
        {
            RankBaseModel info = null;
            int uid;
            if (rankUidList.TryGetValue(rank, out uid))
            {
                info = GetArenaRankInfoByUid(uid);
            }
            return info;
        }

        public void RandomAddArenaRankInfo(int uid, int rank, int index, int min, int max, Dictionary<int, RankBaseModel> list)
        {
            for (int i = 0; i < 20; i++)
            {
                int random = NewRAND.Next(min, max);
                RankBaseModel firstInfo = GetArenaRankInfoByRank(random);
                if (firstInfo != null)
                {
                    //找到对应信息
                    if (firstInfo.Uid != uid && firstInfo.Rank != rank)
                    {
                        //不包含自己
                        if (CheckAddRankInfo(uid, rank, list, firstInfo))
                        {
                            //添加过这个，重新随机
                            continue;
                        }
                        else
                        {
                            list.Add(index, firstInfo);
                            return;
                        }
                    }
                    else
                    {
                        //包含自己，重新随机
                        Log.Warn("player {0} rank {1} get arena challenger failed: random add myself info uid {2} rank {3}", uid, rank, firstInfo.Uid, firstInfo.Rank);
                        continue;
                    }
                }
                else
                {
                    //使用机器人
                    RankBaseModel info = new RankBaseModel();
                    info.Rank = random;
                    info.Uid = 0;
                    list.Add(index, info);
                    return;
                }
            }
            Log.Warn("player {0} rank {1} get arena challenger failed: random add info find different info", uid, rank);
        }

        private bool CheckAddRankInfo(int uid, int rank, Dictionary<int, RankBaseModel> list, RankBaseModel firstInfo)
        {
            //查看信息是否寂静添加
            foreach (var item in list)
            {
                if (firstInfo.Uid == item.Value.Uid || firstInfo.Rank == item.Value.Rank)
                {
                    Log.Warn("player {0} rank {1} get arena challenger failed: random uid {2} rank {3} add same info uid {4} rank {5}",
                        uid, rank, firstInfo.Uid, firstInfo.Rank, item.Value.Uid, item.Value.Rank);
                    return true;
                }
            }
            return false;
        }

        public List<RankBaseModel> GetList(int page)
        {
            List<RankBaseModel> list = new List<RankBaseModel>();
            int totalCount = TotalCount;
            int pCount = ArenaLibrary.RankPerPage;
            int begin = 1;
            int end = pCount;

            if (page > 0 && (page - 1) * pCount < totalCount)
            {
                begin = (page - 1) * pCount;
                end = Math.Min(page * pCount, totalCount);
            }
            for (int i = begin; i < end; i++)
            {
                if (i < totalCount)
                {
                    int uid = rankUidList.ElementAt(i).Value;
                    RankBaseModel info = GetArenaRankInfoByUid(uid);
                    if (info != null)
                    {
                        list.Add(info);
                    }
                }
            }
            return list;
        }

        public void GetDailyRewardList()
        {
            foreach (var kv in rankUidList)
            {
                rewardRankUidList[kv.Key] = kv.Value;
            }
        }
        public void SendDailyReward()
        {
            if (rewardRankUidList.Count > 0)
            {
                KeyValuePair<int, int> item = rewardRankUidList.First();
                RankRewardInfo info = ArenaLibrary.GetDailyRankRewardInfo(item.Key);
                if (info != null)
                {
                    //直接发邮件奖励，清理数据库
                    //发送邮件
                    server.EmailMng.SendPersonEmail(item.Value, info.EmailId, info.Rewards);
                    ////记录已经发送奖励
                    //server.GameDBPool.Call(new QueryClearArenaDailyRankReward(item.Value));

                    //BI
                    server.KomoeEventLogRankFlow(item.Value, RankType.Arena, item.Key, item.Key, item.Key, RewardManager.GetRewardDic(info.Rewards));

                    //通知排名变更
                    //MSG_RZ_ARENA_DAILY_REWARD msg = new MSG_RZ_ARENA_DAILY_REWARD();
                    //msg.RewardId = info.Id;
                    //client.Write(msg);
                }
                else
                {
                    Log.Warn("player {0} rank {1} semd daily reward failed: not find reward info", item.Value, item.Key);
                }
                rewardRankUidList.Remove(item.Key);
            }
        }


        public int GetWeakCamp()
        {
            ulong TianDouCamp = 0;
            ulong XingLuoCamp = 0;
            foreach (var item in rankUidList)
            {
                RedisPlayerInfo baseInfo = server.RPlayerInfoMng.GetPlayerInfo(item.Value);
                if (baseInfo != null)
                {
                    int camp = baseInfo.GetIntValue(HFPlayerInfo.CampId);
                    switch ((CampType)camp)
                    {
                        case CampType.TianDou:
                            TianDouCamp += (ulong)baseInfo.GetIntValue(HFPlayerInfo.BattlePower);
                            break;
                        case CampType.XingLuo:
                            XingLuoCamp += (ulong)baseInfo.GetIntValue(HFPlayerInfo.BattlePower);
                            break;
                        case CampType.None:
                        default:
                            break;
                    }
                }
            }

            if (TianDouCamp > XingLuoCamp)
            {
                if (TianDouCamp - XingLuoCamp > TianDouCamp * 0.1)
                {
                    return (int)CampType.XingLuo;
                }
                else
                {
                    return RAND.Range((int)CampType.TianDou, (int)CampType.XingLuo);
                }
            }
            else if (XingLuoCamp > TianDouCamp)
            {
                if (XingLuoCamp - TianDouCamp > XingLuoCamp * 0.1)
                {
                    return (int)CampType.TianDou;
                }
                else
                {
                    return RAND.Range((int)CampType.TianDou, (int)CampType.XingLuo);
                }
            }
            else
            {
                return RAND.Range((int)CampType.TianDou, (int)CampType.XingLuo);
            }
        }


        private DateTime nextTime { get; set; }
        public void CheckKomoeLog()
        {
            if (server.Now() > nextTime)
            {
                Task t = new Task(() =>
                {
                    Dictionary<int, int> dic = new Dictionary<int, int>(rankUidList);
                    foreach (var item in dic)
                    {
                        RankRewardInfo info = ArenaLibrary.GetDailyRankRewardInfo(item.Key);
                        if (info != null)
                        {
                            //BI
                            server.KomoeUserLogUserListSnapshot(item.Value, RankType.Arena, item.Key, item.Key);
                        }
                    }
                });
                t.Start();
                nextTime = nextTime.AddHours(KomoeLogConfig.RankTime);
            }
        }
    }
}
