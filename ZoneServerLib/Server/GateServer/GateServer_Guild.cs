using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using RedisUtility;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_CreateGuild(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CREATE_GUILD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CREATE_GUILD>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} create guild {1}", uid, msg.GuildName);
            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.CreateGuild(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} CreateGuild is offline.", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} CreateGuild not find player.", pcUid);
                }
            }
        }

    }
}
