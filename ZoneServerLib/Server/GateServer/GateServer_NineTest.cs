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
        public void OnResponse_GetNineTestInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_NINETEST_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_NINETEST_INFO>(stream);
            Log.Write("player {0} request get nine test info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  get nine test info not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetNineTestInfo();
        }

        public void OnResponse_NineTestClickGrid(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_NINETEST_CLICK_GRID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_NINETEST_CLICK_GRID>(stream);
            Log.Write("player {0} request nine test click grid", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  nine test click grid not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.ClickNineTestGrid(msg.Index);
        }

        public void OnResponse_NineTestScoreReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_NINETEST_SCORE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_NINETEST_SCORE_REWARD>(stream);
            Log.Write("player {0} request nine test score reward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  nine test score reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetNineTestScoreReward(msg.RewardId);
        }

        public void OnResponse_NineTestReset(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_NINETEST_RESET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_NINETEST_RESET>(stream);
            Log.Write("player {0} request nine test reset", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} nine test reset not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.NineTestReset(msg.Free);
        }
    }
}
