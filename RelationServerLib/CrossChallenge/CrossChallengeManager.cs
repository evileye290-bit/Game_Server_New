using System;
using System.Collections.Generic;
using System.Linq;
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

namespace RelationServerLib
{
    public class CrossChallengeManager
    {
        private RelationServerApi server { get; set; }
        //uid, rank
        private Dictionary<int, ZR_BattlePlayerMsg> uidInfoList = new Dictionary<int, ZR_BattlePlayerMsg>();
        private Dictionary<int, DateTime> uidAddList = new Dictionary<int, DateTime>();

        public CrossChallengeManager(RelationServerApi server)
        {
            this.server = server;
        }

        public void ClaerLastFinalsPlayers()
        {
            server.GameDBPool.Call(new QueryClearCrossChallengeTimeKey());

            MSG_RZ_CROSS_CHALLENGE_CLEAR_PLAYER_FINAL clearMsg = new MSG_RZ_CROSS_CHALLENGE_CLEAR_PLAYER_FINAL();
            server.ZoneManager.Broadcast(clearMsg);
        }

        public void SyncFinalsPlayerResult(MapField<int, int> dic)
        {
            MSG_RZ_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL msg = new MSG_RZ_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL();

            foreach (var kv in dic)
            {
                int uid = kv.Key;
                int rank = kv.Value;
                msg.List.Add(uid, rank);

                server.GameDBPool.Call(new QueryUpdaateCrossChallengeTimeKey(uid, rank));

                RankRewardInfo reward = CrossChallengeLibrary.GetRankRewardInfo(rank);
                if (reward != null)
                {
                    //通知玩家发送信息回来
                    server.EmailMng.SendPersonEmail(uid, reward.EmailId, reward.Rewards);

                    server.TrackingLoggerMng.TrackRankLog(server.MainId, RankType.CrossChallenge.ToString(), rank, 0, uid, server.Now());

                    //发称号卡邮件
                    if (rank == 1)
                    {
                        //发送全服奖励
                        server.GameDBPool.Call(new QueryUpdateCrossChallengeServerReward(0, (int)CrossRewardState.None));
                        //通知全服
                        MSG_RZ_CROSS_CHALLENGE_SERVER_REWARD rewardMsg = new MSG_RZ_CROSS_CHALLENGE_SERVER_REWARD();
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
            OperateGetRankScore op = new OperateGetRankScore(RankType.CrossChallenge, server.MainId, 0, CrossChallengeLibrary.FightPlayerCount - 1);
            server.GameRedis.Call(op, ret =>
            {
                Dictionary<int, RankBaseModel> uidRank = op.uidRank;

                if (uidRank.Count > 0)
                {
                    MSG_RC_CROSS_CHALLENGE_GET_FINALS_PLAYER_LIST rankMsg = PushFinalsPlayersMsg(uidRank);
                    server.CrossServer.Write(rankMsg);

                    List<int> uids = new List<int>(uidRank.Keys);
                    //检查舒服刷新数据
                    server.RPlayerInfoMng.RefreshPlayerList(uids);

                    MSG_RZ_GET_CROSS_CHALLENGE_CHALLENGER_HERO_INFO zoneMsg = new MSG_RZ_GET_CROSS_CHALLENGE_CHALLENGER_HERO_INFO();
                    zoneMsg.Uids.AddRange(uids);
                    FrontendServer zServer = server.ZoneManager.GetOneServer();
                    if (zServer != null)
                    {
                        zServer.Write(zoneMsg);
                    }

                    //发送通知进入总决赛
                    SendBattleEmail64(uids);
                    //公告
                    BroadcastBattlePlayersAnnouncement(ANNOUNCEMENT_TYPE.CROSS_CHALLENGE_64, uids);
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
                server.EmailMng.SendPersonEmail(uid, CrossChallengeLibrary.BattleEmail64);
            }
        }

        public void NoticePlayerBattleInfo(int timingId, RepeatedField<int> list)
        {
            //邮件通知
            CrossBattleTiming timing = (CrossBattleTiming)timingId;
            int emailId = CrossChallengeLibrary.GetBattleEmailId(timing);
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
            ANNOUNCEMENT_TYPE type = ANNOUNCEMENT_TYPE.CROSS_CHALLENGE_64;
            switch (timing)
            {
                case CrossBattleTiming.ShowTime1:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_CHALLENGE_32;
                    break;
                case CrossBattleTiming.ShowTime2:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_CHALLENGE_16;
                    break;
                case CrossBattleTiming.ShowTime3:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_CHALLENGE_8;
                    break;
                case CrossBattleTiming.ShowTime4:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_CHALLENGE_4;
                    break;
                case CrossBattleTiming.ShowTime5:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_CHALLENGE_2;
                    break;
                case CrossBattleTiming.ShowTime6:
                    isAnnouncement = true;
                    type = ANNOUNCEMENT_TYPE.CROSS_CHALLENGE_1;
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
            server.ZoneManager.BroadcastAnnouncement(ANNOUNCEMENT_TYPE.CROSS_CHALLENGE_1, list);
        }
        public MSG_RC_CROSS_CHALLENGE_GET_FINALS_PLAYER_LIST PushFinalsPlayersMsg(Dictionary<int, RankBaseModel> uidRank)
        {
            MSG_RC_CROSS_CHALLENGE_GET_FINALS_PLAYER_LIST rankMsg = new MSG_RC_CROSS_CHALLENGE_GET_FINALS_PLAYER_LIST();
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


        public MSG_RZ_CROSS_CHALLENGE_RANK_INFO GetArenaRankInfoMsg(PlayerRankBaseInfo baseInfo)
        {
            MSG_RZ_CROSS_CHALLENGE_RANK_INFO info = new MSG_RZ_CROSS_CHALLENGE_RANK_INFO();
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
            server.GameDBPool.Call(new QueryUpdaateCrossChallengeTeamId(uid, teamId));

            MSG_RZ_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID msg = new MSG_RZ_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID();
            msg.Uid = uid;
            msg.TeanId = teamId;
            server.ZoneManager.Broadcast(msg);
        }
    }
}
