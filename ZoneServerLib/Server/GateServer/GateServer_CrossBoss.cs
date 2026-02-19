using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetCrossBossInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_GET_CROSS_BOSS_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_BOSS_INFO>(stream);
            Log.Write("player {0} GetCrossBossInfo", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetCrossBossInfo not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetCrossBossInfo not in map ", uid);
                return;
            }

            player.GetCrossBossInfo();
        }

        /// <summary>
        /// 防守阵容
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_UpdateCrossBossQueue(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_CROSS_BOSS_QUEUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_CROSS_BOSS_QUEUE>(stream);
            Log.Write("player {0} request save cross boss defensive", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} UpdateCrossBossQueue not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.UpdateCrossBossQueue(pks.HeroDefInfos);
        }

        /// <summary>
        /// 领取宝箱奖励
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_GetCrossBossPassReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_BOSS_PASS_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_BOSS_PASS_REWARD>(stream);
            Log.Write("player {0} request GetCrossBossPassReward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetCrossBossPassReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossBossPassReward(pks.DungeonId);
        }

        public void OnResponse_GetCrossBossRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_BOSS_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_BOSS_RANK_REWARD>(stream);
            Log.Write("player {0} request GetCrossBossRankReward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetCrossBossRankReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossBossRankReward(pks.DungeonId);
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_EnterCrossBossMap(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ENTER_CROSS_BOSS_MAP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ENTER_CROSS_BOSS_MAP>(stream);
            Log.Write("player {0} request EnterCrossBossMap", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} EnterCrossBossMap not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.StartChallengeCrossBoss();
        }

        public void OnResponse_CrossBossChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CROSS_BOSS_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CROSS_BOSS_CHALLENGER>(stream);
            Log.Write("player {0} request CrossBossChallenger", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} CrossBossChallenger not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetCrossBossChallenger();
        }

        public void OnResponse_ChallengeCrossBoss(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHALLENGE_CROSS_BOSS_MAP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHALLENGE_CROSS_BOSS_MAP>(stream);
            Log.Write("player {0} request ChallengeCrossBoss", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ChallengeCrossBoss not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ChallengeCrossBossDefense();
        }
    }
}
