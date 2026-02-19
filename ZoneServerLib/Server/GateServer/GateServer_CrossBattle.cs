using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Logger;
using EnumerateUtility;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        /// <summary>
        /// 防守阵容
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_SaveCrossBattleDefensive(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_CROSS_QUEUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_CROSS_QUEUE>(stream);
            Log.Write("player {0} request save cross battle defensive", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} SaveCrossBattleDefensive not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.UpdateCrossQueue(pks.HeroDefInfos);
        }
        //领取活跃奖励
        public void OnResponse_GetCrossActiveReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_BATTLE_ACTIVE_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_BATTLE_ACTIVE_REWARD>(stream);
            Log.Write("player {0} request GetCrossActiveReward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRankLevelReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossActiveReward();
        }

        //领取海选奖励
        public void OnResponse_GetCrossPreliminaryReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_BATTLE_PRELIMINARY_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_BATTLE_PRELIMINARY_REWARD>(stream);
            Log.Write("player {0} request GetCrossPreliminaryReward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRankLevelReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossPreliminaryReward();
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_EnterCrossMap(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ENTER_CROSS_BATTLE_MAP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ENTER_CROSS_BATTLE_MAP>(stream);
            Log.Write("player {0} request EnterCrossMap", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} EnterCrossMap not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossPreliminaryChallenger();
        }

        /// <summary>
        /// 高手殿堂
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_ShowCrossLeaderInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_CROSS_LEADER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CROSS_LEADER_INFO>(stream);
            Log.Write("player {0} request ShowCrossLeaderInfo", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ShowCrossLeaderInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ShowCrossSeasonLeaderInfos();
        }

 

        /// <summary>
        /// 查看决赛信息
        /// </summary>
        /// <param name="stream"></param>
        public void OnResponse_GetCrossFinalsInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_CROSS_BATTLE_FINALS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CROSS_BATTLE_FINALS>(stream);
            Log.Write("player {0} request GetCrossFinalsInfo team {1}", uid, pks.TeamId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetCrossFinalsInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ShowCrossBattleFinalsInfo(pks.TeamId);
        }




        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_ShowCrossBattleChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_CROSS_BATTLE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CROSS_BATTLE_CHALLENGER>(stream);
            Log.Write("player {0} request ShowCrossBattleChallenger showUid {1} mainId {2}", uid, pks.Uid, pks.MainId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ShowCrossBattleChallenger not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ShowCrossBattleChallenger(pks.Uid, pks.MainId);
        }

        public void OnResponse_GetCrossBattleVedio(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_VIDEO>(stream);
            Log.Write("player {0} request GetCrossBattleVedio team {1} vedio {2}", uid, pks.TeamId, pks.VedioId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetCrossBattleVedio not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetCrossBattleVedio(pks.TeamId, pks.VedioId);
        }

        //领取全服奖励
        public void OnResponse_GetCrossServerReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_BATTLE_SERVER_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_BATTLE_SERVER_REWARD>(stream);
            Log.Write("player {0} request GetCrossServerReward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRankLevelReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossServerReward();
        }


        //竞猜
        public void OnResponse_GetCrossGuessingInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_GET_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_GUESSING_INFO>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRankLevelReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetCrossGuessingInfo();
        }

        public void OnResponse_CrossGuessingChoose(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CROSS_GUESSING_CHOOSE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CROSS_GUESSING_CHOOSE>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} CrossGuessingChoose not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.CrossGuessingChoose(pks.Choose);
        }
    }
}
