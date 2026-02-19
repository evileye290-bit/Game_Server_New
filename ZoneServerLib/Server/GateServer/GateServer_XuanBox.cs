using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    partial class GateServer
    {
        public void OnResponse_GetXuanBoxInfo(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} GetXuanBoxInfo", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OnResponse_GetXuanBoxInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetRouletteInfo not in map ", uid);
                return;
            }

            player.GetXuanBoxInfoByLoading();
        }

        public void OnResponse_XuanBoxRandom(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_XUANBOX_RANDOM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_XUANBOX_RANDOM>(stream);

            Log.Write("player {0} XuanBoxRandom", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} XuanBoxRandom not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.XuanBoxRandom(msg.Num);
        }

        public void OnResponse_XuanBoxReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_XUANBOX_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_XUANBOX_REWARD>(stream);

            Log.Write("player {0} XuanBoxReward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} XuanBoxReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.XuanBoxReward(msg.Id);
        }

    }
}
