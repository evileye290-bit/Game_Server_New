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
        public void OnResponse_SaveDefensive(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SAVE_DEFEMSIVE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SAVE_DEFEMSIVE>(stream);
            Log.Write("player {0} request save defensive", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} SaveDefensive not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.SaveDefensiveHeros(pks.HeroIds, pks.HeroPoses);
        }

        //充值挑战时间
        public void OnResponse_ResetArenaFightTime(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_RESET_ARENA_FIGHT_TIME pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RESET_ARENA_FIGHT_TIME>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ResetArenaFightTime not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ResetArenaFightTime();
        }

        //领取段位奖励
        public void OnResponse_GetRankLevelReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_RANK_LEVEL_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_RANK_LEVEL_REWARD>(stream);
            Log.Write("player {0} request get rank level {1} reward", uid, pks.Level);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRankLevelReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetRankReward(pks.Level);
        }

        //换一换
        public void OnResponse_GetArenaChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_ARENA_CHALLENGERS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_ARENA_CHALLENGERS>(stream);
            Log.Write("player {0} request get arena challenger", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetArenaChallenger not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ChangeChallengers();
        }

        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_ShowArenaRankInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_ARENA_RANK_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_ARENA_RANK_INFO>(stream);
            Log.Write("player {0} request show arena rank info page {1}", uid, pks.Page);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ShowArenaRankInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ShowArenaRankInfos(pks.Page);
        }


        /// <summary>
        /// 查看挑战这信息
        /// </summary>
        /// <param name="stream"></param>
        public void OnResponse_ShowArenaChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_ARENA_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_ARENA_CHALLENGER>(stream);
            Log.Write("player {0} request show arena challenger info index {1}", uid, pks.Index);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ShowArenaChallengerInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ShowChallengerInfo(pks.Index);
        }

        public void OnResponse_EnterArenaMap(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ENTER_ARENA_MAP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ENTER_ARENA_MAP>(stream);
            Log.Write("player {0} request enter arena map index {1}", uid, pks.Index);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} EnterArenaMap not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.EnterArenaMap(pks.Index);
        }

        public void OnResponse_EnterVersusMap(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_VERSUS_PLAYER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_VERSUS_PLAYER>(stream);
            Log.Write("player {0} request enter versus map chanllenger {1}", uid, pks.ChallengerUid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} EnterVersusMap not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.EnterVersusMapByUid(pks.ChallengerUid);
        }
    }
}
