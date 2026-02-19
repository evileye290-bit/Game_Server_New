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
        public void OnResponse_TransferEnterMap(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TRANSFER_ENTER_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TRANSFER_ENTER_MAP>(stream);
            Log.Write("player {0} request transfer enter map {1}", uid, msg.MapId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} TransferEnterMap not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.TransferEnterMap(msg.MapId);
        }
    }
}
