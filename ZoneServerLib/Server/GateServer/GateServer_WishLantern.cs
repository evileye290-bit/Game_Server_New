using System.IO;
using Logger;
using Message.Gate.Protocol.GateZ;

namespace ZoneServerLib
{
    partial class GateServer
    {
        public void OnResponse_WishLanternSelect(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} WishLanternSelect", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OnResponse_WishLanternSelect not in gateid {1} pc list", uid, SubId);
                return;
            }

            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} WishLanternSelect not in map ", uid);
                return;
            }

            MSG_GateZ_WISH_LANTERN_SELECT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_WISH_LANTERN_SELECT_REWARD>(stream);

            player.WishLanternSelectReward(msg.Index);
        }

        public void OnResponse_WishLanternLight(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} WishLanternLight", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OnResponse_WishLanternLight not in gateid {1} pc list", uid, SubId);
                return;
            }

            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} WishLanternLight not in map ", uid);
                return;
            }

            MSG_GateZ_WISH_LANTERN_LIGHT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_WISH_LANTERN_LIGHT>(stream);

            player.WishLanternLight(msg.Index);
        }

        public void OnResponse_WishLanternReset(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} WishLanternReset", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OnResponse_WishLanternReset not in gateid {1} pc list", uid, SubId);
                return;
            }

            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} WishLanternReset not in map ", uid);
                return;
            }

            MSG_GateZ_WISH_LANTERN_RESET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_WISH_LANTERN_RESET>(stream);

            player.WishLanternReset();
        }
    }
}
