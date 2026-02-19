using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Relation.Protocol.RR;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using ServerFrame;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using CommonUtility;
using System.Collections.Generic;
using DBUtility;
using System;
using Message.Relation.Protocol.RC;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_GeArenaChallenger(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_GET_ARENA_CHALLENGERS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_ARENA_CHALLENGERS>(stream);
            Log.Write("player {0} get arena challengers info.", uid);
            int rank = 0;
            //到缓存中获取缓存信息
            RankBaseModel rankInfo = Api.ArenaMng.GetArenaRankInfoByUid(uid);
            if (rankInfo != null)
            {
                //排名内
                rank = rankInfo.Rank;
            }
            else
            {
                //未在排名内
                rank = 0;
            }

            //随机规则
            ArenaRandomInfo randomInfo = ArenaLibrary.GetArenaRandomInfo(rank);
            if (randomInfo == null)
            {
                Log.Warn("player {0} get arena challenger failed: not find rank {1} random info ", uid, rank);
                //没有找到随机信息，直接用0照
                randomInfo = ArenaLibrary.GetArenaRandomInfo(0);
                if (randomInfo == null)
                {
                    Log.Warn("player {0} get arena challenger failed: not find rank {1} random info and not find 0 info", uid, rank);
                    return;
                }
            }

            Dictionary<int, RankBaseModel> list = new Dictionary<int, RankBaseModel>();

            //第一位
            Api.ArenaMng.RandomAddArenaRankInfo(uid, rank, 1, randomInfo.FristMin, randomInfo.FristMax, list);

            //第二位
            Api.ArenaMng.RandomAddArenaRankInfo(uid, rank, 2, randomInfo.SecondMin, randomInfo.SecondMax, list);

            //第三位
            Api.ArenaMng.RandomAddArenaRankInfo(uid, rank, 3, randomInfo.ThirdMin, randomInfo.ThirdMax, list);

            //第四位
            Api.ArenaMng.RandomAddArenaRankInfo(uid, rank, 4, randomInfo.FourthMin, randomInfo.FourthMax, list);

            //没有找到玩家，通知ZONE自己去DB读取玩家信息
            MSG_RZ_GET_ARENA_CHALLENGERS msg = new MSG_RZ_GET_ARENA_CHALLENGERS();
            msg.PcUid = uid;
            msg.Rank = rank;
            foreach (var item in list)
            {
                MSG_RZ_ARENA_RANK_INFO info = GetArenaRankInfo(item.Value);
                info.Index = item.Key;
                msg.List.Add(info);
            }
            Write(msg, uid);
        }

        private static MSG_RZ_ARENA_RANK_INFO GetArenaRankInfo(RankBaseModel item)
        {
            MSG_RZ_ARENA_RANK_INFO info = new MSG_RZ_ARENA_RANK_INFO();
            if (item.Uid == 0)
            {
                info.IsRobot = true;
            }
            else
            {
                info.IsRobot = false;
            }
            info.Uid = item.Uid;
            info.Rank = item.Rank;
            return info;
        }

        public void OnResponse_ShowArenaRankInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SHOW_ARENA_RANK_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SHOW_ARENA_RANK_INFO>(stream);

            //到缓存中获取缓存信息
            RankBaseModel rankInfo = Api.ArenaMng.GetArenaRankInfoByUid(uid);
            if (rankInfo == null)
            {
                //没有排行信息
                rankInfo = new RankBaseModel();
                rankInfo.Uid = uid;
                rankInfo.Score = 0;
                rankInfo.Time = Api.Now();
            }
       

            //MSG_RZ_SHOW_ARENA_RANK_INFO msg = new MSG_RZ_SHOW_ARENA_RANK_INFO();
            //msg.PcUid = uid;
            //msg.Rank = rank;
            //msg.Page = pks.Page;
            //msg.TotalCount = Api.ArenaMng.TotalCount;
            //List<RankBaseModel> list = Api.ArenaMng.GetList(pks.Page);
            //foreach (var item in list)
            //{
            //    MSG_RZ_ARENA_RANK_INFO info = GetArenaRankInfo(item);
            //    msg.List.Add(info);
            //}
            //Write(msg);


            RankListModel rankListModel = new RankListModel();
            rankListModel.Type = RankType.Arena;
            rankListModel.Page = pks.Page;
            rankListModel.TotalCount = Api.ArenaMng.TotalCount;
            rankListModel.OwnerInfo = rankInfo;

            List<RankBaseModel> list = Api.ArenaMng.GetList(pks.Page);
            PlayerRankModel info = new PlayerRankModel();
            foreach (var player in list)
            {
                info = new PlayerRankModel();
                info.RankInfo = new RankBaseModel();
                info.RankInfo.Uid = player.Uid;
                info.RankInfo.Rank = player.Rank;
                info.RankInfo.Score = player.Score;

                RedisPlayerInfo baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(player.Uid);
                if (baseInfo != null)
                {
                    info.BaseInfo = baseInfo;
                }
                else
                {
                    Log.Warn($"player {uid} GetCampBattleRankList load rank list error: can not find data in server");
                }
                rankListModel.RankList.Add(info);
            }

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                Log.Warn($"player {uid} GetCampBattleRankList failed: not find client ");
                return;
            }

            MSG_RZ_GET_RANK_LIST rankMsg = RankManager.PushRankListMsg(rankListModel, rankListModel.Type);
            client.Write(rankMsg);
        }

        public void OnResponse_ChallengeWinChangeRank(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CHALLENGE_WIN_CHANGE_RANK pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHALLENGE_WIN_CHANGE_RANK>(stream);
            int pcUid = pks.PcUid;
            int pcRank = pks.PcRank;
            int challengerUid = pks.ChallengerUid;
            int challengerRank = pks.ChallengerRank;

            int pcHistoryRank = pks.HistoryRank;


            MSG_RZ_CHALLENGE_WIN_CHANGE_RANK msg = new MSG_RZ_CHALLENGE_WIN_CHANGE_RANK();
            msg.PcUid = pcUid;
            msg.OldRank = pcRank;
            msg.NewRank = pcRank;

            msg.OldScore = pks.OldScore;
            msg.HistoryRank = pks.HistoryRank;
            msg.Result = pks.Result;
            msg.Reward = pks.Reward;


            if (pks.Result == (int)DungeonResult.Success)
            {
                //玩家，替换排名
                RankBaseModel playerRankInfo = Api.ArenaMng.GetArenaRankInfoByUid(pcUid);
                if (playerRankInfo != null)
                {
                    //排名内
                    pcRank = playerRankInfo.Rank;
                }
                else
                {
                    //未在排名内
                    pcRank = 0;
                }
                RankBaseModel challengerRankInfo = null;
                if (challengerUid > 0)
                {
                    //玩家，替换排名
                    challengerRankInfo = Api.ArenaMng.GetArenaRankInfoByUid(challengerUid);
                    if (challengerRankInfo != null)
                    {
                        //排名内
                        challengerRank = challengerRankInfo.Rank;
                    }
                    else
                    {
                        //未在排名内
                        challengerRank = 0;
                    }
                }
                else
                {
                    //机器人，用rank获取排名位置上的信息
                    challengerRankInfo = Api.ArenaMng.GetArenaRankInfoByRank(challengerRank);
                    if (challengerRankInfo != null)
                    {
                        //排名内
                        challengerUid = challengerRankInfo.Uid;
                    }
                    else
                    {
                        //未在排名内
                        challengerUid = 0;
                    }
                }

                //检查是否替换
                if (Api.ArenaMng.CanExchangeRank(challengerRank, pcRank))
                {
                    //替换
                    Api.ArenaMng.ExchangePlayerAndChallengerRank(pcUid, pcRank, challengerUid, challengerRank);
                    //同步DB
                    if (pcUid > 0 && challengerRank > 0)
                    {
                        if (pcHistoryRank == 0 || challengerRank < pcHistoryRank)
                        {
                            Api.GameDBPool.Call(new QueryUpdateArenaAllRank(pcUid, challengerRank, challengerRank));
                            msg.HistoryRank = challengerRank;

                            int oldRewardId = 0;
                            int newRewardId = 0;
                            //历史最高变更，检查奖励邮件
                            newRewardId = ArenaLibrary.GetFirstRankRewardId(challengerRank);

                            if (pcHistoryRank > 0)
                            {
                                oldRewardId = ArenaLibrary.GetFirstRankRewardId(pcHistoryRank);
                            }

                            //发送奖励邮件
                            List<RankRewardInfo> list = ArenaLibrary.GetFirstRankRewards(oldRewardId, newRewardId);
                            if (list.Count > 0)
                            {
                                foreach (var item in list)
                                {
                                    //发送邮件
                                    Api.EmailMng.SendPersonEmail(pcUid, item.EmailId, item.Rewards);
                                    //记录已经发送奖励
                                    Api.GameDBPool.Call(new QueryUpdateArenaMaxRankReward(pcUid, item.Id));
                                }
                            }
                        }
                        else
                        {
                            Api.GameDBPool.Call(new QueryUpdateArenaRank(pcUid, challengerRank));
                        }
                    }

                    if (challengerUid > 0)
                    {
                        Api.GameDBPool.Call(new QueryUpdateArenaRank(challengerUid, pcRank));

                        if (challengerRank <= ArenaLibrary.LoseEmailRank)
                        {
                            //需要通知被挑战者排名变动
                            EmailInfo email = EmailLibrary.GetEmailInfo(ArenaLibrary.LoseEmail);
                            if (email != null)
                            {
                                if (challengerUid > 0)
                                {
                                    RedisPlayerInfo info = Api.RPlayerInfoMng.GetPlayerInfo(pcUid);
                                    if (info != null)
                                    {
                                        string body = string.Format(email.Body, info.GetStringValue(HFPlayerInfo.Name), pcRank);
                                        Api.EmailMng.SendPersonEmail(challengerUid, email, body);
                                    }
                                }
                            }
                            else
                            {
                                Log.Warn("gm send email not find email id:{0}", EmailLibrary.HeroAppraiseEmail);
                            }
                        }
                        NotifyLoseArenaFirst(challengerUid);
                    }

                    msg.NewRank = challengerRank;
                    msg.IsChange = true;
                    Api.ArenaMng.RankSort();

                    //Client challengerClient = ZoneManager.GetClient(challengerUid);
                    //if (challengerClient != null)
                    {
                        //通知排名变更
                        MSG_RZ_CHALLENGER_RANK_CHANGE challengerMsg = new MSG_RZ_CHALLENGER_RANK_CHANGE();
                        challengerMsg.PcUid = challengerUid;
                        challengerMsg.OldRank = challengerRank;
                        challengerMsg.NewRank = pcRank;
                        ZoneManager.Broadcast(challengerMsg);
                    }
                }
            }

            Write(msg);
        }

        public void OnResponse_GetArenaDailyReward(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ARENA_DAILY_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ARENA_DAILY_REWARD>(stream);

            Client client = ZoneManager.GetClient(uid);
            if (client == null)
            {
                //没有缓存信息，查看玩家是否在线
                Log.Warn("player {0} get arena daily reaward failed: not find ", uid);
                return;
            }

            foreach (var id in pks.Ids)
            {
                RankRewardInfo info = ArenaLibrary.GetDailyRankRewardInfoById(id);
                if (info != null)
                {
                    //发送邮件
                    Api.EmailMng.SendPersonEmail(uid, info.EmailId, info.Rewards);
                    //清理数据库
                    Api.GameDBPool.Call(new QueryClearArenaDailyRankReward(uid));
                }
                else
                {
                    Log.Warn("player {0} get arena daily reward failed: not find reward info", uid);
                }
            }
        }

        public void OnResponse_ChangeChallengerDefensive(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_AREMA_DEFEMDER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_AREMA_DEFEMDER>(stream);
            Api.RPlayerInfoMng.CheckUpdatePlayerInfo(uid, true);

            //将信息添加到缓存中
            RedisPlayerInfo baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(uid);
            if (baseInfo != null)
            {
                string defensive = "";
                foreach (var item in pks.Defensive)
                {
                    defensive += item +":"+pks.DefPoses[pks.Defensive.IndexOf(item)]+ "|";
                }
                baseInfo.SetValue(HFPlayerInfo.ArenaDefensive, defensive);
                baseInfo.SetValue(HFPlayerInfo.ArenaPower, pks.Power);
            }
        }

        private void NotifyLoseArenaFirst(int pcUid)
        {
            MSG_RZ_LOSE_ARENA_FIRST msg = new MSG_RZ_LOSE_ARENA_FIRST();
            msg.Uid = pcUid;
            Client client = Api.ZoneManager.GetClient(pcUid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Api.ZoneManager.Broadcast(msg);
            }
        }

        public void OnResponse_UpdateAreanaDefensivePet(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_ARENA_DEFENSIVE_PET pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_ARENA_DEFENSIVE_PET>(stream);
            Api.RPlayerInfoMng.CheckUpdatePlayerInfo(uid, true);

            //将信息添加到缓存中
            RedisPlayerInfo baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(uid);
            if (baseInfo != null)
            {
                //baseInfo.SetValue(HFPlayerInfo.ArenaPet, pks.PetId);排行榜查看信息才会用到
                baseInfo.SetValue(HFPlayerInfo.ArenaPower, pks.Power);
            }
        }
    }
}
