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
        public void OnResponse_GetThemeFireworkInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_THEME_FIREWORK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_THEME_FIREWORK_INFO>(stream);
            Log.Write("player {0} request get theme firework info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  get theme firework info not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetThemeFireworkInfo();
        }

        public void OnResponse_GetThemeFireworkScoreReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_THEME_FIREWORK_SCORE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_THEME_FIREWORK_SCORE_REWARD>(stream);
            Log.Write("player {0} request get theme firework score reward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  get theme firework score reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetThemeFireworkScoreReward(msg.RewardId);
        }

        public void OnResponse_GetThemeFireworkUseCountReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_THEME_FIREWORK_USECOUNT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_THEME_FIREWORK_USECOUNT_REWARD>(stream);
            Log.Write("player {0} request get theme firework usecount reward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  get theme firework usecount reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetThemeFireworkUseCountReward(msg.RewardId);
        }
    }
}
