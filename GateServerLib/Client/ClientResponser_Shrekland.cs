using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_ShreklandUseRoulette(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHREKLAND_USE_ROULETTE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHREKLAND_USE_ROULETTE>(stream);
            MSG_GateZ_SHREKLAND_USE_ROULETTE request = new MSG_GateZ_SHREKLAND_USE_ROULETTE();
            request.Type = msg.Type;
            request.Num = msg.Num;
            WriteToZone(request);
        }

        public void OnResponse_ShreklandRefreshRewards(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_SHREKLAND_REFRESH_REWARDS request = new MSG_GateZ_SHREKLAND_REFRESH_REWARDS();
            WriteToZone(request);
        }

        public void OnResponse_ShreklandGetScoreReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHREKLAND_GET_SCORE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHREKLAND_GET_SCORE_REWARD>(stream);
            MSG_GateZ_SHREKLAND_GET_SCORE_REWARD request = new MSG_GateZ_SHREKLAND_GET_SCORE_REWARD();
            request.RewardId = msg.RewardId;
            WriteToZone(request);
        }
    }
}
