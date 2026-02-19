using System.IO;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_ReturnCrossChallengePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO>(stream);
            Api.WriteToCross(pks);
        }

        public void OnResponse_GetCrossChallengeChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO>(stream);
            Client client = ZoneManager.GetClient(uid);
            if (client != null)
            {
                ZR_BattlePlayerMsg infoMsg = Api.CrossChallengeMng.GetPlayerInfoMsg(pks.ChallengerUid);
                if (infoMsg != null)
                {
                    pks.Challenger = infoMsg;
                    pks.Result = (int)ErrorCode.Success;
                }
                else
                {
                    pks.Result = (int)ErrorCode.NotFindChallengerInfo;
                }
                client.Write(pks);
            }
            else
            {
                Log.Warn($"player {uid} get client cross challenger failed : not find player");
            }
        }

        public void OnResponse_AddCrossChallengeChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ADD_CROSS_CHALLENGE_CHALLENGER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ADD_CROSS_CHALLENGE_CHALLENGER_INFO>(stream);
            Api.CrossChallengeMng.AddPlayerInfoMsg(pks.Info1);
            Api.CrossChallengeMng.AddPlayerInfoMsg(pks.Info2);
        }

        public void OnResponse_SetCrossChallengeResult(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SET_CROSS_CHALLENGE_BATTLE_RESULT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SET_CROSS_CHALLENGE_BATTLE_RESULT>(stream);

            MSG_RC_SET_CROSS_CHALLENGE_RESULT msg = new MSG_RC_SET_CROSS_CHALLENGE_RESULT();
            msg.TimingId = pks.TimingId;
            msg.GroupId = pks.GroupId;
            msg.TeamId = pks.TeamId;
            msg.FightId = pks.FightId;
            msg.WinUid = pks.WinUid;
            msg.FileName = pks.FileName;
            msg.BattleInfo = pks.BattleInfo;
            Api.CrossServer.Write(msg);
        }

        public void OnResponse_ShowCrossChallengeFinals(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SHOW_CROSS_CHALLENGE_BATTLE_FINALS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SHOW_CROSS_CHALLENGE_BATTLE_FINALS>(stream);
            Log.Write($"player {uid} get ShowCrossChallengeFinals info by team {pks.TeamId}.");
            //找到玩家说明玩家在线 ，通知玩家发送信息回来
            MSG_RC_SHOW_CROSS_CHALLENGE_FINALS msg = new MSG_RC_SHOW_CROSS_CHALLENGE_FINALS();
            msg.TeamId = pks.TeamId;
            msg.MianId = Api.MainId;
            Api.CrossServer.Write(msg, uid);
        }

        public void OnResponse_ShowCrossChallengeChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SHOW_CROSS_CHALLENGE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SHOW_CROSS_CHALLENGE_CHALLENGER>(stream);
            MSG_RC_SHOW_CROSS_CHALLENGE_CHALLENGER request = new MSG_RC_SHOW_CROSS_CHALLENGE_CHALLENGER();
            request.Uid = pks.Uid;
            request.MainId = pks.MainId;
            Api.CrossServer.Write(request, uid);
        }

        public void OnResponse_UpdateCrossChallengeHeroInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CROSS_CHALLENGE_CHALLENGER_HERO_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CROSS_CHALLENGE_CHALLENGER_HERO_INFO>(stream);
            MSG_RC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO request = new MSG_RC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO();
            request.Uid = pks.Uid;
            request.MainId = pks.MainId;
            request.SeeUid = pks.SeeUid;
            request.SeeMainId = pks.SeeMainId;
            foreach (var item in pks.Heros)
            {
                request.Heros.Add(GetPlayerHeroInfoMsg(item));
            }
            Api.CrossServer.Write(request, uid);
        }

        public void OnResponse_GetCrossChallengeVedio(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CROSS_CHALLENGE_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CROSS_CHALLENGE_VIDEO>(stream);
            MSG_RC_GET_CROSS_CHALLENGE_VIDEO request = new MSG_RC_GET_CROSS_CHALLENGE_VIDEO();
            request.TeamId = pks.TeamId;
            request.VedioId = pks.VedioId;
            request.MainId = Api.MainId;
            request.Index = pks.Index;
            Api.CrossServer.Write(request, uid);
        }

        public void OnResponse_GetCrossChallengeStartTime(MemoryStream stream, int uid = 0)
        {
            MSG_RC_GET_CROSS_CHALLENGE_START request = new MSG_RC_GET_CROSS_CHALLENGE_START();
            Api.CrossServer.Write(request, uid);
        }

        public void OnResponse_GetCrossChallengeGuessingInfo(MemoryStream stream, int uid = 0)
        {
            Api.CrossChallengeGuessingMng.GetGuessingPlayersInfo(uid);
        }

        public void OnResponse_CrossChallengeGuessingChoose(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CROSS_CHALLENGE_GUESSING_CHOOSE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CROSS_CHALLENGE_GUESSING_CHOOSE>(stream);

            MSG_RZ_CROSS_CHALLENGE_GUESSING_CHOOSE msg = new MSG_RZ_CROSS_CHALLENGE_GUESSING_CHOOSE();
            msg.TimingId = pks.TimingId;
            msg.Choose = pks.Choose;

            msg.Result = Api.CrossChallengeGuessingMng.CheckGuessingModel(pks.TimingId);
            if (msg.Result == (int)ErrorCode.Success)
            {
                string reward = Api.CrossChallengeGuessingMng.GetGuessingChooseReward(pks.TimingId, uid);
                if (string.IsNullOrEmpty(reward))
                {
                    msg.HasReward = false;
                }
                else
                {
                    msg.HasReward = true;
                    Api.CrossChallengeGuessingMng.SetPlayerChoose(pks.TimingId, uid, pks.Choose);
                }
            }
            Write(msg, uid);
        }

        public void OnResponse_CrossChallengeGuessingReward(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CROSS_CHALLENGE_GUESSING_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CROSS_CHALLENGE_GUESSING_REWARD>(stream);

            Api.CrossChallengeGuessingMng.SetPlayerChoose(pks.TimingId, uid, pks.Choose);

            Api.CrossChallengeGuessingMng.SetPlayerReward(pks.TimingId, uid, pks.Reward);
        }
    }
}
