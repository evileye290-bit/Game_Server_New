using Logger;
using Message.Manager.Protocol.MGate;
using ServerFrame;
using System.IO;
using System.Linq;

namespace GateServerLib
{
    public partial class ManagerServer
    {
        public void OnResponse_ZoneTransform(MemoryStream stream, int uid = 0)
        {
            MSG_MGate_ZONE_TRANSFORM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MGate_ZONE_TRANSFORM>(stream);
            Log.WarnLine($"gm request zone transform server {msg.MainId} from zones {string.Join("-", msg.FromZones)} to zones {string.Join("-", msg.ToZones)}");

            ZoneTransformManager.Instance.UpdateZonesInfo(msg.IsForce, msg.FromZones.ToList(), msg.ToZones.ToList());
        }
    }
}
