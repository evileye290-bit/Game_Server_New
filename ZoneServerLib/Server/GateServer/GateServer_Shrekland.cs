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
        public void OnResponse_ShreklandUseRoulette(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHREKLAND_USE_ROULETTE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHREKLAND_USE_ROULETTE>(stream);
            Log.Write("player {0} request shrekland use roulette type {1} num {2}", uid, msg.Type, msg.Num);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} shrekland use roulette not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.UseShreklandRoulette(msg.Type, msg.Num);
        }

        public void OnResponse_ShreklandRefreshRewards(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request shrekland refresh rewards", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} shrekland refresh rewards not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.RefreshShreklandRewards();
        }

        public void OnResponse_ShreklandGetScoreReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHREKLAND_GET_SCORE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHREKLAND_GET_SCORE_REWARD>(stream);
            Log.Write("player {0} request shrekland get score reward {1}", uid, msg.RewardId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} shrekland get score reward {1} not in gateid {2} pc list", uid, msg.RewardId, SubId);
                return;
            }
            player.GetShreklandScoreReward(msg.RewardId);
        }
    }
}
