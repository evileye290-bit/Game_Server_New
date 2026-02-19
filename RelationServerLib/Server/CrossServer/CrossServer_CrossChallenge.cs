using System.IO;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerFrame;

namespace RelationServerLib
{
    public partial class CrossServer
    {
        /// <summary>
        /// 获取当前服务器前8
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_GetCrossChallengeFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_RANK pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_RANK>(stream);
            Log.Write("cross server GetCrossChallengeFinalsPlayerRank");
            //获取当前赛季人数
            Api.CrossChallengeMng.LoadFinalsPlayers();
        }

        //获取玩家对战信息
        public void OnResponse_GetCrossChallengePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_PLAYER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_PLAYER>(stream);
            Log.Write("cross server GetCrossChallengePlayerInfo");

            MSG_RZ_GET_CROSS_CHALLENGE_PLAYER msg = new MSG_RZ_GET_CROSS_CHALLENGE_PLAYER();
            msg.GetType = pks.GetType;
            msg.Player1 = GetPlayerBaseInfoMsg(pks.Player1);
            msg.Player2 = GetPlayerBaseInfoMsg(pks.Player2);
            msg.TimingId = pks.TimingId;
            msg.GroupId = pks.GroupId;
            msg.TeamId = pks.TeamId;
            msg.FightId = pks.FightId;
            FrontendServer server = Api.ZoneManager.GetOneServer();
            if (server != null)
            {
                server.Write(msg, pks.Player1.Uid);
            }
        }

        //显示决赛信息
        public void OnResponse_ShowCrossChallengeFinals(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_SHOW_CROSS_CHALLENGE_FINALS_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_SHOW_CROSS_CHALLENGE_FINALS_INFO>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                //没有缓存信息，查看玩家是否在线
                Log.Warn("player {0} ShowCrossChallengeFinals failed: not find ", uid);
                return;
            }

            MSG_RZ_SHOW_CROSS_CHALLENGE_FINALS_INFO msg = new MSG_RZ_SHOW_CROSS_CHALLENGE_FINALS_INFO();
            msg.TeamId = pks.TeamId;

            foreach (var item in pks.List)
            {
                msg.List.Add(GetPlayerBaseInfoMsg(item));
            }

            msg.Fight1.AddRange(pks.Fight1);
            msg.Fight2.AddRange(pks.Fight2);
            msg.Fight3.AddRange(pks.Fight3);

            pks.BattleInfoList.ForEach(x=>
            {
                MSG_RZ_CROSS_CHALLENGE_WIN_INFO info = new MSG_RZ_CROSS_CHALLENGE_WIN_INFO(){BattleId = x.BattleId};
                info.BattleInfo.Add(x.BattleInfo);
                msg.BattleInfoList.Add(info);
            });

            client.Write(msg);
        }

        //显示玩家信息
        public void OnResponse_ShowCrossChallengeChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_RCR_CROSS_CHALLENGE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RCR_CROSS_CHALLENGE_CHALLENGER>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                //没有缓存信息，查看玩家是否在线
                Log.Warn("player {0} ShowCrossChallengeChallenger failed: not find ", uid);
                return;
            }

            MSG_RZ_CROSS_CHALLENGE_CHALLENGER msg = new MSG_RZ_CROSS_CHALLENGE_CHALLENGER();
            msg.Uid = pks.Uid;
            msg.MainId = pks.MainId;
            msg.Result = pks.Result;
            foreach (var item in pks.Heros)
            {
                msg.Heros.Add(GetPlayerHeroInfoMsg(item));
            }
            client.Write(msg);
        }

        //显示玩家阵容信息
        public void OnResponse_GetCrossChallengeHeros(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_HEROS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_HEROS>(stream);

            MSG_RZ_GET_CROSS_CHALLENGE_HEROS msg = new MSG_RZ_GET_CROSS_CHALLENGE_HEROS();
            msg.Uid = pks.Uid;
            msg.MainId = pks.MainId;
            msg.SeeUid = pks.SeeUid;
            msg.SeeMainId = pks.SeeMainId;
            Client client = Api.ZoneManager.GetClient(pks.Uid);
            if (client == null)
            {
                FrontendServer server = Api.ZoneManager.GetOneServer();
                if (server != null)
                {
                    server.Write(msg);
                }
            }
            else
            {
                client.Write(msg);
            }
        }

        //获取录像名
        public void OnResponse_GetCrossChallengeVedio(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_GET_CROSS_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_GET_CROSS_VIDEO>(stream);
            MSG_RZ_GET_CROSS_CHALLENGE_VIDEO request = new MSG_RZ_GET_CROSS_CHALLENGE_VIDEO();
            request.TeamId = pks.TeamId;
            request.VedioId = pks.VedioId;
            request.VideoName = pks.VideoName;
            request.Index = pks.Index;
            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(request);
            }
        }

        //发送决赛奖励
        public void OnResponse_SendCrossChallengeFinalsReward(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_SEND_FINALS_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_SEND_FINALS_REWARD>(stream);
            //发送邮件
            Api.EmailMng.SendPersonEmail(pks.Uid, pks.EmailId, pks.Reward, 0, pks.Param);
        }     

        //清理排行榜
        public void OnResponse_ClearCrossChallengeFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            //MSG_CorssR_CLEAR_PLAYER_FINAL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CLEAR_PLAYER_FINAL>(stream);
            Log.Write("cross server ClearCrossChallengeFinalsPlayerRank");
            //清空所有人决战排名
            Api.CrossChallengeMng.ClaerLastFinalsPlayers();
        }

        //更新玩家排名
        public void OnResponse_UpdateCrossChallengeFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL>(stream);
            Log.Write("cross server UpdateCrossChallengeFinalsPlayerRank");
            //更新当前赛季人数
            Api.CrossChallengeMng.SyncFinalsPlayerResult(pks.List);
        }

        //清理排行榜
        public void OnResponse_ClearCrossChallengeRanks(MemoryStream stream, int uid = 0)
        {
            Log.Write("cross server ClearCrossChallengeRanks");
            //更新当前赛季人数
            Api.RankMng.CrossChallengeRank.ResetRankList();

            //清理数据
            Api.GameDBPool.Call(new QueryRefreshAllCrossChallengeResult());

            MSG_RZ_CROSS_CHALLENGE_CLEAR_BATTLE_RANK msg = new MSG_RZ_CROSS_CHALLENGE_CLEAR_BATTLE_RANK();
            Api.ZoneManager.Broadcast(msg);

            //清理下注
            Api.CrossChallengeGuessingMng.ClearGuessingInfo();

            MSG_RZ_CROSS_CHALLENGE_OPEN_GUESSING_TEAM guessMsg = new MSG_RZ_CROSS_CHALLENGE_OPEN_GUESSING_TEAM();
            guessMsg.TeamId = Api.CrossChallengeGuessingMng.TeamId;
            Api.ZoneManager.Broadcast(guessMsg);
        }

        //同步开启时间
        public void OnResponse_CrossChallengeStart(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_BATTLE_START>(stream);
            Log.Write("cross server CrossChallengeStart");

            MSG_RZ_CROSS_CHALLENGE_BATTLE_START msg = new MSG_RZ_CROSS_CHALLENGE_BATTLE_START();
            msg.Time = pks.Time;
            msg.TeamId = Api.CrossChallengeGuessingMng.TeamId;
            foreach (var server in Api.ZoneManager.ServerList)
            {
                server.Value.Write(msg, uid);
            }
        }

        //通知跑马灯和邮件
        public void OnResponse_CrossChallengeNoticePlayerBattleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_BATTLE_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_BATTLE_INFO>(stream);
            Log.Write("cross server CrossChallengeNoticePlayerBattleInfo");
            //更新当前赛季人数
            Api.CrossChallengeMng.NoticePlayerBattleInfo(pks.TimingId, pks.List);
        }
        //通知第一名
        public void OnResponse_CrossChallengeNoticePlayerFirst(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_WIN_FINAL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_WIN_FINAL>(stream);
            Log.Write("cross server CrossChallengeNoticePlayerFirst");
            //更新当前赛季人数
            Api.CrossChallengeMng.NoticePlayerFirst(pks.MainId, pks.Name);
            RepeatedField<int> list = new RepeatedField<int>();
            list.Add(pks.Uid);
            Api.CrossChallengeGuessingMng.SendGuessingReward((int)CrossBattleTiming.BattleTime6, list);
        }
        //获取开战时间
        public void OnResponse_GetCrossChallengeStart(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_START>(stream);
            Log.Write("cross server GetCrossChallengeStart");

            MSG_RZ_GET_CROSS_CHALLENGE_BATTLE_START msg = new MSG_RZ_GET_CROSS_CHALLENGE_BATTLE_START();
            msg.Time = pks.Time;
            msg.TeamId = Api.CrossChallengeGuessingMng.TeamId;
            foreach (var server in Api.ZoneManager.ServerList)
            {
                server.Value.Write(msg, uid);
            }
        }

        //开启竞猜
        public void OnResponse_CrossChallengeGuessingStart(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_INFO>(stream);
            Log.Write("cross server CrossChallengeGuessingStart");
            Api.CrossChallengeGuessingMng.SetGuessingPlayers(pks.TimingId, pks.Uid1, pks.Uid2, pks.TeanId);

            MSG_RZ_CROSS_CHALLENGE_OPEN_GUESSING_TEAM msg = new MSG_RZ_CROSS_CHALLENGE_OPEN_GUESSING_TEAM();
            msg.TeamId = Api.CrossChallengeGuessingMng.TeamId;
            Api.ZoneManager.Broadcast(msg);
        }

        public void OnResponse_OnResponse_CrossChallengeGetGuessingPlayersInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_GET_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_GET_GUESSING_INFO>(stream);
            Log.Write("cross server CrossChallengeGetGuessingPlayersInfo");

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                MSG_RZ_GET_CROSS_CHALLENGE_GUESSING_INFO msg = new MSG_RZ_GET_CROSS_CHALLENGE_GUESSING_INFO();
                foreach (var info in pks.InfoList)
                {
                    msg.InfoList.Add(GetPlayerBaseInfoMsg(info));
                }
                msg.GuessingInfos.AddRange(Api.CrossChallengeGuessingMng.GetGuessingPlayersInfoMsg(uid));
                client.Write(msg);
            }
        }


        public void OnResponse_CrossChallengeGuessingResult(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_RESULT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_RESULT>(stream);
            Log.Write("cross server CrossChallengeGuessingResult");
            Api.CrossChallengeGuessingMng.SendGuessingReward(pks.TimingId, pks.UidList);
        }

        //通知跑马灯和邮件
        public void OnResponse_CrossChallengeNoticePlayerBattleTeamId(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID>(stream);
            Log.Write("cross server CrossChallengeNoticePlayerBattleTeamId");
            //更新当前赛季人数
            Api.CrossChallengeMng.SyncFinalsPlayerTeamId(pks.Uid, pks.TeanId);
        }

        public void OnResponse_ReturnCrossChallengePlayerInfoNew(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO>(stream);
            Log.Write("cross server ReturnCrossChallengePlayerInfoNew");

            FrontendServer server = Api.ZoneManager.GetOneServer();
            if (server != null)
            {
                server.Write(pks, pks.Player1.Uid);
            }
        }

        //返回信息
        public void OnResponse_ReturnCrossChallengePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RCR_RETURN_CROSS_CHALLENGE_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RCR_RETURN_CROSS_CHALLENGE_PLAYER_INFO>(stream);
            Log.Write("cross server ReturnCrossChallengePlayerInfo");

            MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO msg = new MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO();
            msg.GetType = pks.GetType;
            msg.Player1 = GetBattlePlayerInfoMsg(pks.Player1);
            msg.Player2 = GetBattlePlayerInfoMsg(pks.Player2);
            msg.TimingId = pks.TimingId;
            msg.GroupId = pks.GroupId;
            msg.TeamId = pks.TeamId;
            msg.FightId = pks.FightId;

            FrontendServer server = Api.ZoneManager.GetOneServer();
            if (server != null)
            {
                server.Write(msg, pks.Player1.Uid);
            }
        }

        //聊天喇叭
        //public void OnResponse_ChatTrumpet(MemoryStream stream, int uid = 0)
        //{
        //    MSG_CrossR_CHAT_TRUMPET pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CrossR_CHAT_TRUMPET>(stream);
        //    Log.Write("cross server ChatTrumpet");
        //    MSG_RZ_CHAT_TRUMPET msg = new MSG_RZ_CHAT_TRUMPET();
        //    msg.MainId = pks.MainId;
        //    msg.ItemId = pks.ItemId;
        //    msg.Words = pks.Words;
        //    msg.PcInfo = GetRZSpeakerInfo(pks.PcInfo);

        //    FrontendServer zServer = Api.ZoneManager.GetOneServer();
        //    zServer.Write(msg);
        //    //Api.ZoneManager.Broadcast(msg);
        //}

        //private RZ_SPEAKER_INFO GetRZSpeakerInfo(CR_SPEAKER_INFO msg)
        //{
        //    RZ_SPEAKER_INFO pcInfo = new RZ_SPEAKER_INFO();
        //    pcInfo.Uid = msg.Uid;
        //    pcInfo.Name = msg.Name;
        //    pcInfo.Camp = msg.Camp;
        //    pcInfo.Level = msg.Level;
        //    pcInfo.FaceIcon = msg.FaceIcon;
        //    pcInfo.ShowFaceJpg = msg.ShowFaceJpg;
        //    pcInfo.FaceFrame = msg.FaceFrame;
        //    pcInfo.Sex = msg.Sex;
        //    pcInfo.Title = msg.Title;
        //    pcInfo.TeamId = msg.TeamId;
        //    pcInfo.HeroId = msg.HeroId;
        //    pcInfo.GodType = msg.GodType;
        //    pcInfo.ChatFrameId = msg.ChatFrameId;
        //    pcInfo.ArenaLevel = msg.ArenaLevel;
        //    return pcInfo;
        //}
    }
}
