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
        public void OnResponse_GetMidAutumnInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_MIDAUTUMN_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_MIDAUTUMN_INFO>(stream);
            Log.Write("player {0} request get mid autumn info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  get mid autumn info not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetMidAutumnInfo();
        }

        public void OnResponse_DrawMidAutumnReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DRAW_MIDAUTUMN_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DRAW_MIDAUTUMN_REWARD>(stream);
            Log.Write("player {0} request draw mid autumn reward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} draw mid autumn reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.DrawMidAutumnReward(msg.Free, msg.Consecutive);
        }

        public void OnResponse_GetMidAutumnScoreReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_MIDAUTUMN_SCORE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_MIDAUTUMN_SCORE_REWARD>(stream);
            Log.Write("player {0} request get mid autumn score reward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get mid autumn score reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetMidAutumnScoreReward(msg.RewardId);
        }
    }
}
