using EnumerateUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateCM;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_Geography(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GEOGRAPHY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GEOGRAPHY>(stream);
            MSG_GateZ_GEOGRAPHY requestMsg = new MSG_GateZ_GEOGRAPHY();
            requestMsg.PcUid = Uid;
            requestMsg.Longitude = msg.Longitude;
            requestMsg.Latitude = msg.Latitude;
            WriteToZone(requestMsg);

            //if (Rooms.NearbyId <= 0)
            //{
            //    MSG_GateCM_GET_NEARBY_ROOM cmMsg = new MSG_GateCM_GET_NEARBY_ROOM();
            //    cmMsg.PcUid = Uid;
            //    cmMsg.Longitude = msg.Longitude;
            //    cmMsg.Latitude = msg.Latitude;
            //    WriteToChatManager(cmMsg);
            //}
            //else
            //{
            //    if (requestMsg.Longitude == 0 && requestMsg.Latitude == 0)
            //    {
            //        Rooms.ClearNearbyRoomId();
            //    }
            //}
        }
    }
}
