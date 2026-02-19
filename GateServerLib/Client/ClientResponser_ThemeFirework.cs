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
        public void OnResponse_GetThemeFireworkInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_THEME_FIREWORK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_THEME_FIREWORK_INFO>(stream);
            MSG_GateZ_THEME_FIREWORK_INFO request = new MSG_GateZ_THEME_FIREWORK_INFO();
            WriteToZone(request);
        }

        public void OnResponse_GetThemeFireworkScoreReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_THEME_FIREWORK_SCORE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_THEME_FIREWORK_SCORE_REWARD>(stream);
            MSG_GateZ_THEME_FIREWORK_SCORE_REWARD request = new MSG_GateZ_THEME_FIREWORK_SCORE_REWARD();
            request.RewardId = msg.RewardId;
            WriteToZone(request);
        }

        public void OnResponse_GetThemeFireworkUseCountReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_THEME_FIREWORK_USECOUNT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_THEME_FIREWORK_USECOUNT_REWARD>(stream);
            MSG_GateZ_THEME_FIREWORK_USECOUNT_REWARD request = new MSG_GateZ_THEME_FIREWORK_USECOUNT_REWARD();
            request.RewardId = msg.RewardId;
            WriteToZone(request);
        }
    }
}
