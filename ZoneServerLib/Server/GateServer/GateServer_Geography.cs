using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;
using System;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        //public void OnResponse_Geography(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_GEOGRAPHY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GEOGRAPHY>(stream);

        //    PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        player.Geography(msg.Longitude,msg.Latitude);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(msg.PcUid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("sync geography fail, player {0} is offline.", msg.PcUid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("sync geography fail,can not find player {0}.", msg.PcUid);
        //        }
        //    }
        //}

    }
}
