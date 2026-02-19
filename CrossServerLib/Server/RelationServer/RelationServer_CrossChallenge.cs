using System.Collections.Generic;
using System.IO;
using EnumerateUtility;
using Google.Protobuf;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RC;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace CrossServerLib
{
    public partial class RelationServer
    {
        //获取决赛玩家信息
        public void OnResponse_GetCrossChallengeFinalsPlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_CROSS_CHALLENGE_GET_FINALS_PLAYER_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_CROSS_CHALLENGE_GET_FINALS_PLAYER_LIST>(stream);
            Log.Write($"player {uid} get GetFinalsPlayerInfo from main {pks.MainId} rank {pks.RankList.Count}");
            int groupId = CrossChallengeLibrary.GetGroupId(pks.MainId);
            if (groupId > 0)
            {
                int serverId = CrossChallengeLibrary.GetGroupServerId(pks.MainId);
                //将信息添加到缓存中
                foreach (var playerInfo in pks.RankList)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in playerInfo.BaseInfo)
                    {
                        HFPlayerInfo key = (HFPlayerInfo)item.Key;
                        switch (key)
                        {
                            case HFPlayerInfo.MainId:
                                dataList[key] = pks.MainId;
                                break;
                            default:
                                dataList[key] = item.Value;
                                break;
                        }
                    }
                    RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
                    RelationManager.CrossChallengeMng.AddPlayerBaseInfo(playerInfo.Rank.Uid, groupId, rInfo);
                    RelationManager.CrossChallengeMng.AddBattleGroupInfo(playerInfo.Rank.Uid, groupId, serverId, pks.MainId, playerInfo.Rank.Rank);
                }
            }
        }

        //返回挑战者信息
        public void OnResponse_ReturnCrossChallengePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO>(stream);

            int pcUid = msg.Player2.Uid;
            int mainId = msg.Player2.MainId;
            Log.Write($"player {msg.Player1.Uid} ReturnCrossChallengePlayerInfo player 2 {pcUid} mainId {mainId}");
            //没有缓存信息，查看玩家是否在线
            FrontendServer relation = Api.RelationManager.GetSinglePointServer(mainId);
            if (relation != null)
            {
                //通知玩家发送信息回来
                relation.Write(msg, pcUid);
            }
            else
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player {0} mainId {1} relation.", pcUid, mainId);
            }
        }

        //决赛对战结果
        public void OnResponse_SetCrossChallengeResult(MemoryStream stream, int uid = 0)
        {
            MSG_RC_SET_CROSS_CHALLENGE_RESULT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_SET_CROSS_CHALLENGE_RESULT>(stream);
            Log.Write($"player {uid} SetCrossChallengeResult player 2 {pks.TimingId} GroupId {pks.GroupId} TeamId {pks.TeamId} FightId {pks.FightId} FileName {pks.FileName}");

            RelationManager.CrossChallengeMng.SetCrossChallengeResult(pks.TimingId, pks.GroupId, pks.TeamId, pks.FightId, pks.WinUid);

            RelationManager.CrossChallengeMng.SetCrossChallengeVideo(pks.TimingId, pks.GroupId, pks.TeamId, pks.FightId, pks.FileName, pks.BattleInfo);
        }

        //获取决赛信息
        public void OnResponse_ShowCrossChallengeFinals(MemoryStream stream, int uid = 0)
        {
            MSG_RC_SHOW_CROSS_CHALLENGE_FINALS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_SHOW_CROSS_CHALLENGE_FINALS>(stream);
            Log.Write($"player {uid} ShowCrossChallengeFinals MianId{pks.MianId} TeamId {pks.TeamId} ");
            MSG_CorssR_SHOW_CROSS_CHALLENGE_FINALS_INFO msg = RelationManager.CrossChallengeMng.GetFinalsInfoMsg(uid, pks.MianId, pks.TeamId);
            if (msg != null)
            {
                Write(msg, uid);
            }
        }

        //查看决赛阵容信息
        public void OnResponse_ShowCrossChallengeChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_RC_SHOW_CROSS_CHALLENGE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_SHOW_CROSS_CHALLENGE_CHALLENGER>(stream);
            Log.Write($"player {uid} ShowCrossChallengeChallenger uid {pks.Uid} MianId{pks.MainId} MainId {MainId} ");
            RelationManager.CrossChallengeMng.GetPlayerHeroInfoMsg(pks.Uid, pks.MainId, uid, MainId);
        }

        //更新阵容信息
        public void OnResponse_UpdateCrossChallengeHeroInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO>(stream);
            //Log.Write($"player {uid} UpdateCrossChallengeHeroInfo SeeUid {pks.SeeUid} SeeMainId {pks.SeeMainId}  ");

            Dictionary<int, int> queueBattlePower = new Dictionary<int, int>() {{1, 0}, {2, 0}, {3, 0}};
            long battlePower = 0;
            List<CrossHeroInfo> list = new List<CrossHeroInfo>();
            foreach (var item in pks.Heros)
            {
                list.Add(GetPlayerHeroInfoMsg(item));
                battlePower += item.Power;

                if (item.QueueNum > 0 && item.QueueNum < 4)
                {
                    queueBattlePower[item.QueueNum] += item.Power;
                }
            }
            RelationManager.CrossChallengeMng.UpdateHeroInfo(uid, list);

            RedisPlayerInfo playerInfo = RelationManager.CrossChallengeMng.GetRedisPlayerInfo(pks.Uid);
            if (playerInfo != null)
            {
                string queueBattlePowerStr = string.Join("|", queueBattlePower.Values);
                //修改战力
                playerInfo.SetValue(HFPlayerInfo.BattlePower, battlePower);
                playerInfo.SetValue(HFPlayerInfo.CrossChallengePower, battlePower);
                playerInfo.SetValue(HFPlayerInfo.CrossChallengeQueuePower, queueBattlePowerStr);

                Api.CrossRedis.Call(new OperateUpdateCrossChallengePlayerPowerInfo(pks.Uid, battlePower, queueBattlePowerStr));
            }

            if (pks.SeeUid > 0 && pks.SeeMainId > 0)
            {
                RelationManager.CrossChallengeMng.GetPlayerHeroInfoMsg(pks.Uid, pks.MainId, pks.SeeUid, pks.SeeMainId);
            }
        }


        public void OnResponse_GetCrossChallengeVideo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_GET_CROSS_CHALLENGE_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_CROSS_CHALLENGE_VIDEO>(stream);
            Log.Write($"player {uid} GetCrossChallengeVideo  {pks.MainId} uid {uid} TeamId {pks.TeamId} VedioId {pks.VedioId}  ");
            RelationManager.CrossChallengeMng.GetCrossChallengeVideo(pks.MainId, uid, pks.TeamId, pks.VedioId, pks.Index);
        }

        public void OnResponse_GetCrossChallengeStartTime(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_GET_CROSS_CHALLENGE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_CROSS_CHALLENGE_START>(stream);
            MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_START msg = new MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_START();
            msg.Time = RelationManager.CrossChallengeMng.FirstStartTime;
            Write(msg, uid);
        }

        public void OnResponse_GetCrossChallengeGuessingPlayersInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_GET_CROSS_CHALLENGE_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_CROSS_CHALLENGE_GUESSING_INFO>(stream);

            MSG_CorssR_CROSS_CHALLENGE_GET_GUESSING_INFO msg = new MSG_CorssR_CROSS_CHALLENGE_GET_GUESSING_INFO();
            foreach (var pcUid in pks.Uids)
            {
                RedisPlayerInfo rInfo = RelationManager.CrossChallengeMng.GetRedisPlayerInfo(pcUid);
                if (rInfo != null)
                {
                    CorssR_BattlePlayerMsg info = RelationManager.CrossChallengeMng.GetPlayerBaseInfoMsg(rInfo, 0);
                    msg.InfoList.Add(info);
                }
            }
            Write(msg, uid);
        }
    }
}
