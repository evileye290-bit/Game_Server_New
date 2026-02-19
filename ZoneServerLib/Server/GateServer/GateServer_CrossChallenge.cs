using System.IO;
using Logger;
using Message.Gate.Protocol.GateZ;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        /// <summary>
        /// 防守阵容
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        private void OnResponse_SaveCrossChallengeDefensive(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_CROSS_CHALLENGE_QUEUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_CROSS_CHALLENGE_QUEUE>(stream);
            Log.Write("player {0} request save cross battle defensive", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} SaveCrossChallengeBattleDefensive not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.UpdateCrossChallengeQueue(pks.HeroDefInfos);
        }

        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        private void OnResponse_ShowCrossChallengeChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_CROSS_CHALLENGE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CROSS_CHALLENGE_CHALLENGER>(stream);
            Log.Write("player {0} request ShowCrossChallengeBattleChallenger showUid {1} mainId {2}", uid, pks.Uid, pks.MainId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ShowCrossChallengeBattleChallenger not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ShowCrossChallengeChallenger(pks.Uid, pks.MainId);
        }

        //领取活跃奖励
        private void OnResponse_GetCrossChallengeActiveReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_CHALLENGE_ACTIVE_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_CHALLENGE_ACTIVE_REWARD>(stream);
            Log.Write("player {0} request GetCrossChallengeActiveReward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRankLevelReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossChallengeActiveReward();
        }

        //领取海选奖励
        private void OnResponse_GetCrossChallengePreliminaryReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD>(stream);
            Log.Write("player {0} request GetCrossChallengePreliminaryReward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRankLevelReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossChallengePreliminaryReward();
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        private void OnResponse_EnterCrossChallengeMap(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ENTER_CROSS_CHALLENGE_MAP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ENTER_CROSS_CHALLENGE_MAP>(stream);
            Log.Write("player {0} request EnterCrossChallengeMap", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} EnterCrossChallengeMap not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossChallengePreliminaryChallenger();
        }

        /// <summary>
        /// 高手殿堂
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        private void OnResponse_ShowCrossChallengeLeaderInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_CROSS_CHALLENGE_LEADER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CROSS_CHALLENGE_LEADER_INFO>(stream);
            Log.Write("player {0} request ShowCrossChallengeLeaderInfo", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ShowCrossChallengeLeaderInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ShowCrossChallengeSeasonLeaderInfos();
        }

 

        /// <summary>
        /// 查看决赛信息
        /// </summary>
        /// <param name="stream"></param>
        private void OnResponse_GetCrossChallengeFinalsInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_CROSS_CHALLENGE_FINALS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CROSS_CHALLENGE_FINALS>(stream);
            Log.Write("player {0} request GetCrossChallengeFinalsInfo team {1}", uid, pks.TeamId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetCrossChallengeFinalsInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ShowCrossChallengeFinalsInfo(pks.TeamId);
        }

        private void OnResponse_GetCrossChallengeVideo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_CHALLENGE_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_CHALLENGE_VIDEO>(stream);
            Log.Write("player {0} request GetCrossChallengeBattleVedio team {1} vedio {2}", uid, pks.TeamId, pks.VedioId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetCrossChallengeBattleVedio not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetCrossChallengeVideo(pks.TeamId, pks.VedioId, pks.Index);
        }

        //领取全服奖励
        private void OnResponse_GetCrossChallengeServerReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_CHALLENGE_SERVER_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_CHALLENGE_SERVER_REWARD>(stream);
            Log.Write("player {0} request GetCrossChallengeServerReward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRankLevelReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossChallengeServerReward();
        }


        //竞猜
        private void OnResponse_GetCrossChallengeGuessingInfo(MemoryStream stream, int uid = 0)
        {
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRankLevelReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetCrossChallengeGuessingInfo();
        }

        private void OnResponse_CrossChallengeGuessingChoose(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CROSS_CHALLENGE_GUESSING_CHOOSE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CROSS_CHALLENGE_GUESSING_CHOOSE>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} CrossChallengeGuessingChoose not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.CrossChallengeGuessingChoose(pks.Choose);
        }

        private void OnResponse_CrossChallengeSwapQueue(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CROSS_CHALLENGE_SWAP_QUEUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CROSS_CHALLENGE_SWAP_QUEUE>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} CrossChallengeSwapQueue not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.CrossChallengeSwapQueue(pks.Queue1, pks.Queue2);
        }

        private void OnResponse_CrossChallengeSwapHero(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CROSS_CHALLENGE_SWAP_HERO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CROSS_CHALLENGE_SWAP_HERO>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} CrossChallengeSwapHero not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.CrossChallengeSwapHero(pks);
        }
    }
}
